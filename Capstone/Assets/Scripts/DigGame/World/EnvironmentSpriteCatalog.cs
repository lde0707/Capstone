using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>무료 픽셀 환경 시트의 지형·수거 시설 스프라이트를 런타임에 잘라 제공한다.</summary>
    public static class EnvironmentSpriteCatalog
    {
        const string Root = "EnvironmentPixel/";
        const int TilePixels = 32;

        // OPP의 128px 연속 암반(4x4)과 16방향 통로 마스크(각 4x4 변형).
        static readonly Sprite[] GroundFills = new Sprite[16];
        static readonly Sprite[] Passages = new Sprite[16 * 16];
        static readonly Dictionary<int, Sprite> CaveBackdrops = new Dictionary<int, Sprite>();
        static readonly Dictionary<int, Sprite> RecycleItems = new Dictionary<int, Sprite>();
        static Sprite _surfaceBin;

        /// <summary>연속된 128px 암반 패턴의 32px 조각.</summary>
        public static Sprite GroundFill(int pattern)
        {
            pattern = Mathf.Abs(pattern) % GroundFills.Length;
            if (GroundFills[pattern] != null) return GroundFills[pattern];

            var texture = LoadTexture("opp_cave_background");
            if (texture == null) return null;
            GroundFills[pattern] = CropTopLeftGrid(texture, pattern, 4, TilePixels, TilePixels,
                TilePixels / GameConfig.CellSize, $"OPP Cave Ground {pattern:00}");
            return GroundFills[pattern];
        }

        /// <summary>
        /// 열린 방향 비트(위 1, 오른쪽 2, 아래 4, 왼쪽 8)에 맞는 통로.
        /// 15는 자연 연결, 5/10은 직선 통로, 1/2/4/8은 한쪽만 열린 막힌 끝이다.
        /// </summary>
        public static Sprite PassageForMask(int openMask, int pattern)
        {
            openMask &= 15;
            pattern = Mathf.Abs(pattern) % 16;
            int index = openMask * 16 + pattern;
            if (Passages[index] != null) return Passages[index];

            var texture = LoadTexture("opp_cave_passages_32");
            if (texture == null) return null;
            Passages[index] = CropTopLeftGrid(texture, index, 16, TilePixels, TilePixels,
                TilePixels / GameConfig.CellSize, $"OPP Cave Passage {openMask:00}-{pattern:00}");
            return Passages[index];
        }

        /// <summary>DB32 동굴 시트의 여러 칸을 지하 집하장 배경으로 순환 사용한다.</summary>
        public static Sprite CaveBackdrop(int seed)
        {
            int index = Mathf.Abs(seed * 17 + 11) % 40;
            if (CaveBackdrops.TryGetValue(index, out var cached)) return cached;

            var texture = LoadTexture("db32_cave_tileset");
            if (texture == null) return null;
            var sprite = CropTopLeftGrid(texture, index, 10, TilePixels, TilePixels,
                TilePixels / GameConfig.CellSize, $"DB32 Cave Detail {index:00}");
            CaveBackdrops[index] = sprite;
            return sprite;
        }

        public static Sprite SurfaceBin()
        {
            if (_surfaceBin != null) return _surfaceBin;
            var texture = LoadTexture("trash_can");
            if (texture == null) return null;
            _surfaceBin = MakeSprite(texture, new Rect(0f, 0f, texture.width, texture.height), 32f,
                "Surface Trash Drop-off");
            return _surfaceBin;
        }

        /// <summary>같은 DB32 팔레트의 32x64 재활용 물품 16종 중 하나.</summary>
        public static Sprite RecycleItem(int index)
        {
            index = Mathf.Abs(index) % 16;
            if (RecycleItems.TryGetValue(index, out var cached)) return cached;

            var texture = LoadTexture("recycle_items");
            if (texture == null) return null;
            var sprite = MakeSprite(texture,
                new Rect(index * TilePixels, 0f, TilePixels, Mathf.Min(64, texture.height)),
                32f, $"Recycle Item {index:00}");
            RecycleItems[index] = sprite;
            return sprite;
        }

        static Texture2D LoadTexture(string name)
        {
            var texture = Resources.Load<Texture2D>(Root + name);
            if (texture == null)
            {
                Debug.LogWarning($"[DigGame] 환경 픽셀 에셋을 찾지 못했습니다: {Root}{name}");
                return null;
            }
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }

        static Sprite CropTopLeftGrid(Texture2D texture, int index, int columns,
            int width, int height, float pixelsPerUnit, string name)
        {
            int column = index % columns;
            int rowFromTop = index / columns;
            float x = column * width;
            float y = texture.height - (rowFromTop + 1) * height;
            return MakeSprite(texture, new Rect(x, y, width, height), pixelsPerUnit, name);
        }

        static Sprite MakeSprite(Texture2D texture, Rect rect, float pixelsPerUnit, string name)
        {
            var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit,
                0, SpriteMeshType.FullRect);
            sprite.name = name;
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        public static void Clear()
        {
            for (int i = 0; i < GroundFills.Length; i++) DestroyGenerated(GroundFills[i]);
            for (int i = 0; i < Passages.Length; i++) DestroyGenerated(Passages[i]);
            foreach (var sprite in CaveBackdrops.Values) DestroyGenerated(sprite);
            foreach (var sprite in RecycleItems.Values) DestroyGenerated(sprite);
            DestroyGenerated(_surfaceBin);

            System.Array.Clear(GroundFills, 0, GroundFills.Length);
            System.Array.Clear(Passages, 0, Passages.Length);
            CaveBackdrops.Clear();
            RecycleItems.Clear();
            _surfaceBin = null;
        }

        static void DestroyGenerated(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }
    }
}
