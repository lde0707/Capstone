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

            public Entry(string n, int v, int vol, MaterialKind m, float shade, float w)
            {
                Name = n; BaseValue = v; Volume = vol; Material = m; Shade = shade; Weight = w;
            }
        }

        // ---------- 1층: 최근 매립층 — 생활 쓰레기 ----------
        static readonly Entry[] SurfaceLoot =
        {
            new Entry("비닐 뭉치",     15,  1, MaterialKind.General,     0.62f, 5f),
            new Entry("찌그러진 캔",   35,  1, MaterialKind.Metal,       0.75f, 4f),
            new Entry("소주병",        50,  1, MaterialKind.Glass,       0.88f, 3f),
            new Entry("알루미늄 휘장", 85,  1, MaterialKind.Metal,       0.96f, 2.4f),
            new Entry("중고 스마트폰", 180, 1, MaterialKind.Electronics, 1.10f, 1.4f),
            new Entry("기념 은화",     280, 1, MaterialKind.Relic,       1.24f, 0.7f),
            new Entry("페트병 더미",   45,  2, MaterialKind.Plastic,     1.00f, 3f),
            new Entry("고장난 라디오", 140, 2, MaterialKind.Electronics, 1.15f, 1.5f),
            new Entry("옛날 동전",     220, 1, MaterialKind.Relic,       1.25f, 0.5f),
        };

        // 시작 지점 보장 배치용. 최저 가격 변동이 나와도 9개 합계가 첫날 할당량을 넘는다.
        static readonly Entry[] StarterLoot =
        {
            new Entry("재활용 비닐 묶음", 35, 1, MaterialKind.General, 0.68f, 5f),
            new Entry("분리수거 캔",     45, 1, MaterialKind.Metal,   0.82f, 4f),
            new Entry("반납용 유리병",   55, 1, MaterialKind.Glass,   0.96f, 3f),
        };

        // ---------- 2층: 90년대 층 — 가전 ----------
        static readonly Entry[] ShallowLoot =
        {
            new Entry("카세트테이프",    70,  1, MaterialKind.Plastic,     0.58f, 4f),
            new Entry("워크맨",          180, 1, MaterialKind.Electronics, 0.72f, 3.0f),
            new Entry("휴대용 게임기",   300, 1, MaterialKind.Electronics, 0.88f, 2.5f),
            new Entry("초기형 휴대전화", 480, 1, MaterialKind.Electronics, 1.02f, 1.8f),
            new Entry("은제 라이터",     720, 1, MaterialKind.Relic,       1.16f, 1.0f),
            new Entry("한정판 게임팩",   950, 1, MaterialKind.Relic,       1.26f, 0.6f),
            new Entry("녹슨 자전거",    90,  3, MaterialKind.Metal,       0.62f, 4f),
            new Entry("깨진 어항",      130, 2, MaterialKind.Glass,       0.75f, 3f),
            new Entry("장난감 상자",    160, 2, MaterialKind.Plastic,     0.88f, 3f),
            new Entry("브라운관 TV",    380, 4, MaterialKind.Electronics, 1.00f, 2f),
            new Entry("오락기 기판",    550, 2, MaterialKind.Electronics, 1.15f, 1.2f),
            new Entry("금도금 시계",    800, 1, MaterialKind.Relic,       1.25f, 0.5f),
        };

        // ---------- 3층: 산업 폐기물층 ----------
        static readonly Entry[] DeepLoot =
        {
            new Entry("구리선 뭉치",    260,  1, MaterialKind.Metal,       0.56f, 4f),
            new Entry("산업용 센서",    600,  1, MaterialKind.Electronics, 0.68f, 3.2f),
            new Entry("정밀 계측 칩",   900,  1, MaterialKind.Electronics, 0.82f, 2.8f),
            new Entry("백금 촉매",      1400, 1, MaterialKind.Metal,       0.98f, 2.2f),
            new Entry("군용 통신 모듈", 2100, 1, MaterialKind.Electronics, 1.12f, 1.6f),
            new Entry("항공기 블랙박스",3200, 1, MaterialKind.Relic,       1.26f, 0.9f),
            new Entry("고철 덩어리",    350,  3, MaterialKind.Metal,       0.62f, 4f),
            new Entry("산업용 유리판",  480,  2, MaterialKind.Glass,       0.75f, 2.5f),
            new Entry("폐배터리 액",    700,  2, MaterialKind.Hazard,      0.88f, 2.5f),
            new Entry("공장 제어반",    1100, 3, MaterialKind.Electronics, 1.00f, 2f),
            new Entry("의료 폐기물",    1500, 2, MaterialKind.Hazard,      1.12f, 1.2f),
            new Entry("놋쇠 촛대",      2200, 2, MaterialKind.Relic,       1.25f, 0.5f),
        };

        // ---------- 4층: 봉인된 최심층 ----------
        static readonly Entry[] AbyssLoot =
        {
            new Entry("희귀 금속 조각", 1400,  1, MaterialKind.Metal,       0.54f, 3.5f),
            new Entry("초전도 코일",    2200,  1, MaterialKind.Electronics, 0.66f, 3.5f),
            new Entry("운석 합금 파편", 3600,  1, MaterialKind.Metal,       0.80f, 3.0f),
            new Entry("고대 금화 주머니",5500, 1, MaterialKind.Relic,       0.94f, 2.5f),
            new Entry("봉인된 보석함",  8500,  1, MaterialKind.Relic,       1.08f, 2.0f),
            new Entry("왕실 인장",      13000, 1, MaterialKind.Relic,       1.18f, 1.3f),
            new Entry("미확인 데이터 코어",20000,1,MaterialKind.Electronics,1.28f,0.8f),
            new Entry("정체불명 합금",  1800,  3, MaterialKind.Metal,       0.62f, 3.5f),
            new Entry("방사성 폐기물",  3200,  3, MaterialKind.Hazard,      0.78f, 2.5f),
            new Entry("고대 축음기",    5000,  4, MaterialKind.Electronics, 0.95f, 2f),
            new Entry("골동품 도자기",  8000,  2, MaterialKind.Relic,       1.10f, 1.2f),
            new Entry("금괴",           15000, 2, MaterialKind.Relic,       1.25f, 0.5f),
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
                e.Material);
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
        public static Color OreColor(int tier) => OreColors[Mathf.Clamp(tier, 0, OreColors.Length - 1)];
    }
}
