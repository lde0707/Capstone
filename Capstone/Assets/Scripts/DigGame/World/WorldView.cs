using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DigGame
{
    /// <summary>
    /// WorldGrid 를 CC0 픽셀 흙과 16방향 통로 오토타일로 그린다.
    /// 지하 시야 제한(포그), 굴착 사거리 하이라이트도 여기서 담당한다.
    /// </summary>
    public class WorldView : MonoBehaviour
    {
        // 정렬 순서
        /// <summary>파낸 자리 뒤에 깔리는 흙벽. 땅보다 한 단 뒤다.</summary>
        const int OrderBackdrop = -1;
        const int OrderGround = 0;
        const int OrderPollutionTint = 2;
        const int OrderTreasure = 30;  // 보물은 어둠에 가려진다 (직접 찾아야 함)
        const int OrderFog = 45;
        const int OrderMine = 46;      // 지상 시설·고용한 일꾼은 어둠 위에 계속 보인다
        /// <summary>지하 도시광산 그림. 어둠 속에서는 그냥 땅으로 보이도록 포그 아래에 둔다.</summary>
        const int OrderMineArt = OrderTreasure - 4;
        const int OrderWorker = 47;
        const int OrderHighlight = 48;
        const int OrderPlayer = 50;

        /// <summary>
        /// 파낸 자리에 깔리는 뒷벽의 색. 같은 지층 흙을 어둡고 살짝 차갑게 눌러
        /// "한 겹 뒤에 있는 면"으로 읽히게 한다 (멀수록 어둡고 푸른 공기 원근).
        /// </summary>
        static readonly Color BackdropTint = new Color(0.44f, 0.43f, 0.52f, 1f);

        IGameObjectFactory _factory;
        WorldGrid _grid;
        Tilemap _backdrop;
        Tilemap _ground;
        Tilemap _digFeatureMap;
        Tilemap _buriedHintMap;

        readonly Dictionary<TileKind, TileBase> _flatTiles = new Dictionary<TileKind, TileBase>();
        readonly Dictionary<TileKind, TileBase[]> _terrainTiles = new Dictionary<TileKind, TileBase[]>();
        /// <summary>파낸 자리 뒷벽 타일 — (지층, 무늬) 조합만큼 만들어 둔다.</summary>
        readonly Dictionary<int, TileBase> _backdropTiles = new Dictionary<int, TileBase>();
        /// <summary>통로 타일은 (지층, 마스크*변형) 조합이 많아 필요할 때 만들어 캐싱한다.</summary>
        readonly Dictionary<int, TileBase> _passageTiles = new Dictionary<int, TileBase>();
        readonly Dictionary<DigFeature, TileBase> _digFeatureTiles = new Dictionary<DigFeature, TileBase>();
        readonly Dictionary<MaterialKind, TileBase> _buriedHintTiles = new Dictionary<MaterialKind, TileBase>();

        readonly Dictionary<Vector2Int, GameObject> _treasureMarkers = new Dictionary<Vector2Int, GameObject>();
        readonly Dictionary<Vector2Int, GameObject> _relicMarkers = new Dictionary<Vector2Int, GameObject>();
        readonly Dictionary<int, TileBase> _shaftTiles = new Dictionary<int, TileBase>();
        readonly Dictionary<int, GameObject> _mineMarkers = new Dictionary<int, GameObject>();

        Transform _markerRoot;
        SpriteRenderer _highlight;
        SpriteRenderer _fogRenderer;
        SpriteRenderer _pollutionTintRenderer;
        Material _fogMaterial;
        GameObject _surfaceRecycle;

        // ------------------------------------------------------------------
        public void Build(WorldGrid grid, IGameObjectFactory factory)
        {
            _factory = factory ?? throw new System.ArgumentNullException(nameof(factory));
            _grid = grid;

            var gridGo = new GameObject("Tilemap Grid");
            gridGo.transform.SetParent(transform, false);
            var uGrid = gridGo.AddComponent<UnityEngine.Grid>();
            uGrid.cellSize = new Vector3(GameConfig.CellSize, GameConfig.CellSize, 0f);

            _backdrop = MakeTilemap(gridGo.transform, "Ground Backdrop", OrderBackdrop);
            _ground = MakeTilemap(gridGo.transform, "Ground", OrderGround);
            _digFeatureMap = MakeTilemap(gridGo.transform, "Dig Feature Hints", OrderTreasure - 2);
            _digFeatureMap.gameObject.AddComponent<BuriedHintMapPulse>();
            _buriedHintMap = MakeTilemap(gridGo.transform, "Buried Trash Hints", OrderTreasure);
            _buriedHintMap.gameObject.AddComponent<BuriedHintMapPulse>();

            _markerRoot = new GameObject("Markers").transform;
            _markerRoot.SetParent(transform, false);

            BuildTileAssets();
            PaintGround();
            BuildPollutionTint();
            BuildDigFeatureHints();
            BuildBuriedTrashHints();
            BuildSmoothFog();
            BuildHighlight();

            BuildBuriedRelicMarkers();

            _grid.OnTileChanged += HandleTileChanged;
            _grid.OnTreasureRemoved += HandleTreasureRemoved;
            _grid.OnRelicRemoved += HandleRelicRemoved;
        }

        void OnDestroy()
        {
            if (_grid != null)
            {
                _grid.OnTileChanged -= HandleTileChanged;
                _grid.OnTreasureRemoved -= HandleTreasureRemoved;
                _grid.OnRelicRemoved -= HandleRelicRemoved;
            }

            // 런타임에 만든 Tile 에셋 정리 (재시작 시 누수 방지)
            foreach (var t in _flatTiles.Values)
                DestroyGenerated(t);
            foreach (var set in _terrainTiles.Values)
                for (int i = 0; i < set.Length; i++) DestroyGenerated(set[i]);
            foreach (var tile in _passageTiles.Values)
                DestroyGenerated(tile);
            foreach (var tile in _digFeatureTiles.Values)
                DestroyGenerated(tile);
            foreach (var tile in _buriedHintTiles.Values)
                DestroyGenerated(tile);
            foreach (var tile in _shaftTiles.Values) DestroyGenerated(tile);
            _shaftTiles.Clear();
            foreach (var tile in _backdropTiles.Values) DestroyGenerated(tile);
            _backdropTiles.Clear();
            _flatTiles.Clear();
            _terrainTiles.Clear();
            _passageTiles.Clear();
            _digFeatureTiles.Clear();
            _buriedHintTiles.Clear();

            DestroyGenerated(_fogMaterial);
        }

        static void DestroyGenerated(Object obj)
        {
            if (obj == null) return;

            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }

        static Tilemap MakeTilemap(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tm = go.AddComponent<Tilemap>();
            var r = go.AddComponent<TilemapRenderer>();
            r.sortingOrder = sortingOrder;
            return tm;
        }

        void BuildTileAssets()
        {
            var sky = ScriptableObject.CreateInstance<Tile>();
            sky.hideFlags = HideFlags.DontSave;
            sky.sprite = SpriteFactory.Solid(TileInfo.ColorOf(TileKind.Sky));
            sky.color = Color.white;
            _flatTiles[TileKind.Sky] = sky;

            foreach (TileKind kind in System.Enum.GetValues(typeof(TileKind)))
            {
                if (kind == TileKind.Sky || kind == TileKind.Empty) continue;

                // 16px 원본 흙에서 변형을 만들어 반복 무늬를 줄인다.
                var set = new TileBase[16];
                for (int i = 0; i < set.Length; i++)
                {
                    var tile = ScriptableObject.CreateInstance<Tile>();
                    tile.hideFlags = HideFlags.DontSave;
                    tile.sprite = EnvironmentSpriteCatalog.GroundFill(kind, i)
                                  ?? SpriteFactory.Noisy(TileInfo.ColorOf(kind), i + (int)kind * 977);
                    tile.color = Color.white;
                    set[i] = tile;
                }
                _terrainTiles[kind] = set;
            }

            // 통로 타일 자체는 8방향 조합이 256가지라 PassageTile() 에서 필요할 때 만들지만,
            // 지층별 통로 아틀라스는 여기서 한 번 데워둔다 (첫 굴착 때 프레임이 튀지 않도록).
            foreach (TileKind kind in System.Enum.GetValues(typeof(TileKind)))
                if (TileInfo.SupportsPassage(kind)) PassageTile(kind, 0, 0);
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
        }

        /// <summary>grid 좌표(y가 아래로 증가) → 타일맵 셀 좌표(y가 위로 증가).</summary>
        static Vector3Int ToCell(int x, int y) => new Vector3Int(x, -y - 1, 0);

        void PaintGround()
        {
            int w = _grid.Width, h = _grid.Height;
            var positions = new Vector3Int[w * h];
            var tiles = new TileBase[w * h];
            var backdrops = new TileBase[w * h];

            int i = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    positions[i] = ToCell(x, y);
                    tiles[i] = TileAt(x, y);
                    backdrops[i] = BackdropAt(x, y);
                    i++;
                }
            }
            _backdrop.SetTiles(positions, backdrops);
            _ground.SetTiles(positions, tiles);
        }

        /// <summary>오염도가 오를수록 지하 전체에 옅은 보라색이 한 겹씩 쌓인다.</summary>
        void BuildPollutionTint()
        {
            var go = new GameObject("Underground Pollution Tint");
            go.transform.SetParent(transform, false);

            float width = _grid.Width * GameConfig.CellSize;
            float height = (_grid.Height - GameConfig.SurfaceRow) * GameConfig.CellSize;
            go.transform.localPosition = new Vector3(
                width * 0.5f,
                -GameConfig.SurfaceRow * GameConfig.CellSize - height * 0.5f,
                0f);
            go.transform.localScale = new Vector3(width, height, 1f);

            _pollutionTintRenderer = go.AddComponent<SpriteRenderer>();
            _pollutionTintRenderer.sprite = SpriteFactory.Solid(Color.white);
            _pollutionTintRenderer.sortingOrder = OrderPollutionTint;
            SetPollutionLevel(0);
        }

        public void SetPollutionLevel(int level)
        {
            if (_pollutionTintRenderer == null) return;
            float amount = Mathf.Clamp(level, 0, PollutionMeter.MaxValue) / (float)PollutionMeter.MaxValue;
            _pollutionTintRenderer.color = new Color(0.54f, 0.16f, 0.78f, 0.34f * amount);
            _pollutionTintRenderer.enabled = level > 0;
        }

        TileBase TileAt(int x, int y)
        {
            TileKind kind = _grid.Get(x, y);
            if (kind == TileKind.Sky) return _flatTiles[kind];

            if (kind == TileKind.Empty)
            {
                // 잔디층에 뚫린 빈 칸은 지하 입구뿐이다 (잔디는 팔 수 없다).
                // 구멍이 아니라 동바리를 댄 갱도로 그린다.
                if (IsEntranceShaft(x, y)) return ShaftTile(x, y);

                TileKind originalGround = _grid.OriginalGroundAt(x, y);
                if (!TileInfo.SupportsPassage(originalGround)) return null;
                int passagePattern = PositiveMod(x, 2) + PositiveMod(y, 2) * 2;
                return PassageTile(originalGround, PassageMask(x, y), passagePattern);
            }

            int pattern = PositiveMod(x, 4) + PositiveMod(y, 4) * 4;
            return _terrainTiles.TryGetValue(kind, out var set) ? set[pattern] : null;
        }

        /// <summary>
        /// 파낸 자리 뒤에 깔 흙벽. 캐기 전 그 자리에 있던 지층을 어둡게 눌러 쓴다.
        /// 지상·입구 갱도처럼 이미 꽉 찬 칸에는 깔지 않는다.
        /// </summary>
        TileBase BackdropAt(int x, int y)
        {
            if (_grid.Get(x, y) != TileKind.Empty) return null;
            if (y < GameConfig.SurfaceRow) return null;          // 하늘
            if (IsEntranceShaft(x, y)) return null;              // 갱도 타일이 칸을 꽉 채운다

            TileKind original = _grid.OriginalGroundAt(x, y);
            if (original == TileKind.Empty || original == TileKind.Sky) return null;

            // 앞면과 무늬가 겹치면 "같은 타일이 비친" 것처럼 보여서 한 칸씩 어긋낸다.
            int pattern = PositiveMod(x + 2, 4) + PositiveMod(y + 1, 4) * 4;
            return BackdropTile(original, pattern);
        }

        TileBase BackdropTile(TileKind kind, int pattern)
        {
            int key = (int)kind * 16 + pattern;
            if (_backdropTiles.TryGetValue(key, out var cached)) return cached;

            var sprite = EnvironmentSpriteCatalog.GroundFill(kind, pattern)
                         ?? SpriteFactory.Noisy(TileInfo.ColorOf(kind), pattern + (int)kind * 977);
            if (sprite == null) return null;

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.hideFlags = HideFlags.DontSave;
            tile.sprite = sprite;
            tile.color = BackdropTint;
            tile.flags = TileFlags.LockColor;
            _backdropTiles[key] = tile;
            return tile;
        }

        /// <summary>잔디층을 관통하는 지하 입구 칸인가.</summary>
        bool IsEntranceShaft(int x, int y)
        {
            if (y < GameConfig.SurfaceRow ||
                y >= GameConfig.SurfaceRow + GameConfig.GrassLayerRows) return false;
            return _grid.Get(x, y) == TileKind.Empty;
        }

        /// <summary>
        /// 입구 갱도 타일. 위치에 따라 입구/몸통/바닥, 왼쪽/가운데/오른쪽을 고른다.
        /// 사다리는 통로 전체에서 한 칸에만 들어간다.
        /// </summary>
        TileBase ShaftTile(int x, int y)
        {
            int depth = y - GameConfig.SurfaceRow;
            var row = depth == 0
                ? EnvironmentSpriteCatalog.ShaftRow.Mouth
                : depth >= GameConfig.GrassLayerRows - 1
                    ? EnvironmentSpriteCatalog.ShaftRow.Floor
                    : EnvironmentSpriteCatalog.ShaftRow.Body;

            bool openLeft = IsEntranceShaft(x - 1, y);
            bool openRight = IsEntranceShaft(x + 1, y);

            EnvironmentSpriteCatalog.ShaftCol col;
            if (!openLeft && !openRight) col = EnvironmentSpriteCatalog.ShaftCol.Single;
            else if (!openLeft) col = EnvironmentSpriteCatalog.ShaftCol.Left;
            else if (!openRight) col = EnvironmentSpriteCatalog.ShaftCol.Right;
            else col = IsShaftCenter(x, y)
                ? EnvironmentSpriteCatalog.ShaftCol.LadderMid
                : EnvironmentSpriteCatalog.ShaftCol.Mid;

            int key = (int)row * 8 + (int)col;
            if (_shaftTiles.TryGetValue(key, out var cached)) return cached;

            var sprite = EnvironmentSpriteCatalog.EntrancePassage(row, col);
            if (sprite == null) return null;

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.hideFlags = HideFlags.DontSave;
            tile.sprite = sprite;
            _shaftTiles[key] = tile;
            return tile;
        }

        /// <summary>그 줄의 통로 한가운데인가 — 사다리는 여기 한 칸에만 놓는다.</summary>
        bool IsShaftCenter(int x, int y)
        {
            int left = x, right = x;
            while (IsEntranceShaft(left - 1, y)) left--;
            while (IsEntranceShaft(right + 1, y)) right++;
            return x == (left + right) / 2;
        }

        static int PositiveMod(int value, int divisor)
        {
            int result = value % divisor;
            return result < 0 ? result + divisor : result;
        }

        int PassageMask(int x, int y)
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
        }

        bool IsPassage(int x, int y) =>
            _grid.InBounds(x, y) && TileInfo.IsWalkable(_grid.Get(x, y));

        /// <summary>지하 전체를 덮는 연속적인 원형 포그를 만든다.</summary>
        void BuildSmoothFog()
        {
            var shader = Resources.Load<Shader>("SmoothFog");
            if (shader == null)
            {
                Debug.LogError("[DigGame] SmoothFog 셰이더를 찾지 못했습니다.");
                return;
            }

            var go = new GameObject("Smooth Fog");
            go.transform.SetParent(transform, false);

            float undergroundHeight = (_grid.Height - GameConfig.SurfaceRow) * GameConfig.CellSize;
            go.transform.localPosition = new Vector3(
                _grid.Width * GameConfig.CellSize * 0.5f,
                -GameConfig.SurfaceRow * GameConfig.CellSize - undergroundHeight * 0.5f,
                0f);
            go.transform.localScale = new Vector3(_grid.Width * GameConfig.CellSize, undergroundHeight, 1f);

            _fogMaterial = new Material(shader)
            {
                name = "Smooth Fog (Runtime)",
                hideFlags = HideFlags.DontSave
            };

            // 알파 1 — 어둠 속에서는 광맥 균열도, 묻힌 쓰레기 힌트도, 유물도 비치지 않는다.
            _fogMaterial.SetColor("_FogColor", new Color(0.035f, 0.032f, 0.055f, 1f));

            _fogRenderer = go.AddComponent<SpriteRenderer>();
            _fogRenderer.sprite = SpriteFactory.Solid(Color.white);
            _fogRenderer.sharedMaterial = _fogMaterial;
            _fogRenderer.sortingOrder = OrderFog;
        }

        void BuildHighlight()
        {
            var go = new GameObject("DigHighlight");
            go.transform.SetParent(transform, false);
            _highlight = go.AddComponent<SpriteRenderer>();
            _highlight.sprite = SpriteFactory.Framed(new Color(1f, 1f, 1f, 0.16f), new Color(1f, 0.95f, 0.6f, 0.95f));
            _highlight.transform.localScale = Vector3.one * GameConfig.CellSize;
            _highlight.sortingOrder = OrderHighlight;
            _highlight.enabled = false;
        }

        // ------------------------------------------------------------------
        /// <summary>플레이어 위치를 중심으로 타일과 무관한 원형 시야를 갱신한다.</summary>
        public void UpdateFog(Vector3 center, float visionRadius, float fadeRadius)
        {
            if (_fogMaterial == null) return;

            // 기존 칸 시야와 체감 범위를 맞추기 위해 타일 반 칸을 더한다.
            float inner = Mathf.Max(0f, (visionRadius + 0.5f) * GameConfig.CellSize);
            float outer = Mathf.Max(inner + 0.05f, (fadeRadius + 0.5f) * GameConfig.CellSize);
            _fogMaterial.SetVector("_Center", new Vector4(center.x, center.y, 0f, 0f));
            _fogMaterial.SetFloat("_InnerRadius", inner);
            _fogMaterial.SetFloat("_OuterRadius", outer);
        }

        /// <summary>WASD로 선택한 굴착 가능한 인접 칸을 표시한다.</summary>
        public void SetHighlight(bool on, Vector2Int cell, bool diggable)
        {
            if (_highlight == null) return;
            _highlight.enabled = on;
            if (!on) return;

            _highlight.transform.position = WorldGrid.CellToWorld(cell.x, cell.y);
            _highlight.color = diggable
                ? new Color(1f, 1f, 1f, 1f)
                : new Color(1f, 0.45f, 0.45f, 0.85f);
        }

        // ------------------------------------------------------------------
        void HandleTileChanged(int x, int y, TileKind kind)
        {
            // 한 칸을 파면 인접 통로들의 직선·모서리·막힌 끝도 함께 바뀐다.
            for (int oy = -1; oy <= 1; oy++)
                for (int ox = -1; ox <= 1; ox++)
                {
                    int tx = x + ox, ty = y + oy;
                    if (!_grid.InBounds(tx, ty)) continue;
                    var cell = ToCell(tx, ty);
                    _backdrop.SetTile(cell, BackdropAt(tx, ty));
                    _ground.SetTile(cell, TileAt(tx, ty));
                }

            if (TileInfo.IsWalkable(kind) && _grid.HasTreasure(x, y))
            {
                HideBuriedHint(x, y);
                ShowTreasureMarker(x, y);
            }
            if (TileInfo.IsWalkable(kind) && _grid.HasRelic(x, y))
            {
                HideBuriedHint(x, y);
                ShowRelicMarker(x, y);
            }
            if (TileInfo.IsWalkable(kind)) HideDigFeatureHint(x, y);
        }

        /// <summary>특수 지층은 포그 아래에서 색과 크기가 다른 균열 표식으로 보인다.</summary>
        void BuildDigFeatureHints()
        {
            foreach (var (cell, feature) in _grid.AllDigFeatures)
            {
                Vector3Int tileCell = ToCell(cell.x, cell.y);
                _digFeatureMap.SetTile(tileCell, DigFeatureTile(feature));
                float scale = feature == DigFeature.LooseSoil ? 0.42f
                    : feature == DigFeature.CompactCache ? 0.66f : 0.78f;
                _digFeatureMap.SetTransformMatrix(tileCell, Matrix4x4.TRS(
                    Vector3.zero, Quaternion.identity, Vector3.one * scale));
            }
        }

        TileBase DigFeatureTile(DigFeature feature)
        {
            if (_digFeatureTiles.TryGetValue(feature, out var cached)) return cached;
            Color color = DigFeatureInfo.ColorOf(feature);
            color.a = 0.58f;
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.hideFlags = HideFlags.DontSave;
            tile.sprite = SpriteFactory.Diamond(Color.white);
            tile.color = color;
            tile.flags = TileFlags.LockColor;
            _digFeatureTiles[feature] = tile;
            return tile;
        }

        void HideDigFeatureHint(int x, int y)
        {
            if (_digFeatureMap != null)
                _digFeatureMap.SetTile(ToCell(x, y), null);
        }

        /// <summary>
        /// 아직 땅속에 있는 쓰레기를 은은한 재질색 광점으로 표시한다.
        /// 포그보다 아래에 그려져 플레이어 시야 안에 들어왔을 때만 보인다.
        /// </summary>
        void BuildBuriedTrashHints()
        {
            foreach (var (cell, item) in _grid.AllTreasures)
            {
                if (_grid.IsWalkable(cell.x, cell.y)) continue;
                Vector3Int tileCell = ToCell(cell.x, cell.y);
                _buriedHintMap.SetTile(tileCell, BuriedHintTile(item.Material));
                _buriedHintMap.SetTransformMatrix(tileCell, Matrix4x4.TRS(
                    Vector3.zero, Quaternion.identity, Vector3.one * 0.52f));
            }
        }

        TileBase BuriedHintTile(MaterialKind material)
        {
            if (_buriedHintTiles.TryGetValue(material, out var cached)) return cached;
            Color color = MaterialInfo.HintColor(material);
            color.a = 0.54f;
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.hideFlags = HideFlags.DontSave;
            tile.sprite = SpriteFactory.Diamond(Color.white);
            tile.color = color;
            tile.flags = TileFlags.LockColor;
            _buriedHintTiles[material] = tile;
            return tile;
        }

        void HideBuriedHint(int x, int y)
        {
            if (_buriedHintMap != null)
                _buriedHintMap.SetTile(ToCell(x, y), null);
        }

        /// <summary>
        /// 발굴 전 유물 전용 이미지를 처음부터 땅 위, 포그 아래에 배치한다.
        /// 플레이어 시야 안에서는 흙 속 유물의 외형이 보이지만 먼 어둠 너머에서는 가려진다.
        /// </summary>
        void BuildBuriedRelicMarkers()
        {
            foreach (var (cell, _) in _grid.AllRelics)
            {
                ShowRelicMarker(cell.x, cell.y);
            }
        }

        /// <summary>땅속에 묻힌 상태부터 발굴 직전까지 보여 주는 유물 이미지.</summary>
        void ShowRelicMarker(int x, int y)
        {
            var key = new Vector2Int(x, y);
            if (_relicMarkers.ContainsKey(key)) return;
            if (!_grid.TryPeekRelic(x, y, out int relicId)) return;

            var def = RelicCatalog.Get(relicId);
            var go = _factory.Create<SpriteRenderer>(PrefabId.BuriedRelic, _markerRoot, $"Relic({x},{y})").gameObject;
            // 발굴본은 bottom-center 피벗이라 칸 바닥에 앉힌다.
            go.transform.position = WorldGrid.CellToWorld(x, y)
                + Vector3.down * (0.5f * GameConfig.CellSize);
            go.transform.localScale = Vector3.one * GameConfig.RelicBuriedScale;

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = RelicSpriteCatalog.Buried(def);
            sr.sortingOrder = OrderTreasure + 1;
            if (sr.sprite == null)
            {
                sr.sprite = SpriteFactory.Diamond(Color.white);
                // 시트를 못 읽었을 때의 대체 표시. 암석 금테와 같은 금색으로 둬야
                // "여기 유물이 있다" 가 쓰레기 광점과 헷갈리지 않는다.
                sr.color = new Color(0.96f, 0.81f, 0.41f, 1f);
            }

            _relicMarkers[key] = go;
        }

        void HandleRelicRemoved(int x, int y)
        {
            var key = new Vector2Int(x, y);
            HideBuriedHint(x, y);
            if (_relicMarkers.TryGetValue(key, out var go))
            {
                Destroy(go);
                _relicMarkers.Remove(key);
            }
        }

        void ShowTreasureMarker(int x, int y)
        {
            var key = new Vector2Int(x, y);
            if (_treasureMarkers.ContainsKey(key)) return;

            Color color = new Color32(255, 214, 96, 255);
            if (_grid.TryPeekTreasure(x, y, out var item)) color = item.Color;

            var go = _factory.Create<SpriteRenderer>(PrefabId.Treasure, _markerRoot, $"Treasure({x},{y})").gameObject;
            go.transform.position = WorldGrid.CellToWorld(x, y);

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sortingOrder = OrderTreasure;

            Sprite pixelArt = _grid.TryPeekTreasure(x, y, out item)
                ? TrashSpriteCatalog.For(item)
                : null;
            if (pixelArt != null)
            {
                sr.sprite = pixelArt;
                sr.color = Color.white;
                float maxSize = Mathf.Max(pixelArt.bounds.size.x, pixelArt.bounds.size.y);
                go.transform.localScale = Vector3.one *
                    (0.78f * GameConfig.CellSize / Mathf.Max(0.01f, maxSize));
            }
            else
            {
                sr.sprite = SpriteFactory.Diamond(Color.white);
                sr.color = color;
                go.transform.localScale = Vector3.one * (0.6f * GameConfig.CellSize);
            }

            _treasureMarkers[key] = go;
        }

        void HandleTreasureRemoved(int x, int y)
        {
            var key = new Vector2Int(x, y);
            HideBuriedHint(x, y);
            if (_treasureMarkers.TryGetValue(key, out var go))
            {
                Destroy(go);
                _treasureMarkers.Remove(key);
            }
        }

        // ------------------------------------------------------------------
        public void ShowMine(MineSite site)
        {
            if (_mineMarkers.ContainsKey(site.Id)) return;

            var go = new GameObject($"UndergroundRecycleStation#{site.Id}");
            go.transform.SetParent(_markerRoot, false);

            // 발견하며 비워진 구역을 여러 DB32 동굴 칸으로 채워 네모 한 장처럼 보이지 않게 한다.
            foreach (var cell in site.Cells)
            {
                var backdrop = _factory.Create<SpriteRenderer>(PrefabId.CaveTile, go.transform, $"CaveTile({cell.x},{cell.y})").gameObject;
                backdrop.transform.position = WorldGrid.CellToWorld(cell.x, cell.y);
                var bsr = backdrop.GetComponent<SpriteRenderer>();
                bsr.sprite = EnvironmentSpriteCatalog.CaveBackdrop(cell.x * 31 + cell.y * 17 + site.Id);
                bsr.sortingOrder = OrderMineArt;
            }

            Vector3 center = WorldGrid.RectCenterToWorld(site.Rect);
            AddBinArt(go.transform, center, Mathf.Min(1.1f, 0.58f * site.Rect.width), OrderMineArt + 1,
                $"UndergroundBin#{site.Id}");
            AddRecycleSign(go.transform, center + Vector3.up * (0.30f * GameConfig.CellSize),
                site.OreTier * 3 + site.Id, 0.48f, OrderMineArt + 2, $"RecycleSign#{site.Id}");

            _mineMarkers[site.Id] = go;
        }

        public void RefreshMine(MineSite site)
        {
            if (!_mineMarkers.TryGetValue(site.Id, out var go) || go == null) return;
            if (site.Reserve > 0) return;
            foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>())
                sr.color = new Color(0.45f, 0.45f, 0.45f, 1f);
        }

        // ---------------- 지상 시설 (시대에 따라 모습이 바뀐다) ----------------
        GameObject _upgradeShop;
        SpriteRenderer _dropoffRenderer;
        SpriteRenderer _dropoffSign;
        SpriteRenderer _shopSign;
        int _villageEra;
        Color _dropoffTint = Color.white;

        /// <summary>건물 꼭대기 위에 고정으로 떠 있는 간판.</summary>
        SpriteRenderer SpawnSign(Transform parent, Vector3 stand,
            VillageSpriteCatalog.SignKind kind, int order, string name)
        {
            var go = _factory.Create<SpriteRenderer>(PrefabId.Sign, parent, name).gameObject;
            go.transform.position = stand
                + Vector3.up * (VillageSpriteCatalog.SignLiftFor(kind)
                                * GameConfig.BuildingScale * GameConfig.CellSize);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = VillageSpriteCatalog.Sign(kind, _villageEra);
            sr.sortingOrder = order;
            return sr;
        }

        /// <summary>지상 건물이 지표면 위에 발을 딛는 위치 (bottom-center).</summary>
        static Vector3 SurfaceStand(RectInt rect) =>
            new Vector3((rect.xMin + rect.width * 0.5f) * GameConfig.CellSize,
                        -GameConfig.SurfaceRow * GameConfig.CellSize, 0f);

        /// <summary>지상 강화소(처리장).</summary>
        public void ShowBuilding(RectInt rect)
        {
            if (_upgradeShop != null) return;

            _upgradeShop = _factory.Create<SpriteRenderer>(PrefabId.Facility, _markerRoot, "UpgradeShop").gameObject;
            _upgradeShop.transform.position = SurfaceStand(rect);

            _upgradeShop.transform.localScale = Vector3.one * GameConfig.BuildingScale;
            var sr = _upgradeShop.GetComponent<SpriteRenderer>();
            sr.sprite = VillageSpriteCatalog.EraShop(_villageEra);
            sr.sortingOrder = OrderMine;

            _shopSign = SpawnSign(_upgradeShop.transform, SurfaceStand(rect),
                VillageSpriteCatalog.SignKind.Shop, OrderMine + 1, "ShopSign");
        }

        /// <summary>시대가 바뀌면 투입소·강화소도 그 시대 건물로 갈아끼운다.</summary>
        public void ApplyVillageEra(int era)
        {
            _villageEra = era;

            if (_upgradeShop != null)
            {
                var sr = _upgradeShop.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = VillageSpriteCatalog.EraShop(era);
            }
            if (_dropoffRenderer != null)
                _dropoffRenderer.sprite = VillageSpriteCatalog.EraDropoff(era);

            if (_shopSign != null)
                _shopSign.sprite = VillageSpriteCatalog.Sign(VillageSpriteCatalog.SignKind.Shop, era);
            if (_dropoffSign != null)
                _dropoffSign.sprite = VillageSpriteCatalog.Sign(VillageSpriteCatalog.SignKind.Dropoff, era);
        }

        /// <summary>시대 도약 연출 — 지상 시설도 같이 하얗게 탄다.</summary>
        public void FlashVillageBuildings(Color color)
        {
            if (_upgradeShop != null)
            {
                var sr = _upgradeShop.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = color;
            }
            if (_dropoffRenderer != null)
                _dropoffRenderer.color = new Color(
                    color.r * _dropoffTint.r, color.g * _dropoffTint.g, color.b * _dropoffTint.b, 1f);
            if (_shopSign != null) _shopSign.color = color;
            if (_dropoffSign != null) _dropoffSign.color = color;
        }

        /// <summary>쓰레기 투입소(분리수거장).</summary>
        public void ShowRecycleStation(RectInt rect, int level = 0)
        {
            if (_surfaceRecycle == null)
            {
                _surfaceRecycle = new GameObject("SurfaceTrashDropOff");
                _surfaceRecycle.transform.SetParent(_markerRoot, false);
                Vector3 stand = SurfaceStand(rect);
                _surfaceRecycle.transform.position = stand;

                _surfaceRecycle.transform.localScale = Vector3.one * GameConfig.BuildingScale;

                var binGo = _factory.Create<SpriteRenderer>(PrefabId.Facility, _surfaceRecycle.transform, "SurfaceBin").gameObject;
                binGo.transform.position = stand;
                _dropoffRenderer = binGo.GetComponent<SpriteRenderer>();
                _dropoffRenderer.sprite = VillageSpriteCatalog.EraDropoff(_villageEra);
                _dropoffRenderer.sortingOrder = OrderMine;

                _dropoffSign = SpawnSign(_surfaceRecycle.transform, stand,
                    VillageSpriteCatalog.SignKind.Dropoff, OrderMine + 1, "DropoffSign");

                // 분리수거장 업그레이드 단계를 간판 위 작은 배지로 보여 준다.
                AddRecycleSign(_surfaceRecycle.transform,
                    stand + Vector3.up
                        * (5.1f * GameConfig.BuildingScale * GameConfig.CellSize), 3, 0.55f,
                    OrderMine + 2, "SurfaceRecycleSign");
            }

            var badge = _surfaceRecycle.transform.Find("SurfaceRecycleSign")?.GetComponent<SpriteRenderer>();
            _dropoffTint = level > 0 ? new Color(0.78f, 1f, 0.82f, 1f) : Color.white;
            if (_dropoffRenderer != null) _dropoffRenderer.color = _dropoffTint;
            if (badge != null)
            {
                badge.sprite = EnvironmentSpriteCatalog.RecycleItem(level <= 0 ? 3 : level == 1 ? 7 : 11);
                badge.color = Color.white;
            }
        }

        /// <summary>재활용된 종류별 결과물을 공장 위로 잠깐 띄운다.</summary>
        public void ShowRecyclingOutputs(IEnumerable<MaterialKind> materials)
        {
            if (_surfaceRecycle == null) return;
            var outputMaterials = new List<MaterialKind>(materials);
            float centerOffset = (outputMaterials.Count - 1) * 0.5f;
            for (int index = 0; index < outputMaterials.Count; index++)
            {
                MaterialKind material = outputMaterials[index];
                var go = _factory.Create<SpriteRenderer>(PrefabId.RecyclingOutput, _markerRoot, $"RecycledOutput({material})").gameObject;
                go.transform.position = _surfaceRecycle.transform.position
                    + new Vector3((index - centerOffset) * 0.34f, 4.2f, 0f) * GameConfig.CellSize;
                go.transform.localScale = Vector3.one * 0.8f;
                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = EnvironmentSpriteCatalog.RecycledProduct(material);
                sr.sortingOrder = OrderHighlight;
            }
        }

        void AddBinArt(Transform parent, Vector3 worldPosition, float scale,
            int sortingOrder, string name)
        {
            var bin = _factory.Create<SpriteRenderer>(PrefabId.Facility, parent, name).gameObject;
            bin.transform.position = worldPosition;
            bin.transform.localScale = Vector3.one * scale;
            var sr = bin.GetComponent<SpriteRenderer>();
            sr.sprite = EnvironmentSpriteCatalog.SurfaceBin();
            sr.sortingOrder = sortingOrder;
        }

        void AddRecycleSign(Transform parent, Vector3 worldPosition, int iconIndex,
            float scale, int sortingOrder, string name)
        {
            var sign = _factory.Create<SpriteRenderer>(PrefabId.Sign, parent, name).gameObject;
            sign.transform.position = worldPosition;
            sign.transform.localScale = Vector3.one * scale;
            var sr = sign.GetComponent<SpriteRenderer>();
            sr.sprite = EnvironmentSpriteCatalog.RecycleItem(iconIndex);
            sr.sortingOrder = sortingOrder;
        }

        // ---------- 탐지기 핑 ----------
        readonly List<GameObject> _pings = new List<GameObject>();
        float _pingTimer;

        void LateUpdate()
        {
            if (!Application.isPlaying) return;

            var game = GameManager.Instance;
            if (game != null && game.Player != null)
                UpdateFog(game.Player.transform.position, game.VisionRadius, game.VisionFadeRadius);
        }

        /// <summary>radius 안의 쓰레기를 duration 초 동안 벽 너머로 보여준다.</summary>
        public void PingTrash(Vector2Int center, int radius, float duration)
        {
            ClearPings();
            _pingTimer = duration;

            foreach (var (cell, item) in _grid.TreasuresNear(center, radius))
            {
                var go = _factory.Create<SpriteRenderer>(PrefabId.DetectorPing, _markerRoot, $"Ping({cell.x},{cell.y})").gameObject;
                go.transform.position = WorldGrid.CellToWorld(cell.x, cell.y);
                go.transform.localScale = Vector3.one * (0.5f * GameConfig.CellSize);

                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = SpriteFactory.Diamond(Color.white);
                sr.color = new Color(item.Color.r, item.Color.g, item.Color.b, 0.9f);
                sr.sortingOrder = OrderHighlight;   // 포그 위에 보인다
                _pings.Add(go);
            }
        }

        void Update()
        {
            if (_pingTimer > 0f)
            {
                _pingTimer -= Time.deltaTime;

                // 사라지기 직전 깜빡임
                float a = Mathf.Clamp01(_pingTimer / 0.8f);
                foreach (var go in _pings)
                {
                    if (go == null) continue;
                    var sr = go.GetComponent<SpriteRenderer>();
                    var c = sr.color; c.a = 0.35f + 0.55f * a;
                    sr.color = c;
                }

                if (_pingTimer <= 0f) ClearPings();
            }
        }

        void ClearPings()
        {
            foreach (var go in _pings)
                if (go != null) Destroy(go);
            _pings.Clear();
        }

        public static int PlayerSortingOrder => OrderPlayer;
        public static int WorkerSortingOrder => OrderWorker;
    }






}
