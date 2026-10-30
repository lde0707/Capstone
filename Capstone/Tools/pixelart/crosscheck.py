# -*- coding: utf-8 -*-
"""실제 .cs 파일을 읽어, 54개 품목이 전부 '자기 칸의 그림'을 받는지 교차 확인한다.

C# 의 FindCategoryArt 를 파이썬으로 그대로 옮겨 돌린다.
그림과 칸이 어긋나면 2초 분류가 불공정해지므로, 한 건도 어긋나면 안 된다.
"""
import io, os, re, sys

ROOT = sys.argv[1]
LOOT = os.path.join(ROOT, "Assets/Scripts/DigGame/Core/LootTable.cs")
CAT  = os.path.join(ROOT, "Assets/Scripts/DigGame/World/TrashSpriteCatalog.cs")
ART  = os.path.join(ROOT, "Assets/Resources/TrashSprites/Recycle")

loot = io.open(LOOT, encoding="utf-8").read()
cat  = io.open(CAT,  encoding="utf-8").read()

# ---- 품목 (이름, 칸) ----
items = re.findall(r'new Entry\("([^"]+)",[^)]*?RecyclingCategory\.(\w+)\)', loot)
tables = re.findall(r'Entry\[\] (\w+) =', loot)

# ---- CategoryArt ----
block = cat[cat.index("static readonly ArtRef[][] CategoryArt"):]
block = block[:block.index("};")]
order = ["Specialist", "Can", "Pet", "Paper", "Plastic", "Aluminum"]
rows = re.findall(r"new\[\] \{([^}]*)\}", block)
cat_art = {"Specialist": []}
for name, row in zip(order[1:], rows):
    cat_art[name] = re.findall(r'R\("([^"]+)"\)', row)

# ---- 키워드표 ----
kw_block = cat[cat.index("static readonly KeywordArt[] RecycleKeywordMap"):]
kw_block = kw_block[:kw_block.index("};")]
keywords = [(k, v.rsplit("/", 1)[-1])
            for k, v in re.findall(r'new KeywordArt\("([^"]+)",\s*"([^"]+)"\)', kw_block)]


def stable_index(value, count):
    h = 2166136261
    for ch in value or "":
        h ^= ord(ch); h = (h * 16777619) & 0xFFFFFFFF
    return h % max(1, count)


def find_category_art(name, category):
    choices = cat_art.get(category, [])
    if not choices: return None, "칸 없음"
    for kw, art in keywords:
        if kw.lower() in name.lower() and art in choices:
            return art, f"이름('{kw}')"
    return choices[stable_index(name, len(choices))], "해시"


print(f"표 {len(tables)}개 · 품목 {len(items)}종\n")
files = {f[:-4] for f in os.listdir(ART) if f.endswith(".png")}
problems, by_cat, hashed = [], {}, 0

for name, category in items:
    art, how = find_category_art(name, category)
    by_cat.setdefault(category, []).append((name, art, how))
    if how == "해시": hashed += 1
    if art is None:          problems.append(f"{name}: 칸({category}) 그림이 없다")
    elif art not in files:   problems.append(f"{name}: 파일 없음 {art}.png")
    elif art not in cat_art[category]:
        problems.append(f"{name}: {category} 칸인데 {art} 를 받았다")

for category in ("Paper", "Can", "Pet", "Plastic", "Aluminum"):
    rows = by_cat.get(category, [])
    print(f"[{category}] {len(rows)}종")
    for name, art, how in rows:
        print(f"    {name:<22} -> {art:<15} ({how})")
    print()

print(f"이름으로 정확히 잡힌 품목 {len(items)-hashed}/{len(items)} · 해시 배정 {hashed}")
print(f"그림 파일 {len(files)}장 · 쓰이지 않는 그림: {sorted(files - {a for r in by_cat.values() for _,a,_ in r}) or '없음'}")
if problems:
    print("\n⚠ 문제")
    for p in problems: print("   ", p)
    sys.exit(1)
print("\n✓ 모든 품목이 자기 칸의 그림을 받는다")
