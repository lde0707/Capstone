using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 지상 문명. 플레이어의 집 한 채로 시작해서
    ///  - 하루 할당량을 채울 때마다 집이 빛나며 한 단계 화려해지고
    ///  - 하루가 지날 때마다 이웃집·나무·길이 생기고 주민이 이사 온다.
    /// 마을 → 도시로 자라는 모습이 유일하고 확실한 외적 보상이다.
    /// </summary>
    public class Village : MonoBehaviour
    {
        public const int OrderGroundCover = 3;
        public const int OrderHouse = 8;
        public const int OrderProp = 10;
        public const int OrderVillager = 20;
        public const int OrderFx = 49;

        /// <summary>이웃집이 들어설 자리 (중심에서의 칸 오프셋). 안쪽부터 채워진다.</summary>
        static readonly int[] NeighborOffsets =
        {
            -10, 9, -14, 13, -18, 17, -22, 21, -26, 25,
            -30, 29, -34, 33, -38, 37, -42, 41, -46, 45,
            -50, 49, -54, 53, -58, 57
        };

        /// <summary>집과 집 사이를 채울 소품 자리.</summary>
        static readonly int[] PropOffsets =
        {
            -12, 11, -16, 15, -20, 19, -24, 23, -28, 27,
            -32, 31, -36, 35, -40, 39, -44, 43, -48, 47,
            -52, 51, -56, 55, -60, 59
        };

        static readonly VillageSpriteCatalog.Prop[] PropCycle =
        {
            VillageSpriteCatalog.Prop.TreeBig,
            VillageSpriteCatalog.Prop.LampPost,
            VillageSpriteCatalog.Prop.TreeSmall,
            VillageSpriteCatalog.Prop.Fence,
            VillageSpriteCatalog.Prop.Bush,
            VillageSpriteCatalog.Prop.Bench,
            VillageSpriteCatalog.Prop.TreeBig,
            VillageSpriteCatalog.Prop.FlowerBed,
            VillageSpriteCatalog.Prop.Sign,
            VillageSpriteCatalog.Prop.TreeSmall,
            VillageSpriteCatalog.Prop.Crates,
            VillageSpriteCatalog.Prop.LampPost,
            VillageSpriteCatalog.Prop.Well,
            VillageSpriteCatalog.Prop.TreeBig,
            VillageSpriteCatalog.Prop.WaterTower,
            VillageSpriteCatalog.Prop.Bush,
        };

        GameManager _game;
        WorldGrid _grid;
        Transform _root;
        System.Random _rng;

        SpriteRenderer _home;
        int _homeTier;
        int _centerX;
        int _neighborsBuilt;
        int _propsBuilt;
        int _villagerCount;
        int _pendingUpgrades;
        bool _upgrading;

        readonly List<SpriteRenderer> _houses = new List<SpriteRenderer>();
        readonly Dictionary<int, SpriteRenderer> _groundCover = new Dictionary<int, SpriteRenderer>();
        readonly List<Villager> _villagers = new List<Villager>();

        /// <summary>마을에 살 수 있는 주민 수 상한.</summary>
        public const int MaxVillagers = 26;

        public int HomeTier => _homeTier;
        public int HouseCount => _houses.Count + 1;
        public int Population => _villagers.Count;
        public int TownMinX { get; private set; }
        public int TownMaxX { get; private set; }

        /// <summary>마을 발전 단계 이름 (HUD·로그용).</summary>
        public string StageName
        {
            get
            {
                int score = _homeTier * 2 + _houses.Count;
                if (score <= 1) return "허허벌판";
                if (score <= 4) return "작은 정착지";
                if (score <= 8) return "마을";
                if (score <= 13) return "번화한 읍내";
                return "도시";
            }
        }

        // ==================================================================
        public void Build(GameManager game)
        {
            _game = game;
            _grid = game.Grid;
            _rng = new System.Random(game.Seed * 7919 + 13);
            _centerX = _grid.Width / 2;
            TownMinX = _centerX - 8;
            TownMaxX = _centerX + 8;

            _root = new GameObject("Village").transform;
            _root.SetParent(transform, false);

            PaintGroundCover(_centerX - 46, _centerX + 46);
            BuildHome();
            AddProp();
            AddProp();
            AddVillagers(1);

            _grid.OnTileChanged += HandleTileChanged;
        }

        void OnDestroy()
        {
            if (_grid != null) _grid.OnTileChanged -= HandleTileChanged;
        }

        /// <summary>지표면 칸을 파내면 그 위의 잔디·길도 함께 사라진다.</summary>
        void HandleTileChanged(int x, int y, TileKind kind)
        {
            if (y != GameConfig.SurfaceRow) return;
            if (!_groundCover.TryGetValue(x, out var sr) || sr == null) return;
            if (TileInfo.IsWalkable(kind))
            {
                Destroy(sr.gameObject);
                _groundCover.Remove(x);
            }
        }

        // ------------------------------------------------------------------
        static float SurfaceY => -GameConfig.SurfaceRow * GameConfig.CellSize;

        static Vector3 StandOn(int gridX) =>
            new Vector3((gridX + 0.5f) * GameConfig.CellSize, SurfaceY, 0f);

        SpriteRenderer Spawn(string name, Sprite sprite, Vector3 position, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        void PaintGroundCover(int fromX, int toX)
        {
            for (int x = fromX; x <= toX; x++)
            {
                if (!_grid.InBounds(x, GameConfig.SurfaceRow)) continue;
                if (_groundCover.ContainsKey(x)) continue;
                TileKind surface = _grid.Get(x, GameConfig.SurfaceRow);
                if (surface != TileKind.Grass && !TileInfo.IsDiggable(surface)) continue;

                var sr = Spawn($"Grass({x})", VillageSpriteCatalog.Grass(Mathf.Abs(x * 7 + 3)),
                    WorldGrid.CellToWorld(x, GameConfig.SurfaceRow), OrderGroundCover);
                _groundCover[x] = sr;
            }
        }

        /// <summary>마을이 커지면 중심가에 포장도로가 깔린다.</summary>
        void PaveRoad(int fromX, int toX, int style)
        {
            for (int x = fromX; x <= toX; x++)
            {
                if (!_groundCover.TryGetValue(x, out var sr) || sr == null) continue;
                sr.sprite = VillageSpriteCatalog.Road(style * 2 + (Mathf.Abs(x) % 2));
            }
        }

        // ------------------------------------------------------------------
        void BuildHome()
        {
            _homeTier = 0;
            _home = Spawn("Home", VillageSpriteCatalog.House(0), StandOn(_centerX), OrderHouse);
        }

        /// <summary>
        /// 할당량 단계를 넘긴 순간.
        /// 집이 커지는 건 <b>하루의 첫 단계 한 번</b>뿐이다 — 7일이면 집이 정확히 6단계까지 자란다.
        /// 2단계부터는 집 대신 마을이 옆으로 넓어지고 주민이 더 몰려온다.
        /// </summary>
        public void OnQuotaStageCleared(int day, int stage)
        {
            if (stage <= 1)
            {
                // 한 프레임에 여러 단계를 넘길 수도 있으므로 연출을 큐에 쌓아 차례로 재생한다.
                _pendingUpgrades++;
                if (!_upgrading) StartCoroutine(UpgradeHomeRoutine());

                AddNeighborHouse();
                AddProp();
                AddVillagers(2);
            }
            else
            {
                // 집은 그대로, 대신 소문을 듣고 이웃이 몰려온다.
                AddNeighborHouse();
                AddNeighborHouse();
                AddProp();
                AddProp();
                AddVillagers(3);
                _game.Log($"{stage}단계 달성 — 소문을 듣고 이웃이 몰려왔다! " +
                    $"집 {HouseCount}채 · 주민 {Population}명 · 지금은 '{StageName}'");
            }

            UpdateRoad();
        }

        /// <summary>새 아침 — 소문을 듣고 이웃과 주민이 더 들어온다.</summary>
        public void OnNewDay(int day)
        {
            AddNeighborHouse();
            AddProp();
            AddProp();
            AddVillagers(2);
            UpdateRoad();

            _game.Log($"밤 사이 이웃이 이사 왔다 — 집 {HouseCount}채 · 주민 {Population}명 · 지금은 '{StageName}'");
        }

        IEnumerator UpgradeHomeRoutine()
        {
            _upgrading = true;
            while (_pendingUpgrades > 0)
            {
                _pendingUpgrades--;
                yield return UpgradeHomeOnce();
            }
            _upgrading = false;
        }

        IEnumerator UpgradeHomeOnce()
        {
            int next = Mathf.Min(_homeTier + 1, VillageSpriteCatalog.HouseTiers - 1);
            bool grew = next != _homeTier;

            Vector3 at = StandOn(_centerX);
            VillageUpgradeFx.Play(_root, at, 3f * GameConfig.CellSize);

            // 빛이 차오르는 동안 집을 하얗게 태운다
            float t = 0f;
            while (t < 0.45f)
            {
                t += Time.deltaTime;
                if (_home != null)
                    _home.color = Color.Lerp(Color.white, new Color(3f, 3f, 3f, 1f), t / 0.45f);
                yield return null;
            }

            if (grew)
            {
                _homeTier = next;
                if (_home != null) _home.sprite = VillageSpriteCatalog.House(_homeTier);
                _game.Log($"할당량 달성! 집이 빛나며 커졌다 → {HouseName(_homeTier)} (마을 단계: {StageName})");
            }
            else
            {
                _game.Log($"할당량 달성! 집은 이미 최고 단계다 — 대신 마을이 넓어진다 ({StageName})");
            }

            t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                if (_home != null)
                    _home.color = Color.Lerp(new Color(3f, 3f, 3f, 1f), Color.white, t / 0.6f);
                yield return null;
            }
            if (_home != null) _home.color = Color.white;
        }

        public static string HouseName(int tier)
        {
            switch (tier)
            {
                case 0: return "다 쓰러져가는 판잣집";
                case 1: return "초가 오두막";
                case 2: return "벽돌집";
                case 3: return "2층 저택";
                case 4: return "3층 석조 대저택";
                default: return "시계탑이 선 시청";
            }
        }

        // ------------------------------------------------------------------
        void AddNeighborHouse()
        {
            if (_neighborsBuilt >= NeighborOffsets.Length) return;

            int offset = NeighborOffsets[_neighborsBuilt++];
            int x = _centerX + offset;
            if (!_grid.InBounds(x, GameConfig.SurfaceRow)) return;

            // 안쪽 집일수록 잘 지어진 집이 들어선다.
            int tier = Mathf.Clamp(_homeTier - 1 + _rng.Next(0, 2), 0,
                VillageSpriteCatalog.HouseTiers - 1);

            var sr = Spawn($"House({x})", VillageSpriteCatalog.House(tier), StandOn(x), OrderHouse);
            _houses.Add(sr);
            VillageUpgradeFx.Play(_root, StandOn(x), 2.2f * GameConfig.CellSize);

            TownMinX = Mathf.Min(TownMinX, x - 2);
            TownMaxX = Mathf.Max(TownMaxX, x + 2);
            PaintGroundCover(TownMinX - 3, TownMaxX + 3);
        }

        void AddProp()
        {
            if (_propsBuilt >= PropOffsets.Length) return;

            int offset = PropOffsets[_propsBuilt];
            var kind = PropCycle[_propsBuilt % PropCycle.Length];
            _propsBuilt++;

            int x = _centerX + offset;
            if (!_grid.InBounds(x, GameConfig.SurfaceRow)) return;

            Spawn($"Prop({kind})", VillageSpriteCatalog.PropSprite(kind), StandOn(x), OrderProp);
            TownMinX = Mathf.Min(TownMinX, x - 1);
            TownMaxX = Mathf.Max(TownMaxX, x + 1);
        }

        void UpdateRoad()
        {
            int style = _homeTier >= 4 ? 2 : _homeTier >= 2 ? 1 : 0;
            PaveRoad(TownMinX, TownMaxX, style);
        }

        void AddVillagers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_villagers.Count >= MaxVillagers) return;

                int x = _centerX + _rng.Next(-10, 11);
                var go = new GameObject($"Villager#{_villagerCount}");
                go.transform.SetParent(_root, false);
                var v = go.AddComponent<Villager>();
                v.Init(this, _villagerCount++, StandOn(x));
                _villagers.Add(v);
            }
        }

        /// <summary>주민이 돌아다닐 수 있는 지상 x 범위 (월드 좌표).</summary>
        public void WanderRange(out float minX, out float maxX)
        {
            minX = (TownMinX + 0.5f) * GameConfig.CellSize;
            maxX = (TownMaxX + 0.5f) * GameConfig.CellSize;
        }
    }

    /// <summary>집이 업그레이드될 때 터지는 빛기둥과 반짝이.</summary>
    public class VillageUpgradeFx : MonoBehaviour
    {
        float _life;
        float _duration;
        float _width;
        SpriteRenderer _beam;
        readonly List<SpriteRenderer> _sparks = new List<SpriteRenderer>();
        readonly List<Vector3> _velocities = new List<Vector3>();

        public static void Play(Transform parent, Vector3 groundPosition, float width)
        {
            var go = new GameObject("UpgradeFx");
            go.transform.SetParent(parent, false);
            go.transform.position = groundPosition;
            var fx = go.AddComponent<VillageUpgradeFx>();
            fx._width = width;
            fx.Setup();
        }

        void Setup()
        {
            _duration = 1.3f;
            _life = _duration;

            var beamGo = new GameObject("Beam");
            beamGo.transform.SetParent(transform, false);
            beamGo.transform.localPosition = new Vector3(0f, 6f, 0f);
            _beam = beamGo.AddComponent<SpriteRenderer>();
            _beam.sprite = SpriteFactory.Solid(Color.white);
            _beam.color = new Color(1f, 0.96f, 0.72f, 0.55f);
            _beam.sortingOrder = Village.OrderFx;
            beamGo.transform.localScale = new Vector3(_width, 12f, 1f);

            for (int i = 0; i < 14; i++)
            {
                var s = new GameObject("Spark");
                s.transform.SetParent(transform, false);
                s.transform.localPosition = new Vector3(
                    Random.Range(-_width * 0.5f, _width * 0.5f), Random.Range(0f, 1.2f), 0f);
                s.transform.localScale = Vector3.one * Random.Range(0.12f, 0.3f);
                var sr = s.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteFactory.Diamond(Color.white);
                sr.color = new Color(1f, 0.94f, 0.66f, 1f);
                sr.sortingOrder = Village.OrderFx + 1;
                _sparks.Add(sr);
                _velocities.Add(new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1.8f, 4.2f), 0f));
            }
        }

        void Update()
        {
            _life -= Time.deltaTime;
            float k = Mathf.Clamp01(_life / _duration);

            if (_beam != null)
            {
                float pulse = Mathf.Sin((1f - k) * Mathf.PI);
                _beam.color = new Color(1f, 0.96f, 0.72f, 0.62f * pulse);
                _beam.transform.localScale = new Vector3(
                    _width * (0.55f + 0.85f * pulse), 12f + 6f * (1f - k), 1f);
            }

            for (int i = 0; i < _sparks.Count; i++)
            {
                var sr = _sparks[i];
                if (sr == null) continue;
                sr.transform.localPosition += _velocities[i] * Time.deltaTime;
                var c = sr.color;
                c.a = k;
                sr.color = c;
            }

            if (_life <= 0f) Destroy(gameObject);
        }
    }
}
