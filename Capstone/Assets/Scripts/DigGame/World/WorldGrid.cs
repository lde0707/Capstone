using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 월드의 순수 데이터 모델. (x, y) 에서 y=0 이 가장 위(하늘)이고 아래로 갈수록 y가 커진다.
    /// 월드 좌표는 CellSize 를 적용해 (x, -y) 로 매핑된다.
    /// </summary>
    public class WorldGrid
    {
        public readonly int Width;
        public readonly int Height;

        readonly TileKind[] _tiles;
        readonly TileKind[] _dugFrom;
        readonly Dictionary<int, LootItem> _treasures = new Dictionary<int, LootItem>();
        readonly Dictionary<int, DigFeature> _digFeatures = new Dictionary<int, DigFeature>();
        readonly Dictionary<int, MineSite> _mineSites = new Dictionary<int, MineSite>();

        public System.Action<int, int, TileKind> OnTileChanged;
        public System.Action<int, int> OnTreasureRemoved;

        public WorldGrid(int width, int height)
        {
            Width = width;
            Height = height;
            _tiles = new TileKind[width * height];
            _dugFrom = new TileKind[width * height];
        }

        public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        int Index(int x, int y) => y * Width + x;

        public TileKind Get(int x, int y)
        {
            if (!InBounds(x, y)) return TileKind.Bedrock;
            return _tiles[Index(x, y)];
        }

        public void SetRaw(int x, int y, TileKind kind)
        {
            if (!InBounds(x, y)) return;
            int index = Index(x, y);
            _tiles[index] = kind;
            if (kind != TileKind.Empty) _dugFrom[index] = kind;
        }

        public void Set(int x, int y, TileKind kind)
        {
            if (!InBounds(x, y)) return;
            int index = Index(x, y);
            TileKind previous = _tiles[index];
            if (kind == TileKind.Empty && TileInfo.SupportsPassage(previous))
                _dugFrom[index] = previous;
            else if (kind != TileKind.Empty)
                _dugFrom[index] = kind;
            _tiles[index] = kind;
            OnTileChanged?.Invoke(x, y, kind);
        }

        /// <summary>
        /// 현재 칸을 파기 직전의 정확한 지층 종류. 굴착되지 않은 칸은 현재 종류를 반환한다.
        /// 지층 경계가 x축 노이즈로 흔들리므로 깊이만으로 원래 종류를 추정하면 안 된다.
        /// </summary>
        public TileKind OriginalGroundAt(int x, int y)
        {
            if (!InBounds(x, y)) return TileKind.Bedrock;
            int index = Index(x, y);
            TileKind current = _tiles[index];
            if (current != TileKind.Empty) return current;

            TileKind original = _dugFrom[index];
            return TileInfo.SupportsPassage(original) ? original : TileInfo.ForZone(ZoneAt(y));
        }

        public bool IsWalkable(int x, int y) => TileInfo.IsWalkable(Get(x, y));

        // ---------- 깊이 / 구역 ----------
        public static int DepthOf(int y) => y - GameConfig.SurfaceRow;
        public Zone ZoneAt(int y) => ZoneInfo.FromDepth(DepthOf(y));

        // ---------- 보물 ----------
        public void PutTreasure(int x, int y, LootItem item) => _treasures[Index(x, y)] = item;

        public bool TryTakeTreasure(int x, int y, out LootItem item)
        {
            int i = Index(x, y);
            if (_treasures.TryGetValue(i, out item))
            {
                _treasures.Remove(i);
                OnTreasureRemoved?.Invoke(x, y);
                return true;
            }
            item = default;
            return false;
        }

        public bool HasTreasure(int x, int y) => _treasures.ContainsKey(Index(x, y));

        /// <summary>보물을 꺼내지 않고 들여다본다 (표시용).</summary>
        public bool TryPeekTreasure(int x, int y, out LootItem item) => _treasures.TryGetValue(Index(x, y), out item);

        /// <summary>월드 전체 쓰레기를 좌표와 함께 나열한다.</summary>
        public IEnumerable<(Vector2Int cell, LootItem item)> AllTreasures
        {
            get
            {
                foreach (var kv in _treasures)
                    yield return (new Vector2Int(kv.Key % Width, kv.Key / Width), kv.Value);
            }
        }

        // ---------- 특수 지층 ----------
        public void PutDigFeature(int x, int y, DigFeature feature)
        {
            if (!InBounds(x, y)) return;
            int index = Index(x, y);
            if (feature == DigFeature.None) _digFeatures.Remove(index);
            else _digFeatures[index] = feature;
        }

        public DigFeature DigFeatureAt(int x, int y)
        {
            if (!InBounds(x, y)) return DigFeature.None;
            return _digFeatures.TryGetValue(Index(x, y), out var feature)
                ? feature : DigFeature.None;
        }

        public void ClearDigFeature(int x, int y)
        {
            if (InBounds(x, y)) _digFeatures.Remove(Index(x, y));
        }

        public IEnumerable<(Vector2Int cell, DigFeature feature)> AllDigFeatures
        {
            get
            {
                foreach (var pair in _digFeatures)
                    yield return (new Vector2Int(pair.Key % Width, pair.Key / Width), pair.Value);
            }
        }

        /// <summary>중심에서 radius(체비쇼프) 안에 있는 쓰레기 칸들을 나열한다.</summary>
        public IEnumerable<(Vector2Int cell, LootItem item)> TreasuresNear(Vector2Int center, int radius)
        {
            foreach (var kv in _treasures)
            {
                int x = kv.Key % Width, y = kv.Key / Width;
                if (Mathf.Max(Mathf.Abs(x - center.x), Mathf.Abs(y - center.y)) <= radius)
                    yield return (new Vector2Int(x, y), kv.Value);
            }
        }

        /// <summary>radius 안에서 가장 비싼 쓰레기 칸. 없으면 false.</summary>
        public bool FindBestTreasure(Vector2Int center, int radius, out Vector2Int best)
        {
            best = default;
            int bestValue = -1;
            foreach (var (cell, item) in TreasuresNear(center, radius))
            {
                if (item.Value > bestValue)
                {
                    bestValue = item.Value;
                    best = cell;
                }
            }
            return bestValue >= 0;
        }

        // ---------- 광산 ----------
        public void PutMineSite(int x, int y, MineSite site) => _mineSites[Index(x, y)] = site;

        public bool TryGetMineSite(int x, int y, out MineSite site) => _mineSites.TryGetValue(Index(x, y), out site);

        public IEnumerable<MineSite> AllMineSites => _mineSites.Values;

        // ---------- 좌표 변환 ----------
        public static Vector3 GridToWorld(float x, float y) =>
            new Vector3(x * GameConfig.CellSize, -y * GameConfig.CellSize, 0f);

        public static Vector3 CellToWorld(int x, int y) => GridToWorld(x + 0.5f, y + 0.5f);

        public static Vector3 RectCenterToWorld(RectInt rect) =>
            GridToWorld(rect.xMin + rect.width * 0.5f, rect.yMin + rect.height * 0.5f);

        public static Vector2Int WorldToCell(Vector3 p) =>
            new Vector2Int(
                Mathf.FloorToInt(p.x / GameConfig.CellSize),
                Mathf.FloorToInt(-p.y / GameConfig.CellSize));
    }
}
