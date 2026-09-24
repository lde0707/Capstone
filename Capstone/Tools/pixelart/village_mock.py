"""지상 마을 배치 검산 — 실제 에셋을 C# 좌표 규칙 그대로 찍어 본다.

Village.cs / GameConfig.cs 의 값을 그대로 옮겨 놓았다.
건물 0.7배 축소 + 3칸 간격이 실제로 어떻게 보이는지 눈으로 확인하는 용도.
"""
from PIL import Image, ImageDraw, ImageFont
from relic_lib import cell as sheet_cell

TILE = 16                 # 1칸 = 16px
SCALE = 0.70              # GameConfig.BuildingScale
RELIC_SCALE = 0.42        # GameConfig.RelicAliveScale
STEP = 3                  # GameConfig.NeighborSpacing
FONT = "/usr/share/fonts/opentype/noto/NotoSansCJK-Bold.ttc"


def slots(left, right, step, count):
    out = []
    for i in range(count):
        pair = i // 2
        out.append(-(left + pair * step) if i % 2 == 0 else right + pair * step)
    return out


def load(path, size, index):
    sheet = Image.open(path)
    per_row = max(1, sheet.width // size)
    r, c = divmod(index, per_row)
    return sheet.crop((c * size, r * size, (c + 1) * size, (r + 1) * size))


def put(canvas, img, center_cells, origin, scale, ground):
    """bottom-center 피벗으로 찍는다. center_cells 는 마을 중심 기준 칸 좌표."""
    w = max(1, int(round(img.width * scale)))
    h = max(1, int(round(img.height * scale)))
    im = img.resize((w, h), Image.NEAREST)
    x = int(round((center_cells + origin) * TILE)) - w // 2
    canvas.alpha_composite(im, (x, ground - h))


LANDMARK_SCALE = 0.78       # GameConfig.LandmarkScale
LANDMARKS = [-18, 20, -29, 31, -40, 42]


def build(era, houses, props, relics, out_path, span_cells=86, landmarks=0):
    origin = span_cells                     # 캔버스 왼쪽 끝이 -span 칸
    W = (span_cells * 2 + 1) * TILE
    ground = 78
    H = ground + 10
    canvas = Image.new("RGBA", (W, H), (96, 122, 150, 255))
    d = ImageDraw.Draw(canvas)
    d.rectangle([0, ground, W, H], fill=(86, 118, 70, 255))

    B = f"out/Era/era_buildings_{era}.png"
    P = f"out/Era/era_props_{era}.png"

    # 이웃 건물 → 소품 → 유물 순서로 겹쳐 찍는다 (C# 의 sortingOrder 와 같은 순서)
    kinds = 7 if era in (0, 4) else 4
    for i, off in enumerate(houses):
        put(canvas, load(B, 64, (i * 3 + 1) % kinds), off + 0.5, origin, SCALE, ground)

    # 고정 시설
    put(canvas, load("out/Era/era_dropoffs.png", 48, era), -7.0, origin, SCALE, ground)
    put(canvas, load("out/Era/era_houses.png", 48, era), -2.5, origin, SCALE, ground)
    put(canvas, load("out/Era/era_shops.png", 48, era), 7.0, origin, SCALE, ground)
    put(canvas, load("out/Era/era_restorers.png", 48, era), 10.5, origin, SCALE, ground)

    for i, off in enumerate(props):
        put(canvas, load(P, 32, (i * 4 + 1) % 10), off + 0.5, origin, SCALE, ground)

    for off, (tag, idx, size) in relics:
        img = sheet_cell(Image.open(f"out/Relic/relic_alive_{tag}.png"), idx, size)
        put(canvas, img, off + 0.5, origin, RELIC_SCALE, ground)

    # 세트 보너스 랜드마크 — 주변 집·소품을 밀어내고 들어선다
    for i in range(landmarks):
        img = load("out/Era/set_landmarks.png", 96, i)
        put(canvas, img, LANDMARKS[i] + 0.5, origin, LANDMARK_SCALE, ground)

    # 땅굴 입구 (최대 확장 0~+5칸)
    d.rectangle([int((0 + origin) * TILE), ground,
                 int((5 + origin) * TILE), H], fill=(38, 30, 26, 255))

    out = canvas.resize((W * 2, H * 2), Image.NEAREST)
    label = ImageDraw.Draw(out)
    f = ImageFont.truetype(FONT, 20)
    label.rectangle([0, 0, out.width, 30], fill=(22, 20, 26, 235))
    label.text((10, 5),
               f"{['원시','고대','조선','근대','미래'][era]} 시대 · 건물 {len(houses)}채 "
               f"· 소품 {len(props)} · 유물 {len(relics)} · 랜드마크 {landmarks} "
               f"(건물 {int(SCALE*100)}% 크기 · {STEP}칸 간격)",
               font=f, fill=(244, 238, 222))
    out.save(out_path)
    return out.size


if __name__ == "__main__":
    import os
    os.makedirs("out/mock", exist_ok=True)

    H = slots(11, 13, STEP, 52)
    P = slots(12, 14, STEP, 52)
    R = slots(10, 15, STEP, 52)

    # 후반부 = 모든 자리가 찬 상태
    relic_mix = [("a", 4 * 4, 64), ("d", 8 * 4, 64), ("e", 7 * 4, 64), ("b", 4 * 4, 64),
                 ("c", 0 * 4, 64), ("f", 3 * 4, 64), ("d", 6 * 4, 64), ("e", 5 * 4, 64),
                 ("a", 1 * 4, 64), ("f", 1 * 4, 64), ("d", 2 * 4, 64), ("e", 9 * 4, 64)]

    print("가득 찬 마을:",
          build(4, H, P, list(zip(R[:len(relic_mix)], relic_mix)),
                "out/mock/village_full.png", landmarks=6))
    print("초반 마을:",
          build(0, H[:8], P[:8], list(zip(R[:3], relic_mix[:3])), "out/mock/village_early.png"))
