using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>에셋 없이 코드로 스프라이트를 만들어 쓰기 위한 유틸.</summary>
    public static class SpriteFactory
    {
        const int Px = 16;
        static readonly Dictionary<int, Sprite> _cache = new Dictionary<int, Sprite>();

        static int Key(Color c, int variant)
        {
            Color32 c32 = c;
            unchecked
            {
                int h = (c32.r << 24) ^ (c32.g << 16) ^ (c32.b << 8) ^ c32.a;
                return h * 397 ^ variant;
            }
        }

        /// <summary>단색 사각형 스프라이트. 1 유닛 = 1 타일.</summary>
        public static Sprite Solid(Color color)
        {
            int k = Key(color, 0);
            if (_cache.TryGetValue(k, out var cached)) return cached;

            var tex = NewTex();
            var px = new Color[Px * Px];
            for (int i = 0; i < px.Length; i++) px[i] = color;
            tex.SetPixels(px);
            tex.Apply();

            var sp = Make(tex);
            _cache[k] = sp;
            return sp;
        }

        /// <summary>흙 느낌을 주는 노이즈가 들어간 사각형.</summary>
        public static Sprite Noisy(Color color, int seed)
        {
            int k = Key(color, 100 + seed);
            if (_cache.TryGetValue(k, out var cached)) return cached;

            var rng = new System.Random(seed);
            var tex = NewTex();
            var px = new Color[Px * Px];
            for (int i = 0; i < px.Length; i++)
            {
                float d = ((float)rng.NextDouble() - 0.5f) * 0.14f;
                px[i] = new Color(
                    Mathf.Clamp01(color.r + d),
                    Mathf.Clamp01(color.g + d),
                    Mathf.Clamp01(color.b + d),
                    color.a);
            }

            // 위쪽에 살짝 밝은 하이라이트
            for (int x = 0; x < Px; x++)
            {
                int i = (Px - 1) * Px + x;
                px[i] = Color.Lerp(px[i], Color.white, 0.10f);
            }

            tex.SetPixels(px);
            tex.Apply();

            var sp = Make(tex);
            _cache[k] = sp;
            return sp;
        }

        /// <summary>테두리가 있는 사각형 (플레이어/일꾼용).</summary>
        public static Sprite Framed(Color fill, Color border)
        {
            int k = Key(fill, 500 ^ Key(border, 7));
            if (_cache.TryGetValue(k, out var cached)) return cached;

            var tex = NewTex();
            var px = new Color[Px * Px];
            for (int y = 0; y < Px; y++)
            {
                for (int x = 0; x < Px; x++)
                {
                    bool edge = x <= 1 || y <= 1 || x >= Px - 2 || y >= Px - 2;
                    px[y * Px + x] = edge ? border : fill;
                }
            }
            tex.SetPixels(px);
            tex.Apply();

            var sp = Make(tex);
            _cache[k] = sp;
            return sp;
        }

        /// <summary>다이아몬드(마름모) 모양 — 보물/광산 표시용.</summary>
        public static Sprite Diamond(Color fill)
        {
            int k = Key(fill, 900);
            if (_cache.TryGetValue(k, out var cached)) return cached;

            var tex = NewTex();
            var px = new Color[Px * Px];
            float c = (Px - 1) * 0.5f;
            for (int y = 0; y < Px; y++)
            {
                for (int x = 0; x < Px; x++)
                {
                    bool inside = Mathf.Abs(x - c) + Mathf.Abs(y - c) <= c;
                    px[y * Px + x] = inside ? fill : Color.clear;
                }
            }
            tex.SetPixels(px);
            tex.Apply();

            var sp = Make(tex);
            _cache[k] = sp;
            return sp;
        }

        static Texture2D NewTex()
        {
            var tex = new Texture2D(Px, Px, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            return tex;
        }

        static Sprite Make(Texture2D tex)
        {
            var sp = Sprite.Create(tex, new Rect(0, 0, Px, Px), new Vector2(0.5f, 0.5f), Px, 0,
                SpriteMeshType.FullRect);
            sp.hideFlags = HideFlags.DontSave;
            return sp;
        }

        /// <summary>재시작 시 생성해둔 스프라이트/텍스처를 해제한다.</summary>
        public static void ClearCache()
        {
            TrashSpriteCatalog.Clear();
            EnvironmentSpriteCatalog.Clear();
            PlayerSpriteCatalog.Clear();
            VillageSpriteCatalog.Clear();
            RelicSpriteCatalog.Clear();

            foreach (var sp in _cache.Values)
            {
                if (sp == null) continue;
                var tex = sp.texture;
                DestroyGenerated(sp);
                if (tex != null) DestroyGenerated(tex);
            }
            _cache.Clear();
        }

        static void DestroyGenerated(Object obj)
        {
            if (obj == null) return;

            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }
    }
}
