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
        Bedrock = 6     // 파괴 불가
    }

    public enum Zone
    {
        Surface = 0,   // 1층 · 지상 근처
        Shallow = 1,   // 2층 · 얕은 지하
        Deep = 2,      // 3층 · 깊은 지하
        Abyss = 3      // 4층 · 매우 깊은 지하
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
        public bool Broken;           // 유리가 깨졌는가

        public LootItem(string name, int value, Color color, int volume, MaterialKind material)
        {
            Name = name;
            Value = value;
            Color = color;
            Volume = Mathf.Max(1, volume);
            Material = material;
            Broken = false;
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
    }

    public static class TileInfo
    {
        /// <summary>파는 데 필요한 최소 '힘'. 0이면 통과 가능, -1이면 파괴 불가.</summary>
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

        public static bool IsWalkable(TileKind kind) => kind == TileKind.Sky || kind == TileKind.Empty;

        public static bool IsDiggable(TileKind kind) => RequiredPower(kind) > 0;

        public static Color ColorOf(TileKind kind)
        {
            switch (kind)
            {
                case TileKind.Sky:      return Color.clear; // 카메라의 실제 하늘 배경이 비친다
                case TileKind.Empty:    return new Color32(30, 24, 21, 255);
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
                case TileKind.SoftDirt: return Zone.Surface;
                case TileKind.Clay:     return Zone.Shallow;
                case TileKind.Stone:    return Zone.Deep;
                default:                return Zone.Abyss;
            }
        }
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
