"""C. 세계 괴수 · 미지생물 6종 — 인어 / 유니콘 / 예티 / 네시 / 크라켄 / 불사조."""
import math
from relic_lib import *

SEA1, SEA2, SEA3 = C(120, 208, 214), C(70, 156, 176), C(40, 100, 128)     # 비늘 청록
SKIN1, SKIN2 = C(246, 214, 186), C(206, 164, 134)                          # 인어 살색
HAIR1, HAIR2 = C(232, 152, 120), C(178, 100, 78)                           # 붉은 머리
SNOW1, SNOW2, SNOW3 = C(246, 248, 252), C(204, 214, 228), C(152, 168, 190) # 예티·유니콘
PURP1, PURP2 = C(196, 170, 232), C(140, 116, 186)                          # 유니콘 갈기
INK1, INK2, INK3 = C(186, 108, 152), C(134, 66, 108), C(88, 38, 70)        # 크라켄
GLASS = (176, 228, 240, 110)
WATER = (74, 160, 190, 130)


def tank(b, half=20, h=30, feet=True):
    """전시용 수조 — 유리벽 + 물 + 받침. 안쪽 그림은 이 다음에 그린다."""
    cx, g = b.w // 2 - getattr(b, "OX", 0), b.ground
    top = g - h
    b.rect(cx - half, top, cx + half, g - 2, WATER)
    b.rect(cx - half, top, cx + half, top + 1, C(150, 214, 236))
    if feet:
        b.rect(cx - half - 2, g - 4, cx + half + 2, g, STONE2)
        b.rect(cx - half - 2, g - 4, cx + half + 2, g - 3, STONE1)
    return top


def tank_glass(b, half=20, h=30):
    """수조 앞유리 하이라이트 — 내용물을 다 그린 뒤 마지막에 덮는다."""
    cx, g = b.w // 2 - getattr(b, "OX", 0), b.ground
    top = g - h
    for x in (cx - half, cx + half):
        b.rect(x, top, x, g - 3, C(198, 236, 246))
    b.rect(cx - half, top, cx + half, top, C(214, 244, 250))
    b.line(cx - half + 3, top + 4, cx - half + 7, top + 14, C(228, 248, 252))


def bubbles(b, cx, top, g, frame):
    for k in range(4):
        y = g - 6 - ((k * 7 + frame * 4) % (g - top - 6))
        b.px(cx - 9 + k * 6, y, C(226, 250, 254))
        b.px(cx - 9 + k * 6, y - 1, C(196, 236, 246))


# ============================================================ 25 인어
def b_mermaid():
    b = buried()
    matrix(b, 31)
    # 위는 사람 갈비뼈, 아래는 비늘 붙은 꼬리 — 한눈에 반인반어
    b.rect(15, 4, 17, 14, BONE1)
    for k in range(4):
        y = 6 + k * 2
        b.line(10, y + 1, 15, y, BONE2)
        b.line(17, y, 22, y + 1, BONE2)
        b.line(10, y + 1, 15, y, BONE1) if k % 2 == 0 else None
    b.ellipse(13, 15, 19, 20, BONE2)
    for k in range(5):                                        # 꼬리 척추 + 비늘
        y = 19 + k * 2
        b.rect(15 - k // 2, y, 18 - k // 2, y + 1, BONE1)
        b.px(13 - k // 2, y, SEA3)
        b.px(20 - k // 2, y, SEA3)
    b.poly([(12, 28), (8, 24), (10, 29)], SEA3)               # 꼬리 지느러미
    b.poly([(16, 28), (20, 24), (18, 29)], SEA3)
    crack(b, [(9, 8), (14, 12), (23, 9)], MTX3)
    grime(b, 31, 8)
    return fin(b)


def a_mermaid(f):
    b = alive(); g = b.ground
    top = tank(b, 19, 34)
    cx = 24
    t = bob(f, 0, -1)
    y = g - 14 + t
    # 꼬리 — 프레임마다 좌우로 살랑
    sw = wig(f, 2)
    for k in range(9):
        w = 5 - k // 3
        b.rect(cx - w + sw * (k // 4), y + k, cx + w + sw * (k // 4), y + k, SEA2 if k % 2 else SEA1)
    b.poly([(cx - 9 + sw, y + 9), (cx - 2 + sw, y + 5), (cx - 1 + sw, y + 11)], SEA2)
    b.poly([(cx + 2 + sw, y + 5), (cx + 10 + sw, y + 9), (cx + 2 + sw, y + 11)], SEA1)
    # 상체
    b.ellipse(cx - 5, y - 11, cx + 5, y + 1, SKIN1)
    b.ellipse(cx - 3, y - 10, cx + 2, y - 3, C(252, 230, 208))
    b.rect(cx - 6, y - 6, cx + 6, y - 4, SEA3)                 # 조개 상의
    b.rect(cx - 8, y - 9, cx - 5, y - 2, SKIN2)                # 팔
    b.rect(cx + 5, y - 10, cx + 8, y - 3, SKIN2)
    # 머리 + 긴 붉은 머리칼
    b.ellipse(cx - 12, y - 24, cx + 6, y - 5, HAIR2)
    b.ellipse(cx - 5, y - 22, cx + 5, y - 11, SKIN1)
    b.ellipse(cx - 4, y - 21, cx + 2, y - 15, C(252, 232, 212))
    b.rect(cx - 6, y - 23, cx + 5, y - 21, HAIR1)
    b.ellipse(cx - 12, y - 20, cx - 6, y - 4, HAIR1)
    eyes(b, cx - 3, y - 19, 5)
    b.ellipse(cx - 2, y - 13, cx + 1, y - 12, C(212, 118, 120))
    bubbles(b, cx, top, g, f)
    tank_glass(b, 19, 34)
    return fin(b, [(cx, y - 6, 22, C(160, 228, 244))])


# ============================================================ 26 유니콘
def b_horn():
    b = buried()
    matrix(b, 32)
    # 나선으로 꼬인 뿔 — 비틀린 홈이 보이게
    for k in range(20):
        y = 27 - k
        half = 5 - k // 5
        b.rect(16 - half, y, 16 + half, y, BONE2 if k % 4 < 2 else BONE1)
        if k % 3 == 0:
            b.px(16 - half + 1, y, BONE3)
            b.px(16 + half - 1, y, C(250, 246, 232))
    b.ellipse(10, 25, 22, 30, BONE3)                           # 뿌리 부분
    b.ellipse(11, 26, 20, 28, BONE2)
    crack(b, [(13, 20), (17, 16), (15, 11)], MTX3)
    grime(b, 32, 7)
    return fin(b)


def a_unicorn(f):
    b = alive(); g = b.ground
    w1, w2, w3 = SNOW1, SNOW2, SNOW3
    t = bob(f, 0, -1)
    legs2(b, 24, g, 9, f, w1, w2, 4, 11)
    b.ellipse(12, g - 26 + t, 38, g - 9 + t, w2)               # 몸통
    b.ellipse(15, g - 24 + t, 31, g - 15 + t, w1)
    b.poly([(12, g - 22 + t), (2, g - 30 + t), (5, g - 12 + t), (14, g - 14 + t)], PURP1)  # 꼬리
    b.poly([(12, g - 20 + t), (4, g - 26 + t), (7, g - 14 + t)], PURP2)
    # 목 + 머리
    b.poly([(30, g - 24 + t), (34, g - 40 + t), (42, g - 38 + t), (38, g - 16 + t)], w2)
    b.poly([(32, g - 24 + t), (35, g - 38 + t), (39, g - 37 + t), (37, g - 18 + t)], w1)
    b.ellipse(34, g - 44 + t, 47, g - 32 + t, w2)
    b.ellipse(35, g - 43 + t, 44, g - 36 + t, w1)
    b.poly([(43, g - 39 + t), (49, g - 37 + t), (48, g - 32 + t), (42, g - 33 + t)], w1)   # 주둥이
    b.px(48, g - 35 + t, w3)
    b.poly([(35, g - 43 + t), (34, g - 50 + t), (39, g - 42 + t)], w1)                     # 귀
    # 뿔 — 나선 금빛
    for k in range(9):
        b.rect(40 - k // 4, g - 45 - k + t, 42 - k // 4, g - 45 - k + t,
               GOLD1 if k % 2 == 0 else GOLD2)
    b.px(39, g - 54 + t, GOLD1)
    # 갈기
    for k in range(7):
        x = 31 + k
        b.rect(x, g - 44 + t + k, x + 2, g - 36 + t + k, PURP1 if k % 2 else PURP2)
    eyes(b, 37, g - 41 + t, 0)
    return fin(b, [(40, g - 48, 9, C(226, 208, 255))])


# ============================================================ 27 예티
def b_print():
    b = buried()
    matrix(b, 33)
    # 석고로 뜬 거대 발자국 — 발바닥 + 발가락 다섯
    b.ellipse(8, 11, 24, 28, C(228, 228, 232))
    b.ellipse(10, 13, 22, 26, C(198, 200, 208))
    b.ellipse(11, 16, 21, 25, C(166, 170, 182))
    for (x, y, r) in ((9, 8, 2), (13, 5, 2), (17, 5, 2), (21, 7, 2), (24, 12, 2)):
        b.ellipse(x - r, y - r, x + r, y + r, C(228, 228, 232))
        b.ellipse(x - r + 1, y - r + 1, x + r - 1, y + r - 1, C(176, 180, 192))
    crack(b, [(9, 20), (16, 24), (23, 19)], MTX3)
    grime(b, 33, 9)
    return fin(b)


def a_yeti(f):
    b = alive(); g = b.ground
    w1, w2, w3 = SNOW1, SNOW2, SNOW3
    t = bob(f, 0, -1)
    # 다리
    for (x, c) in ((swing(f, 14, 17), w2), (swing(f, 30, 27), w1)):
        b.rect(x - 4, g - 12, x + 4, g, c)
        b.rect(x - 5, g - 3, x + 5, g, w3)
    # 몸통 — 털이 삐죽삐죽
    b.ellipse(9, g - 36 + t, 39, g - 8 + t, w2)
    b.ellipse(13, g - 33 + t, 31, g - 15 + t, w1)
    for k in range(14):
        x = 9 + k * 2
        b.px(x, g - 37 + t - (k % 3), w1)
        b.px(x, g - 36 + t - (k % 3), w2)
    # 팔 — 어깨에서 이어져 아래로 처진다
    sp = wig(f, 2)
    for (sx, d) in ((12, -1), (36, 1)):
        for k in range(12):
            x = sx + d * (k * 5) // 6
            yy = g - 33 + t + k + (sp if d > 0 else -sp) * k // 8
            r = 4 - k // 6
            b.ellipse(x - r, yy - r, x + r, yy + r, w2 if k % 3 else w1)
        b.ellipse(sx + d * 10 - 4, g - 22 + t, sx + d * 10 + 4, g - 14 + t, w1)
    # 머리
    b.ellipse(13, g - 50 + t, 35, g - 32 + t, w2)
    b.ellipse(15, g - 48 + t, 31, g - 36 + t, w1)
    b.ellipse(18, g - 42 + t, 30, g - 33 + t, C(214, 200, 190))   # 얼굴
    eyes(b, 20, g - 43 + t, 6)
    b.ellipse(23, g - 38 + t, 26, g - 36 + t, C(120, 106, 104))
    b.rect(22, g - 35 + t, 27, g - 34 + t, C(150, 132, 128))
    return fin(b, [(24, g - 40, 20, C(214, 238, 255))])


# ============================================================ 28 네시
def b_neck():
    b = buried()
    matrix(b, 34)
    # 아주 긴 목뼈 — 마디가 S 자로 이어진다
    pts = []
    for k in range(10):
        ang = math.radians(198 + k * 13)
        pts.append((18 + int(12 * math.cos(ang)), 24 + int(15 * math.sin(ang))))
    for i in range(len(pts) - 1):                               # 마디를 잇는 뼈대
        b.line(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1], BONE3)
    for (x, y) in pts:
        b.ellipse(x - 3, y - 2, x + 3, y + 2, BONE2)
        b.ellipse(x - 2, y - 1, x + 1, y + 1, BONE1)
        b.px(x, y - 3, BONE3)
        b.px(x, y + 3, BONE3)
    b.ellipse(5, 4, 14, 11, BONE2)                              # 작은 두개골
    b.ellipse(6, 5, 12, 9, BONE1)
    b.px(8, 7, DARK)
    crack(b, [(16, 8), (22, 14), (27, 22)], MTX3)
    grime(b, 34, 9)
    return fin(b)


def a_nessie(f):
    b = alive(); g = b.ground
    d1, d2, d3 = C(122, 176, 150), C(80, 130, 112), C(48, 88, 78)
    top = tank(b, 19, 44)
    t = bob(f, 0, -1)
    wl = g - 26                                                  # 수면
    # 물 위로 드러난 등 세 덩이
    for (x, r) in ((12, 4), (20, 5), (28, 4)):
        b.ellipse(x - r, wl - r + t, x + r, wl + 2, d2)
        b.ellipse(x - r + 1, wl - r + 1 + t, x + 1, wl, d1)
    # 긴 목 — 수면에서 곧게 솟아 살짝 S 를 그린다
    hx = hy = 0
    for k in range(15):
        x = 33 + int(4 * math.sin(math.radians(k * 15 + f * 24)))
        y = wl - k + t
        r = 4 - k // 7
        b.ellipse(x - r, y - r, x + r, y + r, d2)
        b.ellipse(x - r + 1, y - r + 1, x, y, d1)
        hx, hy = x, y
    b.ellipse(hx - 5, hy - 7, hx + 7, hy + 2, d2)
    b.ellipse(hx - 4, hy - 6, hx + 3, hy - 2, d1)
    b.poly([(hx + 4, hy - 4), (hx + 9, hy - 2), (hx + 4, hy + 1)], d1)      # 주둥이
    b.px(hx + 8, hy - 2, d3)
    b.poly([(hx - 3, hy - 7), (hx - 1, hy - 12), (hx + 2, hy - 7)], d3)     # 작은 뿔
    eyes(b, hx - 2, hy - 6, 4)
    # 물결
    for x in range(7, 42, 3):
        b.px(x, wl + 2 + (0 if (x // 3 + f) % 2 else 1), C(168, 226, 240))
    bubbles(b, 24, top, g, f)
    tank_glass(b, 19, 44)
    return fin(b, [(24, wl, 24, C(150, 220, 240))])


# ============================================================ 29 크라켄
def b_sucker():
    b = buried()
    matrix(b, 35)
    # 접시만 한 빨판 화석 — 동심원 + 가장자리 톱니
    b.ellipse(5, 8, 27, 26, STONE2)
    b.ellipse(7, 10, 25, 24, STONE1)
    b.ellipse(10, 13, 22, 21, STONE3)
    b.ellipse(12, 15, 20, 19, STONE2)
    b.ellipse(14, 16, 18, 18, DARK)
    for k in range(12):
        ang = math.radians(k * 30)
        x = 16 + int(11 * math.cos(ang))
        y = 17 + int(9 * math.sin(ang))
        b.px(x, y, STONE3)
    b.rect(3, 22, 10, 26, MTX1)                                  # 떨어져 나간 자리
    crack(b, [(6, 12), (14, 10), (24, 14)], MTX3)
    grime(b, 35, 9)
    return fin(b)


def a_kraken(f):
    b = alive(); g = b.ground
    pedestal(b, 16, 5)
    y = g - 5
    t = bob(f, 0, -1)
    # 굵은 촉수 세 개가 좌대에서 솟아 휜다
    for (base, lean, c1, c2) in ((14, -1, INK2, INK3), (24, 1, INK1, INK2), (34, 1, INK2, INK3)):
        for k in range(26):
            r = 5 - k // 6
            x = base + int(lean * k * 0.55 + 3 * math.sin(math.radians(k * 9 + f * 30)))
            yy = y - 2 - k
            b.ellipse(x - r, yy - r + 3, x + r, yy + r - 3, c1 if k % 5 else c2)
            if k % 4 == 0 and r > 1:                              # 빨판
                b.px(x - r + 1, yy, C(246, 216, 224))
                b.px(x + r - 1, yy, C(246, 216, 224))
    # 가운데 눈 하나 — 크라켄임을 알린다
    b.ellipse(19, y - 16, 29, y - 7, INK3)
    b.ellipse(21, y - 15, 28, y - 9, C(250, 238, 212))
    b.ellipse(23, y - 14, 26, y - 10, DARK)
    b.px(24, y - 13, C(252, 252, 255))
    return fin(b, [(24, y - 20, 22, C(226, 168, 212))])


# ============================================================ 30 불사조
def b_feather():
    b = buried()
    matrix(b, 36)
    # 재 속에 남은 깃털 한 장 — 깃대 + 깃가지
    b.line(8, 27, 24, 6, C(216, 190, 150))
    b.line(9, 27, 25, 6, C(168, 138, 100))
    for k in range(12):
        x = 9 + (k * 14) // 12
        y = 26 - (k * 20) // 12
        L = 5 - abs(k - 6) // 3
        b.line(x, y, x - L, y - L + 1, C(228, 184, 132) if k % 2 else C(196, 150, 104))
        b.line(x + 1, y + 1, x + L + 1, y + L, C(212, 164, 116) if k % 2 else C(172, 128, 88))
    for (x, y) in ((6, 29), (11, 30), (20, 28), (26, 25), (4, 22)):   # 재
        b.px(x, y, C(96, 88, 88))
        b.px(x + 1, y, C(70, 64, 64))
    grime(b, 36, 7)
    return fin(b)


def a_phoenix(f):
    b = alive(); g = b.ground
    t = bob(f, 0, -3)
    y = g - 26 + t
    # 날개 — 프레임마다 펼침/접음
    spread = swing(f, 0, 6)
    for side in (-1, 1):
        # 깃털 한 장씩 — 뿌리는 어깨, 끝은 부채처럼 벌어진다
        for k in range(8):
            ang = math.radians(196 + k * 11) if side < 0 else math.radians(-16 - k * 11)
            L = 20 - abs(k - 3)
            sx, sy = 24 + side * 5, y - 4 - spread
            ex = sx + int(L * math.cos(ang))
            ey = sy + int(L * math.sin(ang)) + k
            c = FIRE1 if k % 3 == 0 else (FIRE2 if k % 3 == 1 else FIRE3)
            for j in range(L + 1):
                px_ = sx + (ex - sx) * j // L
                py_ = sy + (ey - sy) * j // L
                r = 3 - j // (L // 2 + 1)
                b.ellipse(px_ - r, py_ - r, px_ + r, py_ + r, c)
            b.ellipse(ex - 1, ey - 1, ex + 1, ey + 1, FIRE1)
    # 몸통
    b.ellipse(18, y - 8, 31, y + 10, FIRE2)
    b.ellipse(20, y - 6, 28, y + 4, FIRE1)
    # 꼬리 깃 세 갈래
    for (dx, L, c) in ((-4, 18, FIRE3), (0, 22, FIRE2), (4, 17, FIRE3)):
        for k in range(L):
            b.px(24 + dx + int(3 * math.sin(math.radians(k * 12 + f * 32))), y + 8 + k, c)
            b.px(25 + dx + int(3 * math.sin(math.radians(k * 12 + f * 32))), y + 8 + k, FIRE1 if k % 4 == 0 else c)
    # 머리 + 볏
    b.ellipse(21, y - 19, 32, y - 7, FIRE2)
    b.ellipse(23, y - 18, 30, y - 11, FIRE1)
    b.poly([(31, y - 14), (37, y - 12), (31, y - 10)], C(250, 226, 150))    # 부리
    for (dx, h) in ((23, 6), (26, 8), (29, 5)):                              # 볏
        b.rect(dx, y - 19 - h, dx + 1, y - 18, FIRE1)
    eyes(b, 25, y - 17, 4)
    return fin(b, [(24, y - 4, 22, C(255, 216, 140)), (24, y - 16, 12, C(255, 240, 190))],
               shade=False)


# ============================================================
ENTRIES = [
    ("mermaid", "인어", b_mermaid, a_mermaid),
    ("unicorn", "유니콘", b_horn, a_unicorn),
    ("yeti", "예티", b_print, a_yeti),
    ("nessie", "네시", b_neck, a_nessie),
    ("kraken", "크라켄", b_sucker, a_kraken),
    ("phoenix", "불사조", b_feather, a_phoenix),
]

if __name__ == "__main__":
    import os
    os.makedirs("out/Relic", exist_ok=True)
    bs, al = build_sheets(ENTRIES, "c")
    bs.save("out/Relic/relic_buried_c.png")
    al.save("out/Relic/relic_alive_c.png")
    print("ok", bs.size, al.size)
