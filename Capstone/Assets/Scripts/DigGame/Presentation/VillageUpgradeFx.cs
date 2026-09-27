using UnityEngine;
using System.Collections.Generic;

namespace DigGame
{
    /// <summary>집이 업그레이드될 때 터지는 빛기둥과 반짝이.</summary>
    public class VillageUpgradeFx : PooledEffect
    {
        [SerializeField] SpriteRenderer _beam;
        [SerializeField] SpriteRenderer[] _sparkRenderers;
        float _life;
        [SerializeField, Min(0.01f)] float _duration = 1.3f;
        float _width;
        readonly List<SpriteRenderer> _sparks = new List<SpriteRenderer>();
        readonly List<Vector3> _velocities = new List<Vector3>();

        public static void Play(IGameObjectFactory factory, Transform parent, Vector3 groundPosition, float width)
        {
            var fx = factory.Create<VillageUpgradeFx>(PrefabId.UpgradeEffect, parent, "UpgradeFx");
            fx.transform.position = groundPosition;
            fx._width = width;
            fx.Setup();
        }

        void Setup()
        {
            _life = _duration;
            _sparks.Clear();
            _velocities.Clear();

            if (_beam == null || _sparkRenderers == null)
                throw new System.InvalidOperationException("UpgradeEffect 프리팹 연결이 누락되었습니다.");
            var beamGo = _beam.gameObject;
            beamGo.transform.localPosition = new Vector3(0f, 6f, 0f);
            _beam.sprite = SpriteFactory.Solid(Color.white);
            _beam.color = new Color(1f, 0.96f, 0.72f, 0.55f);
            _beam.sortingOrder = Village.OrderFx;
            beamGo.transform.localScale = new Vector3(_width, 12f, 1f);

            for (int i = 0; i < _sparkRenderers.Length; i++)
            {
                var s = _sparkRenderers[i].gameObject;
                s.transform.localPosition = new Vector3(
                    Random.Range(-_width * 0.5f, _width * 0.5f), Random.Range(0f, 1.2f), 0f);
                s.transform.localScale = Vector3.one * Random.Range(0.12f, 0.3f);
                var sr = _sparkRenderers[i];
                sr.sprite = SpriteFactory.Diamond(Color.white);
                sr.color = new Color(1f, 0.94f, 0.66f, 1f);
                sr.sortingOrder = Village.OrderFx + 1;
                _sparks.Add(sr);
                _velocities.Add(new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1.8f, 4.2f), 0f));
            }
        }

        void Update()
        {
            _life -= Time.deltaTime;
            float k = Mathf.Clamp01(_life / _duration);

            if (_beam != null)
            {
                float pulse = Mathf.Sin((1f - k) * Mathf.PI);
                _beam.color = new Color(1f, 0.96f, 0.72f, 0.62f * pulse);
                _beam.transform.localScale = new Vector3(
                    _width * (0.55f + 0.85f * pulse), 12f + 6f * (1f - k), 1f);
            }

            for (int i = 0; i < _sparks.Count; i++)
            {
                var sr = _sparks[i];
                if (sr == null) continue;
                sr.transform.localPosition += _velocities[i] * Time.deltaTime;
                var c = sr.color;
                c.a = k;
                sr.color = c;
            }

            if (_life <= 0f) Release();
        }
    }
}
