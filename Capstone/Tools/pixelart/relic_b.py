"""B. 한국 요괴 8종 — 무섭기보다 귀엽게. 눈은 크고 둥글게, 이빨은 되도록 감춘다."""
from relic_lib import *

GH1, GH2, GH3 = C(240, 242, 248), C(198, 204, 218), C(148, 156, 178)   # 귀신 흰빛
WA1, WA2, WA3 = C(150, 204, 226), C(96, 156, 190), C(58, 106, 144)      # 물빛
RB1, RB2, RB3 = C(214, 124, 96), C(166, 84, 62), C(116, 54, 40)         # 도깨비 살빛
BK1, BK2, BK3 = C(86, 82, 98), C(56, 54, 68), C(34, 32, 44)             # 검은 도포
HB1, HB2 = C(168, 196, 214), C(118, 150, 176)                           # 옥색 저고리


def eyes(b, x, y, gap=5, c=DARK, white=C(248, 248, 252)):
    """큰 흰자 + 둥근 동공. 이 게임 요괴는 전부 이 눈을 쓴다."""
    for dx in (0, gap):
        b.ellipse(x + dx, y, x + dx + 3, y + 3, white)
        b.ellipse(x + dx + 1, y + 1, x + dx + 2, y + 2, c)
        b.px(x + dx + 1, y + 1, C(252, 252, 255))


def ghost_tail(b, cx, g, half, frame, c1, c2):
    """다리 대신 하늘거리는 아랫자락."""
    sway = wig(frame, 1)
    for i, y in enumerate(range(g - 10, g + 1)):
        w = half - i // 3
        off = sway if i % 4 < 2 else -sway
        b.rect(cx - w + off, y, cx + w + off, y, c1 if i < 6 else c2)
    for k in range(3):
        b.px(cx - half + 2 + k * half, g, c2)


# ============================================================ 17 달걀귀신
def b_egg():
    b = buried()
    matrix(b, 21)
    b.ellipse(9, 7, 23, 26, GH2)
    b.ellipse(10, 8, 19, 20, GH1)
    b.ellipse(13, 13, 19, 19, C(226, 228, 236))
    crack(b, [(11, 12), (15, 17), (13, 23)], GH3)
    crack(b, [(20, 10), (18, 15)], GH3)
    grime(b, 21, 7)
    return fin(b)


def a_egg(f):
    b = alive(); g = b.ground
    t = bob(f, 0, -1)
    b.ellipse(12, g - 34 + t, 36, g - 6 + t, GH2)
    b.ellipse(15, g - 32 + t, 30, g - 14 + t, GH1)
    ghost_tail(b, 24, g, 11, f, GH2, GH3)
    b.ellipse(14, g - 22 + t, 19, g - 15 + t, GH2)      # 작은 팔
    b.ellipse(29, g - 22 + t, 34, g - 15 + t, GH2)
    # 얼굴이 없다 — 대신 아주 옅은 홍조만
    b.ellipse(17, g - 22 + t, 20, g - 20 + t, C(240, 214, 214))
    b.ellipse(28, g - 22 + t, 31, g - 20 + t, C(240, 214, 214))
    return fin(b, [(24, g - 22, 18, GH1)])


# ============================================================ 18 물귀신
def b_getabone():
    b = buried()
    matrix(b, 22)
    b.poly([(7, 20), (25, 17), (26, 22), (8, 25)], C(122, 96, 66))
    b.poly([(8, 20), (24, 17), (24, 19), (9, 22)], C(158, 128, 92))
    b.rect(11, 24, 13, 28, C(96, 72, 48))
    b.rect(20, 23, 22, 27, C(96, 72, 48))
    b.line(10, 19, 22, 17, C(78, 58, 40))
    for (x, y) in ((14, 26), (19, 25), (24, 21)):
        b.px(x, y, WA2)
    grime(b, 22, 8)
    return fin(b)


def a_getabone(f):
    b = alive(); g = b.ground
    t = bob(f, 0, -1)
    b.ellipse(12, g - 32 + t, 36, g - 8 + t, WA2)
    b.ellipse(15, g - 30 + t, 29, g - 16 + t, WA1)
    ghost_tail(b, 24, g, 11, f, WA2, WA3)
    eyes(b, 19, g - 26 + t, 6)
    b.ellipse(21, g - 19 + t, 27, g - 17 + t, WA3)       # 벌린 입
    for (x, y) in ((14, g - 30), (33, g - 28), (11, g - 20), (37, g - 22)):
        b.ellipse(x, y + t, x + 2, y + 3, WA1)           # 흘러내리는 물방울
    b.rect(16, g - 2, 32, g, WA3)                        # 젖은 발자국
    return fin(b, [(24, g - 20, 18, WA1)])


# ============================================================ 19 도깨비
def b_club():
    b = buried()
    matrix(b, 23)
    b.poly([(8, 25), (13, 26), (24, 9), (20, 7)], C(142, 104, 66))
    b.poly([(9, 24), (11, 25), (22, 9), (21, 8)], C(180, 140, 96))
    for (x, y) in ((11, 22), (14, 18), (17, 14), (20, 11)):
        b.rect(x, y, x + 2, y + 2, C(104, 74, 46))
    b.poly([(21, 7), (26, 5), (28, 9), (24, 11)], C(160, 120, 80))
    crack(b, [(13, 21), (16, 19), (15, 15)], C(78, 56, 36))
    grime(b, 23, 8)
    return fin(b)


def a_dokkaebi(f):
    b = alive(); g = b.ground
    t = bob(f, 0, -1)
    # 몸통 — 호랑이 무늬 옷
    b.ellipse(12, g - 26 + t, 34, g - 4 + t, C(206, 168, 96))
    b.ellipse(14, g - 24 + t, 27, g - 12 + t, C(232, 200, 130))
    for x in range(16, 32, 5):
        b.rect(x, g - 24 + t, x + 1, g - 8 + t, C(150, 112, 58))
    # 머리
    b.ellipse(13, g - 44 + t, 34, g - 24 + t, RB2)
    b.ellipse(15, g - 42 + t, 28, g - 31 + t, RB1)
    b.poly([(21, g - 44 + t), (23, g - 54 + t), (27, g - 44 + t)], C(238, 232, 214))  # 뿔 하나
    eyes(b, 18, g - 39 + t, 7)
    b.ellipse(21, g - 32 + t, 27, g - 30 + t, RB3)
    b.px(22, g - 31 + t, C(246, 242, 232))               # 살짝 보이는 덧니
    b.ellipse(15, g - 34 + t, 18, g - 32 + t, C(226, 146, 128))
    b.ellipse(29, g - 34 + t, 32, g - 32 + t, C(226, 146, 128))
    # 방망이
    sw = wig(f, -2)
    b.poly([(33, g - 14 + t), (37, g - 14 + t), (44, g - 34 + sw + t), (39, g - 36 + sw + t)],
           C(150, 110, 70))
    for k in range(4):
        b.rect(37 + k, g - 20 - k * 4 + sw + t, 39 + k, g - 18 - k * 4 + sw + t, C(110, 78, 48))
    legs2(b, 23, g, 7, f, C(232, 200, 130), C(180, 138, 78), 5, 6 + t)
    return fin(b)


# ============================================================ 20 처녀귀신
def b_sobok():
    b = buried()
    matrix(b, 24)
    b.poly([(8, 9), (24, 7), (26, 24), (10, 27)], C(206, 204, 196))
    b.poly([(10, 11), (20, 10), (21, 21), (12, 23)], C(234, 232, 226))
    for y in range(12, 25, 4):
        b.line(9, y, 25, y + 1, C(166, 164, 158))
    b.poly([(21, 8), (27, 12), (25, 16), (22, 12)], C(180, 178, 172))
    crack(b, [(13, 12), (17, 18), (14, 24)], C(140, 138, 134))
    grime(b, 24, 8)
    return fin(b)


def a_sobok(f):
    b = alive(); g = b.ground
    t = bob(f, 0, -1)
    # 흰 소복
    b.poly([(16, g - 30 + t), (32, g - 30 + t), (37, g - 4 + t), (11, g - 4 + t)], GH2)
    b.poly([(18, g - 29 + t), (26, g - 29 + t), (26, g - 6 + t), (15, g - 6 + t)], GH1)
    ghost_tail(b, 24, g, 13, f, GH2, GH3)
    b.rect(14, g - 22 + t, 34, g - 20 + t, GH3)          # 고름
    # 긴 검은 머리
    b.ellipse(14, g - 44 + t, 34, g - 24 + t, C(46, 42, 52))
    b.poly([(14, g - 34 + t), (11, g - 12 + t), (16, g - 12 + t), (18, g - 30 + t)], C(46, 42, 52))
    b.poly([(34, g - 34 + t), (37, g - 12 + t), (32, g - 12 + t), (30, g - 30 + t)], C(46, 42, 52))
    # 얼굴
    b.ellipse(18, g - 40 + t, 30, g - 28 + t, C(244, 236, 230))
    eyes(b, 19, g - 37 + t, 6)
    b.ellipse(22, g - 31 + t, 26, g - 30 + t, C(206, 128, 132))
    b.ellipse(18, g - 33 + t, 20, g - 31 + t, C(242, 190, 190))
    b.ellipse(28, g - 33 + t, 30, g - 31 + t, C(242, 190, 190))
    return fin(b, [(24, g - 24, 20, GH1)])


# ============================================================ 21 해태
def b_haetae():
    b = buried()
    matrix(b, 25)
    # 깨진 돌사자 머리 — 갈기를 뾰족한 톱니로 둘러 사자임을 알린다
    for k in range(10):
        ang = k * 36
        import math
        x = 16 + int(9 * math.cos(math.radians(ang)))
        y = 17 + int(9 * math.sin(math.radians(ang)))
        b.ellipse(x - 3, y - 3, x + 3, y + 3, STONE2)
    b.ellipse(7, 8, 25, 26, STONE2)
    b.ellipse(9, 10, 23, 24, STONE1)
    b.poly([(13, 10), (16, 3), (19, 10)], STONE1)          # 이마 뿔
    b.ellipse(11, 14, 14, 17, DARK)
    b.ellipse(18, 14, 21, 17, DARK)
    b.ellipse(13, 19, 19, 23, STONE3)
    b.rect(14, 20, 18, 21, C(196, 192, 182))
    crack(b, [(9, 16), (15, 12), (24, 18)], MTX3)
    b.rect(20, 22, 25, 26, MTX1)                            # 떨어져 나간 자리
    grime(b, 25, 8)
    return fin(b)


def a_haetae(f):
    b = alive(); g = b.ground
    s1, s2, s3 = C(190, 186, 174), C(146, 142, 132), C(102, 98, 90)
    t = bob(f, 0, 0)
    pedestal(b, 17, 5)
    y = g - 5
    # 앉은 몸 — 뒷다리는 접고 앞다리는 세운다
    b.ellipse(4, y - 20, 26, y, s2)
    b.ellipse(7, y - 18, 19, y - 6, s1)
    b.rect(20, y - 16, 25, y, s2)                           # 앞다리
    b.rect(20, y - 16, 22, y, s1)
    b.ellipse(19, y - 3, 27, y, s3)
    b.poly([(5, y - 14), (0, y - 24), (4, y - 24), (8, y - 12)], s2)   # 꼬리
    b.ellipse(0, y - 27, 6, y - 21, s1)
    # 갈기 — 머리 둘레를 뾰족한 톱니로 두른다
    for k in range(11):
        ang = -100 + k * 26
        import math
        x = 33 + int(13 * math.cos(math.radians(ang)))
        yy = y - 22 + int(12 * math.sin(math.radians(ang)))
        b.poly([(x - 3, yy + 2), (x + int(5 * math.cos(math.radians(ang))),
                yy + int(5 * math.sin(math.radians(ang)))), (x + 3, yy - 2)], s3)
    b.ellipse(21, y - 34, 45, y - 10, s2)
    b.ellipse(24, y - 32, 38, y - 18, s1)
    # 얼굴
    b.ellipse(26, y - 30, 44, y - 13, s1)
    b.ellipse(28, y - 28, 39, y - 19, C(214, 210, 198))
    b.poly([(31, y - 30), (33, y - 42), (36, y - 30)], C(236, 232, 220))   # 뿔
    eyes(b, 30, y - 27, 7)
    b.ellipse(33, y - 20, 40, y - 16, s3)
    b.ellipse(35, y - 19, 38, y - 17, C(230, 150, 150))
    b.px(34, y - 21, C(248, 246, 238))
    b.px(39, y - 21, C(248, 246, 238))
    return fin(b)


# ============================================================ 22 저승사자
def b_gat():
    b = buried()
    matrix(b, 26)
    b.ellipse(4, 14, 28, 22, BK2)
    b.ellipse(6, 15, 26, 20, BK1)
    b.ellipse(11, 6, 21, 17, BK2)
    b.ellipse(12, 7, 19, 15, BK1)
    b.line(6, 18, 26, 18, BK3)
    crack(b, [(9, 16), (16, 20), (24, 17)], BK3)
    for (x, y) in ((7, 19), (25, 16)):
        b.rect(x, y, x + 2, y + 1, MTX1)                 # 떨어져 나간 자리
    grime(b, 26, 7)
    return fin(b)


def a_jeoseung(f):
    b = alive(); g = b.ground
    t = bob(f, 0, -1)
    # 검은 도포
    b.poly([(16, g - 30 + t), (32, g - 30 + t), (38, g - 2 + t), (10, g - 2 + t)], BK2)
    b.poly([(18, g - 29 + t), (26, g - 29 + t), (27, g - 4 + t), (14, g - 4 + t)], BK1)
    b.rect(12, g - 4 + t, 36, g - 2 + t, BK3)
    b.rect(14, g - 22 + t, 34, g - 20 + t, BK3)
    b.rect(20, g - 30 + t, 28, g - 22 + t, C(228, 226, 220))   # 흰 깃
    # 얼굴
    b.ellipse(17, g - 42 + t, 31, g - 29 + t, C(232, 224, 216))
    eyes(b, 19, g - 39 + t, 6)
    b.ellipse(22, g - 32 + t, 26, g - 31 + t, C(176, 140, 140))
    # 갓 — 넓은 챙
    b.ellipse(19, g - 50 + t, 29, g - 42 + t, BK2)
    b.ellipse(20, g - 49 + t, 25, g - 44 + t, BK1)
    b.rect(8, g - 44 + t, 40, g - 42 + t, BK2)
    b.rect(8, g - 44 + t, 24, g - 43 + t, BK1)
    b.rect(8, g - 42 + t, 40, g - 41 + t, BK3)
    legs2(b, 23, g, 6, f, BK1, BK2, 4, 3 + t)
    return fin(b)


# ============================================================ 23 구미호
def b_fox():
    b = buried()
    matrix(b, 27)
    # 여우 두개골 — 길쭉한 주둥이가 특징
    b.ellipse(5, 9, 18, 21, BONE2)
    b.ellipse(6, 10, 15, 18, BONE1)
    b.poly([(17, 13), (29, 15), (29, 19), (17, 19)], BONE2)
    b.poly([(17, 14), (27, 16), (27, 18), (17, 18)], BONE1)
    b.ellipse(8, 12, 11, 15, DARK)
    b.px(28, 17, DARK)
    b.poly([(6, 9), (5, 3), (11, 8)], BONE1)               # 귀 뼈
    b.poly([(13, 8), (18, 3), (17, 10)], BONE1)
    for x in range(19, 28, 3):
        b.rect(x, 19, x + 1, 21, BONE1)                     # 이빨
    for k in range(4):                                      # 꼬리뼈 마디
        b.ellipse(6 + k * 5, 23, 12 + k * 5, 28, BONE2)
        b.ellipse(7 + k * 5, 24, 10 + k * 5, 26, BONE1)
    crack(b, [(6, 22), (16, 24), (26, 22)], MTX3)
    grime(b, 27, 8)
    return fin(b)


def a_fox(f):
    b = alive(); g = b.ground
    w1, w2, w3 = C(248, 246, 242), C(212, 208, 202), C(160, 156, 152)
    tip = C(238, 176, 184)
    t = bob(f, 0, -1)
    # 아홉 꼬리 — 하나씩 길쭉한 타원으로, 각도를 벌려 개수가 세어지게
    import math
    for i in range(9):
        ang = 192 + i * 8 + wig(f, 4)
        cx, cy = 20, g - 18 + t
        L = 17 + (i % 3) * 2
        ex = cx + int(L * math.cos(math.radians(ang)))
        ey = cy + int(L * math.sin(math.radians(ang)))
        for k in range(9):
            x = cx + (ex - cx) * k // 8
            yy = cy + (ey - cy) * k // 8
            r = 3 if k < 6 else 2
            b.ellipse(x - r, yy - r, x + r, yy + r, w2 if i % 2 else w1)
        b.ellipse(ex - 2, ey - 2, ex + 2, ey + 2, tip)
    # 몸통
    b.ellipse(14, g - 25 + t, 34, g - 7 + t, w2)
    b.ellipse(16, g - 23 + t, 28, g - 13 + t, w1)
    # 머리
    b.ellipse(28, g - 32 + t, 44, g - 16 + t, w2)
    b.ellipse(30, g - 30 + t, 40, g - 21 + t, w1)
    b.poly([(30, g - 31 + t), (29, g - 41 + t), (35, g - 32 + t)], w2)   # 귀
    b.poly([(37, g - 32 + t), (43, g - 41 + t), (43, g - 30 + t)], w2)
    b.poly([(31, g - 32 + t), (31, g - 38 + t), (34, g - 32 + t)], tip)
    b.poly([(39, g - 32 + t), (42, g - 38 + t), (42, g - 32 + t)], tip)
    eyes(b, 32, g - 28 + t, 6)
    b.poly([(41, g - 24 + t), (48, g - 22 + t), (48, g - 19 + t), (41, g - 18 + t)], w1)
    b.ellipse(46, g - 23 + t, 48, g - 21 + t, DARK)
    b.ellipse(31, g - 22 + t, 34, g - 20 + t, tip)
    b.ellipse(38, g - 22 + t, 41, g - 20 + t, tip)
    legs2(b, 23, g, 6, f, w1, w2, 5, 10 + t)
    return fin(b, [(24, g - 20, 24, C(252, 246, 250))])


# ============================================================ 24 이무기 → 용
def b_spine():
    b = buried()
    matrix(b, 28)
    for k in range(5):
        x = 2 + k * 5
        y = 20 - abs(k - 2) * 3
        b.ellipse(x, y, x + 6, y + 8, BONE2)
        b.ellipse(x + 1, y + 1, x + 4, y + 5, BONE1)
        b.rect(x + 2, y - 3, x + 4, y + 1, BONE1)        # 등 가시
    b.line(3, 24, 24, 18, BONE3)
    crack(b, [(6, 10), (16, 14), (27, 11)], MTX3)
    grime(b, 28, 9)
    return fin(b)


def a_dragon(f):
    b = alive(); g = b.ground
    d1, d2, d3 = C(150, 214, 178), C(98, 166, 136), C(58, 114, 96)
    gold = C(240, 216, 138)
    t = bob(f, 0, -2)
    y0 = g - 30 + t
    # 부드러운 S 자 몸통 — 굵은 구슬을 이어 붙인다
    import math
    body = []
    for k in range(34):
        x = 3 + k
        yy = y0 + 10 + int(7 * math.sin(math.radians(k * 11 + f * 18)))
        body.append((x, yy))
    for i, (x, yy) in enumerate(body):
        r = 5 if 4 < i < 28 else 4
        b.ellipse(x - r, yy - r, x + r, yy + r, d2)
        b.ellipse(x - r + 1, yy - r + 1, x + 1, yy, d1)
    for i in range(2, 30, 4):                                  # 등지느러미
        x, yy = body[i]
        b.poly([(x - 2, yy - 4), (x, yy - 9), (x + 2, yy - 4)], d3)
    # 머리
    hx, hy = body[-1]
    b.ellipse(hx - 5, hy - 8, hx + 12, hy + 6, d2)
    b.ellipse(hx - 3, hy - 6, hx + 7, hy + 1, d1)
    b.poly([(hx - 2, hy - 7), (hx - 5, hy - 17), (hx + 2, hy - 8)], gold)     # 뿔
    b.poly([(hx + 4, hy - 8), (hx + 8, hy - 17), (hx + 8, hy - 7)], gold)
    eyes(b, hx - 1, hy - 5, 6)
    b.poly([(hx + 9, hy + 1), (hx + 13, hy - 1), (hx + 13, hy + 4), (hx + 8, hy + 4)], d1)
    b.px(hx + 12, hy + 1, DARK)
    b.line(hx + 8, hy + 5, hx + 13, hy + 9, gold)                              # 수염
    b.line(hx + 7, hy + 6, hx + 11, hy + 11, gold)
    b.ellipse(hx + 2, hy + 8, hx + 8, hy + 14, gold)                           # 여의주
    b.ellipse(hx + 3, hy + 9, hx + 5, hy + 11, C(255, 246, 206))
    b.ellipse(16, g - 3, 36, g, (40, 38, 44, 90))
    return fin(b, [(24, y0 + 10, 28, C(196, 250, 222))], shade=False)


# ============================================================
ENTRIES = [
    ("egg", "달걀귀신", b_egg, a_egg),
    ("water", "물귀신", b_getabone, a_getabone),
    ("dokkaebi", "도깨비", b_club, a_dokkaebi),
    ("sobok", "처녀귀신", b_sobok, a_sobok),
    ("haetae", "해태", b_haetae, a_haetae),
    ("reaper", "저승사자", b_gat, a_jeoseung),
    ("gumiho", "구미호", b_fox, a_fox),
    ("imugi", "이무기(용)", b_spine, a_dragon),
]

if __name__ == "__main__":
    import os
    os.makedirs("out/Relic", exist_ok=True)
    bs, al = build_sheets(ENTRIES, "b")
    bs.save("out/Relic/relic_buried_b.png")
    al.save("out/Relic/relic_alive_b.png")
    print("ok", bs.size, al.size)
