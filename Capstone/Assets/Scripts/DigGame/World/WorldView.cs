using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DigGame
{
    /// <summary>
    /// WorldGrid 를 OPP 연속 암반과 16방향 통로 오토타일로 그린다.
    /// 지하 시야 제한(포그), 굴착 사거리 하이라이트도 여기서 담당한다.
    /// </summary>
    public class WorldView : MonoBehaviour
    {
        // 정렬 순서
        const int OrderGround = 0;
        const int OrderTreasure = 30;  // 보물은 어둠에 가려진다 (직접 찾아야 함)
        const int OrderFog = 45;
        const int OrderMine = 46;      // 발견한 지하 수거장과 고용한 일꾼은 어둠 위에 계속 보인다
        const int OrderWorker = 47;
        const int OrderHighlight = 48;
        const int OrderPlayer = 50;

        WorldGrid _grid;
        Tilemap _ground;

        readonly Dictionary<TileKind, TileBase> _flatTiles = new Dictionary<TileKind, TileBase>();
        readonly Dictionary<TileKind, TileBase[]> _terrainTiles = new Dictionary<TileKind, TileBase[]>();
        readonly TileBase[] _passageTiles = new TileBase[16 * 16];

        readonly Dictionary<Vector2Int, GameObject> _treasureMarkers = new Dictionary<Vector2Int, GameObject>();
        readonly Dictionary<int, GameObject> _mineMarkers = new Dictionary<int, GameObject>();

        Transform _markerRoot;
        SpriteRenderer _highlight;
        SpriteRenderer _fogRenderer;
        Material _fogMaterial;
        GameObject _surfaceRecycle;

        // ------------------------------------------------------------------
        public void Build(WorldGrid grid)
        {
            _grid = grid;

            var gridGo = new GameObject("Tilemap Grid");
            gridGo.transform.SetParent(transform, false);
            var uGrid = gridGo.AddComponent<UnityEngine.Grid>();
            uGrid.cellSize = new Vector3(GameConfig.CellSize, GameConfig.CellSize, 0f);

            _ground = MakeTilemap(gridGo.transform, "Ground", OrderGround);

            _markerRoot = new GameObject("Markers").transform;
            _markerRoot.SetParent(transform, false);

            BuildTileAssets();
            PaintGround();
            BuildSmoothFog();
            BuildHighlight();

            _grid.OnTileChanged += HandleTileChanged;
            _grid.OnTreasureRemoved += HandleTreasureRemoved;
        }

        void OnDestroy()
        {
            if (_grid != null)
            {
                _grid.OnTileChanged -= HandleTileChanged;
                _grid.OnTreasureRemoved -= HandleTreasureRemoved;
            }

            // 런타임에 만든 Tile 에셋 정리 (재시작 시 누수 방지)
            foreach (var t in _flatTiles.Values)
                DestroyGenerated(t);
            foreach (var set in _terrainTiles.Values)
                for (int i = 0; i < set.Length; i++) DestroyGenerated(set[i]);
            for (int i = 0; i < _passageTiles.Length; i++) DestroyGenerated(_passageTiles[i]);
            _flatTiles.Clear();
            _terrainTiles.Clear();

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

            // 열린 방향 16종 x 연속 배경 패턴 16종. 파낸 칸 자체가 통로가 된다.
            for (int mask = 0; mask < 16; mask++)
            {
                for (int pattern = 0; pattern < 16; pattern++)
                {
                    int index = mask * 16 + pattern;
                    var tile = ScriptableObject.CreateInstance<Tile>();
                    tile.hideFlags = HideFlags.DontSave;
                    tile.sprite = EnvironmentSpriteCatalog.PassageForMask(mask, pattern)
                               ?? SpriteFactory.Solid(TileInfo.ColorOf(TileKind.Empty));
                    tile.color = Color.white;
                    _passageTiles[index] = tile;
                }
            }

            foreach (TileKind kind in System.Enum.GetValues(typeof(TileKind)))
            {
                if (kind == TileKind.Sky || kind == TileKind.Empty) continue;

                // 128px 원본을 4x4로 잘라 월드 좌표에 맞춰 다시 이어 붙인다.
                var set = new TileBase[16];
                for (int i = 0; i < set.Length; i++)
                {
                    var tile = ScriptableObject.CreateInstance<Tile>();
                    tile.hideFlags = HideFlags.DontSave;
                    tile.sprite = EnvironmentSpriteCatalog.GroundFill(i)
                                  ?? SpriteFactory.Noisy(TileInfo.ColorOf(kind), i + (int)kind * 977);
                    tile.color = TerrainTint(kind);
                    set[i] = tile;
                }
                _terrainTiles[kind] = set;
            }
        }

        static Color TerrainTint(TileKind kind)
        {
            switch (kind)
            {
                case TileKind.SoftDirt: return new Color32(255, 242, 205, 255);
                case TileKind.Clay: return new Color32(245, 170, 135, 255);
                case TileKind.Stone: return new Color32(175, 190, 210, 255);
                case TileKind.HardRock: return new Color32(115, 130, 175, 255);
                case TileKind.Bedrock: return new Color32(70, 72, 92, 255);
                default: return Color.white;
            }
        }

        /// <summary>grid 좌표(y가 아래로 증가) → 타일맵 셀 좌표(y가 위로 증가).</summary>
        static Vector3Int ToCell(int x, int y) => new Vector3Int(x, -y - 1, 0);

        void PaintGround()
        {
            int w = _grid.Width, h = _grid.Height;
            var positions = new Vector3Int[w * h];
            var tiles = new TileBase[w * h];

            int i = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    positions[i] = ToCell(x, y);
                    tiles[i] = TileAt(x, y);
                    i++;
                }
            }
            _ground.SetTiles(positions, tiles);
        }

        TileBase TileAt(int x, int y)
        {
            TileKind kind = _grid.Get(x, y);
            if (kind == TileKind.Sky) return _flatTiles[kind];

            int pattern = PositiveMod(x, 4) + PositiveMod(y, 4) * 4;
            if (kind == TileKind.Empty)
                return _passageTiles[PassageMask(x, y) * 16 + pattern];

            return _terrainTiles.TryGetValue(kind, out var set) ? set[pattern] : null;
        }

        static int PositiveMod(int value, int divisor)
        {
            int result = value % divisor;
            return result < 0 ? result + divisor : result;
        }

        int PassageMask(int x, int y)
        {
            int mask = 0;
            if (IsPassage(x, y - 1)) mask |= 1; // 위
            if (IsPassage(x + 1, y)) mask |= 2; // 오른쪽
            if (IsPassage(x, y + 1)) mask |= 4; // 아래
            if (IsPassage(x - 1, y)) mask |= 8; // 왼쪽
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
                    if (_grid.InBounds(tx, ty)) _ground.SetTile(ToCell(tx, ty), TileAt(tx, ty));
                }

            if (TileInfo.IsWalkable(kind) && _grid.HasTreasure(x, y))
                ShowTreasureMarker(x, y);
        }

        void ShowTreasureMarker(int x, int y)
        {
            var key = new Vector2Int(x, y);
            if (_treasureMarkers.ContainsKey(key)) return;

            Color color = new Color32(255, 214, 96, 255);
            if (_grid.TryPeekTreasure(x, y, out var item)) color = item.Color;

            var go = new GameObject($"Treasure({x},{y})");
            go.transform.SetParent(_markerRoot, false);
            go.transform.position = WorldGrid.CellToWorld(x, y);

            var sr = go.AddComponent<SpriteRenderer>();
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

            go.AddComponent<Bobber>();
            _treasureMarkers[key] = go;
        }

        void HandleTreasureRemoved(int x, int y)
        {
            var key = new Vector2Int(x, y);
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
                var backdrop = new GameObject($"CaveTile({cell.x},{cell.y})");
                backdrop.transform.SetParent(go.transform, false);
                backdrop.transform.position = WorldGrid.CellToWorld(cell.x, cell.y);
                var bsr = backdrop.AddComponent<SpriteRenderer>();
                bsr.sprite = EnvironmentSpriteCatalog.CaveBackdrop(cell.x * 31 + cell.y * 17 + site.Id);
                bsr.sortingOrder = OrderMine;
            }

            Vector3 center = WorldGrid.RectCenterToWorld(site.Rect);
            AddBinArt(go.transform, center, Mathf.Min(1.1f, 0.58f * site.Rect.width), OrderMine + 1,
                $"UndergroundBin#{site.Id}");
            AddRecycleSign(go.transform, center + Vector3.up * (0.30f * GameConfig.CellSize),
                site.OreTier * 3 + site.Id, 0.48f, OrderMine + 2, $"RecycleSign#{site.Id}");

            _mineMarkers[site.Id] = go;
        }

        public void RefreshMine(MineSite site)
        {
            if (!_mineMarkers.TryGetValue(site.Id, out var go) || go == null) return;
            if (site.Reserve > 0) return;
            foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>())
                sr.color = new Color(0.45f, 0.45f, 0.45f, 1f);
        }

        /// <summary>지상 강화 건물.</summary>
        public void ShowBuilding(RectInt rect)
        {
            var go = new GameObject("UpgradeShop");
            go.transform.SetParent(_markerRoot, false);
            go.transform.position = WorldGrid.RectCenterToWorld(rect);
            go.transform.localScale = new Vector3(
                rect.width * GameConfig.CellSize, rect.height * GameConfig.CellSize, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Framed(new Color32(180, 130, 80, 255), new Color32(250, 225, 170, 255));
            sr.sortingOrder = OrderMine;

            // 지붕 느낌의 작은 표식
            var roof = new GameObject("Sign");
            roof.transform.SetParent(go.transform, false);
            roof.transform.localPosition = new Vector3(0f, 0.28f, 0f);
            roof.transform.localScale = new Vector3(0.4f, 0.22f, 1f);
            var rsr = roof.AddComponent<SpriteRenderer>();
            rsr.sprite = SpriteFactory.Solid(new Color32(255, 235, 150, 255));
            rsr.sortingOrder = OrderTreasure;
        }

        /// <summary>분리수거장 건물.</summary>
        public void ShowRecycleStation(RectInt rect, int level = 0)
        {
            if (_surfaceRecycle == null)
            {
                _surfaceRecycle = new GameObject("SurfaceTrashDropOff");
                _surfaceRecycle.transform.SetParent(_markerRoot, false);
                Vector3 center = WorldGrid.RectCenterToWorld(rect);
                AddBinArt(_surfaceRecycle.transform, center, 1.28f, OrderMine, "SurfaceBin");
                AddRecycleSign(_surfaceRecycle.transform,
                    center + Vector3.up * (0.34f * GameConfig.CellSize), 3, 0.55f,
                    OrderMine + 1, "SurfaceRecycleSign");
            }

            var bin = _surfaceRecycle.transform.Find("SurfaceBin")?.GetComponent<SpriteRenderer>();
            var sign = _surfaceRecycle.transform.Find("SurfaceRecycleSign")?.GetComponent<SpriteRenderer>();
            if (bin != null)
                bin.color = level > 0 ? new Color32(195, 255, 205, 255) : Color.white;
            if (sign != null)
            {
                sign.sprite = EnvironmentSpriteCatalog.RecycleItem(level <= 0 ? 3 : level == 1 ? 7 : 11);
                sign.color = Color.white;
            }
        }

        static void AddBinArt(Transform parent, Vector3 worldPosition, float scale,
            int sortingOrder, string name)
        {
            var bin = new GameObject(name);
            bin.transform.SetParent(parent, false);
            bin.transform.position = worldPosition;
            bin.transform.localScale = Vector3.one * scale;
            var sr = bin.AddComponent<SpriteRenderer>();
            sr.sprite = EnvironmentSpriteCatalog.SurfaceBin();
            sr.sortingOrder = sortingOrder;
        }

        static void AddRecycleSign(Transform parent, Vector3 worldPosition, int iconIndex,
            float scale, int sortingOrder, string name)
        {
            var sign = new GameObject(name);
            sign.transform.SetParent(parent, false);
            sign.transform.position = worldPosition;
            sign.transform.localScale = Vector3.one * scale;
            var sr = sign.AddComponent<SpriteRenderer>();
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
                var go = new GameObject($"Ping({cell.x},{cell.y})");
                go.transform.SetParent(_markerRoot, false);
                go.transform.position = WorldGrid.CellToWorld(cell.x, cell.y);
                go.transform.localScale = Vector3.one * (0.5f * GameConfig.CellSize);

                var sr = go.AddComponent<SpriteRenderer>();
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

    /// <summary>위아래로 살짝 떠다니는 연출.</summary>
    public class Bobber : MonoBehaviour
    {
        Vector3 _base;
        float _phase;

        void Start()
        {
            _base = transform.position;
            _phase = Random.value * 6.28f;
        }

        void Update()
        {
            transform.position = _base + new Vector3(0f, Mathf.Sin(Time.time * 3f + _phase) * 0.07f, 0f);
        }
    }
}
