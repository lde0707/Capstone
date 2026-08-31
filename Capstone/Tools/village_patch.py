# -*- coding: utf-8 -*-
"""기존 DigGame 스크립트에 마을/애니메이션/통로 수정 패치를 적용한다."""
import io
import os
import sys

ROOT = sys.argv[1] if len(sys.argv) > 1 else '.'
applied = []


def patch(rel, pairs):
    path = os.path.join(ROOT, rel)
    with io.open(path, encoding='utf-8') as f:
        src = f.read()
    for old, new in pairs:
        if new in src and old not in src:
            applied.append('SKIP(이미 적용) ' + rel)
            continue
        if src.count(old) != 1:
            raise SystemExit('!! %s: 앵커를 %d번 찾음\n---\n%s' % (rel, src.count(old), old[:200]))
        src = src.replace(old, new)
    with io.open(path, 'w', encoding='utf-8') as f:
        f.write(src)
    applied.append('OK ' + rel)


# ------------------------------------------------------------------ GameConfig
patch('Core/GameConfig.cs', [(
    "        public const int SkyRows = 4;",
    "        /// <summary>지상 하늘. 마을이 도시로 자랄 공간을 위해 넉넉하게 둔다.</summary>\n"
    "        public const int SkyRows = 10;",
)])

# ------------------------------------------------------- EnvironmentSpriteCatalog
patch('World/EnvironmentSpriteCatalog.cs', [
    (
        "        const int GroundPixels = 16;\n        const int TilePixels = 32;",
        "        const int GroundPixels = 16;\n"
        "        const int TilePixels = 32;\n"
        "        /// <summary>통로 타일의 흙 무늬 변형 수.</summary>\n"
        "        const int PassageVariants = 4;\n"
        "        const int PassageMasks = 256;\n"
        "        const int PassageAtlasColumns = 32;\n"
        "        static readonly int[] PassageGroundVariants = { 0, 5, 10, 15 };",
    ),
    (
        """        /// <summary>
        /// 열린 방향 비트(위 1, 오른쪽 2, 아래 4, 왼쪽 8)에 맞는 통로.
        /// 해당 지층의 흙 픽셀에서 열린 부분만 투명하게 파므로 굴착 전후 재질이 같다.
        /// </summary>
        public static Sprite PassageForMask(TileKind kind, int openMask, int pattern)
        {
            openMask &= 15;
            pattern = Mathf.Abs(pattern) % 16;
            int index = openMask * 16 + pattern;
            if (!Passages.TryGetValue(kind, out var sprites))
            {
                sprites = new Sprite[16 * 16];
                Passages[kind] = sprites;
            }
            if (sprites[index] != null) return sprites[index];

            var texture = BuildMatchingPassageAtlas(kind);
            if (texture == null) return null;
            sprites[index] = CropTopLeftGrid(texture, index, 16, GroundPixels, GroundPixels,
                GroundPixels / GameConfig.CellSize, $"Pixel Passage {kind} {openMask:00}-{pattern:00}");
            return sprites[index];
        }""",
        """        /// <summary>
        /// 열린 방향 비트에 맞는 통로.
        /// 직교: 위 1, 오른쪽 2, 아래 4, 왼쪽 8.
        /// 대각: 우상 16, 우하 32, 좌하 64, 좌상 128.
        /// 대각 비트는 양옆 직교 방향까지 모두 열렸을 때만 세워지므로,
        /// 두 칸이 겹쳐 넓게 파인 자리에서 네 칸이 만나는 모서리의
        /// 흙 찌꺼기가 깨끗이 사라지고 벽에 붙은 모서리는 그대로 남는다.
        /// </summary>
        public static Sprite PassageForMask(TileKind kind, int openMask, int pattern)
        {
            openMask &= PassageMasks - 1;
            pattern = Mathf.Abs(pattern) % PassageVariants;
            int index = openMask * PassageVariants + pattern;
            if (!Passages.TryGetValue(kind, out var sprites))
            {
                sprites = new Sprite[PassageMasks * PassageVariants];
                Passages[kind] = sprites;
            }
            if (sprites[index] != null) return sprites[index];

            var texture = BuildMatchingPassageAtlas(kind);
            if (texture == null) return null;
            sprites[index] = CropTopLeftGrid(texture, index, PassageAtlasColumns,
                GroundPixels, GroundPixels, GroundPixels / GameConfig.CellSize,
                $"Pixel Passage {kind} {openMask:000}-{pattern}");
            return sprites[index];
        }""",
    ),
    (
        """            const int atlasTiles = 16;
            int atlasSize = atlasTiles * GroundPixels;
            var atlas = NewRuntimeTexture(atlasSize, atlasSize, $"Pixel Passages {kind} (Runtime)");
            var source = ground.GetPixels32();
            var output = new Color32[atlasSize * atlasSize];

            for (int mask = 0; mask < 16; mask++)
            {
                int destinationY = atlasSize - (mask + 1) * GroundPixels;
                for (int pattern = 0; pattern < 16; pattern++)
                {
                    int sourceX = (pattern % 4) * GroundPixels;
                    int sourceY = ground.height - (pattern / 4 + 1) * GroundPixels;
                    int destinationX = pattern * GroundPixels;

                    for (int y = 0; y < GroundPixels; y++)
                    {
                        for (int x = 0; x < GroundPixels; x++)
                        {
                            Color32 pixel = source[(sourceY + y) * ground.width + sourceX + x];
                            if (IsDugPixel(mask, x, y, pattern)) pixel.a = 0;
                            output[(destinationY + y) * atlasSize + destinationX + x] = pixel;
                        }
                    }
                }
            }""",
        """            const int tileCount = PassageMasks * PassageVariants;
            int atlasWidth = PassageAtlasColumns * GroundPixels;
            int atlasHeight = (tileCount / PassageAtlasColumns) * GroundPixels;
            var atlas = NewRuntimeTexture(atlasWidth, atlasHeight, $"Pixel Passages {kind} (Runtime)");
            var source = ground.GetPixels32();
            var output = new Color32[atlasWidth * atlasHeight];

            for (int index = 0; index < tileCount; index++)
            {
                int mask = index / PassageVariants;
                int variant = PassageGroundVariants[index % PassageVariants];

                int sourceX = (variant % 4) * GroundPixels;
                int sourceY = ground.height - (variant / 4 + 1) * GroundPixels;
                int destinationX = (index % PassageAtlasColumns) * GroundPixels;
                int destinationY = atlasHeight - (index / PassageAtlasColumns + 1) * GroundPixels;

                for (int y = 0; y < GroundPixels; y++)
                {
                    for (int x = 0; x < GroundPixels; x++)
                    {
                        Color32 pixel = source[(sourceY + y) * ground.width + sourceX + x];
                        if (IsDugPixel(mask, x, y, variant)) pixel.a = 0;
                        output[(destinationY + y) * atlasWidth + destinationX + x] = pixel;
                    }
                }
            }""",
    ),
    (
        """        static bool IsDugPixel(int openMask, int x, int y, int pattern)
        {
            if ((openMask & 15) == 15) return true;

            const float center = (GroundPixels - 1) * 0.5f;
            var point = new Vector2(x, y);
            var middle = new Vector2(center, center);
            float distance = Vector2.Distance(point, middle);

            if ((openMask & 1) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(center, GroundPixels + 1f)));
            if ((openMask & 2) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(GroundPixels + 1f, center)));
            if ((openMask & 4) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(center, -2f)));
            if ((openMask & 8) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(-2f, center)));
""",
        """        static bool IsDugPixel(int openMask, int x, int y, int pattern)
        {
            if ((openMask & 255) == 255) return true;

            const float center = (GroundPixels - 1) * 0.5f;
            const float far = GroundPixels + 1f;
            const float near = -2f;
            var point = new Vector2(x, y);
            var middle = new Vector2(center, center);
            float distance = Vector2.Distance(point, middle);

            if ((openMask & 1) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(center, far)));
            if ((openMask & 2) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(far, center)));
            if ((openMask & 4) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(center, near)));
            if ((openMask & 8) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(near, center)));

            // 네 칸이 모두 열린 모서리는 대각선까지 깎아 떠 있는 흙덩이를 없앤다.
            if ((openMask & 16) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(far, far)));
            if ((openMask & 32) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(far, near)));
            if ((openMask & 64) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(near, near)));
            if ((openMask & 128) != 0)
                distance = Mathf.Min(distance, DistanceToSegment(point, middle, new Vector2(near, far)));
""",
    ),
])

# ------------------------------------------------------------------ WorldView
patch('World/WorldView.cs', [
    (
        "        readonly Dictionary<TileKind, TileBase[]> _passageTiles = new Dictionary<TileKind, TileBase[]>();",
        "        /// <summary>통로 타일은 (지층, 마스크*변형) 조합이 많아 필요할 때 만들어 캐싱한다.</summary>\n"
        "        readonly Dictionary<int, TileBase> _passageTiles = new Dictionary<int, TileBase>();",
    ),
    (
        "            foreach (var set in _passageTiles.Values)\n"
        "                for (int i = 0; i < set.Length; i++) DestroyGenerated(set[i]);",
        "            foreach (var tile in _passageTiles.Values)\n"
        "                DestroyGenerated(tile);",
    ),
    (
        """                _terrainTiles[kind] = set;

                // 캐기 전 지층의 색조를 그대로 유지하는 연결형 통로 세트.
                // 같은 스프라이트라도 지층별 tint를 분리해야 깊이가 바뀌는 경계에서도 이질감이 없다.
                if (TileInfo.IsDiggable(kind))
                {
                    var passageSet = new TileBase[16 * 16];
                    for (int mask = 0; mask < 16; mask++)
                    {
                        for (int pattern = 0; pattern < 16; pattern++)
                        {
                            int index = mask * 16 + pattern;
                            var tile = ScriptableObject.CreateInstance<Tile>();
                            tile.hideFlags = HideFlags.DontSave;
                            tile.sprite = EnvironmentSpriteCatalog.PassageForMask(kind, mask, pattern)
                                       ?? SpriteFactory.Solid(TileInfo.ColorOf(TileKind.Empty));
                            tile.color = Color.white;
                            passageSet[index] = tile;
                        }
                    }
                    _passageTiles[kind] = passageSet;
                }
            }
        }""",
        """                _terrainTiles[kind] = set;
            }
            // 통로 타일은 8방향 조합이 256가지라 PassageTile() 에서 필요할 때 만든다.
        }

        /// <summary>캐기 전 지층 색조를 유지하는 8방향 연결 통로 타일 (지연 생성).</summary>
        TileBase PassageTile(TileKind kind, int openMask, int pattern)
        {
            int index = (openMask & 255) * 4 + (pattern & 3);
            int key = (int)kind * 1024 + index;
            if (_passageTiles.TryGetValue(key, out var cached)) return cached;

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.hideFlags = HideFlags.DontSave;
            tile.sprite = EnvironmentSpriteCatalog.PassageForMask(kind, openMask, pattern)
                       ?? SpriteFactory.Solid(TileInfo.ColorOf(TileKind.Empty));
            tile.color = Color.white;
            _passageTiles[key] = tile;
            return tile;
        }""",
    ),
    (
        """        TileBase TileAt(int x, int y)
        {
            TileKind kind = _grid.Get(x, y);
            if (kind == TileKind.Sky) return _flatTiles[kind];

            int pattern = PositiveMod(x, 4) + PositiveMod(y, 4) * 4;
            if (kind == TileKind.Empty)
            {
                TileKind originalGround = _grid.OriginalGroundAt(x, y);
                return _passageTiles.TryGetValue(originalGround, out var passageSet)
                    ? passageSet[PassageMask(x, y) * 16 + pattern]
                    : null;
            }

            return _terrainTiles.TryGetValue(kind, out var set) ? set[pattern] : null;
        }""",
        """        TileBase TileAt(int x, int y)
        {
            TileKind kind = _grid.Get(x, y);
            if (kind == TileKind.Sky) return _flatTiles[kind];

            if (kind == TileKind.Empty)
            {
                TileKind originalGround = _grid.OriginalGroundAt(x, y);
                if (!TileInfo.IsDiggable(originalGround)) return null;
                int passagePattern = PositiveMod(x, 2) + PositiveMod(y, 2) * 2;
                return PassageTile(originalGround, PassageMask(x, y), passagePattern);
            }

            int pattern = PositiveMod(x, 4) + PositiveMod(y, 4) * 4;
            return _terrainTiles.TryGetValue(kind, out var set) ? set[pattern] : null;
        }""",
    ),
    (
        """        int PassageMask(int x, int y)
        {
            int mask = 0;
            if (IsPassage(x, y - 1)) mask |= 1; // 위
            if (IsPassage(x + 1, y)) mask |= 2; // 오른쪽
            if (IsPassage(x, y + 1)) mask |= 4; // 아래
            if (IsPassage(x - 1, y)) mask |= 8; // 왼쪽
            return mask;
        }""",
        """        int PassageMask(int x, int y)
        {
            bool up = IsPassage(x, y - 1);
            bool right = IsPassage(x + 1, y);
            bool down = IsPassage(x, y + 1);
            bool left = IsPassage(x - 1, y);

            int mask = 0;
            if (up) mask |= 1;
            if (right) mask |= 2;
            if (down) mask |= 4;
            if (left) mask |= 8;

            // 대각은 양옆까지 다 뚫렸을 때만 — 벽에 붙은 모서리는 남기고
            // 넓게 파낸 자리에 떠 있던 흙 찌꺼기만 없앤다.
            if (up && right && IsPassage(x + 1, y - 1)) mask |= 16;
            if (down && right && IsPassage(x + 1, y + 1)) mask |= 32;
            if (down && left && IsPassage(x - 1, y + 1)) mask |= 64;
            if (up && left && IsPassage(x - 1, y - 1)) mask |= 128;
            return mask;
        }""",
    ),
])

# ------------------------------------------------------------------ SpriteFactory
patch('Util/SpriteFactory.cs', [(
    "            TrashSpriteCatalog.Clear();\n"
    "            EnvironmentSpriteCatalog.Clear();\n"
    "            PlayerSpriteCatalog.Clear();",
    "            TrashSpriteCatalog.Clear();\n"
    "            EnvironmentSpriteCatalog.Clear();\n"
    "            PlayerSpriteCatalog.Clear();\n"
    "            VillageSpriteCatalog.Clear();",
)])

# ------------------------------------------------------------------ PlayerController
patch('Player/PlayerController.cs', [
    (
        "        PlayerVisualState _visualState;",
        "        PlayerVisualState _visualState;\n        Facing _facing = Facing.Down;",
    ),
    (
        "            var walkFrames = PlayerSpriteCatalog.Frames(PlayerVisualState.Walk);",
        "            var walkFrames = PlayerSpriteCatalog.Frames(PlayerVisualState.Walk, Facing.Down);",
    ),
    (
        """        void UpdateVisual(float dt)
        {
            if (_sr == null) return;

            PlayerVisualState nextState;
            if (IsDigging) nextState = PlayerVisualState.Dig;
            else if (!AtSurface && Mathf.Abs(_moveInput.y) > Mathf.Abs(_moveInput.x))
                nextState = PlayerVisualState.Climb;
            else nextState = PlayerVisualState.Walk;

            if (nextState != _visualState)
            {
                _visualState = nextState;
                _visualTime = 0f;
            }
            else
            {
                _visualTime += dt;
            }

            Sprite[] frames = PlayerSpriteCatalog.Frames(_visualState);
            if (frames.Length > 0)
            {
                bool active = IsDigging || _moveInput.sqrMagnitude > 0.01f;
                float fps = _visualState == PlayerVisualState.Dig ? 10f : 8f;
                int frame = active ? Mathf.FloorToInt(_visualTime * fps) % frames.Length : 0;
                _sr.sprite = frames[frame];
            }

            if (_moveInput.x < -0.01f) _sr.flipX = true;
            else if (_moveInput.x > 0.01f) _sr.flipX = false;
            else if (IsDigging && _digDirection.x != 0) _sr.flipX = _digDirection.x < 0;
        }""",
        """        void UpdateVisual(float dt)
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
        }""",
    ),
])

# ------------------------------------------------------------------ GameManager
patch('Core/GameManager.cs', [
    (
        "        public GameHUD Hud { get; set; }",
        "        public GameHUD Hud { get; set; }\n"
        "        /// <summary>지상에 자라나는 마을 — 하루하루의 보상.</summary>\n"
        "        public Village Village { get; private set; }",
    ),
    (
        "        PetCompanion _petObj;",
        "        PetCompanion _petObj;\n        bool _quotaCelebrated;",
    ),
    (
        "            HasRecycleStation = false;\n            RecycleLevel = 0;",
        "            HasRecycleStation = false;\n            RecycleLevel = 0;\n            _quotaCelebrated = false;",
    ),
    (
        "            View.ShowRecycleStation(RecycleRect, RecycleLevel);\n\n            var playerGo = new GameObject(\"Player\");",
        "            View.ShowRecycleStation(RecycleRect, RecycleLevel);\n\n"
        "            Village = gameObject.AddComponent<Village>();\n"
        "            Village.Build(this);\n\n"
        "            var playerGo = new GameObject(\"Player\");",
    ),
    (
        """            Log($"1일차 — 해가 지기 전까지 할당량 {Quota:N0}원을 채우자!");""",
        """            Log($"1일차 — 해가 지기 전까지 할당량 {Quota:N0}원을 채우자!");
            Log("할당량을 채우면 지상의 내 집이 빛나면서 커진다 — 7일이면 도시가 된다");""",
    ),
    (
        """            Day++;
            Quota = GameConfig.QuotaForDay(Day);
            DeliveredToday = 0;
            TimeLeft = GameConfig.DayLengthSeconds;
            if (Player != null) Player.Gear.OnNewDay();
            State = GameState.Day;

            Log($"{Day}일차 — 오늘 할당량 {Quota:N0}원");""",
        """            Day++;
            Quota = GameConfig.QuotaForDay(Day);
            DeliveredToday = 0;
            TimeLeft = GameConfig.DayLengthSeconds;
            _quotaCelebrated = false;
            if (Player != null) Player.Gear.OnNewDay();
            State = GameState.Day;

            Log($"{Day}일차 — 오늘 할당량 {Quota:N0}원");
            if (Village != null) Village.OnNewDay(Day);""",
    ),
    (
        """            int paid = Mathf.RoundToInt(value * SellMultiplier);
            Money += paid;
            DeliveredToday += paid;

            string bonus""",
        """            int paid = Mathf.RoundToInt(value * SellMultiplier);
            Money += paid;
            DeliveredToday += paid;
            CheckQuotaCelebration();

            string bonus""",
    ),
    (
        """            Money += paid;
            DeliveredToday += paid;
            View.ShowRecyclingOutputs(materials);""",
        """            Money += paid;
            DeliveredToday += paid;
            CheckQuotaCelebration();
            View.ShowRecyclingOutputs(materials);""",
    ),
    (
        """        /// <summary>현재 공장 설비를 기준으로 한 재질별 재활용 예상 판매가.</summary>""",
        """        /// <summary>
        /// 오늘 할당량을 처음 넘긴 순간 — 정산까지 기다리지 않고 바로 보상을 준다.
        /// 지상의 집이 빛나며 커지고, 이웃과 주민이 늘어난다.
        /// </summary>
        void CheckQuotaCelebration()
        {
            if (_quotaCelebrated || Quota <= 0 || DeliveredToday < Quota) return;
            _quotaCelebrated = true;
            if (Village != null) Village.OnQuotaMet(Day);
        }

        /// <summary>현재 공장 설비를 기준으로 한 재질별 재활용 예상 판매가.</summary>""",
    ),
])

print('\n'.join(applied))
