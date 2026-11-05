using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 플레이어가 들고 있는 집게를 몸통과 별개 렌더러로 그린다.
    ///
    /// 캐릭터 시트에 집게를 박아 넣으면 단계가 5개라 시트도 5벌이 필요하다.
    /// 집게만 따로 겹치면 시트는 한 벌로 두고 단계만 갈아끼우면 된다.
    ///
    /// 굴착 자세(쥐는 점·각도)는 원래 hero_dig_* 에 그려져 있던 도구 픽셀에서
    /// 그대로 뽑아 왔다. 그래서 기존 굴착 동작과 궤적이 어긋나지 않는다.
    /// </summary>
    public sealed class PlayerToolVisual : MonoBehaviour
    {
        readonly struct Pose
        {
            public readonly int GripX;
            public readonly int GripY;
            public readonly PickaxeSpriteCatalog.ToolAngle Angle;
            public readonly bool JawsOpen;

            public Pose(int gripX, int gripY, PickaxeSpriteCatalog.ToolAngle angle, bool jawsOpen)
            {
                GripX = gripX; GripY = gripY; Angle = angle; JawsOpen = jawsOpen;
            }
        }

        const float FramePixels = 16f;
        const float Center = 8f;          // 16px 칸의 한가운데 = 스프라이트 축

        static readonly PickaxeSpriteCatalog.ToolAngle Down = PickaxeSpriteCatalog.ToolAngle.Down;
        static readonly PickaxeSpriteCatalog.ToolAngle DownFwd = PickaxeSpriteCatalog.ToolAngle.DownForward;
        static readonly PickaxeSpriteCatalog.ToolAngle Fwd = PickaxeSpriteCatalog.ToolAngle.Forward;
        static readonly PickaxeSpriteCatalog.ToolAngle UpFwd = PickaxeSpriteCatalog.ToolAngle.UpForward;
        static readonly PickaxeSpriteCatalog.ToolAngle Up = PickaxeSpriteCatalog.ToolAngle.Up;

        // 아래 / 위 / 옆 — 좌우는 같은 자세를 뒤집어 쓴다.
        enum PoseGroup { Down = 0, Up = 1, Side = 2 }

        // 땅을 팔 때: 집게를 치켜들었다가 내리치고, 닿는 순간 발을 다문다.
        static readonly Pose[][] DigPoses =
        {
            new[] { new Pose(12,  9, Down, true),  new Pose(12, 11, Down, false),
                    new Pose(12, 12, Down, false), new Pose(12, 10, Down, true) },
            new[] { new Pose(12,  9, Up, true),    new Pose(12,  9, Up, false),
                    new Pose(12,  8, Up, false),   new Pose(12,  8, Up, true) },
            new[] { new Pose(10,  8, Up, true),    new Pose(11,  8, UpFwd, true),
                    new Pose(11, 11, Fwd, false),  new Pose(11, 11, DownFwd, false) },
        };

        // 걸을 때: 손에 늘어뜨리고 걸음에 맞춰 1px 흔들린다.
        static readonly Pose[][] WalkPoses =
        {
            new[] { new Pose(12, 10, DownFwd, false), new Pose(12,  9, DownFwd, false),
                    new Pose(12, 10, DownFwd, false), new Pose(12,  9, DownFwd, false) },
            new[] { new Pose(12, 10, DownFwd, false), new Pose(12,  9, DownFwd, false),
                    new Pose(12, 10, DownFwd, false), new Pose(12,  9, DownFwd, false) },
            new[] { new Pose(10, 11, DownFwd, false), new Pose(10, 10, DownFwd, false),
                    new Pose(10, 11, DownFwd, false), new Pose(10, 10, DownFwd, false) },
        };

        SpriteRenderer _renderer;
        int _tier = 1;

        /// <summary>플레이어 밑에 집게 렌더러를 붙인다.</summary>
        public static PlayerToolVisual Attach(Transform owner)
        {
            var go = new GameObject("Tongs");
            go.transform.SetParent(owner, false);
            var visual = go.AddComponent<PlayerToolVisual>();
            visual._renderer = go.AddComponent<SpriteRenderer>();
            visual._renderer.sortingOrder = WorldView.PlayerSortingOrder + 1;
            return visual;
        }

        /// <summary>집게 강화 단계. 바뀌면 다음 프레임부터 새 집게가 보인다.</summary>
        public void SetTier(int tier) =>
            _tier = Mathf.Clamp(tier, 1, PickaxeSpriteCatalog.TierCount);

        public void Apply(PlayerVisualState state, Facing facing, int frame, bool flipX)
        {
            if (_renderer == null) return;

            PoseGroup group =
                facing == Facing.Up ? PoseGroup.Up :
                facing == Facing.Down ? PoseGroup.Down : PoseGroup.Side;

            Pose[] table = (state == PlayerVisualState.Dig ? DigPoses : WalkPoses)[(int)group];
            Pose pose = table[((frame % table.Length) + table.Length) % table.Length];

            _renderer.sprite = PickaxeSpriteCatalog.For(_tier, pose.Angle, pose.JawsOpen);
            _renderer.enabled = _renderer.sprite != null;
            _renderer.flipX = flipX;

            // 위를 보면 등을 돌린 상태다. 집게를 몸 뒤로 보내야 손이 앞에 있는 것처럼 보인다.
            _renderer.sortingOrder = WorldView.PlayerSortingOrder + (group == PoseGroup.Up ? -1 : 1);

            float dx = (pose.GripX + 0.5f) - Center;
            float dy = Center - (pose.GripY + 0.5f);
            if (flipX) dx = -dx;
            transform.localPosition = new Vector3(dx / FramePixels, dy / FramePixels, 0f);
        }
    }
}
