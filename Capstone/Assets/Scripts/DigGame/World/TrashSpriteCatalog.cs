using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    /// <summary>CC0 픽셀 물품을 쓰레기 이름·재질에 맞춰 서로 다른 실루엣으로 배정한다.</summary>
    public static class TrashSpriteCatalog
    {
        const float PixelsPerUnit = 32f;
        const string Root = "TrashSprites/Diverse/";

        readonly struct ArtRef
        {
            public readonly string Path;
            public readonly int FrameWidth;
            public readonly string CacheKey;

            public ArtRef(string file, int frameWidth = 0)
            {
                // '/' 가 들어 있으면 Resources 기준 전체 경로로 본다 (Diverse 밖의 그림).
                Path = file.IndexOf('/') >= 0 ? file : Root + file;
                FrameWidth = frameWidth;
                CacheKey = Path + "#" + frameWidth;
            }
        }

        readonly struct KeywordArt
        {
            public readonly string Keyword;
            public readonly ArtRef Art;

            public KeywordArt(string keyword, string file, int frameWidth = 0)
            {
                Keyword = keyword;
                Art = new ArtRef(file, frameWidth);
            }
        }

        const string RecycleRoot = "TrashSprites/Recycle/";

        static ArtRef R(string file) => new ArtRef(RecycleRoot + file);

        /// <summary>
        /// 분리수거 칸이 정해진 물건은 반드시 그 칸의 그림만 쓴다.
        /// 그림과 칸이 어긋나면 2초 안에 판단해야 하는 분류가 불공정해진다.
        /// 순서는 RecyclingCategory (Specialist, Can, Pet, Paper, Plastic, Aluminum).
        /// </summary>
        static readonly ArtRef[][] CategoryArt =
        {
            new ArtRef[0],
            new[] { R("can_steel"), R("can_drink"), R("can_crushed") },
            new[] { R("pet_water"), R("pet_soda"), R("pet_crushed") },
            new[] { R("paper_news"), R("paper_box"), R("paper_books") },
            new[] { R("plastic_tub"), R("plastic_basin"), R("plastic_crate") },
            new[] { R("foil_ball"), R("foil_tray"), R("foil_sheet") },
        };

        /// <summary>
        /// 품목 이름에 딱 맞는 그림. 더 구체적인 단어를 앞에 둔다.
        /// 여기서 고른 그림이라도 그 물건의 칸에 없으면 버린다 (아래 FindCategoryArt 참고).
        /// </summary>
        static readonly KeywordArt[] RecycleKeywordMap =
        {
            // 종이
            new KeywordArt("신문지",   "TrashSprites/Recycle/paper_news"),
            new KeywordArt("골판지",   "TrashSprites/Recycle/paper_box"),
            new KeywordArt("헌책",     "TrashSprites/Recycle/paper_books"),
            new KeywordArt("만화책",   "TrashSprites/Recycle/paper_books"),
            new KeywordArt("고문서",   "TrashSprites/Recycle/paper_books"),
            new KeywordArt("잡지",     "TrashSprites/Recycle/paper_news"),
            new KeywordArt("속지",     "TrashSprites/Recycle/paper_news"),
            new KeywordArt("설계도",   "TrashSprites/Recycle/paper_news"),
            new KeywordArt("두루마리", "TrashSprites/Recycle/paper_news"),
            new KeywordArt("방수 포장지", "TrashSprites/Recycle/paper_news"),
            new KeywordArt("서류함",   "TrashSprites/Recycle/paper_box"),
            new KeywordArt("문서함",   "TrashSprites/Recycle/paper_box"),
            // 캔
            new KeywordArt("찌그러진 캔", "TrashSprites/Recycle/can_crushed"),
            new KeywordArt("드럼",     "TrashSprites/Recycle/can_crushed"),
            new KeywordArt("통조림",   "TrashSprites/Recycle/can_steel"),
            new KeywordArt("분유",     "TrashSprites/Recycle/can_steel"),
            new KeywordArt("윤활유",   "TrashSprites/Recycle/can_steel"),
            new KeywordArt("청동 깡통", "TrashSprites/Recycle/can_steel"),
            new KeywordArt("음료 캔",  "TrashSprites/Recycle/can_drink"),
            new KeywordArt("합금 캔",  "TrashSprites/Recycle/can_drink"),
            // 페트
            new KeywordArt("생수",     "TrashSprites/Recycle/pet_water"),
            new KeywordArt("초기형 페트", "TrashSprites/Recycle/pet_water"),
            new KeywordArt("압착",     "TrashSprites/Recycle/pet_crushed"),
            new KeywordArt("유리질 페트", "TrashSprites/Recycle/pet_water"),
            new KeywordArt("탄산",     "TrashSprites/Recycle/pet_soda"),
            new KeywordArt("페트 탱크", "TrashSprites/Recycle/pet_soda"),
            new KeywordArt("수액 페트", "TrashSprites/Recycle/pet_soda"),
            // 플라스틱
            new KeywordArt("반찬통",   "TrashSprites/Recycle/plastic_tub"),
            new KeywordArt("보관통",   "TrashSprites/Recycle/plastic_tub"),
            new KeywordArt("보관함",   "TrashSprites/Recycle/plastic_tub"),
            new KeywordArt("바가지",   "TrashSprites/Recycle/plastic_basin"),
            new KeywordArt("플라스틱 통", "TrashSprites/Recycle/plastic_basin"),
            new KeywordArt("수납 상자", "TrashSprites/Recycle/plastic_crate"),
            new KeywordArt("운반 상자", "TrashSprites/Recycle/plastic_crate"),
            new KeywordArt("적재함",   "TrashSprites/Recycle/plastic_crate"),
            // 알루미늄
            new KeywordArt("포일 뭉치", "TrashSprites/Recycle/foil_ball"),
            new KeywordArt("과자봉지", "TrashSprites/Recycle/foil_ball"),
            new KeywordArt("단열재",   "TrashSprites/Recycle/foil_ball"),
            new KeywordArt("접시",     "TrashSprites/Recycle/foil_tray"),
            new KeywordArt("도시락",   "TrashSprites/Recycle/foil_tray"),
            new KeywordArt("방열판",   "TrashSprites/Recycle/foil_sheet"),
            new KeywordArt("샷시",     "TrashSprites/Recycle/foil_sheet"),
            new KeywordArt("봉인포",   "TrashSprites/Recycle/foil_sheet"),
            new KeywordArt("합금 포일", "TrashSprites/Recycle/foil_sheet"),
            new KeywordArt("포장지",   "TrashSprites/Recycle/foil_sheet"),
        };

        /// <summary>
        /// 그 물건의 칸에 속한 그림만 돌려준다.
        /// 이름에 맞는 그림이 없으면 칸 안에서 이름 해시로 고정 배정한다
        /// (같은 이름은 늘 같은 그림이라, 플레이어가 그림으로 외울 수 있다).
        /// </summary>
        static ArtRef FindCategoryArt(LootItem item)
        {
            int category = (int)item.Category;
            if (category <= 0 || category >= CategoryArt.Length) return default;

            ArtRef[] choices = CategoryArt[category];
            if (choices.Length == 0) return default;

            if (!string.IsNullOrEmpty(item.Name))
            {
                for (int i = 0; i < RecycleKeywordMap.Length; i++)
                {
                    var candidate = RecycleKeywordMap[i];
                    if (item.Name.IndexOf(candidate.Keyword, System.StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                    for (int j = 0; j < choices.Length; j++)
                        if (choices[j].Path == candidate.Art.Path) return candidate.Art;
                }
            }

            return choices[StableIndex(item.Name, choices.Length)];
        }

        // 더 구체적인 단어를 먼저 둬서 실제 아이템 이름과 가장 가까운 그림을 선택한다.
        static readonly KeywordArt[] KeywordMap =
        {
            new KeywordArt("찌그러진 캔", "broken_can"),
            new KeywordArt("분리수거 캔", "broken_can"),
            new KeywordArt("페트병", "bottle", 32),
            new KeywordArt("유리병", "bottle", 32),
            new KeywordArt("소주병", "bottle", 32),
            new KeywordArt("어항", "bottle", 32),
            new KeywordArt("유리판", "bottle", 32),
            new KeywordArt("자전거", "tire", 32),
            new KeywordArt("타이어", "tire", 32),
            new KeywordArt("의료", "mask", 32),
            new KeywordArt("방사성", "mask", 32),
            new KeywordArt("유해", "mask", 32),
            new KeywordArt("비닐", "takeout_box", 32),
            new KeywordArt("상자", "takeout_box", 32),
            new KeywordArt("보석함", "takeout_box", 32),
            new KeywordArt("카세트테이프", "cassette"),
            new KeywordArt("워크맨", "cassette_player"),
            new KeywordArt("게임팩", "handheld_game_a"),
            new KeywordArt("휴대용 게임기", "handheld_game_b"),
            new KeywordArt("오락기", "lcd_game"),
            new KeywordArt("스마트폰", "phone"),
            new KeywordArt("휴대전화", "phone"),
            new KeywordArt("통신", "walkie_talkie"),
            new KeywordArt("라디오", "radio"),
            new KeywordArt("축음기", "vinyl"),
            new KeywordArt("블랙박스", "camera"),
            new KeywordArt("카메라", "camera"),
            new KeywordArt("브라운관", "vcr"),
            new KeywordArt("VHS", "vhs"),
            new KeywordArt("제어반", "laptop"),
            new KeywordArt("데이터", "laptop"),
            new KeywordArt("계측", "remote"),
            new KeywordArt("센서", "remote"),
            new KeywordArt("기판", "remote"),
            new KeywordArt("배터리", "battery"),
            new KeywordArt("촉매", "battery"),
            new KeywordArt("코일", "headphones"),
            new KeywordArt("구리선", "headphones"),
            new KeywordArt("시계", "watch"),
            new KeywordArt("동전", "watch"),
            new KeywordArt("은화", "watch"),
            new KeywordArt("금화", "watch"),
            new KeywordArt("금괴", "battery"),
            new KeywordArt("인장", "rubiks_cube"),
            new KeywordArt("장난감", "yoyo"),
            new KeywordArt("도자기", "bottle", 32),
            new KeywordArt("촛대", "crayon"),
            new KeywordArt("합금", "pocketknife"),
            new KeywordArt("고철", "tire", 32),
            new KeywordArt("금속", "pocketknife")
        };

        static readonly ArtRef[][] MaterialFallbacks =
        {
            new[] { A("takeout_box", 32), A("flipflop", 32), A("crayon"), A("yoyo"), A("rubiks_cube") },
            new[] { A("broken_can"), A("tire", 32), A("battery"), A("pocketknife"), A("watch") },
            new[] { A("bottle", 32), A("camera"), A("watch") },
            new[] { A("bottle", 32), A("flipflop", 32), A("mask", 32), A("rubiks_cube"), A("yoyo") },
            new[] { A("boombox"), A("camera"), A("laptop"), A("walkie_talkie"), A("handheld_game_a"),
                    A("handheld_game_b"), A("handheld_game_c"), A("cassette_player"), A("lcd_game"),
                    A("phone"), A("radio"), A("remote"), A("vcr"), A("vhs") },
            new[] { A("mask", 32), A("battery"), A("broken_can"), A("takeout_box", 32) },
            new[] { A("watch"), A("vinyl"), A("camera"), A("rubiks_cube"), A("pocketknife") }
        };

        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        static ArtRef A(string file, int frameWidth = 0) => new ArtRef(file, frameWidth);

        public static Sprite For(LootItem item)
        {
            // 분리수거 칸이 있으면 그 칸의 그림이 최우선이다.
            ArtRef categoryArt = FindCategoryArt(item);
            if (!string.IsNullOrEmpty(categoryArt.Path)) return Load(categoryArt);

            Sprite illustration = RecyclingItemSprites.For(item);
            if (illustration != null) return illustration;
            ArtRef art = FindByName(item.Name);
            if (string.IsNullOrEmpty(art.Path))
            {
                int material = Mathf.Clamp((int)item.Material, 0, MaterialFallbacks.Length - 1);
                ArtRef[] choices = MaterialFallbacks[material];
                art = choices[StableIndex(item.Name, choices.Length)];
            }
            return Load(art);
        }

        static ArtRef FindByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return default;
            for (int i = 0; i < KeywordMap.Length; i++)
                if (name.IndexOf(KeywordMap[i].Keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return KeywordMap[i].Art;
            return default;
        }

        static Sprite Load(ArtRef art)
        {
            if (string.IsNullOrEmpty(art.Path)) return null;
            string cacheKey = art.CacheKey;
            if (Cache.TryGetValue(cacheKey, out var cached)) return cached;

            var texture = Resources.Load<Texture2D>(art.Path);
            if (texture == null)
            {
                Debug.LogWarning($"[DigGame] 픽셀 쓰레기 이미지를 찾지 못했습니다: {art.Path}");
                Cache[cacheKey] = null;
                return null;
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            int frameWidth = art.FrameWidth > 0 ? Mathf.Min(art.FrameWidth, texture.width) : texture.width;
            int frameHeight = art.FrameWidth > 0 ? Mathf.Min(art.FrameWidth, texture.height) : texture.height;
            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, frameWidth, frameHeight),
                new Vector2(0.5f, 0.5f),
                PixelsPerUnit,
                0,
                SpriteMeshType.FullRect);
            sprite.name = texture.name + " (Runtime Sprite)";
            sprite.hideFlags = HideFlags.DontSave;
            Cache[cacheKey] = sprite;
            return sprite;
        }

        static int StableIndex(string value, int count)
        {
            unchecked
            {
                uint hash = 2166136261u;
                if (!string.IsNullOrEmpty(value))
                    for (int i = 0; i < value.Length; i++)
                    {
                        hash ^= value[i];
                        hash *= 16777619u;
                    }
                return (int)(hash % (uint)Mathf.Max(1, count));
            }
        }

        public static void Clear()
        {
            foreach (var sprite in Cache.Values)
            {
                if (sprite == null) continue;
                if (Application.isPlaying) Object.Destroy(sprite);
                else Object.DestroyImmediate(sprite);
            }
            Cache.Clear();
        }
    }
}
