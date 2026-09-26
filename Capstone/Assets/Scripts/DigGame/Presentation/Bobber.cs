using UnityEngine;

namespace DigGame
{
    /// <summary>위아래로 살짝 떠다니는 연출.</summary>
    public class Bobber : MonoBehaviour
    {
        [SerializeField, Min(0f)] float _amplitude = 0.07f;
        [SerializeField, Min(0f)] float _frequency = 3f;
        Vector3 _base;
        float _phase;

        void Start()
        {
            _base = transform.position;
            _phase = Random.value * 6.28f;
        }

        void Update()
        {
            transform.position = _base + new Vector3(0f, Mathf.Sin(Time.time * _frequency + _phase) * _amplitude, 0f);
        }
    }
}
