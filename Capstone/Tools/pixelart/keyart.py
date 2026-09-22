"""쓰레기 집게 소년 — 키아트형 메인 로고 (캐릭터 + 마을 + 지하 단면)."""
import math
import random
from PIL import Image

import village as V
import player as P
import npc as N
import logo as L
from pixlib import Canvas

W, H = 384, 216
SURFACE = 132          # 지표면 y

# ---- 색 ----
SKY_TOP = (46, 40, 86)
SKY_MID = (128, 78, 116)
SKY_LOW = (232, 138, 92)
SKY_HORIZON = (250, 196, 120)
SUN = (255, 232, 168)
SUN_CORE = (255, 248, 216)
CLOUD1 = (206, 150, 158)
CLOUD2 = (166, 112, 132)

LAYER = [
    ((150, 112, 76), (118, 86, 58)),     # 표층 흙
    ((146, 96, 74), (112, 72, 54)),      # 90년대 층
    ((116, 108, 106), (88, 82, 82)),     # 산업 폐기물층
    ((74, 70, 84), (54, 50, 64)),        # 최심층
]
CAVE = (54, 40, 46)
CAVE_DARK = (38, 28, 34)


def img_of(canvas):
    return canvas.to_image() if isinstance(canvas, Canvas) else canvas


class Art:
    def __init__(self, w, h):
        self.img = Image.new("RGBA", (w, h), (0, 0, 0, 255))
        self.px = self.img.load()
        self.w, self.h = w, h

    def set(self, x, y, c):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.px[int(x), int(y)] = c if len(c) == 4 else c + (255,)

    def rect(self, x0, y0, x1, y1, c):
        for y in range(max(0, y0), min(self.h, y1 + 1)):
            for x in range(max(0, x0), min(self.w, x1 + 1)):
                self.px[x, y] = c if len(c) == 4 else c + (255,)

    def paste(self, im, x, y):
        self.img.alpha_composite(im.convert("RGBA"), (int(x), int(y)))


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3))


# ------------------------------------------------------------------ 하늘
def draw_sky(a):
    for y in range(SURFACE):
        t = y / (SURFACE - 1)
        if t < 0.45:
            c = lerp(SKY_TOP, SKY_MID, t / 0.45)
        elif t < 0.8:
            c = lerp(SKY_MID, SKY_LOW, (t - 0.45) / 0.35)
        else:
            c = lerp(SKY_LOW, SKY_HORIZON, (t - 0.8) / 0.2)
        a.rect(0, y, W - 1, y, c)

    # 해
    cx, cy, r = 300, 96, 17
    for y in range(cy - r - 1, cy + r + 2):
        for x in range(cx - r - 1, cx + r + 2):
            d = math.hypot(x - cx, y - cy)
            if d <= r - 5:
                a.set(x, y, SUN_CORE)
            elif d <= r:
                a.set(x, y, SUN)
    for i, yy in enumerate((cy - 6, cy + 1, cy + 8)):     # 해 가로줄
        a.rect(cx - r - 6 - i * 3, yy, cx + r + 6 + i * 3, yy + 1, SKY_LOW)

    # 별 (위쪽만)
    rng = random.Random(7)
    for _ in range(40):
        x, y = rng.randrange(W), rng.randrange(46)
        if rng.random() < 0.5:
            a.set(x, y, (238, 226, 240))

    # 구름
    for (cx0, cy0, s) in ((44, 40, 3), (150, 30, 2), (250, 52, 2), (330, 34, 3), (96, 66, 2)):
        for i in range(4):
            w = (5 - i) * s
            a.rect(cx0 - w, cy0 + i * 2, cx0 + w + s * 3, cy0 + i * 2 + 1,
                   CLOUD1 if i < 2 else CLOUD2)


# ------------------------------------------------------------------ 지하
def draw_underground(a):
    rng = random.Random(11)
    bands = [(SURFACE, SURFACE + 22, 0), (SURFACE + 22, SURFACE + 44, 1),
             (SURFACE + 44, SURFACE + 64, 2), (SURFACE + 64, H, 3)]
    def wobble(x, li):
        return int(round(math.sin(x * 0.045 + li * 2.1) * 3 + math.sin(x * 0.11 + li) * 1.5))
    for x in range(W):
        for y in range(SURFACE, H):
            li = 3
            for i, (y0, y1, idx) in enumerate(bands):
                if y < y1 + wobble(x, idx):
                    li = idx
                    break
            base, dark = LAYER[li]
            n = (x * 7 + y * 13 + li * 29) % 11
            a.set(x, y, base if n > 2 else dark)
    for (y0, y1, li) in bands[1:]:
        base, dark = LAYER[li]
        for x in range(W):
            yy = y0 + wobble(x, li)
            a.set(x, yy, dark)
            a.set(x, yy + 1, dark)

    # 잔디 윗면
    for x in range(W):
        a.set(x, SURFACE - 1, (112, 146, 80))
        a.set(x, SURFACE, (86, 118, 62))
        if (x * 5) % 7 == 0:
            a.set(x, SURFACE - 2, (128, 162, 92))
    return rng


def carve(a, cells, r=6, acc=None):
    """굴 파기 — 좌표 리스트를 따라 원형으로 뚫는다. 뚫린 픽셀 좌표를 모아 돌려준다."""
    if acc is None:
        acc = set()
    for (cx, cy) in cells:
        for y in range(cy - r - 1, cy + r + 2):
            for x in range(cx - r - 1, cx + r + 2):
                if math.hypot(x - cx, y - cy) <= r:
                    acc.add((x, y))
    return acc


def finish_cave(a, cave):
    """굴 내부를 칠하고 위쪽 테두리에 빛, 아래쪽에 바닥을 넣는다."""
    RIM = (120, 92, 74)
    FLOOR = (78, 60, 58)
    for (x, y) in cave:
        depth = min(1.0, max(0.0, (y - SURFACE) / 90.0))
        c = tuple(int(CAVE[i] + (CAVE_DARK[i] - CAVE[i]) * depth) for i in range(3))
        a.set(x, y, c)
    for (x, y) in cave:
        if (x, y - 1) not in cave:
            a.set(x, y, RIM)
            a.set(x, y + 1, tuple(int(v * 0.72) for v in RIM))
        if (x, y + 1) not in cave:
            a.set(x, y, FLOOR)
        if (x - 1, y) not in cave or (x + 1, y) not in cave:
            a.set(x, y, tuple(int(v * 0.8) for v in FLOOR))


def tunnel_path(pts, r=6):
    out = []
    for i in range(len(pts) - 1):
        (x0, y0), (x1, y1) = pts[i], pts[i + 1]
        steps = int(max(abs(x1 - x0), abs(y1 - y0)))
        for s in range(steps + 1):
            t = s / max(1, steps)
            out.append((int(x0 + (x1 - x0) * t), int(y0 + (y1 - y0) * t)))
    return out


# ------------------------------------------------------------------ 묻힌 쓰레기
def buried(a, x, y, kind):
    C = {
        "can": ((196, 200, 206), (140, 146, 154)),
        "bottle": ((140, 196, 176), (92, 148, 132)),
        "tv": ((124, 116, 138), (86, 80, 98)),
        "gold": ((246, 208, 116), (198, 154, 62)),
    }[kind]
    lo, hi = C
    if kind == "can":
        a.rect(x, y, x + 4, y + 6, lo)
        a.rect(x, y + 2, x + 4, y + 3, hi)
    elif kind == "bottle":
        a.rect(x + 1, y, x + 3, y + 1, hi)
        a.rect(x, y + 2, x + 4, y + 7, lo)
    elif kind == "tv":
        a.rect(x, y, x + 8, y + 6, lo)
        a.rect(x + 1, y + 1, x + 6, y + 4, hi)
        a.set(x + 7, y + 2, (240, 236, 210))
    else:
        for i in range(3):
            a.rect(x + i, y + i, x + 5 - i, y + 5 - i, lo if i % 2 == 0 else hi)
    for dx in range(-1, 10):
        for dy in range(-1, 9):
            pass


# ------------------------------------------------------------------ 마을
def draw_village(a):
    ground = SURFACE - 1
    road = [V.road_tile("cobble", s).to_image() for s in range(2)]
    for x in range(0, W, 16):
        a.paste(road[(x // 16) % 2], x, SURFACE - 1)

    # 집: 왼쪽으로 갈수록 화려하게 (성장의 흔적)
    plan = [(6, 5), (58, 3), (110, 4), (168, 2), (214, 1), (256, 0)]
    for (x, tier) in plan:
        h = V.house(V.HOUSE_TIERS[tier]).to_image()
        a.paste(h, x, ground - h.height + 2)

    props = [(50, V.lamp_post), (100, lambda: V.tree(True, 1)), (154, V.fence),
             (204, lambda: V.tree(False, 4)), (246, V.well), (300, lambda: V.tree(True, 7)),
             (338, V.bench), (364, V.flowerbed)]
    for (x, fn) in props:
        im = fn().to_image()
        a.paste(im, x, ground - im.height + 2)

    # 주민
    people = N.build()
    names = list(people)
    for i, (x, f) in enumerate(((84, 1), (140, 2), (232, 0), (322, 3))):
        sheet = people[names[i % len(names)]]
        frame = sheet.crop((f * 16, 0, f * 16 + 16, 16))
        if i % 2:
            frame = frame.transpose(Image.FLIP_LEFT_RIGHT)
        a.paste(frame, x, ground - 15)


def glow(a, cx, cy, r, col=(255, 236, 170)):
    """집에서 새어나오는 빛."""
    for y in range(cy - r, cy + r):
        for x in range(cx - r, cx + r):
            d = math.hypot(x - cx, (y - cy) * 1.4)
            if d < r:
                t = (1 - d / r) ** 2 * 0.55
                if 0 <= x < a.w and 0 <= y < a.h:
                    old = a.px[x, y]
                    a.px[x, y] = tuple(int(old[i] + (col[i] - old[i]) * t) for i in range(3)) + (255,)


# ------------------------------------------------------------------ 조립
def build():
    a = Art(W, H)
    draw_sky(a)
    draw_underground(a)

    # 굴 — 지상에서 내려가 갈라지는 통로
    main = tunnel_path([(96, SURFACE + 6), (96, SURFACE + 34), (150, SURFACE + 52),
                        (214, SURFACE + 58), (262, SURFACE + 74)], r=7)
    branch = tunnel_path([(96, SURFACE + 30), (44, SURFACE + 44), (20, SURFACE + 66)], r=6)
    branch2 = tunnel_path([(214, SURFACE + 58), (300, SURFACE + 40), (352, SURFACE + 30)], r=6)
    cave = carve(a, main, 5)
    carve(a, branch, 4, cave)
    carve(a, branch2, 4, cave)
    for (cx, cy, rr) in ((200, SURFACE + 38, 18), (216, SURFACE + 42, 15),
                         (192, SURFACE + 46, 13)):
        carve(a, [(cx, cy)], rr, cave)           # 주인공이 파낸 공간 (원 3개로 울퉁불퉁하게)
    finish_cave(a, cave)

    for (x, y, k) in ((60, SURFACE + 12, "can"), (150, SURFACE + 16, "bottle"),
                      (250, SURFACE + 20, "tv"), (36, SURFACE + 30, "bottle"),
                      (196, SURFACE + 34, "can"), (320, SURFACE + 52, "tv"),
                      (120, SURFACE + 66, "gold"), (272, SURFACE + 66, "gold"),
                      (66, SURFACE + 56, "can")):
        buried(a, x, y, k)

    draw_village(a)
    glow(a, 28, SURFACE - 24, 34)          # 최고 단계 집에서 빛
    glow(a, 130, SURFACE - 18, 24)

    # 주인공 — 굴 안에서 채굴 중
    hero_sheet = P.build()["hero_dig_side"]
    glow(a, 204, SURFACE + 36, 30, (255, 224, 150))                # 헤드랜턴 불빛
    hero = hero_sheet.crop((32, 0, 48, 16)).resize((48, 48), Image.NEAREST)
    a.paste(hero, 182, SURFACE + 12)
    a.rect(190, SURFACE + 58, 220, SURFACE + 58, (24, 16, 22))     # 발밑 그림자

    return a


def add_title(a):
    """제목을 하늘 위에 크게."""
    for y in range(0, 108):                       # 제목 뒤 그라데이션
        t = (1 - abs(y - 48) / 60.0)
        if t <= 0:
            continue
        for x in range(W):
            old = a.px[x, y]
            a.px[x, y] = tuple(int(old[i] * (1 - 0.30 * t)) for i in range(3)) + (255,)
    c = L.Px(W, H)
    tw, _ = L.text_size("쓰레기", 22, spacing=3)
    L.draw_text(c, "쓰레기", 22, (W - tw) // 2 - 30, 16,
                [L.GREEN1, L.GREEN1, L.GREEN2, L.GREEN3],
                shadow=(2, 3, (24, 30, 30, 255)), top_light=L.WHITE, spacing=3)
    tw2, _ = L.text_size("집게소년", 46, spacing=-2)
    L.draw_text(c, "집게소년", 46, (W - tw2) // 2, 40,
                [L.Y1, L.Y1, L.Y2, L.Y3],
                shadow=(4, 5, (28, 22, 28, 255)), top_light=L.WHITE, spacing=-2)
    c.paste(L.recycle_symbol(30), (W - tw) // 2 + tw - 26, 8)
    a.img.alpha_composite(c.img, (0, 0))

    # 하단 부제 밴드 (마을을 가리지 않게 맨 아래로)
    band_y = H - 20
    for y in range(band_y - 6, H):
        t = min(1.0, (y - (band_y - 6)) / 8.0)
        for x in range(W):
            old = a.px[x, y]
            a.px[x, y] = tuple(int(old[i] * (1 - 0.82 * t)) for i in range(3)) + (255,)
    sub = L.Px(W, H)
    stw, _ = L.text_size("7일 안에 도시를 세워라", 14, font_path=L.FONT_BOLD, spacing=2)
    L.draw_text(sub, "7일 안에 도시를 세워라", 14, (W - stw) // 2, band_y + 1,
                [(244, 232, 202, 255)], outline=None, font_path=L.FONT_BOLD, spacing=2)
    a.img.alpha_composite(sub.img, (0, 0))


if __name__ == "__main__":
    import os
    os.makedirs("out/logo", exist_ok=True)
    a = build()
    add_title(a)
    a.img.save("out/logo/keyart.png")
    for s in (2, 4, 5):
        a.img.resize((W * s, H * s), Image.NEAREST).save(f"out/logo/keyart@{s}x.png")
    print("ok", a.img.size)
