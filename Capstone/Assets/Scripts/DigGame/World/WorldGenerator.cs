using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 층(層) 단위로 하나의 땅 종류만 배치한다.
    /// 층 경계는 노이즈로 살짝 흔들어서 직선이 되지 않게 한다.
    /// </summary>
    public static class WorldGenerator
    {
        static readonly Vector2Int[] MineSizes =
        {
            new Vector2Int(2, 2),
            new Vector2Int(2, 3),
            new Vector2Int(3, 2),
            new Vector2Int(3, 3)
        };

        public static WorldGrid Generate(int seed)
        {
            var rng = new System.Random(seed);
            var grid = new WorldGrid(GameConfig.WorldWidth, GameConfig.WorldHeight);

            float n1 = seed % 1000 * 0.37f;
            float n2 = n1 + 41.7f;
            float n3 = n1 + 93.1f;

            // ---------- 1) 지층 ----------
            for (int x = 0; x < grid.Width; x++)
            {
                int b1 = Boundary(GameConfig.Layer1Depth, x, n1);
                int b2 = Boundary(GameConfig.Layer2Depth, x, n2);
                int b3 = Boundary(GameConfig.Layer3Depth, x, n3);

                for (int y = 0; y < grid.Height; y++)
                {
                    if (y < GameConfig.SurfaceRow)
                    {
                        grid.SetRaw(x, y, TileKind.Sky);
                        continue;
                    }

                    if (y >= grid.Height - 2 || x == 0 || x == grid.Width - 1)
                    {
                        grid.SetRaw(x, y, TileKind.Bedrock);
                        continue;
                    }

                    int depth = WorldGrid.DepthOf(y);
                    TileKind kind;
                    if (depth < GameConfig.GrassLayerRows) kind = TileKind.Grass;
                    else if (depth < b1) kind = TileKind.SoftDirt;
                    else if (depth < b2) kind = TileKind.Clay;
                    else if (depth < b3) kind = TileKind.Stone;
                    else kind = TileKind.HardRock;

                    grid.SetRaw(x, y, kind);
                }
            }

            // 스폰 근처의 잔디 지반을 관통하는 고정 입구. 잔디 대신 이 통로로만 진입한다.
            CarveSurfaceEntrance(grid);

            // ---------- 2) 광산 (층마다 일정 개수, 서로 떨어뜨려서) ----------
            var placed = new List<MineSite>();
            int mineId = 1;

            foreach (Zone zone in new[] { Zone.Surface, Zone.Shallow, Zone.Deep, Zone.Abyss })
            {
                int made = 0;
                int attempts = 0;
                while (made < GameConfig.MinesPerLayer && attempts++ < GameConfig.MinePlacementAttempts)
                {
                    var size = MineSizes[rng.Next(MineSizes.Length)];
                    if (!TryPickMineRect(grid, zone, size, rng, out RectInt rect)) continue;
                    if (TooClose(rect, placed)) continue;

                    int tier = OreTierFor(zone);
                    int area = rect.width * rect.height;
                    int reserve = area * (7 + (int)zone * 4) + rng.Next(10);

                    var site = new MineSite(mineId++, rect, zone, tier, reserve);
                    foreach (var c in site.Cells) grid.PutMineSite(c.x, c.y, site);

                    placed.Add(site);
                    made++;
                }
            }

            // ---------- 3) 보물 ----------
            for (int y = GameConfig.SurfaceRow; y < grid.Height - 2; y++)
            {
                for (int x = 1; x < grid.Width - 1; x++)
                {
                    var kind = grid.Get(x, y);
                    if (!TileInfo.IsDiggable(kind)) continue;
                    if (grid.TryGetMineSite(x, y, out _)) continue; // 광산 칸에는 보물 없음

                    Zone zone = TileInfo.ZoneOf(kind);
                    if (rng.NextDouble() < ZoneInfo.TreasureChance(zone))
                        grid.PutTreasure(x, y, LootTable.RollTreasure(zone, rng));
                }
            }

            // 확률 운이 나빠도 각 층에 1칸 쓰레기가 반드시 흩어져 있도록 보장한다.
            PlaceGuaranteedSingleSlotTrash(grid, rng);
            PlaceStarterTrash(grid, rng);
            PlaceDigFeatures(grid, rng);
            PlaceRelics(grid, rng);

            return grid;
        }

        /// <summary>
        /// 유물 58종을 층별로 흩뿌린다.
        /// 한 유물은 <see cref="GameConfig.RelicCopiesPerKind"/> 군데에 묻혀 있어서
        /// 어디를 파도 뭔가 하나쯤은 나오지만, 같은 유물을 두 번 캐면 자동 환전된다.
        /// 서로 너무 붙어 있지 않게 최소 간격을 둔다.
        /// </summary>
        static void PlaceRelics(WorldGrid grid, System.Random rng)
        {
            var placed = new List<Vector2Int>();

            for (int depth = 1; depth <= 4; depth++)
            {
                var defs = RelicCatalog.OfDepth(depth);
                if (defs == null || defs.Count == 0) continue;

                Zone zone = depth == 1 ? Zone.Surface
                    : depth == 2 ? Zone.Shallow
                    : depth == 3 ? Zone.Deep : Zone.Abyss;

                for (int i = 0; i < defs.Count; i++)
                {
                    for (int copy = 0; copy < GameConfig.RelicCopiesPerKind; copy++)
                    {
                        if (TryPickRelicCell(grid, zone, rng, placed, out var cell))
                        {
                            grid.PutRelic(cell.x, cell.y, defs[i].Id);
                            placed.Add(cell);
                        }
                    }
                }
            }
        }

        static bool TryPickRelicCell(WorldGrid grid, Zone zone, System.Random rng,
            List<Vector2Int> placed, out Vector2Int cell)
        {
            cell = default;
            for (int attempt = 0; attempt < GameConfig.RelicPlacementAttempts; attempt++)
            {
                int x = rng.Next(2, grid.Width - 2);
                int y = rng.Next(GameConfig.StarterTrashRow + 2, grid.Height - 3);

                TileKind kind = grid.Get(x, y);
                if (!TileInfo.IsDiggable(kind) || TileInfo.ZoneOf(kind) != zone) continue;
                if (grid.TryGetMineSite(x, y, out _)) continue;
                if (grid.HasRelic(x, y)) continue;

                // 유물끼리 몰려 있으면 한 번에 쓸어 담게 되므로 떨어뜨린다.
                bool tooClose = false;
                for (int i = 0; i < placed.Count; i++)
                {
                    if (Mathf.Max(Mathf.Abs(placed[i].x - x), Mathf.Abs(placed[i].y - y))
                        < GameConfig.RelicMinSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                // 유물 칸은 쓰레기·특수 지층과 겹치지 않게 비워 둔다.
                if (grid.HasTreasure(x, y)) grid.TryTakeTreasure(x, y, out _);
                grid.ClearDigFeature(x, y);

                cell = new Vector2Int(x, y);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 색으로 미리 판단할 수 있는 위험/보상 지층을 배치한다.
        /// 시작 보장 줄과 광산은 건드리지 않아 첫날 진행과 시설 발견을 막지 않는다.
        /// </summary>
        static void PlaceDigFeatures(WorldGrid grid, System.Random rng)
        {
            for (int y = GameConfig.StarterTrashRow + 1; y < grid.Height - 2; y++)
            {
                for (int x = 1; x < grid.Width - 1; x++)
                {
                    TileKind kind = grid.Get(x, y);
                    if (!TileInfo.IsDiggable(kind) || grid.TryGetMineSite(x, y, out _)) continue;

                    Zone zone = TileInfo.ZoneOf(kind);
                    float gasChance = zone == Zone.Deep ? GameConfig.GasPocketChanceDeep
                        : zone == Zone.Abyss ? GameConfig.GasPocketChanceAbyss : 0f;
                    double roll = rng.NextDouble();
                    DigFeature feature;
                    if (roll < GameConfig.LooseSoilChance)
                        feature = DigFeature.LooseSoil;
                    else if (roll < GameConfig.LooseSoilChance + GameConfig.CompactCacheChance)
                        feature = DigFeature.CompactCache;
                    else if (roll < GameConfig.LooseSoilChance + GameConfig.CompactCacheChance + gasChance)
                        feature = DigFeature.GasPocket;
                    else
                        continue;

                    grid.PutDigFeature(x, y, feature);
                    if (feature == DigFeature.CompactCache || feature == DigFeature.GasPocket)
                        grid.PutTreasure(x, y, LootTable.RollFeatureTreasure(zone, feature, rng));
                }
            }
        }

        static void PlaceGuaranteedSingleSlotTrash(WorldGrid grid, System.Random rng)
        {
            foreach (Zone zone in new[] { Zone.Surface, Zone.Shallow, Zone.Deep, Zone.Abyss })
            {
                int placed = 0;
                int attempts = 0;
                int maxAttempts = grid.Width * grid.Height * 2;

                while (placed < GameConfig.GuaranteedSingleSlotTrashPerLayer && attempts++ < maxAttempts)
                {
                    int x = rng.Next(1, grid.Width - 1);
                    int y = rng.Next(GameConfig.SurfaceRow, grid.Height - 2);
                    TileKind kind = grid.Get(x, y);

                    if (!TileInfo.IsDiggable(kind) || TileInfo.ZoneOf(kind) != zone) continue;
                    if (grid.TryGetMineSite(x, y, out _) || grid.HasTreasure(x, y)) continue;

                    grid.PutTreasure(x, y, LootTable.RollSingleSlotTreasure(zone, rng));
                    placed++;
                }

                if (placed < GameConfig.GuaranteedSingleSlotTrashPerLayer)
                    Debug.LogWarning($"[DigGame] {ZoneInfo.DisplayName(zone)} 1칸 쓰레기를 {placed}개만 배치했습니다.");
            }
        }

        /// <summary>시작점 바로 아래에 첫 할당량을 벌 수 있는 1칸 쓰레기를 보장한다.</summary>
        static void PlaceStarterTrash(WorldGrid grid, System.Random rng)
        {
            int entranceX = SurfaceEntranceX(grid);
            int startX = entranceX - GameConfig.StarterTrashCount / 2;
            int y = GameConfig.StarterTrashRow;

            for (int i = 0; i < GameConfig.StarterTrashCount; i++)
            {
                int x = Mathf.Clamp(startX + i, 1, grid.Width - 2);
                if (!TileInfo.IsDiggable(grid.Get(x, y))) continue;
                if (grid.TryGetMineSite(x, y, out _)) continue;
                grid.PutTreasure(x, y, LootTable.RollStarterTrash(rng));
            }
        }

        public static int SurfaceEntranceX(WorldGrid grid) =>
            Mathf.Clamp(grid.Width / 2 + GameConfig.SurfaceEntranceOffsetX, 1, grid.Width - 2);

        static void CarveSurfaceEntrance(WorldGrid grid)
        {
            int x = SurfaceEntranceX(grid);
            for (int y = GameConfig.SurfaceRow;
                 y < GameConfig.SurfaceRow + GameConfig.GrassLayerRows;
                 y++)
            {
                grid.Set(x, y, TileKind.Empty);
            }
        }

        /// <summary>노이즈로 흔든 층 경계 깊이.</summary>
        static int Boundary(int baseDepth, int x, float noiseOffset)
        {
            float n = Mathf.PerlinNoise(x * GameConfig.LayerNoiseScale, noiseOffset);
            return baseDepth + Mathf.RoundToInt((n - 0.5f) * 2f * GameConfig.LayerBoundaryJitter);
        }

        static bool TryPickMineRect(WorldGrid grid, Zone zone, Vector2Int size, System.Random rng, out RectInt rect)
        {
            rect = default;

            int minDepth, maxDepth;
            switch (zone)
            {
                case Zone.Surface: minDepth = 4; maxDepth = GameConfig.Layer1Depth - 4; break;
                case Zone.Shallow: minDepth = GameConfig.Layer1Depth + 3; maxDepth = GameConfig.Layer2Depth - 4; break;
                case Zone.Deep:    minDepth = GameConfig.Layer2Depth + 3; maxDepth = GameConfig.Layer3Depth - 4; break;
                default:           minDepth = GameConfig.Layer3Depth + 3; maxDepth = GameConfig.UndergroundRows - 6; break;
            }
            if (maxDepth <= minDepth) return false;

            int span = grid.Width - 4 - size.x;
            if (span <= 0) return false;

            int depth = minDepth + rng.Next(maxDepth - minDepth);
            int y = GameConfig.SurfaceRow + depth;
            int x = 2 + rng.Next(span);

            var r = new RectInt(x, y, size.x, size.y);

            // 구역 전체가 같은 층의 팔 수 있는 땅이어야 한다.
            for (int cy = r.yMin; cy < r.yMax; cy++)
            {
                for (int cx = r.xMin; cx < r.xMax; cx++)
                {
                    if (!grid.InBounds(cx, cy)) return false;
                    var k = grid.Get(cx, cy);
                    if (!TileInfo.IsDiggable(k)) return false;
                    if (TileInfo.ZoneOf(k) != zone) return false;
                    if (grid.TryGetMineSite(cx, cy, out _)) return false;
                }
            }

            rect = r;
            return true;
        }

        static bool TooClose(RectInt rect, List<MineSite> placed)
        {
            var c = new Vector2(rect.xMin + rect.width * 0.5f, rect.yMin + rect.height * 0.5f);
            for (int i = 0; i < placed.Count; i++)
            {
                var o = placed[i].CenterF;
                float cheb = Mathf.Max(Mathf.Abs(c.x - o.x), Mathf.Abs(c.y - o.y));
                if (cheb < GameConfig.MineMinSpacing) return true;
            }
            return false;
        }

        static int OreTierFor(Zone z)
        {
            // 1층: 구리, 2층: 철, 3층: 금, 4층: 미스릴
            return Mathf.Clamp((int)z + 1, 0, 4);
        }
    }
}
