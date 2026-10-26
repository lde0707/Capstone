using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 매립층별 쓰레기 / 광산 산출물 테이블.
    /// 같은 층의 쓰레기는 그 층 상징색 계열이라 한눈에 구분된다.
    /// </summary>
    public static class LootTable
    {
        struct Entry
        {
            public string Name;
            public int BaseValue;
            public int Volume;
            public MaterialKind Material;
            public float Shade;
            public float Weight;
            public RecyclingCategory Category;

            public Entry(string n, int v, int vol, MaterialKind m, float shade, float w, RecyclingCategory category = RecyclingCategory.Specialist)
            {
                Name = n; BaseValue = v; Volume = vol; Material = m; Shade = shade; Weight = w; Category = category;
            }
        }

        // ---------- 1층: 최근 매립층 — 생활 분리수거 ----------
        static readonly Entry[] SurfaceLoot =
        {
            new Entry("신문지 묶음",         40, 1, MaterialKind.General,   0.92f, 2.00f, RecyclingCategory.Paper),
            new Entry("골판지 상자",         70, 2, MaterialKind.General,   0.80f, 2.50f, RecyclingCategory.Paper),
            new Entry("묶은 헌책",          90, 2, MaterialKind.General,   0.70f, 2.00f, RecyclingCategory.Paper),
            new Entry("찌그러진 캔",         35, 1, MaterialKind.Metal,     0.75f, 1.90f, RecyclingCategory.Can),
            new Entry("통조림 캔",          50, 1, MaterialKind.Metal,     0.84f, 1.74f, RecyclingCategory.Can),
            new Entry("생수 페트병",         40, 1, MaterialKind.Plastic,   1.00f, 1.90f, RecyclingCategory.Pet),
            new Entry("탄산 페트병",         55, 1, MaterialKind.Plastic,   1.08f, 1.74f, RecyclingCategory.Pet),
            new Entry("플라스틱 반찬통",       60, 1, MaterialKind.Plastic,   0.88f, 2.60f, RecyclingCategory.Plastic),
            new Entry("플라스틱 바가지",       80, 2, MaterialKind.Plastic,   1.14f, 2.85f, RecyclingCategory.Plastic),
            new Entry("알루미늄 포일 뭉치",     55, 1, MaterialKind.Metal,     1.00f, 1.30f, RecyclingCategory.Aluminum),
            new Entry("은박 접시",          70, 1, MaterialKind.Metal,     1.12f, 1.20f, RecyclingCategory.Aluminum),
            new Entry("은박 포장지",         45, 1, MaterialKind.Metal,     0.92f, 1.14f, RecyclingCategory.Aluminum),
        };

        // ---------- 시작 지점 보장 배치용. 5개 칸이 정확히 한 번씩 돌아간다 ----------
        static readonly Entry[] StarterLoot =
        {
            new Entry("신문지 묶음",         45, 1, MaterialKind.General,   0.92f, 2.20f, RecyclingCategory.Paper),
            new Entry("생수 페트병",         50, 1, MaterialKind.Plastic,   1.00f, 2.20f, RecyclingCategory.Pet),
            new Entry("통조림 캔",          50, 1, MaterialKind.Metal,     0.84f, 2.20f, RecyclingCategory.Can),
            new Entry("플라스틱 반찬통",       55, 1, MaterialKind.Plastic,   0.88f, 2.20f, RecyclingCategory.Plastic),
            new Entry("알루미늄 포일 뭉치",     50, 1, MaterialKind.Metal,     1.00f, 1.10f, RecyclingCategory.Aluminum),
            new Entry("은박 접시",          60, 1, MaterialKind.Metal,     1.12f, 1.10f, RecyclingCategory.Aluminum),
        };

        // ---------- 2층: 90년대 층 — 그 시절 포장재 ----------
        static readonly Entry[] ShallowLoot =
        {
            new Entry("90년대 잡지 더미",    140, 2, MaterialKind.General,   0.70f, 2.40f, RecyclingCategory.Paper),
            new Entry("만화책 더미",        220, 3, MaterialKind.General,   0.58f, 2.20f, RecyclingCategory.Paper),
            new Entry("레코드 속지 묶음",     180, 1, MaterialKind.General,   0.88f, 1.95f, RecyclingCategory.Paper),
            new Entry("옛날 음료 캔",       150, 1, MaterialKind.Metal,     0.76f, 1.90f, RecyclingCategory.Can),
            new Entry("분유 깡통",         200, 1, MaterialKind.Metal,     0.92f, 1.74f, RecyclingCategory.Can),
            new Entry("초기형 페트병",       170, 1, MaterialKind.Plastic,   1.02f, 1.90f, RecyclingCategory.Pet),
            new Entry("1.5L 탄산병",      240, 2, MaterialKind.Plastic,   1.12f, 4.80f, RecyclingCategory.Pet),
            new Entry("카세트 보관함",       230, 1, MaterialKind.Plastic,   0.64f, 2.20f, RecyclingCategory.Plastic),
            new Entry("장난감 수납 상자",     280, 2, MaterialKind.Plastic,   1.00f, 2.20f, RecyclingCategory.Plastic),
            new Entry("우유 운반 상자",      320, 3, MaterialKind.Plastic,   1.20f, 1.75f, RecyclingCategory.Plastic),
            new Entry("은박 과자봉지 뭉치",    190, 1, MaterialKind.Metal,     0.82f, 1.90f, RecyclingCategory.Aluminum),
            new Entry("알루미늄 도시락",      260, 1, MaterialKind.Metal,     1.16f, 1.74f, RecyclingCategory.Aluminum),
        };

        // ---------- 3층: 산업 폐기물층 — 공장에서 나온 자재 ----------
        static readonly Entry[] DeepLoot =
        {
            new Entry("공장 출하 서류함",     480, 2, MaterialKind.General,   0.62f, 2.40f, RecyclingCategory.Paper),
            new Entry("설계도 뭉치",        900, 1, MaterialKind.General,   0.88f, 1.95f, RecyclingCategory.Paper),
            new Entry("방수 포장지 묶음",     620, 2, MaterialKind.General,   0.74f, 2.20f, RecyclingCategory.Paper),
            new Entry("윤활유 빈 캔",       700, 1, MaterialKind.Metal,     0.58f, 1.90f, RecyclingCategory.Can),
            new Entry("스틸 드럼 조각",      950, 2, MaterialKind.Metal,     0.70f, 4.80f, RecyclingCategory.Can),
            new Entry("압착 페트 더미",       760, 1, MaterialKind.Plastic,   1.00f, 1.90f, RecyclingCategory.Pet),
            new Entry("대용량 페트 탱크",    1150, 3, MaterialKind.Plastic,   1.14f, 4.80f, RecyclingCategory.Pet),
            new Entry("공업용 플라스틱 통",    880, 1, MaterialKind.Plastic,   0.84f, 1.90f, RecyclingCategory.Plastic),
            new Entry("부품 적재함",       1300, 3, MaterialKind.Plastic,   1.22f, 4.80f, RecyclingCategory.Plastic),
            new Entry("알루미늄 방열판",     1050, 1, MaterialKind.Metal,     0.96f, 1.90f, RecyclingCategory.Aluminum),
            new Entry("은박 단열재 뭉치",    1250, 2, MaterialKind.Metal,     1.08f, 2.40f, RecyclingCategory.Aluminum),
            new Entry("알루미늄 샷시 조각",   1450, 2, MaterialKind.Metal,     1.26f, 2.30f, RecyclingCategory.Aluminum),
        };

        // ---------- 4층: 봉인된 최심층 — 고대의 포장 ----------
        static readonly Entry[] AbyssLoot =
        {
            new Entry("고대 금박 두루마리",   4200, 1, MaterialKind.General,   0.94f, 1.95f, RecyclingCategory.Paper),
            new Entry("봉인 문서함",       6000, 2, MaterialKind.General,   0.74f, 2.40f, RecyclingCategory.Paper),
            new Entry("고문서 묶음",       9500, 2, MaterialKind.General,   0.58f, 2.20f, RecyclingCategory.Paper),
            new Entry("의례용 청동 깡통",    4000, 1, MaterialKind.Metal,     0.62f, 1.90f, RecyclingCategory.Can),
            new Entry("밀봉된 합금 캔",     6500, 1, MaterialKind.Metal,     0.80f, 1.74f, RecyclingCategory.Can),
            new Entry("유리질 페트 용기",    5000, 1, MaterialKind.Plastic,   1.02f, 1.90f, RecyclingCategory.Pet),
            new Entry("미지의 수액 페트",    7500, 2, MaterialKind.Plastic,   1.14f, 4.80f, RecyclingCategory.Pet),
            new Entry("합성수지 보관통",     5600, 1, MaterialKind.Plastic,   0.86f, 2.60f, RecyclingCategory.Plastic),
            new Entry("봉인 보관함",       9000, 3, MaterialKind.Plastic,   1.22f, 2.60f, RecyclingCategory.Plastic),
            new Entry("은박 봉인포",       6800, 1, MaterialKind.Metal,     1.00f, 1.30f, RecyclingCategory.Aluminum),
            new Entry("알루미늄 제의 접시",   9500, 2, MaterialKind.Metal,     1.10f, 2.40f, RecyclingCategory.Aluminum),
            new Entry("초경량 합금 포일",   14000, 1, MaterialKind.Metal,     1.28f, 1.20f, RecyclingCategory.Aluminum),
        };

        // ---------- 광산 산출물 (고철류) ----------
        static readonly string[] OreNames  = { "고철 조각", "구리 파이프", "철근 다발", "은식기 뭉치", "귀금속 폐품" };
        static readonly int[]    OreValues = { 100,        300,          800,        2000,          5000 };
        static readonly Color[]  OreColors =
        {
            new Color32(120, 120, 126, 255),
            new Color32(200, 122, 64, 255),
            new Color32(182, 182, 190, 255),
            new Color32(220, 220, 230, 255),
            new Color32(242, 198, 62, 255)
        };

        /// <summary>
        /// 모든 층 + 시작 지점의 품목을 대표값으로 한 번씩 돌려준다.
        /// 에디터 검증(그림-분리수거 칸 일치)에서 쓴다.
        /// </summary>
        public static System.Collections.Generic.IEnumerable<LootItem> AllEntries()
        {
            foreach (Zone zone in System.Enum.GetValues(typeof(Zone)))
            {
                Entry[] table = TableFor(zone);
                Color sig = ZoneInfo.SignatureColor(zone);
                for (int i = 0; i < table.Length; i++) yield return Preview(table[i], sig);
            }

            Color surface = ZoneInfo.SignatureColor(Zone.Surface);
            for (int i = 0; i < StarterLoot.Length; i++) yield return Preview(StarterLoot[i], surface);
        }

        static LootItem Preview(Entry e, Color sig) =>
            new LootItem(e.Name, e.BaseValue, Shaded(sig, e.Shade), e.Volume, e.Material, e.Category);

        static Entry[] TableFor(Zone z)
        {
            switch (z)
            {
                case Zone.Surface: return SurfaceLoot;
                case Zone.Shallow: return ShallowLoot;
                case Zone.Deep:    return DeepLoot;
                default:           return AbyssLoot;
            }
        }

        static Color Shaded(Color baseColor, float shade)
        {
            if (shade >= 1f)
                return Color.Lerp(baseColor, Color.white, Mathf.Clamp01((shade - 1f) * 2.2f));
            return Color.Lerp(baseColor, Color.black, Mathf.Clamp01((1f - shade) * 0.9f));
        }

        public static LootItem RollTreasure(Zone z, System.Random rng)
        {
            var table = TableFor(z);
            return RollFrom(table, ZoneInfo.SignatureColor(z), rng, singleSlotOnly: false);
        }

        /// <summary>해당 지층에서 반드시 부피 1인 쓰레기만 뽑는다.</summary>
        public static LootItem RollSingleSlotTreasure(Zone z, System.Random rng) =>
            RollFrom(TableFor(z), ZoneInfo.SignatureColor(z), rng, singleSlotOnly: true);

        /// <summary>첫날 진행이 운에 막히지 않도록 시작 지점에 놓는 1칸 표층 쓰레기.</summary>
        public static LootItem RollStarterTrash(System.Random rng) =>
            RollFrom(StarterLoot, ZoneInfo.SignatureColor(Zone.Surface), rng, singleSlotOnly: true);

        /// <summary>압축층·가스층에서 보장되는 위험/보상용 1칸 쓰레기.</summary>
        public static LootItem RollFeatureTreasure(Zone zone, DigFeature feature, System.Random rng)
        {
            LootItem item = RollSingleSlotTreasure(zone, rng);
            if (feature == DigFeature.CompactCache)
            {
                item.Name = "압축 보관된 " + item.Name;
                item.Value = Mathf.RoundToInt(item.Value * GameConfig.CompactCacheValueMultiplier);
                item.Color = DigFeatureInfo.ColorOf(feature);
            }
            else if (feature == DigFeature.GasPocket)
            {
                item.Name = "가스에 보존된 " + item.Name;
                item.Value = Mathf.RoundToInt(item.Value * GameConfig.GasPocketValueMultiplier);
                item.Color = DigFeatureInfo.ColorOf(feature);
                // 가스를 열 때 이미 피해를 받으므로, 자동 획득 때 유해물 피해가 중복되지 않게 한다.
                if (item.Material == MaterialKind.Hazard) item.Material = MaterialKind.Metal;
            }
            return item;
        }

        static LootItem RollFrom(Entry[] table, Color sig, System.Random rng, bool singleSlotOnly)
        {

            float total = 0f;
            for (int i = 0; i < table.Length; i++)
                if (!singleSlotOnly || table[i].Volume == 1)
                    total += EffectiveWeight(table[i]);

            double r = rng.NextDouble() * total;
            for (int i = 0; i < table.Length; i++)
            {
                if (singleSlotOnly && table[i].Volume != 1) continue;
                r -= EffectiveWeight(table[i]);
                if (r <= 0) return Build(table[i], sig, rng);
            }

            for (int i = table.Length - 1; i >= 0; i--)
                if (!singleSlotOnly || table[i].Volume == 1)
                    return Build(table[i], sig, rng);

            return default;
        }

        static float EffectiveWeight(Entry entry) =>
            entry.Weight * (entry.Volume == 1 ? GameConfig.SingleSlotLootWeightMultiplier : 1f);

        static LootItem Build(Entry e, Color sig, System.Random rng)
        {
            float jitter = 0.85f + (float)rng.NextDouble() * 0.3f;
            return new LootItem(
                e.Name,
                Mathf.RoundToInt(e.BaseValue * jitter),
                Shaded(sig, e.Shade),
                e.Volume,
                e.Material, e.Category);
        }

        /// <summary>광산에서 나오는 고철. tier 0~4. 부피 1, 재질 고철.</summary>
        public static LootItem RollOre(int tier, System.Random rng)
        {
            tier = Mathf.Clamp(tier, 0, OreNames.Length - 1);
            float jitter = 0.85f + (float)rng.NextDouble() * 0.3f;
            return new LootItem(
                OreNames[tier],
                Mathf.RoundToInt(OreValues[tier] * jitter),
                OreColors[tier],
                1,
                MaterialKind.Metal);
        }

        public static string OreName(int tier) => OreNames[Mathf.Clamp(tier, 0, OreNames.Length - 1)];
    }
}
