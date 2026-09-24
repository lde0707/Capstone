using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 땅속에 숨어 있는 폐기물 집하·분리수거 구역 (2x2 ~ 3x3).
    /// 구역 안의 아무 칸이나 파면 시설 전체가 열린다.
    /// </summary>
    public class MineSite
    {
        public readonly int Id;
        public readonly RectInt Rect;   // grid 좌표계 (y가 아래로 증가)
        public readonly Zone Zone;
        public readonly int OreTier;

        /// <summary>남은 매장량. 0이 되면 고갈된다.</summary>
        public int Reserve;

        public bool Discovered;

        /// <summary>캐놓았지만 아직 지상으로 못 올린 광석들.</summary>
        public readonly List<LootItem> Stockpile = new List<LootItem>();

        public int Miners;
        public int Haulers;
        public int AutoIncomeToday { get; private set; }
        public int AutoItemsToday { get; private set; }
        public int LifetimeAutoIncome { get; private set; }

        public MineSite(int id, RectInt rect, Zone zone, int oreTier, int reserve)
        {
            Id = id;
            Rect = rect;
            Zone = zone;
            OreTier = oreTier;
            Reserve = reserve;
        }

        public Vector2Int CenterCell => new Vector2Int(
            Rect.xMin + Rect.width / 2,
            Rect.yMin + Rect.height / 2);

        /// <summary>구역 중심 (칸 단위 실수 좌표) — 간격 계산용.</summary>
        public Vector2 CenterF => new Vector2(
            Rect.xMin + Rect.width * 0.5f,
            Rect.yMin + Rect.height * 0.5f);

        public IEnumerable<Vector2Int> Cells
        {
            get
            {
                for (int y = Rect.yMin; y < Rect.yMax; y++)
                    for (int x = Rect.xMin; x < Rect.xMax; x++)
                        yield return new Vector2Int(x, y);
            }
        }

        public bool Contains(Vector2Int c) => Rect.Contains(c);

        public bool StockpileFull => Stockpile.Count >= GameConfig.MineStockpileCap;

        public string Label => $"#{Id} {ZoneInfo.DisplayName(Zone)} · {LootTable.OreName(OreTier)} 도시광산 ({Rect.width}x{Rect.height})";

        public bool ExtractOne(System.Random rng)
        {
            if (Reserve <= 0 || StockpileFull) return false;
            Reserve--;
            Stockpile.Add(LootTable.RollOre(OreTier, rng));
            return true;
        }

        public void RecordAutomaticSale(int itemCount, int paid)
        {
            AutoItemsToday += Mathf.Max(0, itemCount);
            AutoIncomeToday += Mathf.Max(0, paid);
            LifetimeAutoIncome += Mathf.Max(0, paid);
        }

        public void ResetDailyIncome()
        {
            AutoItemsToday = 0;
            AutoIncomeToday = 0;
        }

        public List<LootItem> TakeFromStockpile(int count)
        {
            int n = Mathf.Min(count, Stockpile.Count);
            var taken = Stockpile.GetRange(0, n);
            Stockpile.RemoveRange(0, n);
            return taken;
        }

        /// <summary>플레이어 가방을 빈 보관 슬롯만큼 옮긴다.</summary>
        public int StoreFrom(List<LootItem> items)
        {
            if (items == null || items.Count == 0) return 0;
            int count = Mathf.Min(items.Count, Mathf.Max(0, GameConfig.MineStockpileCap - Stockpile.Count));
            if (count <= 0) return 0;

            Stockpile.AddRange(items.GetRange(0, count));
            items.RemoveRange(0, count);
            return count;
        }

        /// <summary>남은 가방 부피에 들어가는 보관품 하나를 꺼낸다.</summary>
        public bool TryTakeFitting(int maxVolume, out LootItem item)
        {
            for (int i = 0; i < Stockpile.Count; i++)
            {
                if (Stockpile[i].Volume > maxVolume) continue;
                item = Stockpile[i];
                Stockpile.RemoveAt(i);
                return true;
            }

            item = default;
            return false;
        }
    }
}
