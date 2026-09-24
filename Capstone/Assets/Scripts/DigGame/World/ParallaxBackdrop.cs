using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 지평선 위로 흐르는 원경 배경. 시대마다 통째로 갈린다.
    ///  - 먼 층(산·거대 구조물)은 카메라를 거의 따라와 아주 느리게 흐르고
    ///  - 가까운 층(집·나무)은 조금 더 빠르게 흘러 깊이가 생긴다.
    /// 두 층 다 가로로 무한히 이어지도록 같은 그림을 여러 장 이어 붙이고,
    /// 카메라가 한 장 폭만큼 움직일 때마다 통째로 감아서 되돌린다.
    /// </summary>
    [DisallowMultipleComponent]
    public class ParallaxBackdrop : MonoBehaviour
    {
        const int OrderFar = -900;      // 하늘(-1000) 위, 땅(0) 아래
        const int OrderNear = -880;
        const int Copies = 5;           // 카메라 좌우로 두 장씩 여유

        /// <summary>1 에 가까울수록 멀다 (카메라에 붙어 거의 안 흐른다).</summary>
        const float FarFollow = 0.90f;
        const float NearFollow = 0.74f;

        const float FarScale = 1.75f;
        const float NearScale = 1.30f;

        class Layer
        {
            public Transform Root;
            public SpriteRenderer[] Tiles;
            public float Follow;
            public float Width;
        }

        Camera _camera;
        GameManager _game;
        Layer _far;
        Layer _near;
        int _era = -1;

        public void Setup(Camera camera, GameManager game)
        {
            _camera = camera;
            _game = game;

            _far = BuildLayer("Backdrop Far", FarFollow, FarScale, OrderFar);
            _near = BuildLayer("Backdrop Near", NearFollow, NearScale, OrderNear);

            ApplyEra(game != null && game.Village != null ? game.Village.Era : 0);
        }

        Layer BuildLayer(string name, float follow, float scale, int order)
        {
            // 카메라의 자식으로 두면 카메라 이동이 두 번 먹으므로 씬 루트에 둔다.
            var root = new GameObject(name).transform;
            root.localScale = Vector3.one * scale;

            var layer = new Layer { Root = root, Follow = follow, Tiles = new SpriteRenderer[Copies] };
            for (int i = 0; i < Copies; i++)
            {
                var go = new GameObject($"Tile{i}");
                go.transform.SetParent(root, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = order;
                layer.Tiles[i] = sr;
            }
            return layer;
        }

        /// <summary>시대가 바뀌면 원경도 통째로 갈아끼운다.</summary>
        public void ApplyEra(int era)
        {
            if (era == _era) return;
            _era = era;

            SetLayerSprite(_far, VillageSpriteCatalog.EraFar(era), FarScale);
            SetLayerSprite(_near, VillageSpriteCatalog.EraNear(era), NearScale);
        }

        static void SetLayerSprite(Layer layer, Sprite sprite, float scale)
        {
            if (layer == null) return;

            float width = sprite != null ? sprite.bounds.size.x * scale : 0f;
            layer.Width = Mathf.Max(0.01f, width);

            for (int i = 0; i < layer.Tiles.Length; i++)
            {
                if (layer.Tiles[i] == null) continue;
                layer.Tiles[i].sprite = sprite;
                // 스프라이트가 bottom-center 피벗이므로 x 만 한 장 폭씩 벌려 둔다.
                float step = sprite != null ? sprite.bounds.size.x : 0f;
                layer.Tiles[i].transform.localPosition =
                    new Vector3((i - layer.Tiles.Length / 2) * step, 0f, 0f);
            }
        }

        void LateUpdate()
        {
            if (_camera == null || _game == null) return;

            if (_game.Village != null) ApplyEra(_game.Village.Era);

            float camX = _camera.transform.position.x;
            float horizon = -GameConfig.SurfaceRow * GameConfig.CellSize;

            // 밤이 되면 원경도 같이 어두워진다.
            var sky = _camera.GetComponent<SkyCycle>();
            float night = sky != null ? sky.NightAmount : 0f;
            Color tint = Color.Lerp(Color.white, new Color(0.34f, 0.36f, 0.52f, 1f), night);

            PlaceLayer(_far, camX, horizon, tint);
            PlaceLayer(_near, camX, horizon, tint);
        }

        static void PlaceLayer(Layer layer, float camX, float horizon, Color tint)
        {
            if (layer == null || layer.Root == null) return;

            float anchored = camX * layer.Follow;                  // 이 층이 있어야 할 자리
            float drift = camX - anchored;                         // 카메라가 앞서간 거리
            float wrapped = anchored + Mathf.Floor(drift / layer.Width + 0.5f) * layer.Width;

            layer.Root.position = new Vector3(wrapped, horizon, 0f);

            for (int i = 0; i < layer.Tiles.Length; i++)
                if (layer.Tiles[i] != null) layer.Tiles[i].color = tint;
        }

        void OnDestroy()
        {
            if (_far != null && _far.Root != null) Destroy(_far.Root.gameObject);
            if (_near != null && _near.Root != null) Destroy(_near.Root.gameObject);
        }
    }
}
