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
        public const int SkyRows = 4;
        public const int UndergroundRows = 120;
        public static int WorldHeight => SkyRows + UndergroundRows;
        public const int SurfaceRow = SkyRows;

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
        public const float DigStaminaPerTile = 6.5f;

        // ---------- 플레이어 ----------
        public const float BaseMaxHealth = 100f;
        public const float BaseMoveSpeed = 5.5f;
        public const float BaseMaxStamina = 100f;
        public const int BasePower = 1;
        /// <summary>가방 기본 용량 (부피 단위).</summary>
        public const int BaseBagVolume = 12;

        // ---------- 기력 (지상에서만 회복) ----------
        public const float StaminaTickInterval = 0.1f;
        public const float StaminaPerTick = 5f;

        // ---------- 깊이 압박 ----------
        public const float DepthDamageStartDepth = Layer3Depth;
        public const float DepthDamagePerSec = 1.4f;

        // ---------- 유해 폐기물 ----------
        public const float HazardPickupDamage = 12f;   // 방호복 없이 주울 때 체력 피해

        // ==================================================================
        // 일일 사이클
        // ==================================================================
        /// <summary>하루(낮)의 길이 (초).</summary>
        public const float DayLengthSeconds = 6f * 60f;
        // 초반은 안정적으로, 후반은 장비·시설 성장을 요구하도록 수동 조정한 할당량.
        static readonly int[] DailyQuotas = { 250, 400, 600, 900, 1350, 2000, 3000 };
        public static int QuotaForDay(int day) => DailyQuotas[Mathf.Clamp(day - 1, 0, DailyQuotas.Length - 1)];
        /// <summary>이 일수를 버티면 승리.</summary>
        public const int DaysToWin = 7;
        /// <summary>마감 경고 시점 (남은 초).</summary>
        public const float DayEndWarnSeconds = 60f;

        // ---------- 경제 ----------
        public const int StartingMoney = 200;

        // ---------- 능력치 업그레이드 ----------
        public const int UpgradeBaseCost = 300;
        public const float UpgradeCostGrowth = 1.6f;
        public const int MaxUpgradeLevel = 8;

        // ---------- 지상 건물 ----------
        public const int BuildingWidth = 2;
        public const int BuildingHeight = 2;
        public const int BuildingOffsetX = 4;       // 처리장(판매소) — 시작점 오른쪽
        public const int RecycleOffsetX = -6;       // 분리수거장 — 시작점 왼쪽
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
        public const int RecycleStationCost = 1200;
        public const float RecycleBonus1 = 1.3f;    // 설치 시 판매 보너스
        public const int RecycleUpgradeCost = 2000;
        public const float RecycleBonus2 = 1.5f;

        // ---------- 일꾼 ----------
        public const int MinerHireCost = 600;
        public const int HaulerHireCost = 750;
        public const int MinerWage = 75;            // 일당
        public const int HaulerWage = 90;
        public const float MinerExtractInterval = 4.0f;
        public const float HaulerSpeed = 5.5f;
        public const int HaulerCarryCapacity = 5;
        public const int MineStockpileCap = 40;

        // ---------- 생성 ----------
        // 깊은 층일수록 쓰레기 밀도와 고가품 비중이 함께 높아진다.
        public const float TreasureChanceSurface = 0.050f;
        public const float TreasureChanceShallow = 0.065f;
        public const float TreasureChanceDeep = 0.085f;
        public const float TreasureChanceAbyss = 0.110f;
        public const float SingleSlotLootWeightMultiplier = 2.75f;
        /// <summary>무작위 생성 결과와 별개로 각 지층에 보장하는 1칸 쓰레기 수.</summary>
        public const int GuaranteedSingleSlotTrashPerLayer = 30;
        public const int StarterTrashCount = 9;
        public const int MinesPerLayer = 12;
        public const int MinePlacementAttempts = 2400;
        public const int MineMinSpacing = 13;
    }
}
