using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 복원되어 마을에 나타난 유물.
    ///  - 전시형은 좌대에 선 채 4프레임으로 숨만 쉰다
    ///  - 배회형은 주민처럼 마을을 어슬렁거린다 (공중형은 지붕 위를 떠다닌다)
    /// </summary>
    public class RelicUnit : SpriteActor
    {
        const float WalkSpeed = 0.85f;
        const float FlySpeed = 1.35f;
        const float FramesPerSecond = 6f;

        static float FootReach => GameConfig.CellSize * 0.6f;

        /// <summary>
        /// 유물마다 그림 높이가 제각각이라, 칸 높이로 이름표를 띄우면
        /// 작은 유물은 머리 위 한참 허공에 뜬다. 스프라이트에서 실제 머리끝을 재 둔다.
        /// </summary>
        static readonly Dictionary<int, float> HeadHeights = new Dictionary<int, float>();
        /// <summary>읽기 설정 경고는 유물 수 x 프레임 수만큼 쏟아지므로 한 번만 남긴다.</summary>
        static bool _warnedUnreadable;

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
        float _headHeight;

        public RelicDef Def => _def;
        public bool Wandering => _wander;
        /// <summary>공중을 떠다니는가 (유물 자체가 공중형이거나, 세트 보너스로 띄웠거나).</summary>
        public bool Flying => _forceFly || (_def != null && _def.Flying);
        public string DisplayName => _def != null ? _def.Name : "";

        /// <summary>그림이 실제로 나오고 있는가 — 시트를 못 읽으면 이름만 허공에 남는다.</summary>
        public bool HasArt => _sr != null && _sr.sprite != null;

        /// <summary>이름표를 찍을 높이 — 머리끝 바로 위 (월드 좌표).</summary>
        public float LabelWorldY => transform.position.y + _headHeight + GameConfig.CellSize * 0.18f;

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

            _sr = ActorRenderer;
            _sr.sprite = _frames.Length > 0 ? _frames[0] : null;
            // 배회형은 주민과 같은 층, 전시형은 건물보다 살짝 앞.
            _sr.sortingOrder = wander ? Village.OrderVillager - 1 : Village.OrderProp + 1;

            _headHeight = HeadHeight(def.Id, _frames) * transform.localScale.y;

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

        /// <summary>
        /// 스프라이트에서 투명하지 않은 맨 윗줄을 찾아 발밑부터의 높이를 잰다.
        /// 피벗이 바닥 가운데라 이 값이 곧 머리끝 높이다. 유물 한 종마다 한 번만 잰다.
        /// </summary>
        static float HeadHeight(int relicId, Sprite[] frames)
        {
            if (frames == null || frames.Length == 0) return 0f;
            if (HeadHeights.TryGetValue(relicId, out float cached)) return cached;

            // 숨 쉬는 4프레임은 높이가 제각각이다. 가장 높은 프레임에 맞춰야
            // 이름표가 초당 여섯 번 머리에 부딪히지 않는다.
            float height = 0f;
            for (int i = 0; i < frames.Length; i++)
                height = Mathf.Max(height, FrameHeight(frames[i]));

            HeadHeights[relicId] = height;
            return height;
        }

        /// <summary>한 프레임의 머리끝 높이 (월드, 배율 적용 전).</summary>
        static float FrameHeight(Sprite sprite)
        {
            if (sprite == null) return 0f;

            float height = sprite.bounds.size.y;     // 못 읽으면 칸 전체로 물러선다
            var texture = sprite.texture;
            if (texture == null || !texture.isReadable)
            {
                // 여기로 빠지면 작은 유물의 이름표가 허공에 뜬다 — 조용히 넘기지 않는다.
                if (!_warnedUnreadable)
                {
                    _warnedUnreadable = true;
                    Debug.LogWarning("[DigGame] 유물 시트 Read/Write 설정을 확인하세요 — " +
                        "이름표가 머리 위로 너무 높이 뜹니다.");
                }
            }
            else
            {
                var area = sprite.rect;
                int w = Mathf.Max(1, (int)area.width);
                int h = Mathf.Max(1, (int)area.height);
                var pixels = texture.GetPixels((int)area.x, (int)area.y, w, h);
                if (pixels != null && pixels.Length >= w * h)
                {
                    // GetPixels 는 아래에서 위로 읽는다 → 위에서부터 훑어 내려온다
                    for (int y = h - 1; y >= 0; y--)
                    {
                        bool solid = false;
                        for (int x = 0; x < w; x++)
                            if (pixels[y * w + x].a > 0.02f) { solid = true; break; }
                        if (!solid) continue;
                        height = (y + 1) / Mathf.Max(0.01f, sprite.pixelsPerUnit);
                        break;
                    }
                }
            }

            return height;
        }

        /// <summary>시대 도약 연출 — 잠깐 하얗게 탄다.</summary>
        public void Flash(Color color)
        {
            if (_sr != null) _sr.color = color;
        }
    }
}
