using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 전역 튜닝 값. 밸런스는 전부 여기서 조절한다.
    /// </summary>
    public static class GameConfig
    {
        // ---------- 월드 ----------
        /// <summary>기존 61칸에서 좌우 탐험 공간을 약 3배로 확장.</summary>
        public const int WorldWidth = 183;
        /// <summary>지상 하늘. 마을이 도시로 자랄 공간을 위해 넉넉하게 둔다.</summary>
        public const int SkyRows = 10;
        public const int UndergroundRows = 120;
        public static int WorldHeight => SkyRows + UndergroundRows;
        public const int SurfaceRow = SkyRows;
        /// <summary>기존 지표 1칸에 아래쪽 2칸을 더한 파괴 불가 잔디 지반.</summary>
        public const int GrassLayerRows = 3;
        public const int SurfaceEntranceOffsetX = 2;
        public static int StarterTrashRow => SurfaceRow + GrassLayerRows;

        /// <summary>한 칸의 실제 월드 크기. 기존 1.0에서 1.5배로 확대.</summary>
        public const float CellSize = 1.5f;

        // ---------- 지층 ----------
        public const int Layer1Depth = 15;
        public const int Layer2Depth = 40;
        public const int Layer3Depth = 75;
        public const float LayerBoundaryJitter = 2.4f;
        public const float LayerNoiseScale = 0.16f;

        // ---------- 화면 / 크기 ----------
        public const float PlayerScale = 1f;
        public const float CameraSize = 8.5f;
        public const float CameraLerp = 22f;
        public const float CameraSnapDistance = 6f;

        // ---------- 시야 ----------
        public const int VisionRadius = 2;      // 기본 시야 (헤드랜턴·펫으로 증가)
        public const int VisionFadeRadius = 3;

        // ---------- 굴착 ----------
        public const float DigTimePerHardness = 0.34f;
        public const float DigStaminaPerTile = 3.25f; // 기존 대비 50% 감소
        /// <summary>힘 강화 1단계당 추가되는 굴착 속도. 지층 해금과는 무관하다.</summary>
        public const float PowerDigSpeedBonusPerLevel = 0.20f;

        // ---------- 플레이어 ----------
        public const float BaseMoveSpeed = 5.5f;
        public const float BaseMaxStamina = 100f;
        public const int BasePower = 1;
        /// <summary>가방 기본 용량 (부피 단위).</summary>
        public const int BaseBagVolume = 12;
        /// <summary>하단 1줄을 포함한 인벤토리 전체 규격.</summary>
        public const int InventoryColumns = 9;
        public const int InventoryRows = 4;
        public const int InventorySlotCapacity = InventoryColumns * InventoryRows;

        // ---------- 기력 (지상에서만 회복) ----------
        public const float StaminaTickInterval = 0.1f;
        public const float StaminaPerTick = 10f;    // 지상 회복 속도 2배

        // ---------- 유해 폐기물 ----------
        public const float HazardPickupStaminaDamage = 20f;

        // ==================================================================
        // 일일 사이클
        // ==================================================================
        /// <summary>하루(낮)의 길이 (초).</summary>
        public const float DayLengthSeconds = 2f * 60f;
        // 재활용 기대가(1층 114 / 2층 509 / 3층 1,667 / 4층 9,657원)를 기준으로
        // 초반은 1가방 안쪽, 후반은 깊은 층 탐사와 시설 투자를 요구하도록 조정한다.
        // 계약 할당량(1단계). 이걸 못 채우면 해고 — 폭을 이전보다 키웠다.
        // 6~7일차가 너무 빡세면 이 배열만 되돌리면 된다.
        static readonly int[] DailyQuotas = { 700, 1800, 4200, 9000, 16000, 28000, 45000 };
        public static int QuotaForDay(int day) => DailyQuotas[Mathf.Clamp(day - 1, 0, DailyQuotas.Length - 1)];

        /// <summary>하루 안에서 열리는 최대 할당량 단계.</summary>
        public const int MaxQuotaStages = 5;

        /// <summary>단계가 오를 때마다 '추가로 요구되는 양'이 커지는 배율.</summary>
        public const float QuotaStageGrowth = 1.6f;

        /// <summary>
        /// 하루 n단계째 누적 목표액. 1단계는 계약 할당량이고,
        /// 그 위로는 단계마다 추가 요구량이 QuotaStageGrowth 배씩 불어난다.
        /// (배율 1.6 기준 누적: 1x / 2.6x / 5.2x / 9.3x / 15.9x)
        /// </summary>
        public static int QuotaForStage(int day, int stage)
        {
            int baseQuota = QuotaForDay(day);
            stage = Mathf.Clamp(stage, 1, MaxQuotaStages);

            float increment = baseQuota;
            float total = baseQuota;
            for (int i = 2; i <= stage; i++)
            {
                increment *= QuotaStageGrowth;
                total += increment;
            }
            return Mathf.RoundToInt(total / 10f) * 10;   // 10원 단위로 정리
        }

        public const float QuotaCompletionBonusRate = 0.10f;
        public const float QuotaShortfallPenaltyMultiplier = 1.25f;
        /// <summary>이 일수를 버티면 승리.</summary>
        public const int DaysToWin = 7;
        /// <summary>마감 경고 시점 (남은 초).</summary>
        public const float DayEndWarnSeconds = 20f;

        // ---------- 경제 ----------
        public const int StartingMoney = 200;

        // ---------- 능력치 업그레이드 ----------
        public const int UpgradeBaseCost = 300;
        public const float UpgradeCostGrowth = 1.6f;
        public const int MaxUpgradeLevel = 8;
        public const int MaxPowerUpgradeLevel = 15;
        public const int PowerUpgradeBaseCost = 400;
        public const float PowerUpgradeCostGrowth = 1.35f;

        // ---------- 지상 건물 ----------
        public const int BuildingWidth = 2;
        public const int BuildingHeight = 2;
        /// <summary>
        /// 모든 지상 건물에 곱하는 크기. 건물이 화면을 덜 잡아먹어야
        /// 같은 폭에 더 많이 들어가고 마을이 빽빽해 보인다. (기존 대비 30% 축소)
        /// </summary>
        public const float BuildingScale = 0.70f;

        // 지상 배치 — 세 시설(투입소·강화소·복원소)을 스폰 왼쪽에 한 덩어리로 모았다.
        // 플레이어는 땅굴 하나를 드나드는 게 전부라, 시설이 흩어져 있으면 아예 보지 못한다.
        //   내 집[-12.55~-10.45]  복원소[-9.55~-7.45]  강화소[-7.05~-4.95]
        //   투입소[-4.05~-1.95]   스폰[-1]  입구[0~+5] (최대 확장)
        // 세 시설 전체 폭 7.6칸 — 스폰에서 가장 먼 시설까지 8.6칸이면 한눈에 들어온다.
        public const int BuildingOffsetX = -7;      // 강화소(처리장)
        public const int RecycleOffsetX = -4;       // 투입소(분리수거장) — 스폰 바로 옆
        public const int RestorerOffsetX = -9;      // 복원소 — 세 시설 중 가장 바깥
        public const int BuildingInteractRange = 2;

        // ---------- 마을 밀도 ----------
        /// <summary>이웃집이 들어서는 간격 (칸). 건물 폭 2.8칸이라 3칸이면 거의 맞붙는다.</summary>
        public const int NeighborSpacing = 3;
        /// <summary>이웃집 자리 수 — 맵 폭 전체를 덮는다.</summary>
        public const int NeighborSlots = 52;
        /// <summary>할당량 1단계를 넘겼을 때 들어서는 집 수.</summary>
        public const int HousesPerFirstStage = 3;
        /// <summary>2단계부터 한 단계당 들어서는 집 수.</summary>
        public const int HousesPerStage = 5;
        /// <summary>새 아침에 이사 오는 집 수.</summary>
        public const int HousesPerNewDay = 3;

        // ---------- 장비 ----------
        public const int HeadlampCost1 = 250;       // 시야 +1
        public const int HeadlampCost2 = 650;       // 시야 +2
        public const int DetectorCost = 400;
        public const float DetectorRadius = 8f;
        public const float DetectorDuration = 3f;
        public const float DetectorCooldown = 15f;
        public const int HazmatCost = 900;
        public const int CompressorCost = 700;
        public const float CompressorFactor = 0.7f; // 부피 30% 감소
        public const int ReturnBellCost = 500;      // 1일 1회 지상 귀환
        public const int EnergyDrinkCost = 50;
        public const int EnergyDrinkMax = 2;
        public const float EnergyDrinkStamina = 30f;

        // ---------- 펫 (1마리만 동행) ----------
        public const int PetMoleCost = 700;         // 두더지: 비싼 쓰레기 방향 표시
        public const int PetFireflyCost = 500;      // 반딧불이: 시야 +1
        public const int PetTurtleCost = 950;       // 거북이: 가방 +6
        public const int PetFeedCost = 40;          // 하루 유지비
        public const float MoleScanRadius = 12f;
        public const int TurtleBagBonus = 6;

        // ---------- 오염 / 분리수거 ----------
        /// <summary>오염도 한 칸당 추가되는 굴착 시간. 0.2 = 20% 느려짐.</summary>
        public const float PollutionDigSlowdownPerLevel = 0.20f;
        public const int RecyclingSuccessPollutionRecovery = 1;
        /// <summary>실패 오염량은 성공 회복량의 정확히 두 배.</summary>
        public const int RecyclingFailurePollution = RecyclingSuccessPollutionRecovery * 2;

        // ---------- 시대 진행 (날짜별) ----------
        /// <summary>1일차부터 7일차까지의 목표 시대. 원시 2일 + 미래 2일.</summary>
        static readonly int[] EraByDay = { 0, 0, 1, 2, 3, 4, 4 };

        /// <summary>그날 도달해야 할 시대 번호 (0 원시 ~ 4 미래).</summary>
        public static int EraForDay(int day) =>
            EraByDay[Mathf.Clamp(day - 1, 0, EraByDay.Length - 1)];

        // ---------- 땅굴 입구 확장 (밤 상점 · 시설) ----------
        /// <summary>확장 단계. 1단계마다 입구가 좌우로 한 칸씩 넓어진다.</summary>
        public const int MaxEntranceWidenLevel = 2;
        static readonly int[] EntranceWidenCosts = { 1200, 2800 };
        public static int EntranceWidenCost(int level) =>
            EntranceWidenCosts[Mathf.Clamp(level, 0, EntranceWidenCosts.Length - 1)];

        public const int RecycleStationCost = 1200;
        public const float RecycleBonus1 = 1.3f;    // 설치 시 판매 보너스
        public const int RecycleUpgradeCost = 2000;
        public const float RecycleBonus2 = 1.5f;

        // ---------- 일꾼 ----------
        public const int MinerHireCost = 600;
        public const int HaulerHireCost = 750;
        public const int MinerWage = 75;            // 일당
        public const int HaulerWage = 90;
        /// <summary>수거 일꾼이 폐기물 1개를 회수·재활용해 판매하는 간격.</summary>
        public const float MinerExtractInterval = 20.0f;
        public const float HaulerSpeed = 5.5f;
        public const int HaulerCarryCapacity = 5;
        public const int MineStockpileCap = 40;
        /// <summary>도시광산을 처음 발견했을 때 보관함에 들어 있는 고철 샘플.</summary>
        public const int MineDiscoveryCacheCount = 2;

        // ---------- 생성 ----------
        // 기존 밀도의 약 2배. 깊은 층일수록 쓰레기 밀도와 고가품 비중이 함께 높아진다.
        public const float TreasureChanceSurface = 0.100f;
        public const float TreasureChanceShallow = 0.130f;
        public const float TreasureChanceDeep = 0.170f;
        public const float TreasureChanceAbyss = 0.220f;
        public const float SingleSlotLootWeightMultiplier = 2.75f;
        /// <summary>무작위 생성 결과와 별개로 각 지층에 보장하는 1칸 쓰레기 수.</summary>
        public const int GuaranteedSingleSlotTrashPerLayer = 60;
        public const int StarterTrashCount = 12;
        // ---------- 특수 지층 ----------
        public const float LooseSoilChance = 0.070f;
        public const float CompactCacheChance = 0.035f;
        public const float GasPocketChanceDeep = 0.025f;
        public const float GasPocketChanceAbyss = 0.040f;
        public const float CompactCacheValueMultiplier = 1.65f;
        public const float GasPocketValueMultiplier = 1.45f;
        public const float GasPocketStaminaDamage = 15f;
        // ---------- 유물 ----------
        /// <summary>같은 유물이 땅속에 묻혀 있는 군데 수.</summary>
        public const int RelicCopiesPerKind = 3;
        public const int RelicMinSpacing = 5;
        public const int RelicPlacementAttempts = 900;
        /// <summary>집게 퀘스트의 지층별 고유 유물 요구량.</summary>
        public static readonly int[] PickaxeQuestRelicCounts = { 7, 8, 10, 7 };
        public const float PickaxeQuestInteractRange = 2.2f;
        /// <summary>한 번에 들고 다닐 수 있는 유물 수 (가방 부피와 별개).</summary>
        /// <summary>유물도 공용 인벤토리 슬롯을 사용한다.</summary>
        public const int RelicCarryLimit = InventorySlotCapacity;
        /// <summary>복원 1건에 걸리는 시간 (초). 낮 2분 기준으로 하루 여러 개가 나온다.</summary>
        public const float RelicRestoreSeconds = 12f;
        /// <summary>복원 비용 — 층이 깊을수록 비싸다 (깊이별 가치의 이 비율).</summary>
        public const float RelicRestoreCostRate = 0.35f;
        /// <summary>복원을 마쳤을 때 마을 기여로 들어오는 돈 (깊이별 가치의 이 비율).</summary>
        public const float RelicRestoreRewardRate = 0.60f;
        /// <summary>동시에 마을을 돌아다닐 수 있는 배회형 유물 수. 넘치면 전시로 바뀐다.</summary>
        public const int MaxWanderingRelics = 8;
        // ---------------- 세트 보너스 ----------------
        /// <summary>세트를 완성할 때마다 판매액에 붙는 보너스.</summary>
        public const float SetSellBonus = 0.06f;
        /// <summary>공룡 공원 — 동시에 돌아다닐 수 있는 유물 수가 이만큼 늘어난다.</summary>
        public const int DinoSetWanderBonus = 6;
        /// <summary>세계 기담관 — 복원 시간이 이 비율로 줄어든다.</summary>
        public const float CurioSetRestoreRate = 0.55f;
        /// <summary>추억의 거리 — 중복 유물 환전액 배율.</summary>
        public const float RetroSetDuplicateRate = 2f;
        /// <summary>국립 박물관 — 판매액에 추가로 붙는 보너스.</summary>
        public const float MuseumSetSellBonus = 0.10f;
        /// <summary>요괴 마을 — 밤에 떠다니는 도깨비불 수.</summary>
        public const int YokaiSetWispCount = 16;
        /// <summary>랜드마크는 일반 건물보다 조금 덜 줄인다 (눈에 띄어야 한다).</summary>
        public const float LandmarkScale = 0.78f;

        /// <summary>복원본 스프라이트(64px=4칸)에 곱하는 크기.</summary>
        public const float RelicAliveScale = 0.42f;
        /// <summary>땅속 발굴본(32px=2칸)에 곱하는 크기.</summary>
        public const float RelicBuriedScale = 0.46f;

        public const int MinesPerLayer = 12;
        public const int MinePlacementAttempts = 2400;
        public const int MineMinSpacing = 13;
    }
}
