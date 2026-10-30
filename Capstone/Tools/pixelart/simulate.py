# -*- coding: utf-8 -*-
"""실제 LootTable.cs 를 읽어 10만 번 굴려 층별 분리수거 칸 분포를 본다."""
import io, random, re, sys

LOOT = sys.argv[1]
SINGLE = 2.75
N = 100000
src = io.open(LOOT, encoding="utf-8").read()

tables = {}
for m in re.finditer(r"Entry\[\] (\w+) =\s*\{(.*?)\n        \};", src, re.S):
    rows = re.findall(
        r'new Entry\("([^"]+)",\s*(\d+),\s*(\d+),\s*MaterialKind\.(\w+),\s*'
        r'([\d.]+)f,\s*([\d.]+)f,\s*RecyclingCategory\.(\w+)\)', m.group(2))
    tables[m.group(1)] = [(n, int(v), int(vol), mat, float(sh), float(w), c)
                          for n, v, vol, mat, sh, w, c in rows]

CATS = ["Paper", "Can", "Pet", "Plastic", "Aluminum"]
ZONES = [("1층", "SurfaceLoot"), ("2층", "ShallowLoot"),
         ("3층", "DeepLoot"), ("4층", "AbyssLoot"), ("시작", "StarterLoot")]

def roll(rows, single_only):
    rows = [e for e in rows if not single_only or e[2] == 1]
    w = [e[5] * (SINGLE if e[2] == 1 else 1.0) for e in rows]
    return random.choices(rows, weights=w, k=N)

rng = random.Random(7)
random.seed(7)
print(f"{'층':<5}{'품목':>5}  " + "".join(f"{c:>10}" for c in CATS) + f"{'값 범위':>18}")
ok = True
for label, key in ZONES:
    rows = tables[key]
    picks = roll(rows, False)
    counts = {c: 0 for c in CATS}
    for p in picks: counts[p[6]] += 1
    vals = [e[1] for e in rows]
    line = f"{label:<5}{len(rows):>5}종 " + "".join(f"{counts[c]/N*100:9.1f}%" for c in CATS)
    print(line + f"{min(vals):>8,}~{max(vals):,}원")
    for c in CATS:
        if counts[c] == 0: ok = False; print(f"      ⚠ {c} 칸이 한 번도 안 나옴")

print("\n부피 1칸만 뽑을 때 (압축층·가스층·시작 지점):")
for label, key in ZONES:
    rows = [e for e in tables[key] if e[2] == 1]
    if not rows: continue
    picks = roll(tables[key], True)
    counts = {c: 0 for c in CATS}
    for p in picks: counts[p[6]] += 1
    print(f"{label:<5}{len(rows):>5}종 " + "".join(f"{counts[c]/N*100:9.1f}%" for c in CATS))
    for c in CATS:
        if counts[c] == 0: ok = False; print(f"      ⚠ {c} 칸이 한 번도 안 나옴")

total = sum(len(v) for v in tables.values())
nocat = [e[0] for v in tables.values() for e in v if not e[6]]
print(f"\n표 {len(tables)}개 · 품목 {total}종 · 분리수거 칸 없는 품목 {len(nocat)}종")
print("✓ 모든 층에서 5개 칸이 전부 나온다" if ok else "⚠ 빠진 칸이 있다")
sys.exit(0 if ok else 1)
