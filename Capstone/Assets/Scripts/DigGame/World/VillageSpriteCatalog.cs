using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>지상 마을 에셋(집·소품·길·잔디·주민)을 잘라 제공한다. 모두 16px 그리드.</summary>
    public static class VillageSpriteCatalog
    {
        const string VillageRoot = "PixelRefresh/Village/";
        const string NpcRoot = "PixelRefresh/NPC/";

        public const int HouseTiers = 6;
        public const int HousePixels = 48;      // 3칸
        public const int PropPixels = 32;       // 2칸
        public const int TilePixels = 16;       // 1칸

        /// <summary>props 시트 인덱스.</summary>
        public enum Prop
        {
            TreeBig = 0, TreeSmall = 1, Bush = 2, LampPost = 3, Fence = 4,
            Well = 5, Crates = 6, Sign = 7, FlowerBed = 8, WaterTower = 9, Bench = 10
        }

        public static readonly string[] VillagerNames =
        {
            "villager_a", "villager_b", "villager_c",
            "villager_d", "villager_e", "villager_kid"
        };

        static readonly Dictionary<string, Sprite[]> Cache = new Dictionary<string, Sprite[]>();

        /// <summary>16px = 한 칸이 되도록 하는 픽셀 밀도.</summary>
        public static float PixelsPerUnit => TilePixels / GameConfig.CellSize;

        // ------------------------------------------------------------------
        public static Sprite House(int tier) =>
            Slice(VillageRoot + "houses", HousePixels, HouseTiers, BottomPivot)[
                Mathf.Clamp(tier, 0, HouseTiers - 1)];

        public static Sprite PropSprite(Prop prop) =>
            Slice(VillageRoot + "props", PropPixels, 11, BottomPivot)[(int)prop];

        public static Sprite Road(int variant) =>
            Slice(VillageRoot + "roads", TilePixels, 6, CenterPivot)[Mathf.Abs(variant) % 6];

        public static Sprite Grass(int variant) =>
            Slice(VillageRoot + "grass", TilePixels, 4, CenterPivot)[Mathf.Abs(variant) % 4];

        /// <summary>주민 걷기 4프레임 (오른쪽 보기).</summary>
        public static Sprite[] Villager(int kind)
        {
            string name = VillagerNames[Mathf.Abs(kind) % VillagerNames.Length];
            // 주민은 플레이어와 같은 크기 기준(ppu 16)을 쓴다.
            return Slice(NpcRoot + name, TilePixels, 4, CenterPivot, TilePixels);
        }

        static readonly Vector2 BottomPivot = new Vector2(0.5f, 0f);
        static readonly Vector2 CenterPivot = new Vector2(0.5f, 0.5f);

        static Sprite[] Slice(string path, int size, int count, Vector2 pivot, float ppuOverride = 0f)
        {
            string key = path + "#" + size;
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var texture = Resources.Load<Texture2D>(path);
            if (texture == null)
            {
                Debug.LogWarning($"[DigGame] 마을 에셋을 찾지 못했습니다: {path}");
                var empty = new Sprite[count];
                Cache[key] = empty;
                return empty;
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            float ppu = ppuOverride > 0f ? ppuOverride : PixelsPerUnit;
            int available = Mathf.Max(1, texture.width / size);
            var sprites = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                int column = Mathf.Min(i, available - 1);
                var sprite = Sprite.Create(texture,
                    new Rect(column * size, 0f, size, Mathf.Min(size, texture.height)),
                    pivot, ppu, 0, SpriteMeshType.FullRect);
                sprite.name = $"{path}#{i:00}";
                sprite.hideFlags = HideFlags.DontSave;
                sprites[i] = sprite;
            }
            Cache[key] = sprites;
            return sprites;
        }

        public static void Clear()
        {
            foreach (var set in Cache.Values)
            {
                if (set == null) continue;
                for (int i = 0; i < set.Length; i++)
                {
                    if (set[i] == null) continue;
                    if (Application.isPlaying) Object.Destroy(set[i]);
                    else Object.DestroyImmediate(set[i]);
                }
            }
            Cache.Clear();
        }
    }
}
