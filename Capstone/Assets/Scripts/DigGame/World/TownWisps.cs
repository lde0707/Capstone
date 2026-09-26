using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 요괴 마을 세트를 완성하면 생기는 도깨비불.
    /// 마을 위를 천천히 떠다니고, <b>밤에만</b> 또렷하게 보인다.
    /// </summary>
    public class TownWisps : MonoBehaviour
    {
        class Wisp
        {
            public SpriteRenderer Sr;
            public Vector3 Home;
            public float Phase;
            public float SpeedX;
            public float SpeedY;
            public float Radius;
            public Color Tint;
        }

        static readonly Color[] Palette =
        {
            new Color(0.58f, 0.95f, 0.78f, 1f),   // 청록 도깨비불
            new Color(0.70f, 0.78f, 1.00f, 1f),   // 푸른빛
            new Color(0.98f, 0.82f, 0.56f, 1f),   // 누런 등불
        };

        readonly List<Wisp> _wisps = new List<Wisp>();
        Village _village;
        SkyCycle _sky;
        float _time;

        public void Setup(Village village, Transform parent, int count, IGameObjectFactory factory)
        {
            _village = village;
            transform.SetParent(parent, false);
            _sky = Camera.main != null ? Camera.main.GetComponent<SkyCycle>() : null;

            village.WanderRange(out float minX, out float maxX);
            float surfaceY = -GameConfig.SurfaceRow * GameConfig.CellSize;

            for (int i = 0; i < count; i++)
            {
                var go = factory.Create<SpriteRenderer>(PrefabId.Wisp, transform, $"Wisp#{i}").gameObject;

                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = SpriteFactory.Diamond(Color.white);
                sr.sortingOrder = Village.OrderVillager + 3;

                float t = count <= 1 ? 0.5f : i / (float)(count - 1);
                var w = new Wisp
                {
                    Sr = sr,
                    Home = new Vector3(Mathf.Lerp(minX, maxX, t),
                        surfaceY + Random.Range(1.2f, 4.6f) * GameConfig.CellSize, 0f),
                    Phase = Random.value * Mathf.PI * 2f,
                    SpeedX = Random.Range(0.18f, 0.42f),
                    SpeedY = Random.Range(0.35f, 0.75f),
                    Radius = Random.Range(1.4f, 3.4f),
                    Tint = Palette[i % Palette.Length]
                };
                go.transform.localScale = Vector3.one * Random.Range(0.22f, 0.40f);
                _wisps.Add(w);
            }
        }

        void Update()
        {
            if (_village == null || _wisps.Count == 0) return;

            _time += Time.deltaTime;

            // 낮에는 거의 보이지 않고, 밤이 깊을수록 또렷해진다.
            if (_sky == null && Camera.main != null) _sky = Camera.main.GetComponent<SkyCycle>();
            float night = _sky != null ? _sky.NightAmount : 1f;
            float visible = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((night - 0.15f) / 0.6f));

            // 마을이 넓어지면 도깨비불도 따라 퍼진다.
            _village.WanderRange(out float minX, out float maxX);

            for (int i = 0; i < _wisps.Count; i++)
            {
                var w = _wisps[i];
                if (w.Sr == null) continue;

                float t = _wisps.Count <= 1 ? 0.5f : i / (float)(_wisps.Count - 1);
                float baseX = Mathf.Lerp(minX, maxX, t);

                float px = baseX + Mathf.Sin(_time * w.SpeedX + w.Phase) * w.Radius * GameConfig.CellSize;
                float py = w.Home.y + Mathf.Sin(_time * w.SpeedY + w.Phase * 1.7f) * 0.55f * GameConfig.CellSize;
                w.Sr.transform.position = new Vector3(px, py, 0f);

                // 각자 다른 속도로 깜빡인다 — 완전히 꺼지지는 않는다.
                float flicker = 0.55f + 0.45f * Mathf.Sin(_time * (1.4f + i * 0.13f) + w.Phase);
                var c = w.Tint;
                c.a = visible * (0.35f + 0.55f * flicker);
                w.Sr.color = c;
                w.Sr.enabled = c.a > 0.02f;
            }
        }
    }
}
