using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 지상을 어슬렁거리는 마을 주민. 상호작용은 없고, 걷다가 가끔 멈춰 선다.
    /// (지금은 분위기 담당 — 마을이 살아 있다는 신호)
    /// </summary>
    public class Villager : SpriteActor
    {
        const float WalkSpeed = 1.15f;
        const float FramesPerSecond = 7f;
        /// <summary>발끝이 몸 중심에서 앞으로 나가는 거리 — 이만큼 앞이 구덩이면 돌아선다.</summary>
        static float FootReach => GameConfig.CellSize * 0.55f;

        Village _village;
        SpriteRenderer _sr;
        Sprite[] _frames;
        int _kind;
        int _era;

        float _dir = 1f;
        float _stateTimer;
        bool _walking = true;
        float _animTime;

        /// <summary>이 주민이 어느 시대 옷을 입고 있는지.</summary>
        public int Era => _era;
        /// <summary>주민 종류 (시대 안에서의 번호).</summary>
        public int Kind => _kind;
        public string DisplayName => VillageSpriteCatalog.EraVillagerName(_era, _kind);

        public void Init(Village village, int kind, int era, Vector3 groundPosition)
        {
            _village = village;
            _kind = kind;
            _era = era;
            _frames = VillageSpriteCatalog.EraVillager(era, kind);

            transform.position = new Vector3(
                groundPosition.x,
                groundPosition.y + GameConfig.PlayerScale * 0.45f,
                0f);

            _sr = ActorRenderer;
            _sr.sprite = _frames.Length > 0 ? _frames[0] : null;

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
                float nextX = p.x + _dir * WalkSpeed * dt;

                if (nextX < minX) { nextX = minX; _dir = 1f; }
                else if (nextX > maxX) { nextX = maxX; _dir = -1f; }

                // 지하 입구나 플레이어가 판 구덩이 앞에서는 돌아선다.
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

                _animTime += dt;
                _sr.sprite = _frames[Mathf.FloorToInt(_animTime * FramesPerSecond) % _frames.Length];
                _sr.flipX = _dir < 0f;
            }
            else
            {
                _sr.sprite = _frames[0];
            }
        }

        /// <summary>시대가 바뀌면 주민도 그 시대 옷으로 갈아입는다.</summary>
        public void SetEra(int era)
        {
            if (era == _era) return;
            _era = era;
            _frames = VillageSpriteCatalog.EraVillager(_era, _kind);
            if (_sr != null && _frames.Length > 0) _sr.sprite = _frames[0];
        }

        /// <summary>시대 도약 연출 — 잠깐 하얗게 탄다.</summary>
        public void Flash(Color color)
        {
            if (_sr != null) _sr.color = color;
        }
    }
}
