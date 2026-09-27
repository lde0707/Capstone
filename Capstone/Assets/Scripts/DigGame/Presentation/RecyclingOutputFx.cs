using UnityEngine;

namespace DigGame
{
    /// <summary>공장에서 나온 재활용 결과물 아이콘을 위로 띄운 뒤 사라지게 한다.</summary>
    public class RecyclingOutputFx : PooledEffect
    {
        SpriteRenderer _renderer;
        [SerializeField, Min(0.01f)] float _duration = 1.8f;
        [SerializeField, Min(0f)] float _riseSpeed = 0.7f;
        [SerializeField, Min(0.01f)] float _fadeSeconds = 0.6f;
        float _life;

        void OnEnable()
        {
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            if (_renderer != null)
            { var color = _renderer.color; color.a = 1f; _renderer.color = color; }
            _life = _duration;
        }

        void Update()
        {
            transform.position += Vector3.up * (_riseSpeed * Time.deltaTime);
            _life -= Time.deltaTime;
            if (_renderer != null)
            {
                Color color = _renderer.color;
                color.a = Mathf.Clamp01(_life / Mathf.Max(0.01f, _fadeSeconds));
                _renderer.color = color;
            }
            if (_life <= 0f) Release();
        }
    }
}
