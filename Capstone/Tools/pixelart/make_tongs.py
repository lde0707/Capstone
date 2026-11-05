# -*- coding: utf-8 -*-
"""집게 5단계 x (다묾/벌림) = 10칸. 한 줄 시트 160x16.

16px 캐릭터 손에 들리는 크기라, 자루가 길면 그냥 막대기로 보인다.
=> 자루는 짧게(3칸), 집게발을 굵고 크게 그려 '집게'가 실루엣으로 읽히게 한다.

그리는 기준
  - 쥐는 점 = 캔버스 한가운데 (8,8). 유니티에서 이 점을 축으로 돌린다.
  - 0도 = 오른쪽. 플레이어 원본 굴착 애니에서 뽑은 각도를 그대로 쓴다.
  - 단계가 오르면 색뿐 아니라 '발 모양'이 바뀐다 (뭉툭 -> 각짐 -> 뾰족 -> 톱니 -> 삼지창).
"""
import math, os
from PIL import Image

W = H = 16
GX, GY = 8, 8                 # 쥐는 점 = 회전축
LINE = (24, 22, 30, 255)


class Cell:
    def __init__(self):
        self.px = [[None] * W for _ in range(H)]

    def set(self, x, y, c):
        x, y = int(round(x)), int(round(y))
        if c and 0 <= x < W and 0 <= y < H: self.px[y][x] = c

    def line(self, x0, y0, x1, y1, c, thick=1):
        x0, y0, x1, y1 = map(lambda v: int(round(v)), (x0, y0, x1, y1))
        dx, dy = abs(x1 - x0), -abs(y1 - y0)
        sx = 1 if x0 < x1 else -1
        sy = 1 if y0 < y1 else -1
        err = dx + dy
        while True:
            self.set(x0, y0, c)
            if thick > 1:
                self.set(x0 + 1, y0, c)
                if thick > 2: self.set(x0, y0 + 1, c)
            if x0 == x1 and y0 == y1: break
            e2 = 2 * err
            if e2 >= dy: err += dy; x0 += sx
            if e2 <= dx: err += dx; y0 += sy

    def blob(self, x, y, c):
        for dx, dy in ((0,0),(1,0),(0,1),(1,1)): self.set(x+dx, y+dy, c)

    def outline(self):
        add = []
        for y in range(H):
            for x in range(W):
                if self.px[y][x] is not None: continue
                for dx, dy in ((1,0),(-1,0),(0,1),(0,-1)):
                    nx, ny = x+dx, y+dy
                    if 0 <= nx < W and 0 <= ny < H and self.px[ny][nx] is not None:
                        add.append((x, y)); break
        for x, y in add: self.set(x, y, LINE)

    def image(self):
        im = Image.new("RGBA", (W, H), (0, 0, 0, 0))
        for y in range(H):
            for x in range(W):
                if self.px[y][x]: im.putpixel((x, y), self.px[y][x])
        return im


#       이름         자루 어둠/밝음            금속 어둠/중간/밝음               발 모양   강조
TIERS = [
    ("낡은 집게",   (74,52,34),  (120,86,54),   (122,106,82), (176,152,120), (216,198,168), "blunt", None),
    ("철 집게",     (80,66,54),  (128,108,88),  (84,92,102),  (138,150,162), (196,206,218), "angled", None),
    ("강철 집게",   (62,70,84),  (104,116,134), (92,110,132), (156,178,202), (220,238,252), "sharp", (126,186,236)),
    ("합금 집게",   (58,54,66),  (96,92,112),   (122,108,70), (192,170,104), (248,232,162), "saw",   (255,226,130)),
    ("최종형 집게", (40,50,62),  (74,92,112),   (62,146,146), (120,218,210), (214,255,250), "trident",(130,255,240)),
]


def at(dist, ang, ox=GX, oy=GY):
    a = math.radians(ang)
    return ox + dist * math.cos(a), oy - dist * math.sin(a)


def draw(tier_index, jaws_open, ang=0.0):
    name, hd, hl, md, mm, ml, shape, glow = TIERS[tier_index]
    hd, hl, md, mm, ml = (c + (255,) for c in (hd, hl, md, mm, ml))
    hd, hl, md, mm, ml = map(tuple, (hd, hl, md, mm, ml))
    c = Cell()

    handle = 1.9                         # 자루는 짧게 — 길면 막대기로 보인다
    hinge = 1.9 + tier_index * 0.10
    reach = 2.1 + tier_index * 0.16      # 발 길이
    spread = (24 if jaws_open else 10) + (4 if tier_index >= 3 else 0)
    curl = 0.55                          # 발끝이 안쪽으로 말린다 = 집게로 읽히는 핵심

    # ---- 자루 ----
    bx, by = at(-handle, ang)
    hx, hy = at(hinge, ang)
    c.line(bx, by, hx, hy, hd, thick=2)
    c.line(*at(-handle + .6, ang), *at(hinge - .8, ang), hl)
    c.blob(*map(int, at(-handle - .4, ang)), hl)     # 손잡이 끝 마구리
    if tier_index >= 3:                              # 자루 보강 밴드
        c.set(*at(-handle + 1.0, ang), ml)

    # ---- 경첩 ----
    c.blob(int(hx) - 1, int(hy) - 1, mm)
    c.set(hx, hy, ml)

    # ---- 집게발 두 짝: 벌어졌다가 끝이 안으로 말린다 ----
    for side in (1, -1):
        mid = at(hinge + reach * .62, ang + side * spread)
        tip = at(hinge + reach, ang + side * spread * curl)
        c.line(hx, hy, mid[0], mid[1], mm, thick=2)
        c.line(mid[0], mid[1], tip[0], tip[1], mm, thick=2)
        c.line(hx, hy, mid[0], mid[1], ml)

        if shape == "blunt":
            c.set(tip[0], tip[1], md)
        elif shape == "angled":
            c.set(mid[0], mid[1], ml); c.set(tip[0], tip[1], ml)
        elif shape == "sharp":
            nose = at(hinge + reach + .8, ang + side * spread * curl * .5)
            c.line(tip[0], tip[1], nose[0], nose[1], ml)
        elif shape == "saw":                         # 합금 — 발이 두껍고 안쪽에 물림턱
            nose = at(hinge + reach + .7, ang + side * spread * curl * .5)
            c.line(tip[0], tip[1], nose[0], nose[1], ml)
            c.set(*at(hinge + reach * .72, ang + side * spread * .45), ml)
        else:                                        # 최종형 — 발 안쪽이 빛난다
            nose = at(hinge + reach + .8, ang + side * spread * curl * .4)
            c.line(tip[0], tip[1], nose[0], nose[1], ml)
            c.set(*at(hinge + reach * .70, ang + side * spread * .40), glow + (255,))

    # ---- 단계 장식 ----
    if glow:
        g = glow + (255,)
        c.set(*at(hinge - 1.0, ang), g)
        if shape == "trident":
            c.set(hx, hy, g)
            c.set(*at(-handle + 1.0, ang), g)

    c.outline()
    return c.image()


# 쓰는 각도만 미리 그린다. 45도 배수라 픽셀 대각선이 깨끗하게 떨어진다.
ANGLES = [-90, -45, 0, 45, 90]


def build():
    cells = len(TIERS) * len(ANGLES) * 2
    sheet = Image.new("RGBA", (W * cells, H), (0, 0, 0, 0))
    for t in range(len(TIERS)):
        for a, ang in enumerate(ANGLES):
            for j, opened in enumerate((False, True)):
                index = (t * len(ANGLES) + a) * 2 + j
                sheet.paste(draw(t, opened, ang), (index * W, 0))
    return sheet


if __name__ == "__main__":
    os.makedirs("out", exist_ok=True)
    sh = build(); sh.save("out/tongs.png")
    print("ok", sh.size,
          f"· 각도 {ANGLES} · index = ((단계-1)*{len(ANGLES)} + 각도) * 2 + (벌림?1:0)")
