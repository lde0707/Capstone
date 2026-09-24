using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DigGame
{
    public enum GameState
    {
        Day = 0,     // 낮 — 수집
        Night = 1,   // 밤 — 상점/정비 (시간 정지)
        Won = 2,
        Lost = 3
    }

    /// <summary>게임 전체의 허브. 일일 사이클/경제/광산/시설/펫을 관리한다.</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public WorldGrid Grid { get; private set; }
        public WorldView View { get; private set; }
        public PlayerController Player { get; private set; }
        public GameHUD Hud { get; set; }
        /// <summary>지상에 자라나는 마을 — 하루하루의 보상.</summary>
        public Village Village { get; private set; }
        public RelicSystem Relics { get; private set; }

        public Vector2Int SurfaceDropCell { get; private set; }
        public Vector2Int RecycleDropCell { get; private set; }
        public RectInt BuildingRect { get; private set; }       // 처리장 (능력치 강화 포함)
        public RectInt RecycleRect { get; private set; }        // 분리수거장 부지
        public RectInt RestorerRect { get; private set; }       // 유물 복원소 부지

        public readonly List<MineSite> DiscoveredMines = new List<MineSite>();
        public readonly List<Worker> Workers = new List<Worker>();

        // ---------- 경제 / 일일 사이클 ----------
        public int Money { get; private set; }
        public int Day { get; private set; } = 1;
        public int Quota { get; private set; }               // 지금 도전 중인 누적 목표
        public int BaseQuota { get; private set; }           // 오늘 꼭 채워야 하는 계약 할당량 (1단계)
        public int PreviousQuota { get; private set; }       // 직전 단계 목표 (진행바 기준선)
        public int QuotaStage { get; private set; } = 1;     // 지금 도전 중인 단계
        public int QuotaCleared { get; private set; }        // 오늘 넘긴 단계 수
        public int DeliveredToday { get; private set; }
        public float TimeLeft { get; private set; }             // 오늘 남은 시간
        public GameState State { get; private set; } = GameState.Day;
        public string ResultReason { get; private set; } = "";

        /// <summary>어제 정산 요약 (밤 화면 표시용).</summary>
        public string SettlementReport { get; private set; } = "";

        // ---------- 시설 ----------
        public bool HasRecycleStation { get; private set; }
        public int RecycleLevel { get; private set; }           // 0 없음 / 1 / 2

        /// <summary>유물 세트를 완성할 때마다 붙는 판매 보너스.</summary>
        public float RelicSellBonus => Relics != null ? Relics.SetSellBonus : 0f;

        public float SellMultiplier =>
            (RecycleLevel >= 2 ? GameConfig.RecycleBonus2 :
             RecycleLevel >= 1 ? GameConfig.RecycleBonus1 : 1f) + RelicSellBonus;

        public bool ShopOpen { get; private set; }              // 낮의 간이 강화 상점
        public bool IsRunning => State == GameState.Day;
        /// <summary>오늘의 단계를 전부 넘겼는가.</summary>
        public bool AllQuotaStagesCleared => QuotaCleared >= GameConfig.MaxQuotaStages;

        /// <summary>지금 단계의 진행률 (직전 목표 → 현재 목표).</summary>
        public float QuotaProgress01
        {
            get
            {
                if (AllQuotaStagesCleared) return 1f;
                int span = Quota - PreviousQuota;
                if (span <= 0) return 0f;
                return Mathf.Clamp01((DeliveredToday - PreviousQuota) / (float)span);
            }
        }

        /// <summary>지금 단계를 채우기까지 남은 금액.</summary>
        public int QuotaRemaining => Mathf.Max(0, Quota - DeliveredToday);

        /// <summary>정산 보너스 — 오늘 넘긴 마지막 단계 목표를 기준으로 커진다.</summary>
        public int QuotaCompletionBonus => Mathf.RoundToInt(
            (QuotaCleared > 0 ? PreviousQuota : BaseQuota) * GameConfig.QuotaCompletionBonusRate);

        /// <summary>계약 할당량(1단계) 기준 부족액. 2단계부터는 못 채워도 해고되지 않는다.</summary>
        public int QuotaShortfall => Mathf.Max(0, BaseQuota - DeliveredToday);
        public int QuotaShortfallPenalty => Mathf.CeilToInt(
            QuotaShortfall * GameConfig.QuotaShortfallPenaltyMultiplier);
        public int Seed { get; private set; }

        PetCompanion _petObj;
        readonly List<string> _log = new List<string>();
        readonly List<float> _logTime = new List<float>();
        int _workerSeed;

        // ==================================================================
        public void StartGame(int seed, bool configureCamera = true)
        {
            Instance = this;
            Seed = seed;
            _workerSeed = seed * 31 + 7;

            State = GameState.Day;
            ShopOpen = false;
            Money = GameConfig.StartingMoney;
            Day = 1;
            BeginDayQuota();
            TimeLeft = GameConfig.DayLengthSeconds;
            HasRecycleStation = false;
            RecycleLevel = 0;
            _log.Clear();
            _logTime.Clear();

            Grid = WorldGenerator.Generate(seed);

            int cx = Grid.Width / 2;
            // 입구가 최대로 넓어져도(중심 +2 에서 좌우 2칸) 파이지 않는 칸에서 시작한다.
            SurfaceDropCell = new Vector2Int(cx - 1, GameConfig.SurfaceRow - 1);

            BuildingRect = new RectInt(
                cx + GameConfig.BuildingOffsetX,
                GameConfig.SurfaceRow - GameConfig.BuildingHeight,
                GameConfig.BuildingWidth, GameConfig.BuildingHeight);

            RecycleRect = new RectInt(
                cx + GameConfig.RecycleOffsetX,
                GameConfig.SurfaceRow - GameConfig.BuildingHeight,
                GameConfig.BuildingWidth, GameConfig.BuildingHeight);
            RecycleDropCell = new Vector2Int(
                RecycleRect.xMin + RecycleRect.width / 2, GameConfig.SurfaceRow - 1);

            RestorerRect = new RectInt(
                cx + GameConfig.RestorerOffsetX,
                GameConfig.SurfaceRow - GameConfig.BuildingHeight,
                GameConfig.BuildingWidth, GameConfig.BuildingHeight);

            var viewGo = new GameObject("WorldView");
            viewGo.transform.SetParent(transform, false);
            View = viewGo.AddComponent<WorldView>();
            View.Build(Grid);
            View.ShowBuilding(BuildingRect);
            // 공용 투입구는 처음부터 존재하고, 구매 업그레이드는 판매 배율만 강화한다.
            View.ShowRecycleStation(RecycleRect, RecycleLevel);

            Relics = gameObject.AddComponent<RelicSystem>();
            Relics.Setup(this);

            Village = gameObject.AddComponent<Village>();
            Village.Build(this);

            var playerGo = new GameObject("Player");
            playerGo.transform.SetParent(transform, false);
            Player = playerGo.AddComponent<PlayerController>();
            Player.Init(this, SurfaceDropCell);

            if (configureCamera)
                SetupCamera(playerGo.transform);
            View.UpdateFog(Player.transform.position, VisionRadius, VisionFadeRadius);

            Log($"정착 지원금 {GameConfig.StartingMoney:N0}원 지급. 지상 입구 아래 쓰레기부터 수거하자!");
            Log("쓰레기는 지상 왼쪽 분리수거장 앞에서 E를 눌러 버린다");
            Log($"잔디 지반은 팔 수 없다 · 스폰 오른쪽 {GameConfig.SurfaceEntranceOffsetX}칸의 통로로 지하에 들어가자");
            Log($"할당량 달성 보너스 +{QuotaCompletionBonus:N0}원 · 미달 시 부족액의 x{GameConfig.QuotaShortfallPenaltyMultiplier:0.00} 긴급 정산");
            Log($"1일차 — 해가 지기 전까지 할당량 {Quota:N0}원을 채우자!");
            Log($"채울 때마다 더 큰 다음 목표가 열린다 (하루 최대 {GameConfig.MaxQuotaStages}단계) — 단계마다 마을이 커진다");
            Log("할당량을 채우면 지상의 내 집이 빛나면서 커진다 — 7일이면 도시가 된다");
            Log($"땅속 보랏빛 광점은 <b>유물</b>이다 — 캐서 오른쪽 복원소 앞에서 E " +
                $"(유물 {RelicCatalog.Count}종 · C 키로 도감)");
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // ---------- 시야 (장비/펫 보너스 포함) ----------
        public int VisionRadius => GameConfig.VisionRadius + (Player != null ? Player.Gear.VisionBonus : 0);
        public int VisionFadeRadius => GameConfig.VisionFadeRadius + (Player != null ? Player.Gear.VisionBonus : 0);

        void SetupCamera(Transform target)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGo.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = GameConfig.CameraSize;
            cam.backgroundColor = new Color32(14, 12, 18, 255);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = target.position + new Vector3(0, 0, -10f);

            var old = cam.GetComponent<CameraFollow>();
            if (old != null) Destroy(old);

            var oldSky = cam.GetComponent<SkyCycle>();
            if (oldSky != null) Destroy(oldSky);

            var follow = cam.gameObject.AddComponent<CameraFollow>();
            follow.Setup(target, Grid);

            var oldBackdrop = cam.GetComponent<ParallaxBackdrop>();
            if (oldBackdrop != null) Destroy(oldBackdrop);

            var sky = cam.gameObject.AddComponent<SkyCycle>();
            sky.Setup(cam, this);

            // 하늘과 땅 사이의 원경 — 시대마다 통째로 갈린다.
            var backdrop = cam.gameObject.AddComponent<ParallaxBackdrop>();
            backdrop.Setup(cam, this);
        }

        // ==================================================================
        /// <summary>StartGame 이 정상적으로 끝났는가.</summary>
        public bool Initialized => Grid != null && Player != null && View != null;

        void Update()
        {
            // 초기화되지 않은 인스턴스는 아무 것도 하지 않는다.
            // (StartGame 이 실패했거나 씬에 수동 배치된 경우 EndDay 가 폭주하는 것을 막는다)
            if (!Initialized) return;

            var kb = Keyboard.current;

            switch (State)
            {
                case GameState.Won:
                case GameState.Lost:
                    if (kb != null && kb.rKey.wasPressedThisFrame) Bootstrap.Restart();
                    return;

                case GameState.Night:
                    // 밤에는 시간이 멈춘다. HUD 의 '다음 날' 버튼이 StartNextDay 를 부른다.
                    if (kb != null)
                    {
                        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
                            StartNextDay();
                        else if (kb.rKey.wasPressedThisFrame && kb.leftShiftKey.isPressed)
                            Bootstrap.Restart();
                    }
                    return;

                case GameState.Day:
                    TickDay(kb);
                    return;
            }
        }

        void TickDay(Keyboard kb)
        {
            TimeLeft -= Time.deltaTime;
            if (TimeLeft <= 0f)
            {
                TimeLeft = 0f;
                EndDay();
                return;
            }

            if (kb != null)
            {
                if (kb.rKey.wasPressedThisFrame && kb.leftShiftKey.isPressed)
                {
                    Bootstrap.Restart();
                    return;
                }

                if (kb.eKey.wasPressedThisFrame) HandleInteract();
                if (kb.escapeKey.wasPressedThisFrame && ShopOpen) ShopOpen = false;

                if (!ShopOpen)
                {
                    if (kb.hKey.wasPressedThisFrame) TryHire(WorkerKind.Miner);
                    if (kb.jKey.wasPressedThisFrame) TryHire(WorkerKind.Hauler);
                }
            }

            UpdateHighlight();
            RefreshMineVisuals();
            TrimLog();
        }

        // ==================================================================
        // 하루 정산
        // ==================================================================
        bool _endingDay;

        void EndDay()
        {
            if (_endingDay) return;         // 재진입 방지
            _endingDay = true;

            // 어떤 예외가 나더라도 반드시 상태가 넘어가게 한다.
            // (안 그러면 TimeLeft 가 0 인 채로 매 프레임 EndDay 가 다시 불린다)
            try
            {
                DoEndDay();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                SettlementReport = "정산 중 오류가 발생했다";
                State = GameState.Night;
            }
            finally
            {
                _endingDay = false;
            }
        }

        void DoEndDay()
        {
            ShopOpen = false;
            var lines = new List<string>();
            var gear = Player != null ? Player.Gear : null;

            // 0) 운반 일꾼이 싣고 있던 화물은 광산 야적장으로 반환 (다음 날 이월 방지)
            foreach (var w in Workers)
                if (w != null) w.DumpCargoToMine();

            // 1) 마감 시 지하에 있었으면 가방 몰수
            if (Player != null && !Player.AtSurface && Player.Inventory.Count > 0)
            {
                int lost = Player.InventoryValue;
                Player.TakeAll();
                lines.Add($"마감 때 지하에 있어서 가방을 몰수당했다 (-{lost:N0}원어치)");
            }

            // 2) 할당량 판정 — 달성하면 보너스, 조금 모자라면 저축으로 긴급 정산한다.
            if (BaseQuota > 0 && DeliveredToday < BaseQuota)
            {
                int shortfall = BaseQuota - DeliveredToday;
                int penalty = Mathf.CeilToInt(shortfall * GameConfig.QuotaShortfallPenaltyMultiplier);
                if (Money < penalty)
                {
                    GameOver($"{Day}일차 할당량 미달 · 긴급 정산금 부족 " +
                        $"({DeliveredToday:N0} / {BaseQuota:N0}원, 필요 {penalty:N0}원)");
                    return;
                }

                Money -= penalty;
                lines.Add($"할당량 {shortfall:N0}원 부족 — 긴급 정산 -{penalty:N0}원");
            }
            else
            {
                int bonus = QuotaCompletionBonus;
                Money += bonus;
                lines.Add($"할당량 {QuotaCleared}단계 달성! {DeliveredToday:N0}원 반입 · 계약 보너스 +{bonus:N0}원");
            }

            // 3) 일꾼 일당 — 못 주면 일꾼이 떠난다
            int autoIncome = 0;
            int autoItems = 0;
            for (int i = 0; i < DiscoveredMines.Count; i++)
            {
                autoIncome += DiscoveredMines[i].AutoIncomeToday;
                autoItems += DiscoveredMines[i].AutoItemsToday;
            }
            if (autoIncome > 0)
                lines.Add($"도시광산 자동 재활용 {autoItems}개 · 오늘 수익 +{autoIncome:N0}원");

            int wages = 0;
            foreach (var w in Workers)
                wages += w.Kind == WorkerKind.Miner ? GameConfig.MinerWage : GameConfig.HaulerWage;

            if (wages > 0)
            {
                if (Money >= wages)
                {
                    Money -= wages;
                    lines.Add($"일꾼 일당 지급 -{wages:N0}원 ({Workers.Count}명)");
                }
                else
                {
                    int quit = FireUnpaidWorkers(ref wages);
                    Money = Mathf.Max(0, Money - wages);
                    lines.Add($"일당이 모자라 일꾼 {quit}명이 떠났다");
                }
            }

            // 4) 펫 먹이
            if (gear != null && gear.Pet != PetKind.None)
            {
                if (Money >= GameConfig.PetFeedCost)
                {
                    Money -= GameConfig.PetFeedCost;
                    lines.Add($"{PlayerGear.PetName(gear.Pet)} 먹이값 -{GameConfig.PetFeedCost}원");
                }
                else
                {
                    lines.Add($"먹이값이 없어 {PlayerGear.PetName(gear.Pet)}가 떠났다...");
                    SetPet(PetKind.None);
                }
            }

            // 플레이어를 지상으로
            if (Player != null) Player.ReturnToSurface(SurfaceDropCell);
            SettlementReport = string.Join("\n", lines);

            // 5) 승리 판정
            if (Day >= GameConfig.DaysToWin)
            {
                State = GameState.Won;
                ResultReason = $"{GameConfig.DaysToWin}일을 버텼다! 매립지의 전설이 되었다";
                return;
            }

            State = GameState.Night;
        }

        int FireUnpaidWorkers(ref int wages)
        {
            int quit = 0;
            while (wages > Money && Workers.Count > 0)
            {
                var w = Workers[Workers.Count - 1];
                Workers.RemoveAt(Workers.Count - 1);
                w.DumpCargoToMine();   // 화물은 광산에 두고 떠난다
                wages -= w.Kind == WorkerKind.Miner ? GameConfig.MinerWage : GameConfig.HaulerWage;
                if (w.Kind == WorkerKind.Miner) w.Mine.Miners--;
                else w.Mine.Haulers--;
                Destroy(w.gameObject);
                quit++;
            }
            return quit;
        }

        /// <summary>밤 화면의 '다음 날 시작' 버튼.</summary>
        public void StartNextDay()
        {
            if (State != GameState.Night) return;

            if (Relics != null) Relics.FinishAllOvernight();

            Day++;
            BeginDayQuota();
            TimeLeft = GameConfig.DayLengthSeconds;
            if (Player != null) Player.Gear.OnNewDay();
            for (int i = 0; i < DiscoveredMines.Count; i++)
                DiscoveredMines[i].ResetDailyIncome();
            State = GameState.Day;

            Log($"{Day}일차 — 오늘 할당량 {Quota:N0}원");
            if (Village != null) Village.OnNewDay(Day);
        }

        // ==================================================================
        void UpdateHighlight()
        {
            if (Player == null || View == null) return;

            if (!Player.HoverValid || ShopOpen)
            {
                View.SetHighlight(false, default, false);
                return;
            }

            var c = Player.HoverCell;
            var kind = Grid.Get(c.x, c.y);
            bool canDig = TileInfo.IsDiggable(kind) && TileInfo.RequiredPower(kind) <= Player.Stats.Power;
            // 이미 판 통로나 힘이 부족한 칸에는 빨간 사각형을 남기지 않는다.
            View.SetHighlight(canDig, c, true);
        }

        public void NotifyPlayerMoved()
        {
            if (View != null && Player != null)
                View.UpdateFog(Player.transform.position, VisionRadius, VisionFadeRadius);
        }

        // ==================================================================
        public bool PlayerNearBuilding() => PlayerNearRect(BuildingRect);
        public bool PlayerNearRecycle() => PlayerNearRect(RecycleRect);
        public bool PlayerNearRestorer() => PlayerNearRect(RestorerRect);

        bool PlayerNearRect(RectInt rect)
        {
            if (Player == null) return false;
            var c = Player.Cell;

            int dx = 0;
            if (c.x < rect.xMin) dx = rect.xMin - c.x;
            else if (c.x >= rect.xMax) dx = c.x - (rect.xMax - 1);

            int dy = 0;
            if (c.y < rect.yMin) dy = rect.yMin - c.y;
            else if (c.y >= rect.yMax) dy = c.y - (rect.yMax - 1);

            return Mathf.Max(dx, dy) <= GameConfig.BuildingInteractRange;
        }

        void HandleInteract()
        {
            // 복원소와 강화소는 사정거리가 겹칠 수 있다.
            // 맡길 유물이 있을 때만 복원소가 E를 가져가고, 없으면 강화소로 넘긴다.
            if (PlayerNearRestorer() && Relics != null && Relics.CarriedCount > 0)
            {
                Relics.DepositAll();
                return;
            }
            if (PlayerNearRestorer() && !PlayerNearBuilding())
            {
                Log(Relics != null && Relics.QueueCount > 0
                    ? $"복원소가 이미 유물 {Relics.QueueCount}개를 되살리는 중이다"
                    : "복원소에 맡길 유물이 없다 — 땅속 보랏빛 광점을 찾아보자");
                return;
            }

            if (PlayerNearRecycle())
            {
                if (Player.Inventory.Count == 0)
                {
                    Log("분리수거장에 버릴 쓰레기가 없다");
                    return;
                }

                RecycleAtFactory(Player.TakeAll());
                return;
            }

            var mine = Player.MineHere();
            if (mine != null)
            {
                InteractMine(mine);
                return;
            }

            ToggleShop();
        }

        void InteractMine(MineSite mine)
        {
            // 가방에 물건이 있으면 먼저 안전 보관한다. 지하에서 마감되어도 이 보관품은 몰수되지 않는다.
            if (Player.Inventory.Count > 0)
            {
                int stored = mine.StoreFrom(Player.Inventory);
                if (stored > 0)
                    Log($"도시광산 #{mine.Id} 안전 보관함에 {stored}개 맡김 · 일꾼이 자동 재활용 가능");
                else
                    Log($"도시광산 #{mine.Id} 보관함이 가득 찼다 ({mine.Stockpile.Count}/{GameConfig.MineStockpileCap})");
                return;
            }

            int taken = 0;
            while (mine.TryTakeFitting(Player.BagCapacity - Player.InventoryVolume, out var item))
            {
                if (!Player.TryReceiveStoredItem(item))
                {
                    mine.Stockpile.Insert(0, item);
                    break;
                }
                taken++;
            }

            if (taken > 0)
                Log($"도시광산 #{mine.Id} 보관품 {taken}개 회수 · 가방 {Player.InventoryVolume}/{Player.BagCapacity}");
            else if (mine.Stockpile.Count > 0)
                Log("가방에 들어갈 공간이 없다");
            else
                Log("도시광산 보관함이 비어 있다 · H 수거 / J 운반 일꾼 고용");
        }

        void ToggleShop()
        {
            if (ShopOpen) { ShopOpen = false; return; }
            if (!PlayerNearBuilding())
            {
                Log("처리장 건물 앞으로 가서 E 를 눌러야 한다");
                return;
            }
            ShopOpen = true;
        }

        public void CloseShop() => ShopOpen = false;

        // ==================================================================
        public void DiscoverMine(MineSite site)
        {
            if (site.Discovered) return;
            site.Discovered = true;

            foreach (var c in site.Cells)
            {
                if (TileInfo.IsDiggable(Grid.Get(c.x, c.y)))
                    Grid.Set(c.x, c.y, TileKind.Empty);
            }

            DiscoveredMines.Add(site);

            // 고용 전에도 시설의 용도를 바로 체험할 수 있게 발견 보상을 보관함에 넣는다.
            var discoveryRng = new System.Random(Seed * 397 ^ site.Id * 7919);
            int cached = 0;
            for (int i = 0; i < GameConfig.MineDiscoveryCacheCount; i++)
                if (site.ExtractOne(discoveryRng)) cached++;

            View.ShowMine(site);
            Log($"도시광산 발견! {site.Label} · 샘플 고철 {cached}개 · E로 회수/안전 보관");
        }

        void RefreshMineVisuals()
        {
            for (int i = 0; i < DiscoveredMines.Count; i++)
                View.RefreshMine(DiscoveredMines[i]);
        }

        /// <summary>
        /// 재질별 결과물로 가공한 뒤 판매한다. 원물 판매보다 높은 재질 배율과
        /// 분리수거장 설비 배율이 함께 적용되며, 서로 다른 결과물 에셋을 보여준다.
        /// </summary>
        public int RecycleAtFactory(List<LootItem> items, string source = "지상 분리수거장")
        {
            if (items == null || items.Count == 0) return 0;

            int rawValue = 0;
            int paid = 0;
            var materials = new HashSet<MaterialKind>();
            var productCounts = new Dictionary<MaterialKind, int>();

            for (int i = 0; i < items.Count; i++)
            {
                LootItem item = items[i];
                rawValue += item.Value;
                float multiplier = RecyclingInfo.ValueMultiplier(item.Material) * SellMultiplier;
                paid += Mathf.RoundToInt(item.Value * multiplier);
                materials.Add(item.Material);
                productCounts[item.Material] = productCounts.TryGetValue(item.Material, out int count)
                    ? count + 1 : 1;
            }

            Money += paid;
            DeliveredToday += paid;
            CheckQuotaStages();
            View.ShowRecyclingOutputs(materials);

            var products = new List<string>();
            foreach (var pair in productCounts)
                products.Add($"{RecyclingInfo.ProductName(pair.Key)} x{pair.Value}");

            float totalMultiplier = rawValue <= 0 ? 1f : paid / (float)rawValue;
            Log($"{source}: {string.Join(", ", products)} → +{paid:N0}원 (원물 대비 x{totalMultiplier:0.00})");
            return paid;
        }

        /// <summary>
        /// 유물에서 나온 돈. 할당량에도 반영되므로 유물 수집이 생존에 직접 보탬이 된다.
        /// </summary>
        public void AddRelicIncome(int amount, string what)
        {
            if (amount <= 0) return;
            Money += amount;
            DeliveredToday += amount;
            CheckQuotaStages();
        }

        /// <summary>외부(유물 복원소 등)에서 돈을 쓸 때.</summary>
        public bool TrySpend(int cost) => TrySpend(cost, "복원");

        /// <summary>새 하루의 할당량 단계를 1단계로 초기화한다.</summary>
        void BeginDayQuota()
        {
            BaseQuota = GameConfig.QuotaForDay(Day);
            Quota = BaseQuota;
            PreviousQuota = 0;
            QuotaStage = 1;
            QuotaCleared = 0;
            DeliveredToday = 0;
        }

        /// <summary>
        /// 할당량을 넘긴 순간 — 정산까지 기다리지 않고 바로 보상을 준다.
        /// 단계를 넘길 때마다 지상의 집이 빛나며 커지고 주민이 늘어나며,
        /// 곧바로 더 큰 다음 목표가 열린다. (한 번에 여러 단계를 넘길 수도 있다)
        /// </summary>
        void CheckQuotaStages()
        {
            if (Quota <= 0 || State != GameState.Day) return;

            while (DeliveredToday >= Quota && QuotaCleared < GameConfig.MaxQuotaStages)
            {
                QuotaCleared++;
                PreviousQuota = Quota;

                // 단계를 넘길 때마다 마을이 자란다.
                // (집이 커지는 건 하루의 첫 단계 한 번뿐 — 나머지는 이웃·주민이 늘어난다)
                if (Village != null) Village.OnQuotaStageCleared(Day, QuotaCleared);

                if (QuotaCleared >= GameConfig.MaxQuotaStages)
                {
                    QuotaStage = GameConfig.MaxQuotaStages;
                    Log($"오늘 할당량 {GameConfig.MaxQuotaStages}단계를 전부 넘겼다! 더 요구할 게 없다");
                    break;
                }

                QuotaStage = QuotaCleared + 1;
                Quota = GameConfig.QuotaForStage(Day, QuotaStage);
                Log($"할당량 {QuotaCleared}단계 달성! 다음 목표 {Quota:N0}원 " +
                    $"— {Quota - PreviousQuota:N0}원 더 모으면 마을이 또 커진다");
            }
        }

        /// <summary>현재 공장 설비를 기준으로 한 재질별 재활용 예상 판매가.</summary>
        public int EstimateRecycledValue(IEnumerable<LootItem> items)
        {
            if (items == null) return 0;

            int total = 0;
            foreach (LootItem item in items)
                total += Mathf.RoundToInt(item.Value
                    * RecyclingInfo.ValueMultiplier(item.Material)
                    * SellMultiplier);
            return total;
        }

        // ==================================================================
        // 구매 (낮 간이 상점 + 밤 상점 공용)
        // ==================================================================
        public bool TryUpgrade(StatKind kind)
        {
            var stats = Player.Stats;
            if (stats.IsMaxed(kind)) { Log($"{PlayerStats.DisplayName(kind)}은(는) 이미 최대 단계다"); return false; }

            int cost = stats.CostOf(kind);
            if (!TrySpend(cost, $"{PlayerStats.DisplayName(kind)} 강화")) return false;

            stats.Increase(kind);
            Log($"{PlayerStats.DisplayName(kind)} 강화! → {stats.ValueText(kind)} (-{cost:N0}원)");
            return true;
        }

        public bool TryBuyHeadlamp()
        {
            var g = Player.Gear;
            if (g.HeadlampLevel >= 2) { Log("헤드랜턴은 최대 단계다"); return false; }
            int cost = g.HeadlampLevel == 0 ? GameConfig.HeadlampCost1 : GameConfig.HeadlampCost2;
            if (!TrySpend(cost, "헤드랜턴")) return false;
            g.HeadlampLevel++;
            NotifyPlayerMoved();
            Log($"헤드랜턴 Lv.{g.HeadlampLevel} — 시야 +{g.HeadlampLevel}");
            return true;
        }

        public bool TryBuyDetector()
        {
            if (Player.Gear.HasDetector) { Log("이미 탐지기가 있다"); return false; }
            if (!TrySpend(GameConfig.DetectorCost, "쓰레기 탐지기")) return false;
            Player.Gear.HasDetector = true;
            Log("쓰레기 탐지기 구입 — 지하에서 Q 로 사용");
            return true;
        }

        public bool TryBuyHazmat()
        {
            if (Player.Gear.HasHazmat) { Log("이미 방호복이 있다"); return false; }
            if (!TrySpend(GameConfig.HazmatCost, "방호복")) return false;
            Player.Gear.HasHazmat = true;
            Log("방호복 구입 — 유해 폐기물을 안전하게 주울 수 있다");
            return true;
        }

        public bool TryBuyCompressor()
        {
            if (Player.Gear.HasCompressor) { Log("이미 압축기가 있다"); return false; }
            if (!TrySpend(GameConfig.CompressorCost, "휴대용 압축기")) return false;
            Player.Gear.HasCompressor = true;
            Log("휴대용 압축기 구입 — 이제 줍는 쓰레기의 부피가 30% 줄어든다");
            return true;
        }

        public bool TryBuyReturnBell()
        {
            if (Player.Gear.HasReturnBell) { Log("이미 귀환 벨이 있다"); return false; }
            if (!TrySpend(GameConfig.ReturnBellCost, "귀환 벨")) return false;
            Player.Gear.HasReturnBell = true;
            Log("귀환 벨 구입 — 지하에서 B 로 하루 1회 귀환");
            return true;
        }

        public bool TryBuyEnergyDrink()
        {
            if (Player.Gear.EnergyDrinks >= GameConfig.EnergyDrinkMax)
            { Log($"에너지 드링크는 {GameConfig.EnergyDrinkMax}개까지만 들 수 있다"); return false; }
            if (!TrySpend(GameConfig.EnergyDrinkCost, "에너지 드링크")) return false;
            Player.Gear.EnergyDrinks++;
            Log($"에너지 드링크 구입 (보유 {Player.Gear.EnergyDrinks}개) — 지하에서 F");
            return true;
        }

        public bool TryBuyPet(PetKind kind)
        {
            if (Player.Gear.Pet == kind) { Log("이미 데리고 있는 펫이다"); return false; }
            int cost = PlayerGear.PetCost(kind);
            if (!TrySpend(cost, PlayerGear.PetName(kind) + " 입양")) return false;
            SetPet(kind);
            Log($"{PlayerGear.PetName(kind)} 입양! {PlayerGear.PetDesc(kind)} (먹이값 하루 {GameConfig.PetFeedCost}원)");
            return true;
        }

        void SetPet(PetKind kind)
        {
            if (_petObj != null) { Destroy(_petObj.gameObject); _petObj = null; }
            Player.Gear.Pet = kind;
            NotifyPlayerMoved();

            if (kind == PetKind.None) return;
            var go = new GameObject("Pet_" + kind);
            go.transform.SetParent(transform, false);
            _petObj = go.AddComponent<PetCompanion>();
            _petObj.Init(this, kind);
        }

        /// <summary>땅굴 입구 확장 단계 (0 = 1칸).</summary>
        public int EntranceWidenLevel { get; private set; }

        /// <summary>현재 땅굴 입구의 폭 (칸).</summary>
        public int EntranceWidth => 1 + EntranceWidenLevel * 2;

        /// <summary>땅굴 입구를 좌우 한 칸씩 넓힌다. 잔디층을 관통해 파낸다.</summary>
        public bool TryBuyEntranceWiden()
        {
            if (EntranceWidenLevel >= GameConfig.MaxEntranceWidenLevel)
            {
                Log("땅굴 입구는 이미 최대 폭이다");
                return false;
            }

            int cost = GameConfig.EntranceWidenCost(EntranceWidenLevel);
            if (!TrySpend(cost, "땅굴 입구 확장")) return false;

            EntranceWidenLevel++;
            int center = WorldGenerator.SurfaceEntranceX(Grid);
            var unearthed = new List<LootItem>();

            for (int side = -1; side <= 1; side += 2)
            {
                int x = center + side * EntranceWidenLevel;
                for (int y = GameConfig.SurfaceRow;
                     y < GameConfig.SurfaceRow + GameConfig.GrassLayerRows;
                     y++)
                {
                    if (!Grid.InBounds(x, y)) continue;
                    if (Grid.TryTakeTreasure(x, y, out var item)) unearthed.Add(item);
                    if (TileInfo.IsWalkable(Grid.Get(x, y))) continue;
                    Grid.Set(x, y, TileKind.Empty);
                }
            }

            Log($"땅굴 입구 확장! 좌우 한 칸씩 넓어져 폭 {EntranceWidth}칸이 되었다");
            if (unearthed.Count > 0)
            {
                int paid = RecycleAtFactory(unearthed, "입구 확장 공사");
                Log($"공사 중에 나온 쓰레기 {unearthed.Count}개도 같이 처분했다 (+{paid:N0}원)");
            }
            return true;
        }

        public bool TryBuyRecycleStation()
        {
            if (RecycleLevel >= 2) { Log("분리수거장은 최대 단계다"); return false; }

            if (RecycleLevel == 0)
            {
                if (!TrySpend(GameConfig.RecycleStationCost, "분리수거 설비 가동")) return false;
                RecycleLevel = 1;
                HasRecycleStation = true;
                View.ShowRecycleStation(RecycleRect, RecycleLevel);
                Log($"분리수거 설비 가동! 판매가 x{GameConfig.RecycleBonus1:0.0}");
            }
            else
            {
                if (!TrySpend(GameConfig.RecycleUpgradeCost, "분리수거장 확장")) return false;
                RecycleLevel = 2;
                View.ShowRecycleStation(RecycleRect, RecycleLevel);
                Log($"분리수거장 확장! 판매가 x{GameConfig.RecycleBonus2:0.0}");
            }
            return true;
        }

        bool TrySpend(int cost, string what)
        {
            if (Money < cost)
            {
                Log($"돈이 부족하다 ({what} {cost:N0}원 / 보유 {Money:N0}원)");
                return false;
            }
            Money -= cost;
            return true;
        }

        public void TryHire(WorkerKind kind)
        {
            var mine = Player.MineHere();
            if (mine == null)
            {
                Log("일꾼은 발견한 지하 분리수거 구역 안에서 고용할 수 있다");
                return;
            }
            int cost = kind == WorkerKind.Miner ? GameConfig.MinerHireCost : GameConfig.HaulerHireCost;
            int wage = kind == WorkerKind.Miner ? GameConfig.MinerWage : GameConfig.HaulerWage;
            if (!TrySpend(cost, "일꾼 고용")) return;

            var go = new GameObject(kind == WorkerKind.Miner ? "Miner" : "Hauler");
            go.transform.SetParent(transform, false);
            var w = go.AddComponent<Worker>();
            w.Init(this, kind, mine, _workerSeed++);
            Workers.Add(w);

            if (kind == WorkerKind.Miner) mine.Miners++;
            else mine.Haulers++;

            Log($"{(kind == WorkerKind.Miner ? "자동 재활용" : "대량 운반")} 일꾼 고용 → " +
                $"도시광산 #{mine.Id} (-{cost:N0}원, 일당 {wage}원)");
        }

        // ==================================================================
        public void GameOver(string reason)
        {
            // 낮에만 죽을 수 있다. (밤/결과 화면 중의 호출은 무시)
            if (State != GameState.Day) return;
            State = GameState.Lost;
            ShopOpen = false;
            ResultReason = reason;
            Log($"게임 오버: {reason}");
        }

        // ==================================================================
        public void Log(string msg)
        {
            _log.Add(msg);
            _logTime.Add(Time.time);
            if (_log.Count > 40) { _log.RemoveAt(0); _logTime.RemoveAt(0); }
        }

        void TrimLog()
        {
            while (_log.Count > 0 && Time.time - _logTime[0] > 12f)
            {
                _log.RemoveAt(0);
                _logTime.RemoveAt(0);
            }
        }

        public List<string> RecentLog(int count)
        {
            var result = new List<string>();
            int start = Mathf.Max(0, _log.Count - count);
            for (int i = start; i < _log.Count; i++) result.Add(_log[i]);
            return result;
        }
    }

    /// <summary>플레이어를 항상 따라다니는 카메라.</summary>
    public class CameraFollow : MonoBehaviour
    {
        Transform _target;
        WorldGrid _grid;
        Camera _cam;

        public void Setup(Transform target, WorldGrid grid)
        {
            _target = target;
            _grid = grid;
            _cam = GetComponent<Camera>();

            if (_target != null)
                transform.position = new Vector3(_target.position.x, _target.position.y, -10f);
        }

        void LateUpdate()
        {
            if (_target == null || _cam == null) return;

            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;

            Vector3 p = _target.position;

            float worldWidth = _grid.Width * GameConfig.CellSize;
            float worldHeight = _grid.Height * GameConfig.CellSize;
            float minX = halfW, maxX = worldWidth - halfW;
            float x = maxX > minX ? Mathf.Clamp(p.x, minX, maxX) : worldWidth * 0.5f;

            float minY = -worldHeight + halfH;
            float y = Mathf.Max(p.y, minY);

            Vector3 want = new Vector3(x, y, -10f);

            if ((transform.position - want).sqrMagnitude >
                GameConfig.CameraSnapDistance * GameConfig.CameraSnapDistance)
            {
                transform.position = want;
                return;
            }

            transform.position = Vector3.Lerp(
                transform.position, want, 1f - Mathf.Exp(-GameConfig.CameraLerp * Time.deltaTime));
        }
    }
}
