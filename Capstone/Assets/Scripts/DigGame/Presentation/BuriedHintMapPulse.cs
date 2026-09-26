using UnityEngine;
using UnityEngine.Tilemaps;

namespace DigGame
{
    /// <summary>수천 개의 개별 Update 없이 묻힌 쓰레기 힌트 타일맵 전체를 맥동시킨다.</summary>
    public class BuriedHintMapPulse : MonoBehaviour
    {
        Tilemap _tilemap;
        float _phase;

        void Start()
        {
            _tilemap = GetComponent<Tilemap>();
            _phase = Random.value * 6.28f;
        }

        void Update()
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 3.4f + _phase);
            if (_tilemap != null)
                _tilemap.color = new Color(1f, 1f, 1f, 0.55f + pulse * 0.40f);
        }
    }
}
