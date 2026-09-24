using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DigGame
{
    /// <summary>
    /// WASD 로 자유롭게 움직이며 마지막 입력 방향의 인접 한 칸을 조준하고 Space 로 판다.
    /// 이동은 중력 없이 입력에 즉시 반응하며, 땅과는 AABB 충돌로 막힌다.
    /// 지상에서는 화면 위로 더 올라갈 수 없고, 파낸 통로 안에서는 네 방향으로 자유 이동한다.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        /// <summary>충돌 상자 반너비. 한 칸보다 작아야 좁은 굴을 지나갈 수 있다.</summary>
        public const float HalfSize = GameConfig.PlayerScale * 0.45f;
        const float Eps = 0.001f;

        public PlayerStats Stats { get; private set; }
        public PlayerGear Gear { get; private set; }
        public List<LootItem> Inventory { get; private set; } = new List<LootItem>();

        /// <summary>플레이어 중심이 들어 있는 칸.</summary>
        public Vector2Int Cell { get; private set; }

        public bool IsDigging { get; private set; }
        public float DigProgress01 { get; private set; }
        public Vector2Int DigTarget { get; private set; }
        public string BlockedReason { get; private set; } = "";

        public Vector2Int HoverCell { get; private set; }
        public bool HoverValid { get; private set; }
        public DigFeature TargetDigFeature => HoverValid && _grid != null
            ? _grid.DigFeatureAt(HoverCell.x, HoverCell.y) : DigFeature.None;
        public int TargetDigSpeedBonusPercent
        {
            get
            {
                if (!HoverValid || _grid == null || Stats == null) return 0;
                TileKind kind = _grid.Get(HoverCell.x, HoverCell.y);
                if (!TileInfo.IsDiggable(kind)) return 0;

                float durationMultiplier = TileInfo.PowerDigDurationMultiplier(kind, Stats.Power);
                return Mathf.Max(0, Mathf.RoundToInt((1f / durationMultiplier - 1f) * 100f));
            }
        }

        GameManager _game;
        WorldGrid _grid;
        SpriteRenderer _sr;

        float _digAccum;
        float _digNeeded;
        float _blockedTimer;
        float _staminaTick;
        float _visualTime;
        Vector2 _moveInput;
        PlayerVisualState _visualState;
        Facing _facing = Facing.Down;
        Vector2Int _lastCell = new Vector2Int(int.MinValue, int.MinValue);
        Vector2Int _digDirection = new Vector2Int(0, 1);

        /// <summary>무중력 이동이므로 낙하 상태는 항상 꺼져 있다.</summary>
        public bool IsFalling => false;

        /// <summary>지면 위에 서 있을 때의 중심 y. 이보다 위로는 올라갈 수 없다.</summary>
        static float SurfaceStandY => -GameConfig.SurfaceRow * GameConfig.CellSize + HalfSize;

        public int InventoryValue
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < Inventory.Count; i++) sum += Inventory[i].Value;
                return sum;
            }
        }

        /// <summary>가방에 담긴 총 부피.</summary>
        public int InventoryVolume
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < Inventory.Count; i++) sum += Inventory[i].Volume;
                return sum;
            }
        }

        public int BagCapacity => Stats.BagVolume + Gear.BagBonus;
        public bool InventoryFull => InventoryVolume >= BagCapacity;

        bool CanCarry(LootItem item) => InventoryVolume + EffectiveVolume(item) <= BagCapacity;

        int EffectiveVolume(LootItem item) =>
            Gear.HasCompressor ? Mathf.Max(1, Mathf.CeilToInt(item.Volume * GameConfig.CompressorFactor))
                               : item.Volume;
        public int Depth => WorldGrid.DepthOf(Cell.y);
        public Zone CurrentZone => ZoneInfo.FromDepth(Mathf.Max(0, Depth));
        public bool AtSurface => transform.position.y > -GameConfig.SurfaceRow * GameConfig.CellSize + Eps;

        // ------------------------------------------------------------------
        public void Init(GameManager game, Vector2Int startCell)
        {
            _game = game;
            _grid = game.Grid;
            Stats = new PlayerStats();
            Gear = new PlayerGear();

            transform.position = new Vector3(
                (startCell.x + 0.5f) * GameConfig.CellSize, SurfaceStandY, 0f);
            Cell = WorldGrid.WorldToCell(transform.position);
            _lastCell = Cell;

            _sr = gameObject.AddComponent<SpriteRenderer>();
            var walkFrames = PlayerSpriteCatalog.Frames(PlayerVisualState.Walk, Facing.Down);
            _sr.sprite = walkFrames.Length > 0
                ? walkFrames[0]
                : SpriteFactory.Framed(new Color32(245, 245, 250, 255), new Color32(210, 70, 70, 255));
            _sr.sortingOrder = WorldView.PlayerSortingOrder;
            transform.localScale = Vector3.one * (GameConfig.PlayerScale * 1.18f);

            CancelDig();
        }

        void Update()
        {
            if (_game == null || !_game.IsRunning) return;
            float dt = Time.deltaTime;

            if (_blockedTimer > 0f)
            {
                _blockedTimer -= dt;
                if (_blockedTimer <= 0f) BlockedReason = "";
            }

            TickMovement(dt);
            RefreshCell();
            TickSurvival(dt);
            UpdateDigAim();
            TickKeyboardDig(dt);
            UpdateVisual(dt);
            PickUpNearby();
            TickGearKeys(dt);
        }

        // ---------------- 장비 키 (Q 탐지기 / B 귀환 벨 / F 드링크) ----------------
        void TickGearKeys(float dt)
        {
            if (Gear.DetectorCooldown > 0f) Gear.DetectorCooldown -= dt;

            var kb = Keyboard.current;
            if (kb == null || _game.ShopOpen) return;

            if (kb.qKey.wasPressedThisFrame && Gear.HasDetector)
            {
                if (Gear.DetectorCooldown > 0f)
                {
                    SetBlocked($"탐지기 재충전 중 ({Gear.DetectorCooldown:0.0}초)");
                }
                else
                {
                    Gear.DetectorCooldown = GameConfig.DetectorCooldown;
                    _game.View.PingTrash(Cell,
                        Mathf.RoundToInt(GameConfig.DetectorRadius), GameConfig.DetectorDuration);
                    _game.Log("탐지기 작동 — 주변 쓰레기가 잠시 보인다");
                }
            }

            if (kb.bKey.wasPressedThisFrame && Gear.HasReturnBell)
            {
                if (Gear.BellUsedToday) SetBlocked("귀환 벨은 하루에 한 번만 쓸 수 있다");
                else if (AtSurface) SetBlocked("이미 지상에 있다");
                else
                {
                    Gear.BellUsedToday = true;
                    transform.position = new Vector3(
                        (_game.SurfaceDropCell.x + 0.5f) * GameConfig.CellSize, SurfaceStandY, 0f);
                    RefreshCell();
                    _game.Log("귀환 벨 사용! 지상으로 순간이동했다");
                }
            }

            if (kb.fKey.wasPressedThisFrame && Gear.EnergyDrinks > 0)
            {
                if (Stats.Stamina >= Stats.MaxStamina - 1f)
                {
                    SetBlocked("기력이 이미 가득하다");
                }
                else
                {
                    Gear.EnergyDrinks--;
                    Stats.Stamina = Mathf.Min(Stats.MaxStamina,
                        Stats.Stamina + GameConfig.EnergyDrinkStamina);
                    _game.Log($"에너지 드링크! 기력 +{GameConfig.EnergyDrinkStamina:0} (남은 개수 {Gear.EnergyDrinks})");
                }
            }
        }

        void SetBlocked(string reason)
        {
            BlockedReason = reason;
            _blockedTimer = 1.8f;
        }

        // ---------------- 이동 (자유 이동 + AABB 충돌) ----------------
        void TickMovement(float dt)
        {
            var kb = Keyboard.current;
            if (kb == null) { _moveInput = Vector2.zero; return; }

            float ix = 0f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) ix -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) ix += 1f;
            float iy = 0f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) iy += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) iy -= 1f;
            _moveInput = new Vector2(ix, iy);

            float speed = Stats.MoveSpeed * GameConfig.CellSize;

            // 중력과 관성 없이 입력한 축으로 바로 움직인다.
            if (ix != 0f) MoveAxisX(ix * speed * dt);
            if (iy != 0f) MoveAxisY(iy * speed * dt);

            // 지면 위로는 올라갈 수 없다 (지상에서 W 무효)
            var p = transform.position;
            if (p.y > SurfaceStandY)
            {
                p.y = SurfaceStandY;
                transform.position = p;
            }
        }

        void UpdateVisual(float dt)
        {
            if (_sr == null) return;

            PlayerVisualState nextState = IsDigging ? PlayerVisualState.Dig : PlayerVisualState.Walk;
            Facing nextFacing = _facing;

            if (IsDigging)
            {
                // 그리드 y 는 아래로 증가한다.
                if (_digDirection.x > 0) nextFacing = Facing.Right;
                else if (_digDirection.x < 0) nextFacing = Facing.Left;
                else if (_digDirection.y > 0) nextFacing = Facing.Down;
                else nextFacing = Facing.Up;
            }
            else if (Mathf.Abs(_moveInput.x) > 0.01f)
            {
                nextFacing = _moveInput.x > 0f ? Facing.Right : Facing.Left;
            }
            else if (Mathf.Abs(_moveInput.y) > 0.01f)
            {
                nextFacing = _moveInput.y > 0f ? Facing.Up : Facing.Down;
            }

            if (nextState != _visualState || nextFacing != _facing)
            {
                _visualState = nextState;
                _facing = nextFacing;
                _visualTime = 0f;
            }
            else
            {
                _visualTime += dt;
            }

            Sprite[] frames = PlayerSpriteCatalog.Frames(_visualState, _facing);
            if (frames.Length > 0)
            {
                bool active = IsDigging || _moveInput.sqrMagnitude > 0.01f;
                float fps = _visualState == PlayerVisualState.Dig ? 11f : 8f;
                int frame = active ? Mathf.FloorToInt(_visualTime * fps) % frames.Length : 0;
                _sr.sprite = frames[frame];
            }

            _sr.flipX = PlayerSpriteCatalog.NeedsFlip(_facing);
        }

        void MoveAxisX(float dx)
        {
            if (dx == 0f) return;

            var p = transform.position;
            float oldX = p.x;
            p.x += dx;

            if (BoxBlocked(p))
            {
                if (dx > 0f)
                {
                    int col = Mathf.FloorToInt((p.x + HalfSize - Eps) / GameConfig.CellSize);
                    p.x = col * GameConfig.CellSize - HalfSize - Eps;
                }
                else
                {
                    int col = Mathf.FloorToInt((p.x - HalfSize + Eps) / GameConfig.CellSize);
                    p.x = (col + 1f) * GameConfig.CellSize + HalfSize + Eps;
                }
                if (BoxBlocked(p)) p.x = oldX;
            }
            transform.position = p;
        }

        /// <summary>한 프레임의 빠른 이동으로 벽을 뚫지 않도록 잘게 나눠 움직인다.</summary>
        void MoveAxisY(float dy)
        {
            if (dy == 0f) return;

            float maxStep = 0.25f * GameConfig.CellSize;
            float remaining = Mathf.Abs(dy);
            float sign = Mathf.Sign(dy);

            while (remaining > 0f)
            {
                float step = Mathf.Min(maxStep, remaining);
                remaining -= step;

                float before = transform.position.y;
                MoveAxisYStep(sign * step);
                if (Mathf.Abs(transform.position.y - before) < step - 1e-5f) return; // 막힘
            }
        }

        void MoveAxisYStep(float dy)
        {
            var p = transform.position;
            float oldY = p.y;
            p.y += dy;

            if (BoxBlocked(p))
            {
                if (dy > 0f)
                {
                    // 위로 가는 중: 위쪽 칸의 아래 경계에 붙인다
                    int row = Mathf.FloorToInt((-(p.y + HalfSize) + Eps) / GameConfig.CellSize);
                    p.y = -(row + 1f) * GameConfig.CellSize - HalfSize - Eps;
                }
                else
                {
                    int row = Mathf.FloorToInt((-(p.y - HalfSize) - Eps) / GameConfig.CellSize);
                    p.y = -row * GameConfig.CellSize + HalfSize + Eps;
                }
                if (BoxBlocked(p)) p.y = oldY;
            }
            transform.position = p;
        }

        /// <summary>플레이어 상자가 단단한 땅과 겹치는가.</summary>
        bool BoxBlocked(Vector3 p)
        {
            float minX = p.x - HalfSize + Eps;
            float maxX = p.x + HalfSize - Eps;
            float minY = p.y - HalfSize + Eps;
            float maxY = p.y + HalfSize - Eps;

            int c0 = Mathf.FloorToInt(minX / GameConfig.CellSize);
            int c1 = Mathf.FloorToInt(maxX / GameConfig.CellSize);
            int r0 = Mathf.FloorToInt(-maxY / GameConfig.CellSize);
            int r1 = Mathf.FloorToInt(-minY / GameConfig.CellSize);

            for (int r = r0; r <= r1; r++)
                for (int c = c0; c <= c1; c++)
                    if (!_grid.IsWalkable(c, r)) return true;

            return false;
        }

        void RefreshCell()
        {
            Cell = WorldGrid.WorldToCell(transform.position);
            if (Cell != _lastCell)
            {
                _lastCell = Cell;
                _game.NotifyPlayerMoved();
            }
        }

        // ---------------- WASD 조준 + Space 굴착 ----------------
        static readonly Vector2Int AimLeft = new Vector2Int(-1, 0);
        static readonly Vector2Int AimRight = new Vector2Int(1, 0);
        static readonly Vector2Int AimDown = new Vector2Int(0, 1);   // 그리드 y 는 아래로 증가
        static readonly Vector2Int AimUp = new Vector2Int(0, -1);

        void UpdateDigAim()
        {
            HoverValid = false;
            var kb = Keyboard.current;
            if (kb == null || _game.ShopOpen) return;

            // 1) 이번 프레임에 새로 누른 방향이 최우선 — 즉시 조준을 틀 수 있다.
            if (TryTappedDirection(kb, out Vector2Int tapped))
            {
                _digDirection = tapped;
            }
            // 2) 새로 누른 게 없으면 '지금 누르고 있는' 방향을 계속 따라간다.
            //    이래야 Space 를 누른 채 이동해 새 땅에 닿아도 조준이 끊기지 않는다.
            else if (TryHeldDirection(kb, out Vector2Int held))
            {
                _digDirection = held;
            }

            // 3) Space 를 누르고 있는데 조준한 칸이 이미 파낸 자리라면,
            //    함께 누르고 있는 다른 방향 중 팔 수 있는 칸으로 조준을 옮긴다.
            if (kb.spaceKey.isPressed && !CanDigCell(Cell + _digDirection)
                && TryFindHeldDiggable(kb, out Vector2Int rescue))
            {
                _digDirection = rescue;
            }

            HoverCell = Cell + _digDirection;
            if (!_grid.InBounds(HoverCell.x, HoverCell.y)) return;
            HoverValid = true;
        }

        /// <summary>이번 프레임에 새로 눌린 방향키.</summary>
        bool TryTappedDirection(Keyboard kb, out Vector2Int direction)
        {
            if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame) direction = AimLeft;
            else if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame) direction = AimRight;
            else if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame) direction = AimDown;
            else if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame) direction = AimUp;
            else { direction = Vector2Int.zero; return false; }
            return true;
        }

        /// <summary>
        /// 지금 누르고 있는 방향키. 현재 조준 방향의 키를 아직 누르고 있으면 그대로 유지해서,
        /// 여러 키를 같이 눌러도 조준이 덜덜 떨리지 않는다.
        /// </summary>
        bool TryHeldDirection(Keyboard kb, out Vector2Int direction)
        {
            bool left = kb.aKey.isPressed || kb.leftArrowKey.isPressed;
            bool right = kb.dKey.isPressed || kb.rightArrowKey.isPressed;
            bool down = kb.sKey.isPressed || kb.downArrowKey.isPressed;
            bool up = kb.wKey.isPressed || kb.upArrowKey.isPressed;

            if ((_digDirection == AimLeft && left) || (_digDirection == AimRight && right)
                || (_digDirection == AimDown && down) || (_digDirection == AimUp && up))
            {
                direction = _digDirection;
                return true;
            }

            if (left) direction = AimLeft;
            else if (right) direction = AimRight;
            else if (down) direction = AimDown;
            else if (up) direction = AimUp;
            else { direction = Vector2Int.zero; return false; }
            return true;
        }

        /// <summary>누르고 있는 방향 중 실제로 팔 수 있는 칸을 찾는다.</summary>
        bool TryFindHeldDiggable(Keyboard kb, out Vector2Int direction)
        {
            if ((kb.aKey.isPressed || kb.leftArrowKey.isPressed) && CanDigCell(Cell + AimLeft))
                direction = AimLeft;
            else if ((kb.dKey.isPressed || kb.rightArrowKey.isPressed) && CanDigCell(Cell + AimRight))
                direction = AimRight;
            else if ((kb.sKey.isPressed || kb.downArrowKey.isPressed) && CanDigCell(Cell + AimDown))
                direction = AimDown;
            else if ((kb.wKey.isPressed || kb.upArrowKey.isPressed) && CanDigCell(Cell + AimUp))
                direction = AimUp;
            else { direction = Vector2Int.zero; return false; }
            return true;
        }

        /// <summary>지금 힘으로 팔 수 있는 칸인가.</summary>
        bool CanDigCell(Vector2Int cell)
        {
            if (_grid == null || !_grid.InBounds(cell.x, cell.y)) return false;
            TileKind kind = _grid.Get(cell.x, cell.y);
            return TileInfo.IsDiggable(kind) && TileInfo.RequiredPower(kind) <= Stats.Power;
        }

        void TickKeyboardDig(float dt)
        {
            var kb = Keyboard.current;
            if (kb == null || _game.ShopOpen || !HoverValid || !kb.spaceKey.isPressed)
            {
                CancelDig();
                return;
            }

            var target = HoverCell;
            TileKind kind = _grid.Get(target.x, target.y);

            if (!TileInfo.IsDiggable(kind))
            {
                CancelDig();
                if (kind == TileKind.Grass)
                    SetBlocked("잔디 지반은 팔 수 없다 — 지상 입구를 이용하자");
                else if (kind == TileKind.Bedrock)
                    SetBlocked("베드락은 팔 수 없다");
                return;
            }

            int req = TileInfo.RequiredPower(kind);
            if (req > Stats.Power)
            {
                CancelDig();
                SetBlocked($"'{TileInfo.DisplayName(kind)}'을(를) 파려면 힘 {req} 필요 (현재 {Stats.Power})");
                return;
            }

            if (Stats.Stamina <= 0f)
            {
                CancelDig();
                SetBlocked("기력이 없다 — 지상으로 올라가서 회복해야 한다");
                return;
            }

            int hardness = TileInfo.Hardness(kind);
            DigFeature feature = _grid.DigFeatureAt(target.x, target.y);
            float featureDurationMultiplier = DigFeatureInfo.DurationMultiplier(feature);
            float powerDurationMultiplier = TileInfo.PowerDigDurationMultiplier(kind, Stats.Power);
            float durationMultiplier = featureDurationMultiplier * powerDurationMultiplier;
            if (DigTarget != target)
            {
                DigTarget = target;
                _digAccum = 0f;
                _digNeeded = GameConfig.DigTimePerHardness * hardness * durationMultiplier;
            }

            // 힘은 시간만 단축한다. 한 칸을 완전히 팠을 때의 총 기력은 기존과 같게 유지한다.
            float staminaRateMultiplier = DigFeatureInfo.StaminaMultiplier(feature)
                / Mathf.Max(0.01f, durationMultiplier);
            float cost = GameConfig.DigStaminaPerTile / GameConfig.DigTimePerHardness
                * staminaRateMultiplier * dt;

            IsDigging = true;
            Stats.Stamina = Mathf.Max(0f, Stats.Stamina - cost);
            _digAccum += dt;
            DigProgress01 = Mathf.Clamp01(_digAccum / Mathf.Max(0.01f, _digNeeded));

            if (_digAccum >= _digNeeded) CompleteDig(target);
        }

        void CompleteDig(Vector2Int target)
        {
            DigFeature feature = _grid.DigFeatureAt(target.x, target.y);
            CancelDig();

            // 먼저 땅을 제거해 묻혀 있던 1칸 쓰레기가 실제 오브젝트로 나타나게 한다.
            // 바로 인벤토리에 넣지 않고 가까이 가면 PickUpNearby가 줍는다.
            _grid.Set(target.x, target.y, TileKind.Empty);

            if (_grid.TryGetMineSite(target.x, target.y, out var site) && !site.Discovered)
                _game.DiscoverMine(site);

            ResolveDigFeature(target, feature);
            _grid.ClearDigFeature(target.x, target.y);

            _game.NotifyPlayerMoved();
        }

        void ResolveDigFeature(Vector2Int target, DigFeature feature)
        {
            switch (feature)
            {
                case DigFeature.LooseSoil:
                    _game.Log("푸석한 흙을 빠르게 통과했다 — 기력 소모 절반");
                    break;

                case DigFeature.CompactCache:
                    if (_grid.TryPeekTreasure(target.x, target.y, out var cache))
                        _game.Log($"압축 폐기물층 개봉! {cache.Name} ({cache.Value:N0}원)");
                    else
                        _game.Log("압축 폐기물층을 해체했다");
                    break;

                case DigFeature.GasPocket:
                    if (Gear.HasHazmat)
                    {
                        _game.Log("방호복이 매립 가스를 차단했다 — 보존된 쓰레기 발견");
                    }
                    else
                    {
                        Stats.Health -= GameConfig.GasPocketHealthDamage;
                        Stats.Stamina = Mathf.Max(0f, Stats.Stamina - GameConfig.GasPocketStaminaDamage);
                        SetBlocked($"매립 가스 분출! 체력 -{GameConfig.GasPocketHealthDamage:0} · 기력 -{GameConfig.GasPocketStaminaDamage:0}");
                        _game.Log("매립 가스에 노출됐다 — 방호복이 있으면 피해를 막을 수 있다");
                    }
                    break;
            }
        }

        void CancelDig()
        {
            IsDigging = false;
            DigProgress01 = 0f;
            _digAccum = 0f;
            DigTarget = new Vector2Int(int.MinValue, int.MinValue);
        }

        // ---------------- 획득 ----------------
        /// <summary>주변 칸의 보물을 가까이 가면 자동으로 줍는다.</summary>
        void PickUpNearby()
        {
            Vector3 p = transform.position;
            PickUpRelicsNearby(p);
            if (InventoryFull) return;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int x = Cell.x + dx, y = Cell.y + dy;
                    if (!_grid.InBounds(x, y) || !_grid.HasTreasure(x, y)) continue;
                    if (!_grid.IsWalkable(x, y)) continue;   // 아직 파지 않은 땅 속은 안 됨

                    Vector3 c = WorldGrid.CellToWorld(x, y);
                    float pickupRadius = 0.72f * GameConfig.CellSize;
                    if ((c - p).sqrMagnitude > pickupRadius * pickupRadius) continue;

                    Collect(new Vector2Int(x, y));
                    if (InventoryFull)
                    {
                        SetBlocked("가방이 가득 찼다 — 지상 분리수거장에서 E로 비우자");
                        return;
                    }
                }
            }
        }

        /// <summary>파낸 자리에 드러난 유물을 줍는다. 가방 부피와는 무관하게 등에 진다.</summary>
        void PickUpRelicsNearby(Vector3 p)
        {
            var relics = _game.Relics;
            if (relics == null || relics.CarryFull) return;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int x = Cell.x + dx, y = Cell.y + dy;
                    if (!_grid.InBounds(x, y) || !_grid.HasRelic(x, y)) continue;
                    if (!_grid.IsWalkable(x, y)) continue;

                    Vector3 c = WorldGrid.CellToWorld(x, y);
                    float radius = 0.80f * GameConfig.CellSize;
                    if ((c - p).sqrMagnitude > radius * radius) continue;

                    // 꺼낸 뒤에 자리가 없으면 유물이 사라지므로 미리 확인한다.
                    if (!_grid.TryPeekRelic(x, y, out int peek)) continue;
                    if (!relics.CanAccept(peek))
                    {
                        SetBlocked($"유물을 더 들 수 없다 ({GameConfig.RelicCarryLimit}개) — 복원소에 맡기자");
                        return;
                    }
                    if (_grid.TryTakeRelic(x, y, out int relicId))
                        relics.Excavate(relicId);
                    if (relics.CarryFull) return;
                }
            }
        }

        Vector2Int _cantCarryCell = new Vector2Int(int.MinValue, int.MinValue);
        float _cantCarryTimer;

        void Collect(Vector2Int c)
        {
            if (!_grid.TryPeekTreasure(c.x, c.y, out var peek)) return;

            if (!CanCarry(peek))
            {
                // 같은 칸에 대한 경고는 잠시 억제 (매 프레임 스팸 방지)
                if (_cantCarryCell != c || Time.time >= _cantCarryTimer)
                {
                    _cantCarryCell = c;
                    _cantCarryTimer = Time.time + 2.5f;
                    SetBlocked($"'{peek.Name}'(부피 {EffectiveVolume(peek)})이 가방에 안 들어간다");
                }
                return;
            }

            if (_grid.TryTakeTreasure(c.x, c.y, out var item))
            {
                // 유해 폐기물: 방호복이 없으면 다친다
                if (item.Material == MaterialKind.Hazard && !Gear.HasHazmat)
                {
                    Stats.Health -= GameConfig.HazardPickupDamage;
                    SetBlocked($"유해 폐기물을 맨손으로 만졌다! 체력 -{GameConfig.HazardPickupDamage:0}");
                }

                item.Volume = EffectiveVolume(item);
                Inventory.Add(item);
                _game.Log($"{item.Name} 획득 ({item.Value:N0}원 · 부피 {item.Volume})");
            }
        }

        // ---------------- 생존 ----------------
        void TickSurvival(float dt)
        {
            if (AtSurface)
            {
                // 기력은 지상에서만, 0.1초마다 10씩 회복 (지상 회복 2배)
                _staminaTick += dt;
                while (_staminaTick >= GameConfig.StaminaTickInterval)
                {
                    _staminaTick -= GameConfig.StaminaTickInterval;
                    Stats.Stamina = Mathf.Min(Stats.MaxStamina, Stats.Stamina + GameConfig.StaminaPerTick);
                }
                Stats.Health = Mathf.Min(Stats.MaxHealth, Stats.Health + 5f * dt);

            }
            else
            {
                _staminaTick = 0f;

                int depth = Depth;
                if (depth > GameConfig.DepthDamageStartDepth)
                {
                    float t = Mathf.InverseLerp(GameConfig.DepthDamageStartDepth,
                                                GameConfig.UndergroundRows, depth);
                    Stats.Health -= GameConfig.DepthDamagePerSec * t * dt;
                }
            }

            if (Stats.Health <= 0f)
            {
                Stats.Health = 0f;
                _game.GameOver("체력이 0이 되었다");
            }
        }

        /// <summary>정산 후 지상 복귀 (밤 페이즈 진입 시).</summary>
        public void ReturnToSurface(Vector2Int dropCell)
        {
            transform.position = new Vector3(
                (dropCell.x + 0.5f) * GameConfig.CellSize, SurfaceStandY, 0f);
            RefreshCell();
        }

        public List<LootItem> TakeAll()
        {
            var all = new List<LootItem>(Inventory);
            Inventory.Clear();
            return all;
        }

        /// <summary>지하 보관함에서 꺼낸 아이템을 현재 부피 그대로 가방에 넣는다.</summary>
        public bool TryReceiveStoredItem(LootItem item)
        {
            if (InventoryVolume + item.Volume > BagCapacity) return false;
            Inventory.Add(item);
            return true;
        }

        /// <summary>플레이어가 있는 칸의 지하 분리수거장 (없으면 null).</summary>
        public MineSite MineHere()
        {
            if (_grid.TryGetMineSite(Cell.x, Cell.y, out var site) && site.Discovered) return site;
            return null;
        }
    }
}
