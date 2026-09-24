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
        public const int DigRange = 1;
        public const float DigTimePerHardness = 0.34f;
        public const float DigStaminaPerTile = 3.25f; // 기존 대비 50% 감소
        /// <summary>땅의 요구 힘을 초과한 힘 1당 추가되는 굴착 속도.</summary>
        public const float PowerDigSpeedBonusPerSurplus = 0.35f;

        // ---------- 플레이어 ----------
        public const float BaseMaxHealth = 100f;
        public const float BaseMoveSpeed = 5.5f;
        public const float BaseMaxStamina = 100f;
        public const int BasePower = 1;
        /// <summary>가방 기본 용량 (부피 단위).</summary>
        public const int BaseBagVolume = 12;

        // ---------- 기력 (지상에서만 회복) ----------
        public const float StaminaTickInterval = 0.1f;
        public const float StaminaPerTick = 10f;    // 지상 회복 속도 2배

        // ---------- 깊이 압박 ----------
        public const float DepthDamageStartDepth = Layer3Depth;
        public const float DepthDamagePerSec = 1.4f;

        // ---------- 유해 폐기물 ----------
        public const float HazardPickupDamage = 12f;   // 방호복 없이 주울 때 체력 피해

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

        // ---------- 지상 건물 ----------
        public const int BuildingWidth = 2;
        public const int BuildingHeight = 2;
        // 지상 배치 — 건물 스프라이트는 3칸 폭이므로 입구(1칸)와 겹치지 않게 벌려 둔다.
        //   투입소[-9.5~-6.5]  내 집[-4~-1]  스폰[-1]  입구[+2] (최대 확장 시 [0~+5])
        //   강화소[+5.5~+8.5]  복원소[+11~+14]
        public const int BuildingOffsetX = 6;       // 강화소(처리장) — 입구 오른쪽
        public const int RecycleOffsetX = -9;       // 투입소(분리수거장) — 내 집 왼쪽
        public const int BuildingInteractRange = 2;

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

        // ---------- 분리수거장 ----------
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
        public const float GasPocketHealthDamage = 10f;
        public const float GasPocketStaminaDamage = 15f;
        public const int MinesPerLayer = 12;
        public const int MinePlacementAttempts = 2400;
        public const int MineMinSpacing = 13;
    }
}
