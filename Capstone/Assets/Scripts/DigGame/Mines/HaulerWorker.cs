using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    public sealed class HaulerWorker : Worker
    {
        public override WorkerKind Kind => WorkerKind.Hauler;
        public override int Wage => GameConfig.HaulerWage;

        protected override void OnInitialized()
        {
            if (ActorRenderer.sprite == null)
                ActorRenderer.sprite = SpriteFactory.Framed(new Color32(120, 210, 255, 255), new Color32(20, 60, 90, 255));
        }

        enum HaulState { ToMine, Loading, ToSurface, Unloading, Stuck }
        HaulState _state = HaulState.ToMine;
        readonly List<LootItem> _carrying = new List<LootItem>();
        List<Vector2Int> _path;
        int _pathIndex;
        float _stateTimer;
        float _repathTimer;

        public override void DumpCargoToMine()
        {
            if (_carrying.Count == 0) return;
            Mine.Stockpile.AddRange(_carrying);
            _carrying.Clear();
            _path = null;
            _state = HaulState.ToMine;
        }

        protected override void TickWork(float deltaTime)
        {
            switch (_state)
            {
                case HaulState.ToMine:
                    StatusText = "지하 수거장으로 이동";
                    if (MoveAlongPath(Mine.CenterCell, deltaTime))
                    {
                        _state = HaulState.Loading;
                        _stateTimer = 0f;
                    }
                    break;

                case HaulState.Loading:
                    StatusText = "적재 중";
                    _stateTimer += deltaTime;
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
                    if (MoveAlongPath(Game.RecycleDropCell, deltaTime))
                    {
                        _state = HaulState.Unloading;
                        _stateTimer = 0f;
                    }
                    break;

                case HaulState.Unloading:
                    StatusText = "하역 중";
                    _stateTimer += deltaTime;
                    if (_stateTimer >= 0.4f)
                    {
                        _stateTimer = 0f;
                        // 광산 산출물과 플레이어가 맡긴 쓰레기를 지상 공장에서 정식 재활용한다.
                        int itemCount = _carrying.Count;
                        int paid = Game.RecycleAtFactory(_carrying, $"대량 운반 일꾼 · 도시광산 #{Mine.Id}");
                        Mine.RecordAutomaticSale(itemCount, paid);
                        _carrying.Clear();
                        _path = null;
                        _state = HaulState.ToMine;
                    }
                    break;

                case HaulState.Stuck:
                    StatusText = "길이 막힘";
                    _repathTimer += deltaTime;
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
        bool MoveAlongPath(Vector2Int goal, float deltaTime)
        {
            var here = WorldGrid.WorldToCell(transform.position);

            if (_path == null || _pathIndex >= _path.Count || _path[_path.Count - 1] != goal)
            {
                _path = Pathfinder.Find(Game.Grid, here, goal);
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
                GameConfig.HaulerSpeed * GameConfig.CellSize * deltaTime);

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
