using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 복원되어 마을에 나타난 유물.
    ///  - 전시형은 좌대에 선 채 4프레임으로 숨만 쉰다
    ///  - 배회형은 주민처럼 마을을 어슬렁거린다 (공중형은 지붕 위를 떠다닌다)
    /// </summary>
    public class RelicUnit : MonoBehaviour
    {
        const float WalkSpeed = 0.85f;
        const float FlySpeed = 1.35f;
        const float FramesPerSecond = 6f;

        static float FootReach => GameConfig.CellSize * 0.6f;

        Village _village;
        SpriteRenderer _sr;
        Sprite[] _frames;
        RelicDef _def;
        bool _wander;
        bool _forceFly;

        float _dir = 1f;
        float _stateTimer;
        bool _walking = true;
        float _animTime;
        float _baseY;
        float _floatPhase;

        public RelicDef Def => _def;
        public bool Wandering => _wander;
        /// <summary>공중을 떠다니는가 (유물 자체가 공중형이거나, 세트 보너스로 띄웠거나).</summary>
        public bool Flying => _forceFly || (_def != null && _def.Flying);
        public string DisplayName => _def != null ? _def.Name : "";

        /// <summary>
        /// forceFly 는 세트 보너스로 마을 위에 상주시키는 경우에 쓴다
        /// (전시형 UFO 를 하늘에 띄우는 식).
        /// </summary>
        public void Init(Village village, RelicDef def, Vector3 groundPosition, bool wander,
            bool forceFly = false, float scale = 0f)
        {
            _village = village;
            _def = def;
            _wander = wander;
            _forceFly = forceFly;
            _frames = RelicSpriteCatalog.Alive(def);

            float lift = Flying && wander ? GameConfig.CellSize * (forceFly ? 5.4f : 3.2f) : 0f;
            transform.position = new Vector3(groundPosition.x, groundPosition.y + lift, 0f);
            transform.localScale = Vector3.one * (scale > 0f ? scale : GameConfig.RelicAliveScale);
            _baseY = transform.position.y;

            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = _frames.Length > 0 ? _frames[0] : null;
            // 배회형은 주민과 같은 층, 전시형은 건물보다 살짝 앞.
            _sr.sortingOrder = wander ? Village.OrderVillager - 1 : Village.OrderProp + 1;

            _dir = Random.value < 0.5f ? -1f : 1f;
            _stateTimer = Random.Range(1.5f, 5f);
            _animTime = Random.value * 2f;
            _floatPhase = Random.value * Mathf.PI * 2f;
        }

        void Update()
        {
            if (_sr == null || _frames == null || _frames.Length == 0) return;

            float dt = Time.deltaTime;
            _animTime += dt;
            _sr.sprite = _frames[Mathf.FloorToInt(_animTime * FramesPerSecond) % _frames.Length];

            if (!_wander) return;

            bool flying = Flying;

            _stateTimer -= dt;
            if (_stateTimer <= 0f)
            {
                _walking = flying || !_walking || Random.value < 0.7f;
                if (_walking && Random.value < 0.4f) _dir = -_dir;
                _stateTimer = _walking ? Random.Range(2.5f, 7f) : Random.Range(0.8f, 2.2f);
            }

            var p = transform.position;

            if (flying)
            {
                // 공중형은 땅 상태를 무시하고 마을 위를 오간다.
                _village.WanderRange(out float fMinX, out float fMaxX);
                float nx = p.x + _dir * FlySpeed * dt;
                if (nx < fMinX) { nx = fMinX; _dir = 1f; }
                else if (nx > fMaxX) { nx = fMaxX; _dir = -1f; }
                _floatPhase += dt * 1.6f;
                transform.position = new Vector3(nx, _baseY + Mathf.Sin(_floatPhase) * 0.35f, 0f);
                _sr.flipX = _dir < 0f;
                return;
            }

            if (!_walking) return;

            _village.WanderRange(out float minX, out float maxX);
            float nextX = p.x + _dir * WalkSpeed * dt;
            if (nextX < minX) { nextX = minX; _dir = 1f; }
            else if (nextX > maxX) { nextX = maxX; _dir = -1f; }

            float toe = nextX + _dir * FootReach;
            if (!_village.CanWalkTo(toe))
            {
                _dir = -_dir;
                _stateTimer = Random.Range(0.6f, 1.8f);
            }
            else
            {
                p.x = nextX;
                transform.position = p;
            }
            _sr.flipX = _dir < 0f;
        }

        /// <summary>시대 도약 연출 — 잠깐 하얗게 탄다.</summary>
        public void Flash(Color color)
        {
            if (_sr != null) _sr.color = color;
        }
    }
}
