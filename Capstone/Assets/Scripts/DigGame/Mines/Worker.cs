using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 고용된 일꾼. 자동 재활용 일꾼은 20초마다 단독으로 수거·판매하고,
    /// 대량 운반 일꾼은 보관함을 파놓은 통로를 따라 지상 분리수거장까지 옮긴다.
    /// </summary>
    public class Worker : MonoBehaviour
    {
        public WorkerKind Kind;
        public MineSite Mine;

        GameManager _game;
        SpriteRenderer _sr;
        System.Random _rng;

        // 채굴용
        float _extractTimer;

        // 운반용
        enum HaulState { ToMine, Loading, ToSurface, Unloading, Stuck }
        HaulState _state = HaulState.ToMine;
        readonly List<LootItem> _carrying = new List<LootItem>();
        List<Vector2Int> _path;
        int _pathIndex;
        float _stateTimer;
        float _repathTimer;

        public string StatusText { get; private set; } = "";

        /// <summary>싣고 있던 화물을 광산 야적장으로 되돌린다 (정산·해고 시).</summary>
        public void DumpCargoToMine()
        {
            if (_carrying.Count == 0) return;
            Mine.Stockpile.AddRange(_carrying);
            _carrying.Clear();
            _path = null;
            _state = HaulState.ToMine;
        }

        public void Init(GameManager game, WorkerKind kind, MineSite mine, int seed)
        {
            _game = game;
            Kind = kind;
            Mine = mine;
            _rng = new System.Random(seed);

            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = kind == WorkerKind.Miner
                ? SpriteFactory.Framed(new Color32(255, 190, 70, 255), new Color32(90, 55, 10, 255))
                : SpriteFactory.Framed(new Color32(120, 210, 255, 255), new Color32(20, 60, 90, 255));
            _sr.sortingOrder = WorldView.WorkerSortingOrder;
            transform.localScale = Vector3.one * (GameConfig.PlayerScale * 0.85f);

            var c = mine.CenterCell;
            transform.position = WorldGrid.CellToWorld(c.x, c.y);

            if (kind == WorkerKind.Miner)
            {
                // 지하 수거 구역 안에 흩어놓아 겹치지 않게 한다.
                Vector2 center = mine.CenterF;
                float ox = center.x + ((float)_rng.NextDouble() - 0.5f) * (mine.Rect.width - 0.5f);
                float oy = center.y + ((float)_rng.NextDouble() - 0.5f) * (mine.Rect.height - 0.5f);
                transform.position = WorldGrid.GridToWorld(ox, oy);
            }
        }

        void Update()
        {
            if (_game == null || !_game.IsRunning) return;

            if (Kind == WorkerKind.Miner) TickMiner();
            else TickHauler();
        }

        // ---------------- 자동 수거·재활용 일꾼 ----------------
        void TickMiner()
        {
            if (Mine.Reserve <= 0 && Mine.Stockpile.Count == 0)
            {
                StatusText = "고갈";
                return;
            }

            StatusText = Mine.Stockpile.Count > 0 ? "보관품 자동 재활용" : "폐기물 자동 회수";
            _extractTimer += Time.deltaTime;
            if (_extractTimer >= GameConfig.MinerExtractInterval)
            {
                _extractTimer -= GameConfig.MinerExtractInterval;

                // 보관품이 없으면 광산 매장량에서 새 고철을 하나 회수한다.
                if (Mine.Stockpile.Count == 0 && !Mine.ExtractOne(_rng)) return;

                // 수거 일꾼 단독으로도 지상 반입과 재활용까지 끝내 즉시 수익을 낸다.
                var batch = Mine.TakeFromStockpile(1);
                int paid = _game.RecycleAtFactory(batch, $"자동 수거 일꾼 · 도시광산 #{Mine.Id}");
                Mine.RecordAutomaticSale(batch.Count, paid);
            }
        }

        // ---------------- 운반 일꾼 ----------------
        void TickHauler()
        {
            switch (_state)
            {
                case HaulState.ToMine:
                    StatusText = "지하 수거장으로 이동";
                    if (MoveAlongPath(Mine.CenterCell))
                    {
                        _state = HaulState.Loading;
                        _stateTimer = 0f;
                    }
                    break;

                case HaulState.Loading:
                    StatusText = "적재 중";
                    _stateTimer += Time.deltaTime;
                    if (_stateTimer >= 0.6f)
                    {
                        _stateTimer = 0f;
                        if (Mine.Stockpile.Count > 0)
                        {
                            _carrying.AddRange(Mine.TakeFromStockpile(GameConfig.HaulerCarryCapacity));
                            _path = null;
                            _state = HaulState.ToSurface;
                        }
                        else if (Mine.Reserve <= 0)
                        {
                            StatusText = "수거장 비었음";
                        }
                    }
                    break;

                case HaulState.ToSurface:
                    StatusText = $"운반 중 ({_carrying.Count})";
                    if (MoveAlongPath(_game.RecycleDropCell))
                    {
                        _state = HaulState.Unloading;
                        _stateTimer = 0f;
                    }
                    break;

                case HaulState.Unloading:
                    StatusText = "하역 중";
                    _stateTimer += Time.deltaTime;
                    if (_stateTimer >= 0.4f)
                    {
                        _stateTimer = 0f;
                        // 광산 산출물과 플레이어가 맡긴 쓰레기를 지상 공장에서 정식 재활용한다.
                        int itemCount = _carrying.Count;
                        int paid = _game.RecycleAtFactory(_carrying, $"대량 운반 일꾼 · 도시광산 #{Mine.Id}");
                        Mine.RecordAutomaticSale(itemCount, paid);
                        _carrying.Clear();
                        _path = null;
                        _state = HaulState.ToMine;
                    }
                    break;

                case HaulState.Stuck:
                    StatusText = "길이 막힘";
                    _repathTimer += Time.deltaTime;
                    if (_repathTimer >= 1.5f)
                    {
                        _repathTimer = 0f;
                        _path = null;
                        _state = _carrying.Count > 0 ? HaulState.ToSurface : HaulState.ToMine;
                    }
                    break;
            }
        }

        /// <summary>목표 칸까지 경로를 따라 이동. 도착하면 true.</summary>
        bool MoveAlongPath(Vector2Int goal)
        {
            var here = WorldGrid.WorldToCell(transform.position);

            if (_path == null || _pathIndex >= _path.Count || _path[_path.Count - 1] != goal)
            {
                _path = Pathfinder.Find(_game.Grid, here, goal);
                _pathIndex = 0;
                if (_path == null)
                {
                    _state = HaulState.Stuck;
                    _repathTimer = 0f;
                    return false;
                }
            }

            Vector3 target = WorldGrid.CellToWorld(_path[_pathIndex].x, _path[_pathIndex].y);
            transform.position = Vector3.MoveTowards(
                transform.position, target,
                GameConfig.HaulerSpeed * GameConfig.CellSize * Time.deltaTime);

            if ((transform.position - target).sqrMagnitude < 0.0004f)
            {
                transform.position = target;
                _pathIndex++;
                if (_pathIndex >= _path.Count)
                {
                    _path = null;
                    return true;
                }
            }
            return false;
        }
    }
}
