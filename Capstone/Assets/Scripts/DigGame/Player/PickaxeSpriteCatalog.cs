using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 단계별 집게 그림. 한 줄 시트에서 (단계 x 각도 x 벌림) 칸을 꺼내 쓴다.
    ///
    /// 각도를 미리 그려 둔 이유: 실행 중에 스프라이트를 돌리면 45도가 아닌 각에서
    /// 대각선이 뭉개진다. 쓰는 각도만 픽셀로 직접 그려 두면 어느 자세든 깨끗하다.
    /// </summary>
    public static class PickaxeSpriteCatalog
    {
        const string Resource = "PixelRefresh/Player/tongs";
        const int FramePixels = 16;

        /// <summary>집게가 가리키는 방향. 시트 칸 순서와 같다.</summary>
        public enum ToolAngle
        {
            Down = 0,        // -90도
            DownForward = 1, // -45도
            Forward = 2,     //   0도
            UpForward = 3,   //  45도
            Up = 4,          //  90도
        }

        public const int TierCount = 5;
        public const int AngleCount = 5;
        const int JawStates = 2;

        static Sprite[] _frames;

        /// <summary>칸 번호 = ((단계-1) * 각도수 + 각도) * 2 + (벌림 ? 1 : 0).</summary>
        public static int IndexOf(int tier, ToolAngle angle, bool jawsOpen)
        {
            int t = Mathf.Clamp(tier, 1, TierCount) - 1;
            int a = Mathf.Clamp((int)angle, 0, AngleCount - 1);
            return (t * AngleCount + a) * JawStates + (jawsOpen ? 1 : 0);
        }

        public static Sprite For(int tier, ToolAngle angle, bool jawsOpen)
        {
            var frames = Load();
            if (frames.Length == 0) return null;
            return frames[Mathf.Clamp(IndexOf(tier, angle, jawsOpen), 0, frames.Length - 1)];
        }

        /// <summary>HUD 미리보기용 — 옆으로 누운 다묾 자세.</summary>
        public static Sprite Preview(int tier) => For(tier, ToolAngle.Forward, false);

        static Sprite[] Load()
        {
            if (_frames != null) return _frames;

            var texture = Resources.Load<Texture2D>(Resource);
            if (texture == null)
            {
                Debug.LogWarning($"[DigGame] 집게 그림을 찾지 못했습니다: {Resource}");
                _frames = new Sprite[0];
                return _frames;
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            int count = Mathf.Max(1, texture.width / FramePixels);
            _frames = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                // 축(쥐는 점)은 칸 한가운데다. 그 점을 손 위치에 맞추면 자세가 완성된다.
                _frames[i] = Sprite.Create(texture,
                    new Rect(i * FramePixels, 0f, FramePixels, FramePixels),
                    new Vector2(0.5f, 0.5f), FramePixels, 0, SpriteMeshType.FullRect);
                _frames[i].name = $"tongs {i:00}";
                _frames[i].hideFlags = HideFlags.DontSave;
            }
            return _frames;
        }

        public static void Clear()
        {
            if (_frames == null) return;
            for (int i = 0; i < _frames.Length; i++)
            {
                if (_frames[i] == null) continue;
                if (Application.isPlaying) Object.Destroy(_frames[i]);
                else Object.DestroyImmediate(_frames[i]);
            }
            _frames = null;
        }
    }
}
