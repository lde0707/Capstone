using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 지상 문명. 원시 시대의 움집 한 채로 시작해서
    ///  - 하루의 첫 할당량을 채우면 <b>시대가 도약</b>하며 마을 전체가 다른 시대의 모습으로 바뀌고
    ///  - 같은 날 2단계부터는 이웃집·소품·주민이 옆으로 불어난다.
    /// 원시 → 고대 → 조선 → 근대 → 미래. 마을이 자라는 모습이 유일하고 확실한 외적 보상이다.
    /// </summary>
    public class Village : MonoBehaviour
    {
        public const int OrderGroundCover = 3;
        public const int OrderHouse = 8;
        public const int OrderProp = 10;
        public const int OrderVillager = 20;
        public const int OrderFx = 49;

        /// <summary>
        /// 안쪽부터 좌우로 번갈아 채워 나가는 자리표를 만든다.
        /// 건물이 30% 작아졌으므로 간격도 그만큼 좁혀 마을이 빽빽하게 채워진다.
        /// </summary>
        static int[] BuildSlots(int startLeft, int startRight, int step, int count)
        {
            var slots = new int[count];
            for (int i = 0; i < count; i++)
            {
                int pair = i / 2;
                slots[i] = (i % 2 == 0)
                    ? -(startLeft + pair * step)
                    : startRight + pair * step;
            }
            return slots;
        }

        /// <summary>
        /// 이웃집이 들어설 자리 (중심에서의 칸 오프셋). 안쪽부터 채워진다.
        /// 고정 시설(투입소 -8 · 내 집 -3 · 입구 +2 · 강화소 +6 · 복원소 +10) 바깥에서 시작한다.
        /// </summary>
        static readonly int[] NeighborOffsets =
            BuildSlots(11, 13, GameConfig.NeighborSpacing, GameConfig.NeighborSlots);

        /// <summary>집 앞을 채울 소품 자리 — 집보다 한 칸 앞에 서서 거리처럼 겹쳐 보인다.</summary>
        static readonly int[] PropOffsets =
            BuildSlots(12, 14, GameConfig.NeighborSpacing, GameConfig.NeighborSlots);

        /// <summary>복원된 전시형 유물이 놓일 자리.</summary>
        static readonly int[] RelicOffsets =
            BuildSlots(10, 15, GameConfig.NeighborSpacing, GameConfig.NeighborSlots);

        /// <summary>
        /// 세트 보너스 랜드마크 자리. 도감을 다 채운 보람이 보여야 하니
        /// 마을 안쪽, 플레이어가 자주 지나는 구간에 놓는다.
        /// 들어설 때 주변의 작은 집·소품은 밀어낸다.
        /// </summary>
        static readonly int[] LandmarkOffsets = { -18, 20, -29, 31, -40, 42 };

        /// <summary>
        /// 장식물이 놓이는 순서. 숫자는 시대별 장식물 시트의 인덱스다.
        /// 0 나무 · 1 덤불 · 2 바위 · 3 울타리 · 4 조명 · 5 물 · 6 표지 · 7 저장물 · 8 앉을자리 · 9 기념물
        /// 나무와 조명을 자주 섞어 마을이 휑해 보이지 않게 한다.
        /// </summary>
        static readonly int[] PropCycle =
        { 0, 4, 1, 3, 0, 8, 2, 7, 0, 5, 1, 6, 4, 0, 9, 3, 8, 1, 0, 2, 7, 4, 5, 0 };

        /// <summary>이웃 건물 추첨표 — 보통집이 가장 흔하고 높은 건물이 가장 드물다.</summary>
        static readonly int[] BuildingRoll = { 0, 0, 0, 1, 1, 1, 1, 2, 2, 3 };

        GameManager _game;
        WorldGrid _grid;
        Transform _root;
        System.Random _rng;

        SpriteRenderer _home;
        SpriteRenderer _restorer;
        SpriteRenderer _restorerSign;
        int _era;
        int _centerX;
        int _neighborsBuilt;
        int _propsBuilt;
        int _villagerCount;
        int _pendingUpgrades;
        bool _upgrading;

        readonly List<SpriteRenderer> _houses = new List<SpriteRenderer>();
        /// <summary>이웃집마다 '몇 시대 뒤처진 집인지'. 마을이 한 시대로 통일되지 않게 섞어 준다.</summary>
        readonly List<int> _houseLag = new List<int>();
        /// <summary>이웃집 종류 (작은집·보통집·넓은건물·높은건물).</summary>
        readonly List<int> _houseKind = new List<int>();

        readonly List<SpriteRenderer> _props = new List<SpriteRenderer>();
        readonly List<int> _propKind = new List<int>();
        readonly Dictionary<int, SpriteRenderer> _groundCover = new Dictionary<int, SpriteRenderer>();
        readonly List<Villager> _villagers = new List<Villager>();
        readonly List<RelicUnit> _relicUnits = new List<RelicUnit>();
        readonly List<SpriteRenderer> _landmarks = new List<SpriteRenderer>();
        readonly HashSet<int> _landmarkGroups = new HashSet<int>();
        /// <summary>랜드마크가 차지한 칸 구간 (x=왼쪽, y=오른쪽). 다른 건 여기에 못 들어선다.</summary>
        readonly List<Vector2Int> _reserved = new List<Vector2Int>();
        TownWisps _wisps;
        int _relicSlotsUsed;
        int _wanderingRelics;

        /// <summary>마을에 살 수 있는 주민 수 상한.</summary>
        public const int MaxVillagers = 44;

        /// <summary>현재 시대 (0 원시 ~ 4 미래).</summary>
        public int Era => _era;
        /// <summary>HUD 호환용 별칭.</summary>
        public int HomeTier => _era;
        public string EraName => VillageSpriteCatalog.EraName(_era);

        /// <summary>이 시대에 사는 사람들 이름을 나열한다.</summary>
        public string VillagerLineup =>
            string.Join(", ", VillageSpriteCatalog.EraVillagerNames[
                VillageSpriteCatalog.ClampEra(_era)]);
        public bool IsFinalEra => _era >= VillageSpriteCatalog.EraCount - 1;
        public int HouseCount => _houses.Count + 1;
        public int Population => _villagers.Count;
        /// <summary>마을에 나와 있는 복원 유물 수.</summary>
        public int RelicCount => _relicUnits.Count;
        public int WanderingRelicCount => _wanderingRelics;
        /// <summary>세트 보너스로 들어선 랜드마크 수.</summary>
        public int LandmarkCount => _landmarks.Count;

        /// <summary>공룡 공원이 생기면 더 많은 유물이 동시에 돌아다닐 수 있다.</summary>
        int MaxWandering => GameConfig.MaxWanderingRelics
            + (_landmarkGroups.Contains((int)RelicGroup.Dino) ? GameConfig.DinoSetWanderBonus : 0);
        public int TownMinX { get; private set; }
        public int TownMaxX { get; private set; }

        /// <summary>마을 발전 단계 이름 (HUD·로그용).</summary>
        public string StageName
        {
            get
            {
                int score = _era * 2 + _houses.Count + _relicUnits.Count / 2 + _landmarks.Count * 2;
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
            TownMinX = _centerX - 11;
            TownMaxX = _centerX + 15;

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

        SpriteRenderer Spawn(string name, Sprite sprite, Vector3 position, int order,
            float scale = 1f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            go.transform.position = position;
            if (!Mathf.Approximately(scale, 1f))
                go.transform.localScale = Vector3.one * scale;
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
                sr.color = VillageSpriteCatalog.EraGrassTint(_era);
                _groundCover[x] = sr;
            }
        }

        /// <summary>지표 잔디·길을 그 시대 색조로 물들인다.</summary>
        void ApplyGrassTint()
        {
            Color tint = VillageSpriteCatalog.EraGrassTint(_era);
            foreach (var pair in _groundCover)
                if (pair.Value != null) pair.Value.color = tint;
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
        /// <summary>내 집 자리. 스폰 지점과 지하 입구를 비워 두려고 왼쪽으로 물러나 있다.</summary>
        const int HomeOffset = -3;

        /// <summary>복원소가 설 자리. 강화소 바깥.</summary>
        const int RestorerOffset = GameConfig.RestorerOffsetX;

        /// <summary>내 집이 서 있는 칸.</summary>
        int HomeX => _centerX + HomeOffset;

        void BuildHome()
        {
            _era = 0;
            _home = Spawn("Home", VillageSpriteCatalog.EraHouse(_era), StandOn(HomeX), OrderHouse,
                GameConfig.BuildingScale);
            _restorer = Spawn("Restorer", VillageSpriteCatalog.EraRestorer(_era),
                StandOn(_centerX + RestorerOffset), OrderHouse, GameConfig.BuildingScale);
            _restorerSign = Spawn("RestorerSign",
                VillageSpriteCatalog.Sign(VillageSpriteCatalog.SignKind.Restorer, _era),
                StandOn(_centerX + RestorerOffset)
                    + Vector3.up * (VillageSpriteCatalog.SignLift
                                    * GameConfig.BuildingScale * GameConfig.CellSize),
                OrderVillager + 5, GameConfig.BuildingScale);
        }

        /// <summary>
        /// 할당량 단계를 넘긴 순간.
        /// 시대는 <b>날짜</b>가 정한다 (원시 2일 → 고대 → 조선 → 근대 → 미래 2일).
        /// 그날의 목표 시대가 지금보다 앞서 있으면, 하루의 첫 단계를 넘길 때 도약한다.
        /// 2단계부터는 시대는 그대로 두고 마을이 옆으로 넓어지고 주민이 더 몰려온다.
        /// </summary>
        public void OnQuotaStageCleared(int day, int stage)
        {
            if (stage <= 1)
            {
                int target = GameConfig.EraForDay(day);
                if (target > _era)
                {
                    // 하루를 건너뛰었다면 여러 시대를 차례로 넘긴다.
                    _pendingUpgrades += target - _era;
                    if (!_upgrading) StartCoroutine(UpgradeHomeRoutine());
                }
                else
                {
                    VillageUpgradeFx.Play(_root, StandOn(HomeX), 3f * GameConfig.CellSize);
                    _game.Log($"할당량 달성! {EraName} 시대의 마을이 한 뼘 넓어졌다 ({StageName})");
                }

                AddNeighborHouses(GameConfig.HousesPerFirstStage);
                AddProps(GameConfig.HousesPerFirstStage);
                AddVillagers(2);
            }
            else
            {
                // 집은 그대로, 대신 소문을 듣고 이웃이 몰려온다.
                AddNeighborHouses(GameConfig.HousesPerStage);
                AddProps(GameConfig.HousesPerStage);
                AddVillagers(3);
                _game.Log($"{stage}단계 달성 — 소문을 듣고 이웃이 몰려왔다! " +
                    $"집 {HouseCount}채 · 주민 {Population}명 · 지금은 '{StageName}'");
            }

            UpdateRoad();
        }

        /// <summary>새 아침 — 소문을 듣고 이웃과 주민이 더 들어온다.</summary>
        public void OnNewDay(int day)
        {
            AddNeighborHouses(GameConfig.HousesPerNewDay);
            AddProps(GameConfig.HousesPerNewDay);
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
            int next = Mathf.Min(_era + 1, VillageSpriteCatalog.EraCount - 1);
            bool grew = next != _era;

            Vector3 at = StandOn(HomeX);
            // 시대가 바뀌는 순간은 마을 전체가 빛에 휩싸인다.
            VillageUpgradeFx.Play(_root, at, (grew ? 4.5f : 3f) * GameConfig.CellSize);
            if (grew)
            {
                VillageUpgradeFx.Play(_root, StandOn(_centerX + RestorerOffset), 3.4f * GameConfig.CellSize);
                for (int i = 0; i < _houses.Count; i++)
                {
                    if (_houses[i] == null) continue;
                    VillageUpgradeFx.Play(_root, _houses[i].transform.position, 2.4f * GameConfig.CellSize);
                }
            }

            // 빛이 차오르는 동안 집을 하얗게 태운다
            float t = 0f;
            while (t < 0.45f)
            {
                t += Time.deltaTime;
                Flash(Color.Lerp(Color.white, new Color(3f, 3f, 3f, 1f), t / 0.45f), grew);
                yield return null;
            }

            if (grew)
            {
                string before = VillageSpriteCatalog.EraName(_era);
                _era = next;
                ApplyEraSprites();
                UpdateRoad();
                _game.Log($"■ 시대 도약! {before} 시대 → <b>{EraName} 시대</b> — " +
                    $"{VillageSpriteCatalog.EraTagline(_era)}");
                _game.Log($"   내 집이 {HouseName(_era)}(으)로, 복원소가 " +
                    $"{VillageSpriteCatalog.EraRestorerName(_era)}(으)로 바뀌었다. (마을 단계: {StageName})");
                _game.Log($"   주민들도 옷을 갈아입었다 — {VillagerLineup}");
            }
            else
            {
                _game.Log($"할당량 달성! 마을은 이미 {EraName} 시대에 닿았다 — 대신 도시가 옆으로 넓어진다 ({StageName})");
            }

            t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                Flash(Color.Lerp(new Color(3f, 3f, 3f, 1f), Color.white, t / 0.6f), grew);
                yield return null;
            }
            Flash(Color.white, grew);
        }

        /// <summary>집(과 시대 도약 때는 마을 전체)을 하얗게 태운다.</summary>
        void Flash(Color color, bool wholeTown)
        {
            if (_home != null) _home.color = color;
            if (!wholeTown) return;
            if (_restorer != null) _restorer.color = color;
            if (_restorerSign != null) _restorerSign.color = color;
            for (int i = 0; i < _houses.Count; i++)
                if (_houses[i] != null) _houses[i].color = color;
            for (int i = 0; i < _props.Count; i++)
                if (_props[i] != null) _props[i].color = color;
            for (int i = 0; i < _villagers.Count; i++)
                if (_villagers[i] != null) _villagers[i].Flash(color);
            for (int i = 0; i < _relicUnits.Count; i++)
                if (_relicUnits[i] != null) _relicUnits[i].Flash(color);
            for (int i = 0; i < _landmarks.Count; i++)
                if (_landmarks[i] != null) _landmarks[i].color = color;
            if (_game != null && _game.View != null) _game.View.FlashVillageBuildings(color);
        }

        /// <summary>현재 시대에 맞춰 집·복원소·이웃집 스프라이트를 전부 갈아끼운다.</summary>
        void ApplyEraSprites()
        {
            if (_home != null) _home.sprite = VillageSpriteCatalog.EraHouse(_era);
            if (_restorer != null) _restorer.sprite = VillageSpriteCatalog.EraRestorer(_era);
            if (_restorerSign != null)
                _restorerSign.sprite =
                    VillageSpriteCatalog.Sign(VillageSpriteCatalog.SignKind.Restorer, _era);
            // 쓰레기 투입소·강화소도 같은 시대로
            if (_game != null && _game.View != null) _game.View.ApplyVillageEra(_era);
            for (int i = 0; i < _houses.Count; i++)
            {
                if (_houses[i] == null) continue;
                int lag = i < _houseLag.Count ? _houseLag[i] : 0;
                int kind = i < _houseKind.Count ? _houseKind[i] : 1;
                _houses[i].sprite = VillageSpriteCatalog.EraBuilding(_era - lag, kind);
            }
            for (int i = 0; i < _props.Count; i++)
            {
                if (_props[i] == null) continue;
                _props[i].sprite = VillageSpriteCatalog.EraProp(_era, _propKind[i]);
            }
            ApplyGrassTint();
            // 주민도 그 시대 옷으로 갈아입는다.
            for (int i = 0; i < _villagers.Count; i++)
                if (_villagers[i] != null) _villagers[i].SetEra(_era);
        }

        /// <summary>그 시대의 집 이름 (HUD 호환용 — 인자는 시대 번호).</summary>
        public static string HouseName(int era) => VillageSpriteCatalog.EraHouseName(era);

        // ------------------------------------------------------------------
        void AddNeighborHouses(int count)
        {
            for (int i = 0; i < count; i++) AddNeighborHouse();
        }

        void AddProps(int count)
        {
            for (int i = 0; i < count; i++) AddProp();
        }

        void AddNeighborHouse()
        {
            // 랜드마크가 차지한 자리는 건너뛴다.
            int x = 0;
            bool found = false;
            while (_neighborsBuilt < NeighborOffsets.Length)
            {
                x = _centerX + NeighborOffsets[_neighborsBuilt++];
                if (!SlotBlocked(x)) { found = true; break; }
            }
            if (!found) return;
            if (!_grid.InBounds(x, GameConfig.SurfaceRow)) return;

            // 마을이 한 시대로 딱 떨어지면 심심하니 일부는 한 시대 뒤처진 집으로 둔다.
            int lag = _rng.Next(0, 3) == 0 ? 1 : 0;
            int kind = RollBuildingKind();

            var sr = Spawn($"House({x})",
                VillageSpriteCatalog.EraBuilding(_era - lag, kind), StandOn(x), OrderHouse,
                GameConfig.BuildingScale);
            _houses.Add(sr);
            _houseLag.Add(lag);
            _houseKind.Add(kind);
            VillageUpgradeFx.Play(_root, StandOn(x), 2.2f * GameConfig.CellSize);

            TownMinX = Mathf.Min(TownMinX, x - 2);
            TownMaxX = Mathf.Max(TownMaxX, x + 2);
            PaintGroundCover(TownMinX - 3, TownMaxX + 3);
        }

        void AddProp()
        {
            int x = 0, kind = 0;
            bool found = false;
            while (_propsBuilt < PropOffsets.Length)
            {
                x = _centerX + PropOffsets[_propsBuilt];
                kind = PropCycle[_propsBuilt % PropCycle.Length];
                _propsBuilt++;
                if (!SlotBlocked(x)) { found = true; break; }
            }
            if (!found) return;
            if (!_grid.InBounds(x, GameConfig.SurfaceRow)) return;

            var sr = Spawn($"Prop#{kind}({x})",
                VillageSpriteCatalog.EraProp(_era, kind), StandOn(x), OrderProp,
                GameConfig.BuildingScale);
            _props.Add(sr);
            _propKind.Add(kind);

            TownMinX = Mathf.Min(TownMinX, x - 1);
            TownMaxX = Mathf.Max(TownMaxX, x + 1);
        }

        void UpdateRoad()
        {
            int style = _era >= 3 ? 2 : _era >= 2 ? 1 : 0;
            PaveRoad(TownMinX, TownMaxX, style);
        }

        void AddVillagers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_villagers.Count >= MaxVillagers) return;

                int x = PickStandCell();
                var go = new GameObject($"Villager#{_villagerCount}");
                go.transform.SetParent(_root, false);
                var v = go.AddComponent<Villager>();
                v.Init(this, RollVillagerKind(), _era, StandOn(x));
                _villagerCount++;
                _villagers.Add(v);
            }
        }

        /// <summary>
        /// 이웃 건물 종류 추첨. 기본 4종은 가중치대로, 시대 전용 건물은 그보다 드물게.
        /// </summary>
        int RollBuildingKind()
        {
            int kinds = VillageSpriteCatalog.BuildingKinds(_era);
            if (kinds <= BuildingRoll.Length && kinds <= 4)
                return BuildingRoll[_rng.Next(0, BuildingRoll.Length)];

            // 시대 전용 건물이 있는 시대(원시·미래)는 35% 확률로 그쪽을 뽑는다.
            if (_rng.Next(0, 100) < 35)
                return 4 + _rng.Next(0, kinds - 4);
            return BuildingRoll[_rng.Next(0, BuildingRoll.Length)];
        }

        /// <summary>주민 종류 추첨. 꼬마(인덱스 4)는 10%로 드물게 나온다.</summary>
        int RollVillagerKind()
        {
            int kinds = VillageSpriteCatalog.EraVillagerKinds(_era);
            int kid = VillageSpriteCatalog.VillagerKidIndex;
            if (_rng.Next(0, 10) == 0) return kid;

            int r = _rng.Next(0, kinds - 1);      // 꼬마 자리를 건너뛴다
            return r >= kid ? r + 1 : r;
        }

        // ------------------------------------------------------------------
        /// <summary>
        /// 복원을 마친 유물을 마을에 내놓는다.
        /// 배회형은 살아나서 돌아다니고(동시 상한을 넘으면 전시로 돌린다),
        /// 전시형은 자기 자리에 좌대째 놓인다.
        /// </summary>
        /// <returns>돌아다니기 시작했으면 true.</returns>
        public bool PlaceRestoredRelic(RelicDef def)
        {
            if (def == null) return false;

            bool wander = def.Behavior == RelicBehavior.Wander
                && _wanderingRelics < MaxWandering;

            int x = wander ? PickStandCell() : NextRelicSlot();
            Vector3 at = StandOn(x);

            var go = new GameObject($"Relic({def.Name})");
            go.transform.SetParent(_root, false);
            var unit = go.AddComponent<RelicUnit>();
            unit.Init(this, def, at, wander);

            _relicUnits.Add(unit);
            if (wander) _wanderingRelics++;
            else
            {
                TownMinX = Mathf.Min(TownMinX, x - 2);
                TownMaxX = Mathf.Max(TownMaxX, x + 2);
                PaintGroundCover(TownMinX - 3, TownMaxX + 3);
                UpdateRoad();
            }

            VillageUpgradeFx.Play(_root, at, 2.6f * GameConfig.CellSize);
            return wander;
        }

        // ------------------------------------------------------------------
        /// <summary>
        /// 유물 한 분류를 전부 복원했을 때 들어서는 랜드마크.
        /// 자리를 넓게 쓰므로 주변의 작은 집·소품·전시 유물은 밀어낸다.
        /// </summary>
        public void PlaceSetLandmark(RelicGroup group)
        {
            int g = (int)group;
            if (!_landmarkGroups.Add(g)) return;

            int slot = _landmarkGroups.Count - 1;
            int x = _centerX + LandmarkOffsets[slot % LandmarkOffsets.Length];
            // 먼저 자리를 예약해야 밀어낸 것들이 도로 들어오지 않는다.
            _reserved.Add(new Vector2Int(x - 4, x + 4));
            ClearAround(x, 4);

            var sr = Spawn($"Landmark({VillageSpriteCatalog.SetLandmarkNames[g]})",
                VillageSpriteCatalog.SetLandmark(g), StandOn(x),
                OrderHouse + 2, GameConfig.LandmarkScale);
            _landmarks.Add(sr);

            TownMinX = Mathf.Min(TownMinX, x - 4);
            TownMaxX = Mathf.Max(TownMaxX, x + 4);
            PaintGroundCover(TownMinX - 3, TownMaxX + 3);
            UpdateRoad();

            VillageUpgradeFx.Play(_root, StandOn(x), 6f * GameConfig.CellSize);

            // 분류마다 마을에 남는 흔적이 다르다.
            if (group == RelicGroup.Yokai) SpawnWisps();
            else if (group == RelicGroup.Mystery) PromoteResidentUfo();
            else if (group == RelicGroup.Dino) WakeWanderers();
        }

        /// <summary>랜드마크가 설 자리를 비운다 — 있던 집·소품·전시 유물을 치운다.</summary>
        void ClearAround(int centerX, int radius)
        {
            float minX = (centerX - radius + 0.5f) * GameConfig.CellSize;
            float maxX = (centerX + radius + 0.5f) * GameConfig.CellSize;

            RemoveWhere(_houses, minX, maxX, _houseLag, _houseKind);
            RemoveWhere(_props, minX, maxX, _propKind, null);

            for (int i = _relicUnits.Count - 1; i >= 0; i--)
            {
                var unit = _relicUnits[i];
                if (unit == null) { _relicUnits.RemoveAt(i); continue; }
                if (unit.Wandering) continue;               // 돌아다니는 애들은 알아서 비켜 간다
                float ux = unit.transform.position.x;
                if (ux < minX || ux > maxX) continue;

                // 전시 유물은 지우지 않고 다른 빈자리로 옮겨 준다.
                unit.transform.position = StandOn(NextRelicSlot());
            }
        }

        static void RemoveWhere(List<SpriteRenderer> list, float minX, float maxX,
            List<int> sideA, List<int> sideB)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var sr = list[i];
                bool gone = sr == null;
                if (!gone)
                {
                    float x = sr.transform.position.x;
                    if (x < minX || x > maxX) continue;
                    Destroy(sr.gameObject);
                }
                list.RemoveAt(i);
                if (sideA != null && i < sideA.Count) sideA.RemoveAt(i);
                if (sideB != null && i < sideB.Count) sideB.RemoveAt(i);
            }
        }

        /// <summary>
        /// 배회 상한이 늘어났을 때, 자리가 없어 전시로 밀려 있던 배회형 유물을 다시 깨운다.
        /// (공룡 공원이 서면 우리 안 공룡들이 일어나 걸어 다니기 시작한다)
        /// </summary>
        public void WakeWanderers()
        {
            int woke = 0;
            for (int i = 0; i < _relicUnits.Count; i++)
            {
                if (_wanderingRelics >= MaxWandering) break;

                var unit = _relicUnits[i];
                if (unit == null || unit.Wandering) continue;
                var def = unit.Def;
                if (def == null || def.Behavior != RelicBehavior.Wander) continue;

                Vector3 at = StandOn(PickStandCell());
                Destroy(unit.gameObject);

                var go = new GameObject($"Relic({def.Name})");
                go.transform.SetParent(_root, false);
                var fresh = go.AddComponent<RelicUnit>();
                fresh.Init(this, def, at, true);
                _relicUnits[i] = fresh;
                _wanderingRelics++;
                woke++;
            }
            if (woke > 0 && _game != null)
                _game.Log($"   전시돼 있던 유물 {woke}마리가 깨어나 마을을 돌아다니기 시작했다");
        }

        /// <summary>요괴 마을 — 밤마다 마을을 떠다니는 도깨비불.</summary>
        void SpawnWisps()
        {
            if (_wisps != null) return;
            var go = new GameObject("TownWisps");
            _wisps = go.AddComponent<TownWisps>();
            _wisps.Setup(this, _root, GameConfig.YokaiSetWispCount);
        }

        /// <summary>
        /// 미확인 구역 — 이미 마을에 전시돼 있던 UFO 를 하늘로 띄워 상주시킨다.
        /// 새로 만들면 같은 유물이 둘이 되므로 반드시 기존 개체를 바꿔 끼운다.
        /// </summary>
        void PromoteResidentUfo()
        {
            var ufo = FindRelic(RelicGroup.Mystery, "UFO");
            if (ufo == null) return;

            int index = -1;
            for (int i = 0; i < _relicUnits.Count; i++)
            {
                var u = _relicUnits[i];
                if (u != null && u.Def == ufo) { index = i; break; }
            }

            Vector3 at = StandOn(_centerX);
            if (index >= 0)
            {
                var old = _relicUnits[index];
                at = new Vector3(old.transform.position.x, SurfaceY, 0f);
                if (old.Wandering) _wanderingRelics = Mathf.Max(0, _wanderingRelics - 1);
                Destroy(old.gameObject);
            }

            var go = new GameObject("ResidentUFO");
            go.transform.SetParent(_root, false);
            var unit = go.AddComponent<RelicUnit>();
            unit.Init(this, ufo, at, true, true, GameConfig.RelicAliveScale * 1.35f);
            _wanderingRelics++;

            if (index >= 0) _relicUnits[index] = unit;
            else _relicUnits.Add(unit);
        }

        static RelicDef FindRelic(RelicGroup group, string name)
        {
            var defs = RelicCatalog.OfGroup(group);
            for (int i = 0; i < defs.Count; i++)
                if (defs[i].Name == name) return defs[i];
            return defs.Count > 0 ? defs[defs.Count - 1] : null;
        }

        /// <summary>랜드마크가 차지한 자리인가.</summary>
        bool SlotBlocked(int gridX)
        {
            for (int i = 0; i < _reserved.Count; i++)
                if (gridX >= _reserved[i].x && gridX <= _reserved[i].y) return true;
            return false;
        }

        /// <summary>전시형 유물이 놓일 다음 자리. 자리가 다 차면 안쪽부터 다시 쓴다.</summary>
        int NextRelicSlot()
        {
            for (int tries = 0; tries < RelicOffsets.Length; tries++)
            {
                int offset = RelicOffsets[_relicSlotsUsed % RelicOffsets.Length];
                _relicSlotsUsed++;
                int x = _centerX + offset;
                if (SlotBlocked(x)) continue;
                if (_grid != null && _grid.InBounds(x, GameConfig.SurfaceRow) && CanStandOn(x))
                    return x;
            }
            // 전부 막혔으면 랜드마크 바깥쪽 끝자리로 밀어낸다.
            for (int i = 0; i < RelicOffsets.Length; i++)
            {
                int x = _centerX + RelicOffsets[RelicOffsets.Length - 1 - i];
                if (!SlotBlocked(x) && _grid != null && _grid.InBounds(x, GameConfig.SurfaceRow))
                    return x;
            }
            return _centerX + RelicOffsets[0];
        }

        /// <summary>주민이 돌아다닐 수 있는 지상 x 범위 (월드 좌표).</summary>
        public void WanderRange(out float minX, out float maxX)
        {
            minX = (TownMinX + 0.5f) * GameConfig.CellSize;
            maxX = (TownMaxX + 0.5f) * GameConfig.CellSize;
        }

        /// <summary>그 칸의 지표면이 아직 멀쩡한 땅인지. 지하 입구나 플레이어가 판 구덩이면 false.</summary>
        public bool CanStandOn(int gridX)
        {
            if (_grid == null || !_grid.InBounds(gridX, GameConfig.SurfaceRow)) return false;
            return !TileInfo.IsWalkable(_grid.Get(gridX, GameConfig.SurfaceRow));
        }

        /// <summary>주민이 그 월드 x 로 발을 디딜 수 있는지.</summary>
        public bool CanWalkTo(float worldX) =>
            CanStandOn(Mathf.FloorToInt(worldX / GameConfig.CellSize));

        /// <summary>주민을 세울 만한 땅 한 칸을 고른다 (구덩이 위는 피한다).</summary>
        int PickStandCell()
        {
            for (int tries = 0; tries < 32; tries++)
            {
                int x = _centerX + _rng.Next(-10, 11);
                if (CanStandOn(x)) return x;
            }
            for (int d = 0; d < 24; d++)
            {
                if (CanStandOn(_centerX - d)) return _centerX - d;
                if (CanStandOn(_centerX + d)) return _centerX + d;
            }
            return _centerX;
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
