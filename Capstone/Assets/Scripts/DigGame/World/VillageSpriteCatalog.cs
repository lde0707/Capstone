using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>지상 마을 에셋(집·소품·길·잔디·주민)을 잘라 제공한다. 모두 16px 그리드.</summary>
    public static class VillageSpriteCatalog
    {
        const string VillageRoot = "PixelRefresh/Village/";

        public const int TilePixels = 16;       // 1칸

        // ==================================================================
        // 시대 발전 — 마을은 5개 시대를 거치며 통째로 모습이 바뀐다.
        // 각 시대마다 '집' 1종과 '복원소' 1종을 가진다.
        // ==================================================================
        const string EraRoot = "PixelRefresh/Era/";

        public const int EraCount = 5;
        public const int EraPixels = 48;        // 3칸

        public static readonly string[] EraNames =
        { "원시", "고대", "조선", "근대", "미래" };

        public static readonly string[] EraHouseNames =
        { "움집", "토담집", "기와 한옥", "벽돌 양옥", "부유 캡슐" };

        public static readonly string[] EraRestorerNames =
        { "돌 제단", "토기 가마", "오름가마", "기계 작업장", "홀로그램 복원소" };

        /// <summary>시대 분위기를 정리한 한 줄 (로그·HUD용).</summary>
        public static readonly string[] EraTaglines =
        {
            "돌과 흙뿐인 땅. 불씨 하나가 전부다.",
            "흙담을 쌓고 그릇을 굽기 시작했다.",
            "기와가 올라가고 가마에 불이 들어왔다.",
            "벽돌과 전선이 마을을 가로지른다.",
            "집이 떠오르고 빛으로 무엇이든 되살린다."
        };

        public static int ClampEra(int era) => Mathf.Clamp(era, 0, EraCount - 1);

        public static string EraName(int era) => EraNames[ClampEra(era)];
        public static string EraHouseName(int era) => EraHouseNames[ClampEra(era)];
        public static string EraRestorerName(int era) => EraRestorerNames[ClampEra(era)];
        public static string EraTagline(int era) => EraTaglines[ClampEra(era)];

        /// <summary>그 시대의 플레이어 집.</summary>
        public static Sprite EraHouse(int era) =>
            Slice(EraRoot + "era_houses", EraPixels, EraCount, BottomPivot)[ClampEra(era)];

        /// <summary>그 시대의 복원소(유물을 되살리는 건물).</summary>
        public static Sprite EraRestorer(int era) =>
            Slice(EraRoot + "era_restorers", EraPixels, EraCount, BottomPivot)[ClampEra(era)];

        // ---------------- 시대별 배경 ----------------------------------------
        public const int BackdropWidth = 256;

        /// <summary>하늘 색에 곱할 시대 색조. 세상 전체의 공기를 바꾼다.</summary>
        static readonly Color[] SkyTints =
        {
            new Color(1.08f, 0.90f, 0.74f),   // 원시 — 붉은 흙먼지
            new Color(1.02f, 1.00f, 0.88f),   // 고대 — 누런 볕
            new Color(0.93f, 1.00f, 1.08f),   // 조선 — 맑고 푸른 하늘
            new Color(1.04f, 0.95f, 0.78f),   // 근대 — 매연 낀 누런 하늘
            new Color(0.86f, 0.92f, 1.14f),   // 미래 — 차가운 보랏빛
        };

        /// <summary>지표 잔디·길에 곱할 시대 색조.</summary>
        static readonly Color[] GrassTints =
        {
            new Color(1.26f, 0.92f, 0.52f),   // 원시 — 마르고 거친 황토빛 풀
            new Color(1.10f, 1.02f, 0.76f),   // 고대 — 누런 기 도는 들판
            new Color(0.76f, 1.12f, 0.74f),   // 조선 — 짙고 선명한 초록
            new Color(0.84f, 0.80f, 0.66f),   // 근대 — 매연에 절은 회록
            new Color(0.58f, 1.02f, 1.08f),   // 미래 — 서늘한 청록
        };

        public static Color EraSkyTint(int era) => SkyTints[ClampEra(era)];
        public static Color EraGrassTint(int era) => GrassTints[ClampEra(era)];

        /// <summary>먼 원경 (산·거대 구조물). 가로로 끝없이 이어진다.</summary>
        public static Sprite EraFar(int era) =>
            Slice(EraRoot + "era_far_" + ClampEra(era), BackdropWidth, 1, BottomPivot)[0];

        /// <summary>가까운 원경 (집·나무).</summary>
        public static Sprite EraNear(int era) =>
            Slice(EraRoot + "era_near_" + ClampEra(era), BackdropWidth, 1, BottomPivot)[0];

        // ---------------- 시대별 이웃 건물 / 장식물 --------------------------
        /// <summary>
        /// 시대마다 있는 이웃 건물 종류 수.
        /// 기본 4종(작은집·보통집·넓은건물·높은건물) + 이틀 머무는 원시·미래는 3종 더.
        /// </summary>
        static readonly int[] BuildingKindsByEra = { 7, 4, 4, 4, 7 };

        public static int BuildingKinds(int era) => BuildingKindsByEra[ClampEra(era)];

        public const int BuildingPixels = 64;       // 4칸

        /// <summary>시대마다 있는 장식물 종류 수.</summary>
        public const int PropKinds = 10;
        public const int EraPropPixels = 32;        // 2칸

        /// <summary>그 시대의 이웃 건물. kind 마다 크기가 다르다.</summary>
        public static Sprite EraBuilding(int era, int kind)
        {
            int e = ClampEra(era);
            int kinds = BuildingKinds(e);
            return Slice(EraRoot + "era_buildings_" + e,
                         BuildingPixels, kinds, BottomPivot)[Mathf.Abs(kind) % kinds];
        }

        /// <summary>그 시대의 장식물.</summary>
        public static Sprite EraProp(int era, int kind) =>
            Slice(EraRoot + "era_props_" + ClampEra(era),
                  EraPropPixels, PropKinds, BottomPivot)[Mathf.Abs(kind) % PropKinds];

        // ---------------- 세트 보너스 랜드마크 ------------------------------
        /// <summary>
        /// 유물 한 분류를 전부 복원하면 마을에 들어서는 건물.
        /// 일반 이웃 건물(64px)보다 크고, 시대를 타지 않는다.
        /// 순서는 <c>RelicGroup</c> 과 같다.
        /// </summary>
        public const int LandmarkWidth = 96;        // 6칸

        public static readonly string[] SetLandmarkNames =
        { "공룡 공원", "요괴 마을", "세계 기담관", "국립 박물관", "추억의 거리", "미확인 구역" };

        public static readonly string[] SetLandmarkDescs =
        {
            "울타리 안에서 공룡들이 어슬렁거린다",
            "밤마다 도깨비불이 마을을 떠다닌다",
            "천막 안에 세상의 기담이 모여 있다",
            "나라의 보물이 한자리에 모였다",
            "골목 하나가 통째로 1970년대가 됐다",
            "무언가가 마을 위에 상주하기 시작했다"
        };

        public static Sprite SetLandmark(int group)
        {
            var all = Slice(EraRoot + "set_landmarks", LandmarkWidth,
                SetLandmarkNames.Length, BottomPivot);
            int i = Mathf.Abs(group) % SetLandmarkNames.Length;
            return i < all.Length ? all[i] : null;
        }

        // ---------------- 건물 간판 (고정 표식) -----------------------------
        /// <summary>간판 종류. 아이콘·색은 시대가 바뀌어도 고정이다.</summary>
        public enum SignKind { Dropoff = 0, Shop = 1, Restorer = 2 }

        public const int SignWidth = 64;        // 4칸
        public const int SignStyles = 3;        // 나무 / 함석 / 홀로

        /// <summary>간판이 건물 꼭대기 위로 뜨는 높이 (칸).</summary>
        public const float SignLift = 3.15f;

        /// <summary>
        /// 시설을 바짝 붙여 놓아서 간판(4칸 폭)끼리 닿는다.
        /// 가운데 선 강화소 간판만 한 단 올려 세 개가 다 읽히게 한다.
        /// </summary>
        public static float SignLiftFor(SignKind kind) =>
            kind == SignKind.Shop ? SignLift + 1.7f : SignLift;

        public static int SignStyleForEra(int era)
        {
            int e = ClampEra(era);
            return e >= 4 ? 2 : e >= 3 ? 1 : 0;
        }

        /// <summary>투입소·강화소·복원소를 구분시키는 간판.</summary>
        public static Sprite Sign(SignKind kind, int era)
        {
            var all = Slice(EraRoot + "era_signs", SignWidth, SignStyles * 3, BottomPivot);
            int index = SignStyleForEra(era) * 3 + (int)kind;
            return index < all.Length ? all[index] : null;
        }

        // ---------------- 시대별 지상 시설 ----------------------------------
        /// <summary>그 시대의 쓰레기 투입소(분리수거장).</summary>
        public static Sprite EraDropoff(int era) =>
            Slice(EraRoot + "era_dropoffs", EraPixels, EraCount, BottomPivot)[ClampEra(era)];

        /// <summary>그 시대의 강화소(처리장).</summary>
        public static Sprite EraShop(int era) =>
            Slice(EraRoot + "era_shops", EraPixels, EraCount, BottomPivot)[ClampEra(era)];

        // ---------------- 시대별 주민 --------------------------------------
        /// <summary>꼬마가 들어 있는 자리. 어느 시대든 5번째(인덱스 4)가 아이다.</summary>
        public const int VillagerKidIndex = 4;

        /// <summary>시대 x 주민 이름. 원시·미래는 이틀을 머무르므로 3종씩 더 있다.</summary>
        public static readonly string[][] EraVillagerNames =
        {
            new[] { "사냥꾼", "채집인", "불지기", "주술사", "원시 꼬마", "작살꾼", "원시 노인", "짐꾼" },
            new[] { "토기장이", "농부", "무사", "제사장", "고대 꼬마" },
            new[] { "선비", "등짐장수", "아낙", "포졸", "댕기 꼬마" },
            new[] { "넝마주이", "공장 노동자", "양복 신사", "교복 학생", "몸빼 아주머니" },
            new[] { "복원 연구원", "배송 드론", "작업 로봇", "네온 시민", "호버 꼬마", "조종사", "노점상", "정비공" },
        };

        /// <summary>그 시대에 사는 주민 종류 수.</summary>
        public static int EraVillagerKinds(int era) => EraVillagerNames[ClampEra(era)].Length;

        public static string EraVillagerName(int era, int kind)
        {
            int e = ClampEra(era);
            return EraVillagerNames[e][Mathf.Abs(kind) % EraVillagerNames[e].Length];
        }

        /// <summary>그 시대 주민의 걷기 4프레임 (오른쪽 보기).</summary>
        public static Sprite[] EraVillager(int era, int kind)
        {
            int e = ClampEra(era);
            int kinds = EraVillagerKinds(e);
            int k = Mathf.Abs(kind) % kinds;
            var all = Slice(EraRoot + "era_villagers_" + e, TilePixels,
                kinds * 4, CenterPivot, TilePixels);

            var frames = new Sprite[4];
            for (int i = 0; i < 4; i++)
            {
                int index = k * 4 + i;
                frames[i] = index < all.Length ? all[index] : null;
            }
            return frames;
        }

        static readonly Dictionary<string, Sprite[]> Cache = new Dictionary<string, Sprite[]>();

        /// <summary>16px = 한 칸이 되도록 하는 픽셀 밀도.</summary>
        public static float PixelsPerUnit => TilePixels / GameConfig.CellSize;

        public static Sprite Road(int variant) =>
            Slice(VillageRoot + "roads", TilePixels, 6, CenterPivot)[Mathf.Abs(variant) % 6];

        public static Sprite Grass(int variant) =>
            Slice(VillageRoot + "grass", TilePixels, 4, CenterPivot)[Mathf.Abs(variant) % 4];

        static readonly Vector2 BottomPivot = new Vector2(0.5f, 0f);
        static readonly Vector2 CenterPivot = new Vector2(0.5f, 0.5f);

        static Sprite[] Slice(string path, int size, int count, Vector2 pivot, float ppuOverride = 0f)
        {
            string key = path + "#" + size;
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var texture = Resources.Load<Texture2D>(path);
            if (texture == null)
            {
                Debug.LogWarning($"[DigGame] 마을 에셋을 찾지 못했습니다: {path}");
                var empty = new Sprite[count];
                Cache[key] = empty;
                return empty;
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            float ppu = ppuOverride > 0f ? ppuOverride : PixelsPerUnit;
            int available = Mathf.Max(1, texture.width / size);
            var sprites = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                int column = Mathf.Min(i, available - 1);
                var sprite = Sprite.Create(texture,
                    new Rect(column * size, 0f, size, Mathf.Min(size, texture.height)),
                    pivot, ppu, 0, SpriteMeshType.FullRect);
                sprite.name = $"{path}#{i:00}";
                sprite.hideFlags = HideFlags.DontSave;
                sprites[i] = sprite;
            }
            Cache[key] = sprites;
            return sprites;
        }

        public static void Clear()
        {
            foreach (var set in Cache.Values)
            {
                if (set == null) continue;
                for (int i = 0; i < set.Length; i++)
                {
                    if (set[i] == null) continue;
                    if (Application.isPlaying) Object.Destroy(set[i]);
                    else Object.DestroyImmediate(set[i]);
                }
            }
            Cache.Clear();
        }
    }
}
