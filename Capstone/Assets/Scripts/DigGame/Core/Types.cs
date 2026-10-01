using UnityEngine;

namespace DigGame
{
    public enum TileKind
    {
        Sky = 0,        // 지상 공기
        Empty = 1,      // 파낸 자리
        SoftDirt = 2,   // 1층 — 힘 1
        Clay = 3,       // 2층 — 힘 2
        Stone = 4,      // 3층 — 힘 3
        HardRock = 5,   // 4층 — 힘 4
        Bedrock = 6,    // 파괴 불가
        Grass = 7       // 지상 잔디 지반 — 파괴 불가
    }

    public enum Zone
    {
        Surface = 0,   // 1층 · 지상 근처
        Shallow = 1,   // 2층 · 얕은 지하
        Deep = 2,      // 3층 · 깊은 지하
        Abyss = 3      // 4층 · 매우 깊은 지하
    }

    /// <summary>파기 전에 색으로 식별할 수 있는 지층 변수.</summary>
    public enum DigFeature
    {
        None = 0,
        LooseSoil = 1,      // 빠르고 기력 소모가 적다
        CompactCache = 2,   // 느리지만 고가품이 보장된다
        GasPocket = 3       // 방호복이 없으면 피해를 받는 위험 지층
    }

    public static class DigFeatureInfo
    {
        public static string DisplayName(DigFeature feature)
        {
            switch (feature)
            {
                case DigFeature.LooseSoil: return "푸석한 흙";
                case DigFeature.CompactCache: return "압축 폐기물층";
                case DigFeature.GasPocket: return "매립 가스층";
                default: return "일반 지층";
            }
        }

        public static string Description(DigFeature feature)
        {
            switch (feature)
            {
                case DigFeature.LooseSoil: return "빠르게 파고 기력을 절반만 사용";
                case DigFeature.CompactCache: return "단단하지만 고가품이 들어 있음";
                case DigFeature.GasPocket: return "방호복 없이 열면 기력 감소";
                default: return "";
            }
        }

        public static float DurationMultiplier(DigFeature feature)
        {
            switch (feature)
            {
                case DigFeature.LooseSoil: return 0.55f;
                case DigFeature.CompactCache: return 1.55f;
                case DigFeature.GasPocket: return 1.10f;
                default: return 1f;
            }
        }

        /// <summary>굴착 완료까지 드는 총 기력의 배율.</summary>
        public static float StaminaMultiplier(DigFeature feature)
        {
            switch (feature)
            {
                case DigFeature.LooseSoil: return 0.50f;
                case DigFeature.CompactCache: return 1.35f;
                default: return 1f;
            }
        }

        public static Color ColorOf(DigFeature feature)
        {
            switch (feature)
            {
                case DigFeature.LooseSoil: return new Color32(100, 205, 255, 255);
                case DigFeature.CompactCache: return new Color32(255, 198, 70, 255);
                case DigFeature.GasPocket: return new Color32(155, 255, 80, 255);
                default: return Color.clear;
            }
        }
    }

    public enum WorkerKind
    {
        Miner = 0,   // 지하 수거장에서 폐기물을 분류하는 일꾼
        Hauler = 1   // 폐기물을 지상 분리수거장으로 옮기는 일꾼
    }

    /// <summary>쓰레기 재질. 분리수거·특수 효과의 기준이 된다.</summary>
    public enum MaterialKind
    {
        General = 0,     // 일반 쓰레기
        Metal = 1,       // 캔/고철
        Glass = 2,       // 유리
        Plastic = 3,     // 플라스틱 — 가볍고 부피가 크다
        Electronics = 4, // 전자제품 — 비싸고 크다
        Hazard = 5,      // 유해 폐기물 — 방호복 없이 주우면 다친다
        Relic = 6        // 유물급 — 잭팟
    }

    /// <summary>인벤토리에 들어가는 아이템 1종.</summary>
    public struct LootItem
    {
        public string Name;
        public int Value;
        public Color Color;
        public int Volume;            // 가방에서 차지하는 부피
        public MaterialKind Material;
        public RecyclingCategory Category { get; private set; }
        public bool Broken;           // 유리가 깨졌는가

        public LootItem(string name, int value, Color color, int volume, MaterialKind material, RecyclingCategory category = RecyclingCategory.Specialist)
        {
            Name = name;
            Value = value;
            Color = color;
            Volume = Mathf.Max(1, volume);
            Material = material;
            Broken = false;
            Category = category;
        }
    }

    public static class MaterialInfo
    {
        public static string DisplayName(MaterialKind m)
        {
            switch (m)
            {
                case MaterialKind.General: return "일반";
                case MaterialKind.Metal: return "고철";
                case MaterialKind.Glass: return "유리";
                case MaterialKind.Plastic: return "플라스틱";
                case MaterialKind.Electronics: return "전자제품";
                case MaterialKind.Hazard: return "유해 폐기물";
                default: return "유물";
            }
        }

        /// <summary>땅속 쓰레기 힌트와 재활용 결과물에 쓰는 재질별 색.</summary>
        public static Color HintColor(MaterialKind m)
        {
            switch (m)
            {
                case MaterialKind.General: return new Color32(210, 205, 180, 255);
                case MaterialKind.Metal: return new Color32(165, 220, 255, 255);
                case MaterialKind.Glass: return new Color32(120, 245, 235, 255);
                case MaterialKind.Plastic: return new Color32(255, 155, 220, 255);
                case MaterialKind.Electronics: return new Color32(120, 255, 145, 255);
                case MaterialKind.Hazard: return new Color32(255, 235, 80, 255);
                default: return new Color32(210, 135, 255, 255);
            }
        }
    }

    /// <summary>쓰레기 재질별 재활용 결과와 원물 대비 가격 배율.</summary>
    public static class RecyclingInfo
    {
        public static string ProductName(MaterialKind material)
        {
            switch (material)
            {
                case MaterialKind.General: return "재생 섬유 묶음";
                case MaterialKind.Metal: return "정제 금속 캔";
                case MaterialKind.Glass: return "재생 유리 구슬";
                case MaterialKind.Plastic: return "플라스틱 펠릿";
                case MaterialKind.Electronics: return "회수 전자부품";
                case MaterialKind.Hazard: return "안전 처리 촉매";
                default: return "복원된 골동품";
            }
        }

        public static float ValueMultiplier(MaterialKind material)
        {
            switch (material)
            {
                case MaterialKind.General: return 1.25f;
                case MaterialKind.Metal: return 1.55f;
                case MaterialKind.Glass: return 1.45f;
                case MaterialKind.Plastic: return 1.40f;
                case MaterialKind.Electronics: return 1.75f;
                case MaterialKind.Hazard: return 1.90f;
                default: return 2.10f;
            }
        }
    }

    public static class TileInfo
    {
        /// <summary>파는 데 필요한 최소 집게 단계. 0이면 통과 가능, -1이면 파괴 불가.</summary>
        public static int RequiredPower(TileKind kind)
        {
            switch (kind)
            {
                case TileKind.Sky:
                case TileKind.Empty: return 0;
                case TileKind.SoftDirt: return 1;
                case TileKind.Clay: return 2;
                case TileKind.Stone: return 3;
                case TileKind.HardRock: return 4;
                default: return -1; // Bedrock
            }
        }

        public static int Hardness(TileKind kind)
        {
            int p = RequiredPower(kind);
            return p <= 0 ? 0 : p;
        }

        /// <summary>
        /// 힘 강화는 지층 해금과 무관하며 모든 팔 수 있는 땅의 굴착 시간을 줄인다.
        /// </summary>
        public static float PowerDigDurationMultiplier(TileKind kind, int power)
        {
            if (!IsDiggable(kind)) return 1f;
            int level = Mathf.Max(0, power - GameConfig.BasePower);
            float speed = 1f + level * GameConfig.PowerDigSpeedBonusPerLevel;
            return 1f / speed;
        }

        public static bool IsWalkable(TileKind kind) => kind == TileKind.Sky || kind == TileKind.Empty;

        public static bool IsDiggable(TileKind kind) => RequiredPower(kind) > 0;

        public static Color ColorOf(TileKind kind)
        {
            switch (kind)
            {
                case TileKind.Sky:      return Color.clear; // 카메라의 실제 하늘 배경이 비친다
                case TileKind.Empty:    return new Color32(30, 24, 21, 255);
                case TileKind.Grass:    return new Color32(78, 122, 61, 255);
                case TileKind.SoftDirt: return new Color32(156, 114, 70, 255);  // 밝은 갈색
                case TileKind.Clay:     return new Color32(142, 78, 58, 255);   // 붉은 갈색
                case TileKind.Stone:    return new Color32(112, 114, 124, 255); // 회색
                case TileKind.HardRock: return new Color32(60, 66, 88, 255);    // 어두운 청회색
                case TileKind.Bedrock:  return new Color32(20, 20, 24, 255);
                default: return Color.magenta;
            }
        }

        public static string DisplayName(TileKind kind)
        {
            switch (kind)
            {
                case TileKind.Grass: return "잔디 지반";
                case TileKind.SoftDirt: return "무른 흙";
                case TileKind.Clay: return "굳은 흙";
                case TileKind.Stone: return "암반";
                case TileKind.HardRock: return "경암";
                case TileKind.Bedrock: return "베드락";
                default: return "빈 공간";
            }
        }

        /// <summary>층(구역)을 이루는 땅의 종류.</summary>
        public static TileKind ForZone(Zone z)
        {
            switch (z)
            {
                case Zone.Surface: return TileKind.SoftDirt;
                case Zone.Shallow: return TileKind.Clay;
                case Zone.Deep:    return TileKind.Stone;
                default:           return TileKind.HardRock;
            }
        }

        public static Zone ZoneOf(TileKind kind)
        {
            switch (kind)
            {
                case TileKind.Grass:
                case TileKind.SoftDirt: return Zone.Surface;
                case TileKind.Clay:     return Zone.Shallow;
                case TileKind.Stone:    return Zone.Deep;
                default:                return Zone.Abyss;
            }
        }

        /// <summary>파낸 통로의 가장자리 재질로 그릴 수 있는 지형.</summary>
        public static bool SupportsPassage(TileKind kind) => IsDiggable(kind) || kind == TileKind.Grass;
    }

    public static class ZoneInfo
    {
        public static Zone FromDepth(int depth)
        {
            if (depth < GameConfig.Layer1Depth) return Zone.Surface;
            if (depth < GameConfig.Layer2Depth) return Zone.Shallow;
            if (depth < GameConfig.Layer3Depth) return Zone.Deep;
            return Zone.Abyss;
        }

        public static string DisplayName(Zone z)
        {
            switch (z)
            {
                case Zone.Surface: return "1층 · 지상 근처";
                case Zone.Shallow: return "2층 · 얕은 지하";
                case Zone.Deep:    return "3층 · 깊은 지하";
                default:           return "4층 · 매우 깊은 지하";
            }
        }

        /// <summary>층을 상징하는 색 — 보물과 지층 표시에 함께 쓴다.</summary>
        public static Color SignatureColor(Zone z)
        {
            switch (z)
            {
                case Zone.Surface: return new Color32(245, 240, 225, 255); // 은백색
                case Zone.Shallow: return new Color32(60, 220, 130, 255);  // 에메랄드
                case Zone.Deep:    return new Color32(75, 140, 255, 255);  // 사파이어
                default:           return new Color32(230, 90, 235, 255);  // 자수정
            }
        }

        public static float TreasureChance(Zone z)
        {
            switch (z)
            {
                case Zone.Surface: return GameConfig.TreasureChanceSurface;
                case Zone.Shallow: return GameConfig.TreasureChanceShallow;
                case Zone.Deep:    return GameConfig.TreasureChanceDeep;
                default:           return GameConfig.TreasureChanceAbyss;
            }
        }
    }
}
