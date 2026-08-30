using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 카메라 화면 전체를 채우는 실제 월드 하늘.
    /// 낮 진행률에 따라 해가 이동하고 하늘이 일출에서 밤까지 연속으로 변한다.
    /// </summary>
    [DisallowMultipleComponent]
    public class SkyCycle : MonoBehaviour
    {
        const int BackgroundSortingOrder = -1000;

        Camera _camera;
        GameManager _game;
        GameObject _backdrop;
        SpriteRenderer _renderer;
        Material _material;

        public void Setup(Camera camera, GameManager game)
        {
            _camera = camera;
            _game = game;

            var shader = Resources.Load<Shader>("SkyCycle");
            if (shader == null)
            {
                Debug.LogError("[DigGame] SkyCycle 셰이더를 찾지 못했습니다.");
                return;
            }

            _material = new Material(shader)
            {
                name = "Sky Cycle (Runtime)",
                hideFlags = HideFlags.DontSave
            };

            _backdrop = new GameObject("Sky Backdrop");
            _backdrop.transform.SetParent(camera.transform, false);
            _backdrop.transform.localPosition = new Vector3(0f, 0f, 50f);

            _renderer = _backdrop.AddComponent<SpriteRenderer>();
            _renderer.sprite = SpriteFactory.Solid(Color.white);
            _renderer.sharedMaterial = _material;
            _renderer.sortingOrder = BackgroundSortingOrder;

            Refresh();
        }

        void LateUpdate()
        {
            Refresh();
        }

        void Refresh()
        {
            if (_camera == null || _game == null || _material == null || _backdrop == null)
                return;

            float height = _camera.orthographicSize * 2f;
            float width = height * Mathf.Max(0.01f, _camera.aspect);
            _backdrop.transform.localScale = new Vector3(width * 1.04f, height * 1.04f, 1f);

            float progress = 1f - Mathf.Clamp01(_game.TimeLeft / GameConfig.DayLengthSeconds);
            EvaluateSky(progress, out var top, out var bottom, out var sun, out float nightAmount);

            float sunX = Mathf.Lerp(0.08f, 0.92f, progress);
            float sunY = 0.50f + Mathf.Sin(progress * Mathf.PI) * 0.38f;
            float sunVisibility = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.86f, 1f, progress));

            _material.SetColor("_TopColor", top);
            _material.SetColor("_BottomColor", bottom);
            _material.SetColor("_SunColor", new Color(sun.r, sun.g, sun.b, sunVisibility));
            _material.SetVector("_SunPosition", new Vector4(sunX, sunY, 0f, 0f));
            _material.SetFloat("_Aspect", _camera.aspect);
            _material.SetFloat("_NightAmount", nightAmount);

            // 셰이더가 보이지 않는 상황에서도 빈 화면이 생기지 않게 같은 계열색으로 지운다.
            _camera.backgroundColor = bottom;
        }

        static void EvaluateSky(float progress, out Color top, out Color bottom,
            out Color sun, out float nightAmount)
        {
            progress = Mathf.Clamp01(progress);

            var dawnTop = new Color(0.18f, 0.10f, 0.34f, 1f);
            var dawnBottom = new Color(0.90f, 0.38f, 0.18f, 1f);
            var noonTop = new Color(0.08f, 0.34f, 0.76f, 1f);
            var noonBottom = new Color(0.48f, 0.76f, 0.96f, 1f);
            var sunsetTop = new Color(0.25f, 0.07f, 0.34f, 1f);
            var sunsetBottom = new Color(0.96f, 0.20f, 0.07f, 1f);
            var nightTop = new Color(0.015f, 0.018f, 0.075f, 1f);
            var nightBottom = new Color(0.055f, 0.045f, 0.12f, 1f);

            var sunriseSun = new Color(1f, 0.42f, 0.12f, 1f);
            var noonSun = new Color(1f, 0.95f, 0.58f, 1f);
            var sunsetSun = new Color(1f, 0.18f, 0.05f, 1f);

            if (progress < 0.52f)
            {
                float t = Mathf.SmoothStep(0f, 1f, progress / 0.52f);
                top = Color.Lerp(dawnTop, noonTop, t);
                bottom = Color.Lerp(dawnBottom, noonBottom, t);
                sun = Color.Lerp(sunriseSun, noonSun, t);
            }
            else if (progress < 0.82f)
            {
                float t = Mathf.SmoothStep(0f, 1f, (progress - 0.52f) / 0.30f);
                top = Color.Lerp(noonTop, sunsetTop, t);
                bottom = Color.Lerp(noonBottom, sunsetBottom, t);
                sun = Color.Lerp(noonSun, sunsetSun, t);
            }
            else
            {
                float t = Mathf.SmoothStep(0f, 1f, (progress - 0.82f) / 0.18f);
                top = Color.Lerp(sunsetTop, nightTop, t);
                bottom = Color.Lerp(sunsetBottom, nightBottom, t);
                sun = sunsetSun;
            }

            nightAmount = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.78f, 1f, progress));
        }

        void OnDestroy()
        {
            DestroyGenerated(_backdrop);
            DestroyGenerated(_material);
        }

        static void DestroyGenerated(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }
    }
}
