using UnityEngine;

namespace DigGame
{
    public enum PlayerVisualState
    {
        Walk,
        Dig
    }

    /// <summary>플레이어가 바라보는 방향.</summary>
    public enum Facing
    {
        Down,
        Up,
        Left,
        Right
    }

    /// <summary>
    /// 16px 플레이어 시트에서 방향별 걷기·굴착 프레임을 제공한다.
    /// 좌/우는 같은 옆모습 시트를 쓰고 SpriteRenderer.flipX 로 뒤집는다.
    /// </summary>
    public static class PlayerSpriteCatalog
    {
        const string Root = "PixelRefresh/Player/";
        const int FramePixels = 16;
        const int FrameCount = 4;

        static readonly System.Collections.Generic.Dictionary<string, Sprite[]> Cache =
            new System.Collections.Generic.Dictionary<string, Sprite[]>();

        /// <summary>좌/우 방향이면 스프라이트를 좌우 반전해야 한다.</summary>
        public static bool NeedsFlip(Facing facing) => facing == Facing.Left;

        public static Sprite[] Frames(PlayerVisualState state, Facing facing)
        {
            string resource;
            if (state == PlayerVisualState.Dig)
            {
                if (facing == Facing.Up) resource = "hero_dig_up";
                else if (facing == Facing.Down) resource = "hero_dig_down";
                else resource = "hero_dig_side";
            }
            else
            {
                if (facing == Facing.Up) resource = "hero_up";
                else if (facing == Facing.Down) resource = "hero_down";
                else resource = "hero_side";
            }
            return Load(resource);
        }

        static Sprite[] Load(string resource)
        {
            if (Cache.TryGetValue(resource, out var cached)) return cached;

            var texture = Resources.Load<Texture2D>(Root + resource);
            if (texture == null)
            {
                Debug.LogWarning($"[DigGame] 플레이어 에셋을 찾지 못했습니다: {Root}{resource}");
                Cache[resource] = new Sprite[0];
                return Cache[resource];
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            int count = Mathf.Max(1, Mathf.Min(FrameCount, texture.width / FramePixels));
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                frames[i] = Sprite.Create(texture,
                    new Rect(i * FramePixels, 0f, FramePixels, FramePixels),
                    new Vector2(0.5f, 0.5f), FramePixels, 0, SpriteMeshType.FullRect);
                frames[i].name = $"{resource} {i + 1}";
                frames[i].hideFlags = HideFlags.DontSave;
            }
            Cache[resource] = frames;
            return frames;
        }

        public static void Clear()
        {
            foreach (var frames in Cache.Values)
            {
                if (frames == null) continue;
                for (int i = 0; i < frames.Length; i++)
                {
                    if (frames[i] == null) continue;
                    if (Application.isPlaying) Object.Destroy(frames[i]);
                    else Object.DestroyImmediate(frames[i]);
                }
            }
            Cache.Clear();
        }
    }
}
