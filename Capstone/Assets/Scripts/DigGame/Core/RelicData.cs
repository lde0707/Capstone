using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>복원된 유물이 마을에서 어떻게 존재하는지.</summary>
    public enum RelicBehavior
    {
        Display = 0,   // 좌대와 함께 한자리에 전시된다
        Wander = 1     // 살아나서 마을을 어슬렁거린다
    }

    /// <summary>유물 분류. 세트 보너스의 단위이기도 하다.</summary>
    public enum RelicGroup
    {
        Dino = 0,      // A 공룡·고생물
        Yokai = 1,     // B 한국 요괴
        Beast = 2,     // C 세계 괴수
        History = 3,   // D 한국 역사
        Modern = 4,    // E 근현대 물건
        Mystery = 5    // F 미스터리·SF
    }

    /// <summary>유물 1종의 정의. 전부 <see cref="RelicCatalog"/> 에 상수로 박혀 있다.</summary>
    public class RelicDef
    {
        public int Id;                  // 0 ~ 57 (도감 번호 - 1)
        public RelicGroup Group;
        public int Index;               // 그룹 시트 안에서의 열 번호
        public string Name;
        public int Depth;               // 1~4 층
        public RelicBehavior Behavior;
        public bool Flying;             // 배회형이 공중에 뜨는가

        public string GroupName => RelicCatalog.GroupName(Group);
        /// <summary>도감에 보이는 번호 (1부터).</summary>
        public int Number => Id + 1;

        /// <summary>중복 발굴했을 때 자동 환전되는 금액.</summary>
        public int DuplicateValue => RelicCatalog.DepthValue(Depth);
    }

    /// <summary>유물 58종 정의표.</summary>
    public static class RelicCatalog
    {
        public const int GroupCount = 6;

        static readonly string[] GroupNames =
        { "공룡·고생물", "한국 요괴", "세계 괴수", "한국 역사", "근현대 물건", "미스터리·SF" };

        /// <summary>그룹별 시트 파일 꼬리표 (relic_buried_a … relic_alive_f).</summary>
        public static readonly string[] GroupTags = { "a", "b", "c", "d", "e", "f" };

        public static string GroupName(RelicGroup g) => GroupNames[(int)g];

        /// <summary>층이 깊을수록 값이 뛴다. 중복 발굴 환전과 복원 보상에 쓴다.</summary>
        public static int DepthValue(int depth)
        {
            switch (Mathf.Clamp(depth, 1, 4))
            {
                case 1: return 260;
                case 2: return 900;
                case 3: return 2600;
                default: return 7200;
            }
        }

        static readonly List<RelicDef> _all = new List<RelicDef>();
        static readonly List<RelicDef>[] _byGroup = new List<RelicDef>[GroupCount];
        static readonly List<RelicDef>[] _byDepth = new List<RelicDef>[5];

        public static IReadOnlyList<RelicDef> All => _all;
        public static int Count => _all.Count;

        public static RelicDef Get(int id) =>
            id >= 0 && id < _all.Count ? _all[id] : null;

        public static IReadOnlyList<RelicDef> OfGroup(RelicGroup g) => _byGroup[(int)g];
        public static IReadOnlyList<RelicDef> OfDepth(int depth) =>
            _byDepth[Mathf.Clamp(depth, 1, 4)];

        // D = Display, W = Wander, F = 공중 배회
        const char D = 'D', W = 'W', F = 'F';

        static RelicCatalog()
        {
            for (int i = 0; i < GroupCount; i++) _byGroup[i] = new List<RelicDef>();
            for (int i = 0; i < _byDepth.Length; i++) _byDepth[i] = new List<RelicDef>();

            // ---- A 공룡·고생물 16종 ----
            Add(RelicGroup.Dino, "삼엽충", 1, D);
            Add(RelicGroup.Dino, "암모나이트", 1, D);
            Add(RelicGroup.Dino, "호박 속 곤충", 1, D);
            Add(RelicGroup.Dino, "벨로키랍토르", 2, W);
            Add(RelicGroup.Dino, "검치호", 2, W);
            Add(RelicGroup.Dino, "매머드", 2, W);
            Add(RelicGroup.Dino, "스테고사우루스", 3, W);
            Add(RelicGroup.Dino, "트리케라톱스", 3, W);
            Add(RelicGroup.Dino, "프테라노돈", 3, F);
            Add(RelicGroup.Dino, "시조새", 3, F);
            Add(RelicGroup.Dino, "안킬로사우루스", 3, W);
            Add(RelicGroup.Dino, "공룡알 둥지", 2, W);
            Add(RelicGroup.Dino, "스피노사우루스", 4, W);
            Add(RelicGroup.Dino, "모사사우루스", 4, D);
            Add(RelicGroup.Dino, "티라노사우루스", 4, W);
            Add(RelicGroup.Dino, "브라키오사우루스", 4, W);

            // ---- B 한국 요괴 8종 ----
            Add(RelicGroup.Yokai, "달걀귀신", 1, W);
            Add(RelicGroup.Yokai, "물귀신", 1, W);
            Add(RelicGroup.Yokai, "도깨비", 2, W);
            Add(RelicGroup.Yokai, "처녀귀신", 2, W);
            Add(RelicGroup.Yokai, "해태", 3, D);
            Add(RelicGroup.Yokai, "저승사자", 3, W);
            Add(RelicGroup.Yokai, "구미호", 3, W);
            Add(RelicGroup.Yokai, "이무기(용)", 4, F);

            // ---- C 세계 괴수 6종 ----
            Add(RelicGroup.Beast, "인어", 2, D);
            Add(RelicGroup.Beast, "유니콘", 3, W);
            Add(RelicGroup.Beast, "예티", 3, W);
            Add(RelicGroup.Beast, "네시", 4, D);
            Add(RelicGroup.Beast, "크라켄", 4, D);
            Add(RelicGroup.Beast, "불사조", 4, F);

            // ---- D 한국 역사 10종 ----
            Add(RelicGroup.History, "신라 토우", 1, W);
            Add(RelicGroup.History, "고려청자", 2, D);
            Add(RelicGroup.History, "백자 달항아리", 2, D);
            Add(RelicGroup.History, "앙부일구", 2, D);
            Add(RelicGroup.History, "측우기", 2, D);
            Add(RelicGroup.History, "비파형 동검", 3, D);
            Add(RelicGroup.History, "금관", 3, D);
            Add(RelicGroup.History, "거북선", 3, D);
            Add(RelicGroup.History, "첨성대", 3, D);
            Add(RelicGroup.History, "에밀레종", 4, D);

            // ---- E 근현대 물건 10종 ----
            Add(RelicGroup.Modern, "삐삐", 1, D);
            Add(RelicGroup.Modern, "다이얼 전화기", 1, D);
            Add(RelicGroup.Modern, "자전거", 1, D);
            Add(RelicGroup.Modern, "재봉틀", 1, D);
            Add(RelicGroup.Modern, "태엽 로봇", 1, W);
            Add(RelicGroup.Modern, "흑백 TV", 1, D);
            Add(RelicGroup.Modern, "오락실 기판", 2, D);
            Add(RelicGroup.Modern, "주크박스", 2, D);
            Add(RelicGroup.Modern, "영사기", 2, D);
            Add(RelicGroup.Modern, "우주복 헬멧", 3, D);

            // ---- F 미스터리·SF 8종 ----
            Add(RelicGroup.Mystery, "타임캡슐", 2, D);
            Add(RelicGroup.Mystery, "운석", 3, D);
            Add(RelicGroup.Mystery, "태엽 자동인형", 3, W);
            Add(RelicGroup.Mystery, "수정 해골", 4, D);
            Add(RelicGroup.Mystery, "고대 기계", 4, D);
            Add(RelicGroup.Mystery, "UFO", 4, D);
            Add(RelicGroup.Mystery, "외계 생물", 4, W);
            Add(RelicGroup.Mystery, "미지의 알", 4, W);
        }

        static void Add(RelicGroup group, string name, int depth, char behavior)
        {
            var def = new RelicDef
            {
                Id = _all.Count,
                Group = group,
                Index = _byGroup[(int)group].Count,
                Name = name,
                Depth = depth,
                Behavior = behavior == D ? RelicBehavior.Display : RelicBehavior.Wander,
                Flying = behavior == F
            };
            _all.Add(def);
            _byGroup[(int)group].Add(def);
            _byDepth[depth].Add(def);
        }

        /// <summary>그룹 전체를 모았을 때 주어지는 세트 보너스 이름.</summary>
        public static string SetBonusName(RelicGroup g)
        {
            switch (g)
            {
                case RelicGroup.Dino: return "공룡 공원";
                case RelicGroup.Yokai: return "요괴 마을";
                case RelicGroup.Beast: return "세계 기담관";
                case RelicGroup.History: return "국립 박물관";
                case RelicGroup.Modern: return "추억의 거리";
                default: return "미확인 구역";
            }
        }

    }

    /// <summary>유물 스프라이트(발굴본 1장 / 복원본 4프레임)를 잘라 제공한다.</summary>
    public static class RelicSpriteCatalog
    {
        const string Root = "PixelRefresh/Relic/";

        public const int BuriedPixels = 32;     // 2칸
        public const int AlivePixels = 64;      // 4칸
        public const int Frames = 4;

        static readonly Dictionary<string, Sprite[]> Cache = new Dictionary<string, Sprite[]>();
        static readonly Vector2 BottomPivot = new Vector2(0.5f, 0f);

        // 도감은 매 프레임 다시 그려지므로 그룹별 배열을 통째로 들고 있는다.
        static readonly Sprite[][] BuriedByGroup = new Sprite[RelicCatalog.GroupCount][];
        static readonly Sprite[][] AliveByGroup = new Sprite[RelicCatalog.GroupCount][];

        static Sprite[] BuriedSet(RelicGroup g)
        {
            int i = (int)g;
            if (BuriedByGroup[i] == null)
                BuriedByGroup[i] = Slice(Root + "relic_buried_" + RelicCatalog.GroupTags[i],
                    BuriedPixels, GroupSize(g));
            return BuriedByGroup[i];
        }

        static Sprite[] AliveSet(RelicGroup g)
        {
            int i = (int)g;
            if (AliveByGroup[i] == null)
                AliveByGroup[i] = Slice(Root + "relic_alive_" + RelicCatalog.GroupTags[i],
                    AlivePixels, GroupSize(g) * Frames);
            return AliveByGroup[i];
        }

        static int GroupSize(RelicGroup g) => RelicCatalog.OfGroup(g).Count;

        /// <summary>땅속 화석/깨진 물건 (1장).</summary>
        public static Sprite Buried(RelicDef def)
        {
            if (def == null) return null;
            var all = BuriedSet(def.Group);
            return def.Index < all.Length ? all[def.Index] : null;
        }

        /// <summary>복원된 모습 4프레임.</summary>
        public static Sprite[] Alive(RelicDef def)
        {
            var frames = new Sprite[Frames];
            if (def == null) return frames;

            var all = AliveSet(def.Group);
            for (int i = 0; i < Frames; i++)
            {
                int index = def.Index * Frames + i;
                frames[i] = index < all.Length ? all[index] : null;
            }
            return frames;
        }

        /// <summary>
        /// 도감 미리보기용 첫 프레임.
        /// 매 프레임 그려지는 자리라 배열을 새로 만들지 않고 캐시를 바로 인덱싱한다.
        /// </summary>
        public static Sprite AliveIcon(RelicDef def)
        {
            if (def == null) return null;
            var all = AliveSet(def.Group);
            int index = def.Index * Frames;
            return index < all.Length ? all[index] : null;
        }

        static Sprite[] Slice(string path, int size, int count)
        {
            string key = path + "#" + size;
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var texture = Resources.Load<Texture2D>(path);
            if (texture == null)
            {
                Debug.LogWarning($"[DigGame] 유물 에셋을 찾지 못했습니다: {path}");
                var empty = new Sprite[count];
                Cache[key] = empty;
                return empty;
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            float ppu = VillageSpriteCatalog.PixelsPerUnit;
            // 시트는 2048px 를 넘지 않도록 여러 줄로 접혀 있다.
            // 줄당 칸 수는 텍스처 폭에서 역산하므로 줄 수를 따로 넘기지 않아도 된다.
            int perRow = Mathf.Max(1, texture.width / size);
            int rows = Mathf.Max(1, texture.height / size);
            var sprites = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                int index = Mathf.Min(i, perRow * rows - 1);
                int column = index % perRow;
                int row = index / perRow;
                var sprite = Sprite.Create(texture,
                    new Rect(column * size, texture.height - (row + 1) * size, size, size),
                    BottomPivot, ppu, 0, SpriteMeshType.FullRect);
                sprite.name = $"{path}#{i:000}";
                sprite.hideFlags = HideFlags.DontSave;
                sprites[i] = sprite;
            }
            Cache[key] = sprites;
            return sprites;
        }

        public static void Clear()
        {
            for (int i = 0; i < RelicCatalog.GroupCount; i++)
            {
                BuriedByGroup[i] = null;
                AliveByGroup[i] = null;
            }
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
