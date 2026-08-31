using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 지상을 어슬렁거리는 마을 주민. 상호작용은 없고, 걷다가 가끔 멈춰 선다.
    /// (지금은 분위기 담당 — 마을이 살아 있다는 신호)
    /// </summary>
    public class Villager : MonoBehaviour
    {
        const float WalkSpeed = 1.15f;
        const float FramesPerSecond = 7f;

        Village _village;
        SpriteRenderer _sr;
        Sprite[] _frames;

        float _dir = 1f;
        float _stateTimer;
        bool _walking = true;
        float _animTime;

        public void Init(Village village, int kind, Vector3 groundPosition)
        {
            _village = village;
            _frames = VillageSpriteCatalog.Villager(kind);

            transform.position = new Vector3(
                groundPosition.x,
                groundPosition.y + GameConfig.PlayerScale * 0.45f,
                0f);
            transform.localScale = Vector3.one * (GameConfig.PlayerScale * 1.12f);

            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = _frames.Length > 0 ? _frames[0] : null;
            _sr.sortingOrder = Village.OrderVillager;

            _dir = Random.value < 0.5f ? -1f : 1f;
            _stateTimer = Random.Range(1.5f, 5f);
            _animTime = Random.value * 2f;
        }

        void Update()
        {
            if (_sr == null || _frames == null || _frames.Length == 0) return;

            float dt = Time.deltaTime;
            _stateTimer -= dt;
            if (_stateTimer <= 0f)
            {
                _walking = !_walking || Random.value < 0.65f;
                if (_walking && Random.value < 0.45f) _dir = -_dir;
                _stateTimer = _walking ? Random.Range(2f, 6f) : Random.Range(0.8f, 2.5f);
            }

            if (_walking)
            {
                _village.WanderRange(out float minX, out float maxX);
                var p = transform.position;
                p.x += _dir * WalkSpeed * dt;

                if (p.x < minX) { p.x = minX; _dir = 1f; }
                else if (p.x > maxX) { p.x = maxX; _dir = -1f; }

                transform.position = p;
                _animTime += dt;
                _sr.sprite = _frames[Mathf.FloorToInt(_animTime * FramesPerSecond) % _frames.Length];
                _sr.flipX = _dir < 0f;
            }
            else
            {
                _sr.sprite = _frames[0];
            }
        }
    }
}
