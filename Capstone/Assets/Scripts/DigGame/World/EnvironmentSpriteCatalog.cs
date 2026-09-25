using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>CC0 픽셀 지형·재활용 시설·결과물 스프라이트를 런타임에 잘라 제공한다.</summary>
    public static class EnvironmentSpriteCatalog
    {
        const string EnvironmentRoot = "EnvironmentPixel/";
        const string PixelRoot = "PixelRefresh/";
        const int GroundPixels = 16;
        const int TilePixels = 32;
        /// <summary>통로 타일의 흙 무늬 변형 수.</summary>
        const int PassageVariants = 4;
        const int PassageMasks = 256;
        const int PassageAtlasColumns = 32;
        static readonly int[] PassageGroundVariants = { 0, 5, 10, 15 };

        static readonly Dictionary<TileKind, Sprite[]> GroundFills = new Dictionary<TileKind, Sprite[]>();
        static readonly Dictionary<TileKind, Sprite[]> Passages = new Dictionary<TileKind, Sprite[]>();
        static readonly Dictionary<TileKind, Texture2D> GroundAtlases = new Dictionary<TileKind, Texture2D>();
        static readonly Dictionary<TileKind, Texture2D> PassageAtlases = new Dictionary<TileKind, Texture2D>();
        static readonly Dictionary<int, Sprite> CaveBackdrops = new Dictionary<int, Sprite>();
        static Sprite[] _entrancePassage;
        static readonly Dictionary<int, Sprite> RecycleItems = new Dictionary<int, Sprite>();
        static readonly Dictionary<MaterialKind, Sprite> RecycledProducts = new Dictionary<MaterialKind, Sprite>();

        static Sprite _recyclerFactory;
        static Sprite _surfaceBin;

        // ---------------- 지하 입구 통로 ----------------
        /// <summary>입구 통로 시트의 행 (위에서부터).</summary>
        public enum ShaftRow { Mouth = 0, Body = 1, Floor = 2 }

        /// <summary>
        /// 입구 통로 시트의 열.
        /// 사다리는 <see cref="Single"/> 과 <see cref="LadderMid"/> 에만 들어 있다 —
        /// 넓어진 입구의 모든 칸에 사다리를 깔면 사다리 벽처럼 보인다.
        /// </summary>
        public enum ShaftCol { Left = 0, Mid = 1, Right = 2, Single = 3, LadderMid = 4 }

        public const int ShaftCols = 5;
        public const int ShaftRows = 3;

        /// <summary>잔디층을 관통하는 갱도 타일. 나무 동바리를 댄 '사람이 판 통로'.</summary>
        public static Sprite EntrancePassage(ShaftRow row, ShaftCol col)
        {
            if (_entrancePassage == null) _entrancePassage = new Sprite[ShaftRows * ShaftCols];
            int index = (int)row * ShaftCols + (int)col;
            if (index < 0 || index >= _entrancePassage.Length) return null;
            if (_entrancePassage[index] != null) return _entrancePassage[index];

            var texture = LoadPixelTexture("Ground/entrance_passage");
            if (texture == null) return null;

            _entrancePassage[index] = CropTopLeftGrid(texture, index, ShaftRows * ShaftCols,
                GroundPixels, GroundPixels, GroundPixels / GameConfig.CellSize,
                $"Entrance Passage {row}-{col}");
            return _entrancePassage[index];
        }

        public static Sprite GroundFill(int pattern) => GroundFill(TileKind.SoftDirt, pattern);

        /// <summary>지층별 CC0 16px 흙 타일. 중앙 채움 타일을 회전·반전해 16개 변형을 만든다.</summary>
        public static Sprite GroundFill(TileKind kind, int pattern)
        {
            pattern = Mathf.Abs(pattern) % 16;
            if (!GroundFills.TryGetValue(kind, out var sprites))
            {
                sprites = new Sprite[16];
                GroundFills[kind] = sprites;
            }
            if (sprites[pattern] != null) return sprites[pattern];

            var texture = BuildGroundAtlas(kind);
            if (texture == null) return null;
            sprites[pattern] = CropTopLeftGrid(texture, pattern, 4, GroundPixels, GroundPixels,
                GroundPixels / GameConfig.CellSize, $"Pixel Ground {kind} {pattern:00}");
            return sprites[pattern];
        }

        public static Sprite PassageForMask(int openMask, int pattern) =>
            PassageForMask(TileKind.SoftDirt, openMask, pattern);

        /// <summary>
        /// 열린 방향 비트에 맞는 통로.
        /// 직교: 위 1, 오른쪽 2, 아래 4, 왼쪽 8.
        /// 대각: 우상 16, 우하 32, 좌하 64, 좌상 128.
        /// 대각 비트는 양옆 직교 방향까지 모두 열렸을 때만 세워지므로,
        /// 두 칸이 겹쳐 넓게 파인 자리에서 네 칸이 만나는 모서리의
        /// 흙 찌꺼기가 깨끗이 사라지고 벽에 붙은 모서리는 그대로 남는다.
        /// </summary>
        public static Sprite PassageForMask(TileKind kind, int openMask, int pattern)
        {
            openMask &= PassageMasks - 1;
            pattern = Mathf.Abs(pattern) % PassageVariants;
            int index = openMask * PassageVariants + pattern;
            if (!Passages.TryGetValue(kind, out var sprites))
            {
                sprites = new Sprite[PassageMasks * PassageVariants];
                Passages[kind] = sprites;
            }
            if (sprites[index] != null) return sprites[index];

            var texture = BuildMatchingPassageAtlas(kind);
            if (texture == null) return null;
            sprites[index] = CropTopLeftGrid(texture, index, PassageAtlasColumns,
                GroundPixels, GroundPixels, GroundPixels / GameConfig.CellSize,
                $"Pixel Passage {kind} {openMask:000}-{pattern}");
            return sprites[index];
        }

        static Texture2D BuildGroundAtlas(TileKind kind)
        {
            if (GroundAtlases.TryGetValue(kind, out var cached)) return cached;

            bool grass = kind == TileKind.Grass;
            string file;
            switch (kind)
            {
                case TileKind.SoftDirt: file = "tileset4"; break;
                case TileKind.Clay: file = "tileset5"; break;
                case TileKind.Stone: file = "tileset6"; break;
                case TileKind.HardRock: file = "tileset2"; break;
                default: file = "tileset1"; break;
            }

            var sourceTexture = LoadPixelTexture(grass ? "Village/grass" : "Ground/" + file);
            if (sourceTexture == null || !sourceTexture.isReadable)
            {
                Debug.LogError($"[DigGame] 픽셀 땅 Read/Write 설정을 확인하세요: {file}");
                return null;
            }

            const int variationsPerRow = 4;
            int atlasSize = variationsPerRow * GroundPixels;
            var atlas = NewRuntimeTexture(atlasSize, atlasSize, $"Pixel Ground {kind} (Runtime)");
            var source = sourceTexture.GetPixels32();
            var output = new Color32[atlasSize * atlasSize];

            // TinyWorlds 3x3 시트의 중앙 완전 채움 타일.
            const int sourceX = GroundPixels;
            const int sourceY = GroundPixels;
            for (int pattern = 0; pattern < 16; pattern++)
            {
                int destinationX = (pattern % variationsPerRow) * GroundPixels;
                int destinationY = atlasSize - (pattern / variationsPerRow + 1) * GroundPixels;
                if (grass)
                {
                    int grassX = (pattern % 4) * GroundPixels;
                    for (int y = 0; y < GroundPixels; y++)
                        for (int x = 0; x < GroundPixels; x++)
                            output[(destinationY + y) * atlasSize + destinationX + x] =
                                source[y * sourceTexture.width + grassX + x];
                    continue;
                }

                bool flipX = (pattern & 1) != 0;
                bool flipY = (pattern & 2) != 0;
                bool transpose = (pattern & 4) != 0;
                int shift = (pattern & 8) != 0 ? 5 : 0;

                for (int y = 0; y < GroundPixels; y++)
                {
                    for (int x = 0; x < GroundPixels; x++)
                    {
                        int sx = flipX ? GroundPixels - 1 - x : x;
                        int sy = flipY ? GroundPixels - 1 - y : y;
                        if (transpose) { int swap = sx; sx = sy; sy = swap; }
                        sx = (sx + shift) % GroundPixels;
                        sy = (sy + shift * 2) % GroundPixels;
                        output[(destinationY + y) * atlasSize + destinationX + x] =
                            source[(sourceY + sy) * sourceTexture.width + sourceX + sx];
                    }
                }
            }

            atlas.SetPixels32(output);
            atlas.Apply(false, false);
            GroundAtlases[kind] = atlas;
            return atlas;
        }

        static Texture2D BuildMatchingPassageAtlas(TileKind kind)
        {
            if (PassageAtlases.TryGetValue(kind, out var cached)) return cached;
            var ground = BuildGroundAtlas(kind);
            if (ground == null) return null;

            const int tileCount = PassageMasks * PassageVariants;
            int atlasWidth = PassageAtlasColumns * GroundPixels;
            int atlasHeight = (tileCount / PassageAtlasColumns) * GroundPixels;
            var atlas = NewRuntimeTexture(atlasWidth, atlasHeight, $"Pixel Passages {kind} (Runtime)");
            var source = ground.GetPixels32();
            var output = new Color32[atlasWidth * atlasHeight];

            for (int index = 0; index < tileCount; index++)
            {
                int mask = index / PassageVariants;
                int variant = PassageGroundVariants[index % PassageVariants];

                int sourceX = (variant % 4) * GroundPixels;
                int sourceY = ground.height - (variant / 4 + 1) * GroundPixels;
                int destinationX = (index % PassageAtlasColumns) * GroundPixels;
                int destinationY = atlasHeight - (index / PassageAtlasColumns + 1) * GroundPixels;

                for (int y = 0; y < GroundPixels; y++)
                {
                    for (int x = 0; x < GroundPixels; x++)
                    {
                        Color32 pixel = source[(sourceY + y) * ground.width + sourceX + x];
                        if (IsDugPixel(mask, x, y, variant)) pixel.a = 0;
                        output[(destinationY + y) * atlasWidth + destinationX + x] = pixel;
                    }
                }
            }

            atlas.SetPixels32(output);
            atlas.Apply(false, false);
            PassageAtlases[kind] = atlas;
            return atlas;
        }

        static bool IsDugPixel(int openMask, int x, int y, int pattern)
        {
            if ((openMask & 255) == 255) return true;

            const float center = (GroundPixels - 1) * 0.5f;
            const float far = GroundPixels + 1f;
            const float near = -2f;
            var point = new Vector2(x, y);
            var middle = new Vector2(center, center);
            float distance = Vector2.Distance(point, middle);

            if ((openMask & 1) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(center, far)));
            if ((openMask & 2) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(far, center)));
            if ((openMask & 4) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(center, near)));
            if ((openMask & 8) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(near, center)));

            // 네 칸이 모두 열린 모서리는 대각선까지 깎아 떠 있는 흙덩이를 없앤다.
            if ((openMask & 16) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(far, far)));
            if ((openMask & 32) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(far, near)));
            if ((openMask & 64) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(near, near)));
            if ((openMask & 128) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(near, far)));

            float edgeWobble = Mathf.Sin((x + pattern * 3) * 0.77f + y * 0.31f) * 0.32f
                             + Mathf.Sin(x * 0.29f - (y + pattern * 5) * 0.67f) * 0.22f;
            return distance <= 5.25f + edgeWobble;
        }

        static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= 0.0001f) return Vector2.Distance(point, start);
            float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared);
            return Vector2.Distance(point, start + segment * t);
        }

        public static Sprite RecyclerFactory()
        {
            if (_recyclerFactory != null) return _recyclerFactory;
            var texture = LoadPixelTexture("Factory/recycler_factory");
            if (texture == null) return null;
            _recyclerFactory = MakeSprite(texture, new Rect(0f, 0f, texture.width, texture.height), 32f,
                "Pixel Recycling Factory");
            return _recyclerFactory;
        }

        public static Sprite RecycledProduct(MaterialKind material)
        {
            if (RecycledProducts.TryGetValue(material, out var cached)) return cached;
            int[] indices = { 22, 10, 19, 11, 13, 15, 14 };
            int index = indices[Mathf.Clamp((int)material, 0, indices.Length - 1)];
            var texture = LoadPixelTexture("Products/recycled_products");
            if (texture == null) return null;
            var sprite = CropTopLeftGrid(texture, index, 10, TilePixels, TilePixels, 32f,
                $"Recycled Product {material}");
            RecycledProducts[material] = sprite;
            return sprite;
        }

        public static Sprite CaveBackdrop(int seed)
        {
            int index = Mathf.Abs(seed * 17 + 11) % 40;
            if (CaveBackdrops.TryGetValue(index, out var cached)) return cached;
            var texture = LoadEnvironmentTexture("db32_cave_tileset");
            if (texture == null) return null;
            var sprite = CropTopLeftGrid(texture, index, 10, TilePixels, TilePixels,
                TilePixels / GameConfig.CellSize, $"DB32 Cave Detail {index:00}");
            CaveBackdrops[index] = sprite;
            return sprite;
        }

        public static Sprite SurfaceBin()
        {
            if (_surfaceBin != null) return _surfaceBin;
            var texture = LoadEnvironmentTexture("trash_can");
            if (texture == null) return null;
            _surfaceBin = MakeSprite(texture, new Rect(0f, 0f, texture.width, texture.height), 32f,
                "Surface Trash Drop-off");
            return _surfaceBin;
        }

        public static Sprite RecycleItem(int index)
        {
            index = Mathf.Abs(index) % 16;
            if (RecycleItems.TryGetValue(index, out var cached)) return cached;
            var texture = LoadEnvironmentTexture("recycle_items");
            if (texture == null) return null;
            var sprite = MakeSprite(texture,
                new Rect(index * TilePixels, 0f, TilePixels, Mathf.Min(64, texture.height)),
                32f, $"Recycle Item {index:00}");
            RecycleItems[index] = sprite;
            return sprite;
        }

        static Texture2D LoadEnvironmentTexture(string name) => LoadTexture(EnvironmentRoot + name);
        static Texture2D LoadPixelTexture(string name) => LoadTexture(PixelRoot + name);

        static Texture2D LoadTexture(string path)
        {
            var texture = Resources.Load<Texture2D>(path);
            if (texture == null)
            {
                Debug.LogWarning($"[DigGame] 픽셀 에셋을 찾지 못했습니다: {path}");
                return null;
            }
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }

        static Texture2D NewRuntimeTexture(int width, int height, string name) => new Texture2D(
            width, height, TextureFormat.RGBA32, false)
        {
            name = name,
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.DontSave
        };

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
            if (_entrancePassage != null)
            {
                for (int i = 0; i < _entrancePassage.Length; i++)
                    DestroyGenerated(_entrancePassage[i]);
                _entrancePassage = null;
            }
            foreach (var set in GroundFills.Values)
                for (int i = 0; i < set.Length; i++) DestroyGenerated(set[i]);
            foreach (var set in Passages.Values)
                for (int i = 0; i < set.Length; i++) DestroyGenerated(set[i]);
            foreach (var sprite in CaveBackdrops.Values) DestroyGenerated(sprite);
            foreach (var sprite in RecycleItems.Values) DestroyGenerated(sprite);
            foreach (var sprite in RecycledProducts.Values) DestroyGenerated(sprite);
            foreach (var texture in GroundAtlases.Values) DestroyGenerated(texture);
            foreach (var texture in PassageAtlases.Values) DestroyGenerated(texture);
            DestroyGenerated(_recyclerFactory);
            DestroyGenerated(_surfaceBin);

            GroundFills.Clear();
            Passages.Clear();
            GroundAtlases.Clear();
            PassageAtlases.Clear();
            CaveBackdrops.Clear();
            RecycleItems.Clear();
            RecycledProducts.Clear();
            _recyclerFactory = null;
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
