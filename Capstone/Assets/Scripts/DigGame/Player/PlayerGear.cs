using UnityEngine;

namespace DigGame
{
    public enum PetKind { None = 0, Mole = 1, Firefly = 2, Turtle = 3 }

    /// <summary>
    /// 밤 상점에서 사는 장비·소모품·펫의 보유 상태.
    /// </summary>
    public class PlayerGear
    {
        // ---------- 영구 장비 ----------
        public int HeadlampLevel;      // 0~2, 시야 +레벨
        public bool HasDetector;       // Q: 주변 쓰레기 표시
        public bool HasHazmat;         // 유해 폐기물 무피해
        public bool HasCompressor;     // 수집 시 부피 30% 감소
        public bool HasReturnBell;     // B: 하루 1회 지상 귀환

        // ---------- 소모품 / 일일 상태 ----------
        public int EnergyDrinks;       // F: 기력 +30
        public bool BellUsedToday;
        public float DetectorCooldown;

        // ---------- 펫 ----------
        public PetKind Pet = PetKind.None;

        public int VisionBonus =>
            HeadlampLevel + (Pet == PetKind.Firefly ? 1 : 0);

        public int BagBonus =>
            Pet == PetKind.Turtle ? GameConfig.TurtleBagBonus : 0;

        /// <summary>새 하루가 시작될 때 리셋.</summary>
        public void OnNewDay()
        {
            BellUsedToday = false;
            DetectorCooldown = 0f;
        }

        public static string PetName(PetKind p)
        {
            switch (p)
            {
                case PetKind.Mole: return "두더지";
                case PetKind.Firefly: return "반딧불이";
                case PetKind.Turtle: return "거북이";
                default: return "없음";
            }
        }

        public static string PetDesc(PetKind p)
        {
            switch (p)
            {
                case PetKind.Mole: return "근처의 가장 비싼 쓰레기 방향을 가리킨다";
                case PetKind.Firefly: return "시야 +1, 지하가 덜 어둡다";
                case PetKind.Turtle: return $"등껍질 가방 +{GameConfig.TurtleBagBonus}";
                default: return "";
            }
        }

        public static int PetCost(PetKind p)
        {
            switch (p)
            {
                case PetKind.Mole: return GameConfig.PetMoleCost;
                case PetKind.Firefly: return GameConfig.PetFireflyCost;
                case PetKind.Turtle: return GameConfig.PetTurtleCost;
                default: return 0;
            }
        }
    }
}
