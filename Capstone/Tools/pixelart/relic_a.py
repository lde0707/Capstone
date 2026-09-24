"""A. 공룡 · 고생물 16종 — 발굴본 32x32 / 복원본 48x48 x 2프레임."""
from relic_lib import *


# ============================================================ 1 삼엽충
def b_trilobite():
    b = buried()
    matrix(b, 1)
    b.ellipse(11, 11, 21, 23, BONE2)
    b.ellipse(12, 11, 20, 17, BONE1)
    for y in range(17, 23, 2):
        b.line(12, y, 20, y, BONE3)
    b.px(14, 14, DARK); b.px(18, 14, DARK)
    crack(b, [(6, 18), (12, 20), (18, 16), (26, 19)], MTX3)
    grime(b, 1, 8)
    return fin(b)


def a_trilobite(f):
    b = alive(); g = b.ground
    pedestal(b, 10, 4)
    y = g - 6 - bob(f, 0, 1)
    b.ellipse(14, y - 16, 34, y, C(122, 110, 84))
    b.ellipse(16, y - 14, 32, y - 2, C(168, 152, 116))
    b.ellipse(17, y - 14, 31, y - 8, C(198, 184, 148))
    for yy in range(y - 7, y - 1, 2):
        b.line(17, yy, 31, yy, C(122, 110, 84))
    b.px(20, y - 11, DARK); b.px(28, y - 11, DARK)
    return fin(b)


# ============================================================ 2 암모나이트
def b_ammonite():
    b = buried()
    matrix(b, 2)
    cx, cy = 16, 17
    for i, r in enumerate((11, 8, 5, 3, 1)):
        c = BONE1 if i % 2 == 0 else BONE2
        b.ellipse(cx - r, cy - r, cx + r, cy + r, c)
    for k in range(0, 12):
        b.line(cx, cy, cx + int(11 * (1 if k % 2 else 0.8)) - k, cy - 10 + k, BONE3)
    crack(b, [(4, 14), (11, 17), (20, 12), (28, 16)], MTX3)
    grime(b, 2, 8)
    return fin(b)


def a_ammonite(f):
    b = alive(); g = b.ground
    pedestal(b, 11, 5)
    cy = g - 22 - bob(f, 0, 1)
    for i, r in enumerate((15, 11, 8, 5, 3, 1)):
        c = [C(214, 186, 128), C(168, 136, 86)][i % 2]
        b.ellipse(24 - r, cy - r, 24 + r, cy + r, c)
    for k in range(14):
        b.line(24, cy, 24 + 14 - k, cy - 14 + k, C(126, 98, 58))
    b.ellipse(30, cy + 6, 40, cy + 14, C(196, 164, 110))
    for x in range(32, 41, 3):
        b.line(x, cy + 12, x + 3, cy + 16, C(150, 122, 78))
    return fin(b, [(24, cy, 16, C(230, 200, 140))])


# ============================================================ 3 호박 속 곤충
def b_amber():
    b = buried()
    b.ellipse(7, 6, 25, 27, C(140, 96, 40))
    b.ellipse(9, 8, 23, 25, C(196, 142, 54))
    b.ellipse(11, 10, 19, 19, C(228, 180, 86))
    b.rect(14, 15, 18, 20, C(96, 66, 34))
    b.rect(15, 12, 17, 16, C(96, 66, 34))
    for (x, y) in ((13, 14), (19, 14), (12, 18), (20, 18)):
        b.line(16, 17, x, y, C(96, 66, 34))
    crack(b, [(9, 12), (14, 9), (20, 13)], C(120, 82, 34))
    grime(b, 3, 6)
    return fin(b)


def a_amber(f):
    b = alive(); g = b.ground
    pedestal(b, 9, 4)
    cy = g - 22
    b.ellipse(14, cy - 14, 34, cy + 12, C(206, 150, 58))
    b.ellipse(16, cy - 12, 32, cy + 10, C(244, 196, 96))
    b.ellipse(18, cy - 10, 27, cy - 2, C(255, 228, 152))
    b.rect(22, cy - 2, 26, cy + 6, C(90, 62, 32))
    b.rect(23, cy - 7, 25, cy - 2, C(90, 62, 32))
    for (x, y) in ((19, cy - 3), (29, cy - 3), (18, cy + 3), (30, cy + 3)):
        b.line(24, cy + 1, x, y, C(90, 62, 32))
    return fin(b, [(24, cy - 2, 18 + bob(f, 0, 2), C(255, 214, 120))])


# ============================================================ 4 벨로키랍토르
def b_raptor():
    b = buried()
    matrix(b, 3)
    # 낫 발톱
    pts = [(9, 24), (12, 18), (17, 13), (23, 10), (25, 12), (20, 16), (15, 21), (12, 26)]
    for i in range(len(pts) - 1):
        b.line(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1], BONE2)
    b.poly(pts, BONE1)
    b.line(11, 23, 22, 12, BONE3)
    b.ellipse(8, 23, 13, 27, BONE2)
    crack(b, [(5, 16), (12, 20), (22, 22), (27, 18)], MTX3)
    grime(b, 4, 8)
    return fin(b)


def a_raptor(f):
    b = alive(); g = b.ground
    body, mid, dk = SK
    t = bob(f, 0, -1)
    b.ellipse(14, g - 22 + t, 34, g - 10 + t, mid)
    b.ellipse(16, g - 21 + t, 28, g - 14 + t, body)
    # 목 + 머리
    b.poly([(30, g - 21 + t), (36, g - 30 + t), (40, g - 28 + t), (34, g - 16 + t)], mid)
    b.poly([(34, g - 33 + t), (46, g - 30 + t), (46, g - 26 + t), (34, g - 27 + t)], body)
    b.rect(38, g - 27 + t, 45, g - 26 + t, dk)
    b.px(37, g - 31 + t, DARK)
    # 꼬리
    b.poly([(16, g - 20 + t), (2, g - 26 + t), (2, g - 23 + t), (16, g - 14 + t)], mid)
    # 다리
    legs2(b, 24, g, 6, f, body, mid, 4, 12 + t)
    for x in (swing(f, 18, 21), swing(f, 30, 27)):
        b.rect(x - 2, g - 1, x + 4, g, dk)
        b.px(x + 4, g - 3, C(232, 228, 216))
    return fin(b)


# ============================================================ 5 검치호
def b_smilodon():
    b = buried()
    matrix(b, 4)
    b.ellipse(9, 9, 23, 20, BONE2)
    b.ellipse(10, 10, 21, 17, BONE1)
    b.px(13, 13, DARK); b.px(18, 13, DARK)
    b.rect(14, 16, 17, 19, BONE3)
    for x in (11, 20):                     # 송곳니
        b.rect(x, 19, x + 1, 27, BONE1)
        b.px(x, 27, BONE2); b.px(x + 1, 26, BONE2)
    crack(b, [(6, 14), (12, 11), (22, 15)], MTX3)
    grime(b, 5, 9)
    return fin(b)


def a_smilodon(f):
    b = alive(); g = b.ground
    body, mid, dk = C(206, 158, 96), C(160, 116, 66), C(110, 78, 44)
    t = bob(f, 0, -1)
    b.ellipse(10, g - 24 + t, 36, g - 8 + t, mid)
    b.ellipse(12, g - 22 + t, 28, g - 14 + t, body)
    for x in range(14, 34, 5):
        b.line(x, g - 22 + t, x + 2, g - 12 + t, dk)
    b.ellipse(32, g - 30 + t, 45, g - 18 + t, mid)     # 머리
    b.ellipse(34, g - 28 + t, 42, g - 22 + t, body)
    b.px(40, g - 26 + t, DARK); b.px(43, g - 25 + t, DARK)
    for x in (38, 42):                                 # 송곳니
        b.rect(x, g - 20 + t, x + 1, g - 14 + t, C(246, 242, 228))
    b.poly([(11, g - 22 + t), (2, g - 30 + t), (2, g - 27 + t), (12, g - 16 + t)], mid)
    legs2(b, 23, g, 10, f, body, mid, 5, 11 + t)
    return fin(b)


# ============================================================ 6 매머드
def b_mammoth():
    b = buried()
    matrix(b, 5)
    # 크게 휘어진 상아 하나 — 실루엣이 확실하다
    pts = [(25, 7), (20, 10), (13, 14), (8, 19), (6, 24), (9, 27), (13, 26)]
    for i in range(len(pts) - 1):
        for t in range(3):
            b.line(pts[i][0], pts[i][1] + t, pts[i + 1][0], pts[i + 1][1] + t,
                   BONE1 if t == 0 else BONE2)
    b.ellipse(22, 5, 28, 11, BONE1)
    b.ellipse(11, 24, 16, 28, BONE1)
    for (x0, y0, x1, y1) in ((24, 12), (18, 17), (12, 22)) and (
            ((24, 12, 25, 14), (18, 17, 19, 19), (12, 22, 13, 24))):
        b.line(x0, y0, x1, y1, BONE3)
    crack(b, [(5, 20), (14, 24), (27, 20)], MTX3)
    grime(b, 6, 9)
    return fin(b)


def a_mammoth(f):
    b = alive(); g = b.ground
    body, mid, dk = C(150, 104, 70), C(112, 76, 50), C(78, 52, 34)
    t = bob(f, 0, -1)
    b.ellipse(6, g - 34 + t, 38, g - 8 + t, mid)
    b.ellipse(9, g - 32 + t, 26, g - 16 + t, body)
    for x in range(8, 38, 4):
        b.line(x, g - 30 + t, x + 1, g - 10 + t, dk)
    b.ellipse(30, g - 32 + t, 46, g - 14 + t, mid)      # 머리
    b.ellipse(32, g - 30 + t, 42, g - 20 + t, body)
    b.px(41, g - 26 + t, DARK)
    b.rect(38, g - 16 + t, 41, g - 2 + t, mid)          # 코
    b.rect(38, g - 16 + t, 39, g - 2 + t, body)
    for s, x0 in ((1, 36), (-1, 44)):                   # 상아
        b.line(x0, g - 16 + t, x0 + s * 4, g - 8 + t, C(244, 238, 220))
        b.line(x0 + s * 4, g - 8 + t, x0 + s * 7, g - 12 + t, C(244, 238, 220))
    legs2(b, 20, g, 11, f, body, mid, 6, 12 + t)
    return fin(b)


# ============================================================ 7 스테고사우루스
def b_stego():
    b = buried()
    matrix(b, 6)
    b.poly([(16, 6), (26, 18), (16, 27), (6, 18)], BONE2)
    b.poly([(16, 8), (23, 18), (16, 24), (10, 18)], BONE1)
    for y in range(11, 24, 3):
        b.line(9, y, 23, y, BONE3)
    crack(b, [(7, 14), (16, 12), (25, 17)], MTX3)
    grime(b, 7, 8)
    return fin(b)


def a_stego(f):
    b = alive(); g = b.ground
    body, mid, dk = C(128, 156, 110), C(94, 118, 84), C(64, 82, 58)
    t = bob(f, 0, -1)
    b.ellipse(8, g - 24 + t, 38, g - 8 + t, mid)
    b.ellipse(10, g - 22 + t, 26, g - 14 + t, body)
    for (x, h) in ((14, 10), (20, 13), (26, 13), (32, 9)):   # 등판
        b.poly([(x - 4, g - 22 + t), (x + 4, g - 22 + t), (x, g - 22 - h + t)], C(198, 156, 92))
        b.poly([(x - 2, g - 22 + t), (x + 2, g - 22 + t), (x, g - 19 - h + t)], C(236, 202, 128))
    b.ellipse(34, g - 22 + t, 46, g - 14 + t, mid)           # 머리
    b.px(43, g - 19 + t, DARK)
    b.poly([(9, g - 20 + t), (0, g - 26 + t), (0, g - 22 + t), (10, g - 14 + t)], mid)
    for (x, y) in ((2, g - 25 + t), (4, g - 22 + t)):
        b.rect(x, y, x + 1, y + 2, C(236, 202, 128))
    legs2(b, 22, g, 10, f, body, mid, 5, 11 + t)
    return fin(b)


# ============================================================ 8 트리케라톱스
def b_trike():
    b = buried()
    matrix(b, 7)
    b.ellipse(6, 11, 26, 26, BONE3)        # 프릴 테두리
    b.ellipse(8, 13, 24, 24, BONE2)
    b.ellipse(10, 15, 22, 22, BONE1)
    for x in range(8, 25, 4):              # 프릴 돌기
        b.rect(x, 10, x + 2, 13, BONE1)
    b.poly([(12, 22), (20, 22), (18, 29), (14, 29)], BONE2)
    b.poly([(6, 14), (10, 3), (12, 4), (10, 16)], BONE1)           # 왼 뿔
    b.poly([(26, 14), (22, 3), (20, 4), (22, 16)], BONE1)          # 오른 뿔
    b.poly([(14, 26), (16, 20), (18, 26)], BONE1)                  # 코뿔
    b.px(13, 19, DARK); b.px(19, 19, DARK)
    crack(b, [(6, 18), (14, 15), (26, 20)], MTX3)
    grime(b, 8, 9)
    return fin(b)


def a_trike(f):
    b = alive(); g = b.ground
    body, mid, dk = C(164, 142, 106), C(122, 104, 76), C(86, 72, 52)
    frill, frill2 = C(196, 146, 108), C(138, 96, 70)
    t = bob(f, 0, -1)
    # 몸통 + 꼬리
    b.ellipse(2, g - 26 + t, 28, g - 8 + t, mid)
    b.ellipse(5, g - 24 + t, 19, g - 14 + t, body)
    b.poly([(4, g - 22 + t), (0, g - 26 + t), (0, g - 22 + t), (5, g - 14 + t)], mid)
    # 프릴 — 머리 뒤에 낮게 펼친다
    b.poly([(22, g - 12 + t), (22, g - 30 + t), (28, g - 37 + t), (37, g - 33 + t),
            (40, g - 20 + t), (36, g - 12 + t)], frill2)
    b.poly([(24, g - 14 + t), (24, g - 29 + t), (29, g - 34 + t), (35, g - 31 + t),
            (37, g - 21 + t), (33, g - 14 + t)], frill)
    for (x, y) in ((22, g - 28 + t), (26, g - 38 + t), (34, g - 35 + t), (39, g - 25 + t)):
        b.ellipse(x - 1, y, x + 2, y + 3, frill2)
        b.px(x, y + 1, C(230, 196, 160))
    # 머리 — 프릴 앞으로 크게 나오고 부리로 끝난다
    b.poly([(30, g - 12 + t), (30, g - 29 + t), (40, g - 29 + t), (46, g - 22 + t),
            (48, g - 16 + t), (44, g - 12 + t)], mid)
    b.poly([(32, g - 14 + t), (32, g - 27 + t), (39, g - 27 + t), (44, g - 21 + t),
            (44, g - 16 + t), (38, g - 14 + t)], body)
    b.poly([(44, g - 18 + t), (48, g - 16 + t), (48, g - 13 + t), (43, g - 13 + t)], dk)
    # 뿔 셋 — 얇게, 머리에서 곧게 솟는다
    b.poly([(34, g - 28 + t), (33, g - 41 + t), (35, g - 41 + t), (36, g - 27 + t)],
           C(246, 242, 228))
    b.poly([(41, g - 26 + t), (41, g - 39 + t), (43, g - 39 + t), (43, g - 25 + t)],
           C(246, 242, 228))
    b.poly([(45, g - 20 + t), (48, g - 26 + t), (48, g - 18 + t)], C(246, 242, 228))
    b.px(40, g - 21 + t, DARK)
    legs2(b, 16, g, 9, f, body, mid, 6, 11 + t)
    return fin(b)


# ============================================================ 9 프테라노돈
def b_ptera():
    b = buried()
    matrix(b, 8)
    bone_rod(b, 5, 20, 16, 12, 2)
    bone_rod(b, 16, 12, 28, 19, 2)
    b.poly([(12, 14), (22, 14), (24, 20), (10, 20)], BONE2)
    b.rect(14, 11, 19, 13, BONE1)
    b.poly([(19, 10), (27, 11), (19, 13)], BONE1)   # 볏
    crack(b, [(5, 17), (14, 22), (27, 16)], MTX3)
    grime(b, 9, 8)
    return fin(b)


def a_ptera(f):
    b = alive(); g = b.ground
    body, mid, dk = C(198, 178, 146), C(150, 130, 102), C(102, 88, 68)
    y = 20 + bob(f, 0, 3)
    span = swing(f, 22, 18)
    tip = cyc(f, -10, -4, 3, -4)                 # 날개 끝 높이
    b.poly([(24, y), (24 - span, y + tip), (24 - span + 4, y + tip + 12), (24, y + 4)], mid)
    b.poly([(24, y), (24 + span, y + tip), (24 + span - 4, y + tip + 12), (24, y + 4)], body)
    b.ellipse(19, y - 2, 29, y + 8, mid)
    b.poly([(28, y + 1), (44, y + 4), (28, y + 6)], body)     # 부리
    b.poly([(26, y - 1), (34, y - 8), (27, y + 1)], C(212, 150, 96))  # 볏
    b.px(30, y + 2, DARK)
    b.rect(22, y + 8, 23, y + 13, dk)
    b.rect(26, y + 8, 27, y + 13, dk)
    b.ellipse(16, b.ground - 2, 32, b.ground, (40, 38, 44, 90))
    return fin(b, shade=False)


# ============================================================ 10 시조새
def b_archaeo():
    b = buried()
    matrix(b, 9)
    bone_rod(b, 10, 22, 18, 12, 2)
    b.ellipse(16, 9, 23, 15, BONE2)
    b.poly([(21, 11), (28, 13), (21, 15)], BONE1)
    for (x0, y0, dx) in ((11, 20, -1), (13, 17, -1), (16, 14, 1), (19, 20, 1)):
        for k in range(6):
            b.px(x0 + dx * k, y0 + k // 2, BONE2 if k % 2 else BONE3)
    b.line(10, 23, 5, 27, BONE2)
    crack(b, [(6, 16), (16, 20), (26, 14)], MTX3)
    grime(b, 10, 8)
    return fin(b)


def a_archaeo(f):
    b = alive(); g = b.ground
    body, mid, dk = C(180, 126, 78), C(136, 92, 54), C(92, 62, 36)
    y = 20 + bob(f, 0, 3)
    lift0 = cyc(f, 9, 3, -3, 3)          # 날갯짓 한 바퀴 (위 → 수평 → 아래 → 수평)
    # 날개 — 양쪽이 같이 오르내린다. 몸통보다 먼저 깔아야 몸이 가려지지 않는다
    for s in (-1, 1):
        tipy = y - lift0
        span = 17 if lift0 < 6 else 14           # 위로 접을수록 폭이 줄어 보인다
        b.poly([(24, y - 1), (24 + s * span, tipy), (24 + s * (span - 3), tipy + 6), (24, y + 4)],
               body if s > 0 else mid)
        for k in range(4):
            b.line(24 + s * (5 + k * 3), y + 1, 24 + s * (8 + k * 3), tipy + 5, dk)
    # 몸통
    b.ellipse(18, y - 3, 31, y + 9, mid)
    b.ellipse(20, y - 2, 27, y + 4, body)
    # 머리 + 부리
    b.ellipse(26, y - 9, 35, y - 1, body)
    b.poly([(33, y - 7), (42, y - 4), (33, y - 2)], C(236, 190, 92))
    b.px(31, y - 6, DARK)
    # 긴 꼬리 깃털
    b.poly([(19, y + 5), (4, y + 11), (8, y + 14), (21, y + 8)], mid)
    for k in range(4):
        b.line(6 + k * 3, y + 11 + k // 2, 10 + k * 3, y + 14, dk)
    b.rect(23, y + 9, 24, y + 12, dk)
    b.rect(27, y + 9, 28, y + 12, dk)
    b.ellipse(17, g - 2, 31, g, (40, 38, 44, 90))
    return fin(b, shade=False)


# ============================================================ 11 안킬로사우루스
def b_ankylo():
    b = buried()
    matrix(b, 10)
    b.ellipse(12, 4, 28, 21, BONE3)        # 곤봉 — 크고 울퉁불퉁하게
    b.ellipse(14, 6, 26, 19, BONE2)
    b.ellipse(15, 7, 22, 14, BONE1)
    for (x, y) in ((12, 9), (24, 8), (18, 3), (19, 20), (25, 15)):
        b.ellipse(x, y, x + 4, y + 4, BONE2)
        b.ellipse(x + 1, y + 1, x + 3, y + 3, BONE1)
    for t in range(4):                     # 꼬리뼈 마디
        b.line(16 - t, 18 + t, 5, 27, BONE2)
    for x in (8, 11, 14):
        b.rect(x, 22 + (14 - x) // 3, x + 2, 25 + (14 - x) // 3, BONE1)
    crack(b, [(6, 20), (14, 24), (26, 20)], MTX3)
    grime(b, 11, 8)
    return fin(b)


def a_ankylo(f):
    b = alive(); g = b.ground
    body, mid, dk = C(142, 132, 104), C(104, 96, 74), C(72, 66, 52)
    t = bob(f, 0, -1)
    b.ellipse(8, g - 22 + t, 38, g - 6 + t, mid)
    b.ellipse(10, g - 20 + t, 26, g - 12 + t, body)
    for x in range(12, 36, 5):                       # 등 가시
        b.poly([(x - 2, g - 20 + t), (x + 2, g - 20 + t), (x, g - 25 + t)], C(196, 178, 132))
    b.ellipse(34, g - 20 + t, 46, g - 10 + t, mid)
    b.px(43, g - 17 + t, DARK)
    tail = swing(f, -3, 3)
    b.rect(6, g - 18 + tail + t, 14, g - 14 + tail + t, mid)       # 꼬리 자루
    b.rect(6, g - 18 + tail + t, 14, g - 17 + tail + t, body)
    b.ellipse(0, g - 26 + tail + t, 12, g - 10 + tail + t, C(154, 140, 104))
    b.ellipse(1, g - 24 + tail + t, 9, g - 13 + tail + t, C(206, 190, 146))
    for (x, y) in ((0, g - 21 + tail + t), (4, g - 27 + tail + t),
                   (9, g - 24 + tail + t), (4, g - 11 + tail + t)):
        b.ellipse(x, y, x + 4, y + 4, C(154, 140, 104))
        b.ellipse(x + 1, y + 1, x + 3, y + 3, C(226, 212, 170))
    legs2(b, 22, g, 10, f, body, mid, 5, 8 + t)
    return fin(b)


# ============================================================ 12 공룡알 둥지
def b_eggs():
    b = buried()
    b.ellipse(3, 16, 28, 29, DIRT2)
    b.ellipse(5, 18, 26, 27, DIRT1)
    for (cx, cy, r) in ((10, 18, 5), (21, 17, 6), (16, 22, 5)):
        b.ellipse(cx - r, cy - r - 2, cx + r, cy + r, BONE2)
        b.ellipse(cx - r + 1, cy - r - 1, cx + r - 3, cy + 1, BONE1)
        crack(b, [(cx - r + 1, cy), (cx, cy - 3), (cx + r - 1, cy + 1)], BONE3)
    grime(b, 12, 10)
    return fin(b)


def a_eggs(f):
    b = alive(); g = b.ground
    b.ellipse(4, g - 12, 44, g, C(128, 100, 66))
    b.ellipse(7, g - 10, 40, g - 2, C(170, 138, 94))
    for cx in (12, 36):                               # 안 깬 알
        b.ellipse(cx - 6, g - 18, cx + 6, g - 4, C(228, 220, 196))
        b.ellipse(cx - 5, g - 17, cx, g - 10, C(248, 244, 228))
    # 갓 깬 새끼 두 마리
    for i, cx in enumerate((22, 30)):
        t = bob((f + i) % 2, 0, -1)
        b.ellipse(cx - 5, g - 16 + t, cx + 5, g - 6 + t, SK[1])
        b.ellipse(cx - 4, g - 15 + t, cx + 1, g - 10 + t, SK[0])
        b.ellipse(cx + 2, g - 21 + t, cx + 9, g - 13 + t, SK[1])
        b.px(cx + 6, g - 18 + t, DARK)
        b.poly([(cx + 7, g - 17 + t), (cx + 11, g - 16 + t), (cx + 7, g - 15 + t)], C(228, 200, 140))
        b.rect(cx - 3, g - 6 + t, cx - 2, g, SK[2])
        b.rect(cx + 2, g - 6 + t, cx + 3, g, SK[2])
    return fin(b)


# ============================================================ 13 스피노사우루스
def b_spino():
    b = buried()
    matrix(b, 11)
    b.rect(6, 24, 26, 27, BONE2)
    for i, x in enumerate(range(8, 25, 4)):          # 돛 가시
        h = 16 - abs(i - 2) * 4
        b.rect(x, 24 - h, x + 2, 25, BONE1)
        b.rect(x, 24 - h, x, 25, BONE2)
    crack(b, [(5, 18), (14, 22), (27, 17)], MTX3)
    grime(b, 13, 9)
    return fin(b)


def a_spino(f):
    b = alive(); g = b.ground
    body, mid, dk = C(120, 128, 152), C(86, 94, 116), C(58, 64, 82)
    t = bob(f, 0, -1)
    for i, x in enumerate(range(8, 36, 4)):          # 돛
        h = 20 - abs(i - 3) * 3
        b.rect(x, g - 26 - h + t, x + 3, g - 22 + t, C(168, 120, 92))
        b.rect(x, g - 26 - h + t, x + 1, g - 22 + t, C(206, 152, 118))
    b.ellipse(6, g - 28 + t, 36, g - 10 + t, mid)
    b.ellipse(9, g - 26 + t, 24, g - 16 + t, body)
    b.poly([(32, g - 28 + t), (38, g - 36 + t), (43, g - 34 + t), (37, g - 20 + t)], mid)
    b.poly([(37, g - 37 + t), (48, g - 33 + t), (48, g - 29 + t), (36, g - 31 + t)], body)
    b.rect(40, g - 30 + t, 47, g - 29 + t, dk)
    b.px(39, g - 35 + t, DARK)
    b.poly([(7, g - 24 + t), (0, g - 30 + t), (0, g - 26 + t), (8, g - 16 + t)], mid)
    legs2(b, 20, g, 8, f, body, mid, 6, 13 + t)
    return fin(b)


# ============================================================ 14 모사사우루스
def b_mosa():
    b = buried()
    matrix(b, 12)
    b.poly([(5, 14), (27, 12), (27, 17), (5, 19)], BONE2)
    b.poly([(5, 14), (16, 13), (16, 16), (5, 17)], BONE1)
    for x in range(7, 27, 3):                        # 이빨
        b.rect(x, 19, x + 1, 22, BONE1)
    b.poly([(6, 20), (26, 21), (26, 25), (6, 24)], BONE3)
    crack(b, [(5, 22), (16, 25), (27, 20)], MTX3)
    grime(b, 14, 9)
    return fin(b)


def a_mosa(f):
    b = alive(); g = b.ground
    b.rect(2, g - 4, 46, g, IRON3)                   # 수조
    b.rect(3, g - 34, 45, g - 4, C(56, 118, 148))
    b.rect(4, g - 33, 24, g - 5, C(74, 148, 178))
    for y in range(g - 30 + bob(f, 0, 2), g - 6, 7):
        b.line(4, y, 44, y, C(102, 178, 206))
    body, mid, dk = C(96, 120, 128), C(64, 84, 96), C(40, 56, 68)
    t = bob(f, 0, -2)
    b.ellipse(8, g - 26 + t, 36, g - 14 + t, mid)
    b.ellipse(10, g - 25 + t, 24, g - 19 + t, body)
    b.poly([(30, g - 24 + t), (46, g - 22 + t), (46, g - 17 + t), (30, g - 16 + t)], mid)
    for x in range(33, 46, 3):
        b.rect(x, g - 19 + t, x + 1, g - 17 + t, C(240, 238, 228))
    b.px(34, g - 22 + t, C(236, 220, 120))
    b.poly([(9, g - 22 + t), (0, g - 28 + t), (2, g - 20 + t), (0, g - 13 + t)], mid)
    b.poly([(18, g - 15 + t), (24, g - 10 + t), (14, g - 13 + t)], dk)
    b.rect(2, g - 36, 46, g - 33, IRON2)
    return fin(b, [(24, g - 20, 22, C(120, 200, 220))])


# ============================================================ 15 티라노사우루스
def b_trex():
    b = buried()
    matrix(b, 13)
    b.poly([(5, 12), (22, 8), (29, 14), (26, 20), (7, 20)], BONE2)
    b.poly([(6, 12), (20, 9), (25, 14), (14, 18), (7, 17)], BONE1)
    b.ellipse(19, 11, 24, 15, DARK)                  # 눈구멍
    b.poly([(6, 20), (27, 20), (25, 26), (8, 25)], BONE2)
    for x in range(9, 26, 3):
        b.rect(x, 20, x + 1, 24, BONE1)
    crack(b, [(4, 16), (13, 13), (28, 18)], MTX3)
    grime(b, 15, 10)
    return fin(b)


def a_trex(f):
    b = alive(); g = b.ground
    body, mid, dk = C(122, 110, 84), C(88, 78, 58), C(58, 52, 40)
    t = bob(f, 0, -1)
    # 몸통 + 꼬리
    b.ellipse(6, g - 32 + t, 32, g - 12 + t, mid)
    b.ellipse(9, g - 30 + t, 22, g - 20 + t, body)
    b.poly([(7, g - 28 + t), (0, g - 36 + t), (0, g - 30 + t), (8, g - 20 + t)], mid)
    # 굵은 목 — 몸통과 머리를 확실히 잇는다
    b.poly([(24, g - 32 + t), (28, g - 44 + t), (36, g - 44 + t), (32, g - 24 + t)], mid)
    b.poly([(26, g - 32 + t), (30, g - 42 + t), (33, g - 42 + t), (31, g - 26 + t)], body)
    # 머리 — 위턱 (주둥이를 둥글게 깎아 덜 사납게)
    b.ellipse(28, g - 48 + t, 47, g - 37 + t, mid)
    b.ellipse(30, g - 46 + t, 42, g - 41 + t, body)
    b.ellipse(43, g - 45 + t, 48, g - 38 + t, mid)
    # 살짝만 벌린 입
    b.poly([(32, g - 38 + t), (47, g - 37 + t), (47, g - 35 + t), (32, g - 36 + t)], DARK)
    b.poly([(31, g - 36 + t), (46, g - 35 + t), (46, g - 31 + t), (31, g - 32 + t)], mid)
    b.poly([(32, g - 35 + t), (44, g - 34 + t), (44, g - 32 + t), (32, g - 33 + t)], body)
    # 이빨 — 성글고 짧게. 윗니 넷, 아랫니 셋만 보인다.
    TOOTH = C(248, 245, 234)
    for x in (36, 39, 42, 45):
        b.rect(x, g - 37 + t, x, g - 36 + t, TOOTH)
    for x in (37, 40, 43):
        b.rect(x, g - 35 + t, x, g - 34 + t, TOOTH)
    # 눈 — 흰자에 동그란 동공, 위에 눈두덩을 얹어 순하게
    b.ellipse(33, g - 45 + t, 37, g - 41 + t, C(244, 242, 236))
    b.ellipse(34, g - 44 + t, 36, g - 42 + t, DARK)
    b.px(34, g - 44 + t, C(252, 250, 246))
    b.rect(32, g - 46 + t, 37, g - 45 + t, mid)
    b.rect(26, g - 26 + t, 31, g - 24 + t, body)     # 작은 앞발
    b.px(31, g - 23 + t, dk)
    legs2(b, 18, g, 8, f, body, mid, 8, 16 + t)
    return fin(b)


# ============================================================ 16 브라키오사우루스
def b_brachio():
    b = buried()
    matrix(b, 14)
    # 기둥 같은 목뼈 한 마디 — 위아래로 관절이 물린 모양
    b.rect(11, 8, 21, 24, BONE2)
    b.rect(12, 9, 17, 23, BONE1)
    b.ellipse(8, 4, 24, 11, BONE2)
    b.ellipse(10, 5, 20, 10, BONE1)
    b.ellipse(8, 21, 24, 28, BONE2)
    b.ellipse(10, 22, 20, 27, BONE1)
    for y in range(12, 22, 3):
        b.line(11, y, 21, y, BONE3)
    b.poly([(21, 12), (28, 9), (28, 12), (21, 15)], BONE1)     # 옆으로 뻗은 돌기
    b.poly([(11, 18), (4, 15), (4, 18), (11, 21)], BONE1)
    crack(b, [(6, 10), (16, 16), (26, 12)], MTX3)
    grime(b, 16, 9)
    return fin(b)


def a_brachio(f):
    b = alive(); g = b.ground
    body, mid, dk = C(122, 136, 160), C(88, 100, 124), C(58, 68, 88)
    t = bob(f, 0, -1)
    b.ellipse(4, g - 26 + t, 34, g - 8 + t, mid)
    b.ellipse(7, g - 24 + t, 22, g - 14 + t, body)
    # 아주 긴 목
    b.poly([(26, g - 24 + t), (34, g - 46 + t), (39, g - 46 + t), (32, g - 18 + t)], mid)
    b.poly([(28, g - 24 + t), (34, g - 44 + t), (36, g - 44 + t), (31, g - 20 + t)], body)
    b.ellipse(32, g - 48 + t, 43, g - 40 + t, mid)
    b.ellipse(33, g - 47 + t, 39, g - 43 + t, body)
    b.px(40, g - 45 + t, DARK)
    b.poly([(5, g - 22 + t), (0, g - 28 + t), (0, g - 24 + t), (6, g - 14 + t)], mid)
    legs2(b, 18, g, 11, f, body, mid, 6, 12 + t)
    return fin(b)


# ============================================================
ENTRIES = [
    ("trilobite", "삼엽충", b_trilobite, a_trilobite),
    ("ammonite", "암모나이트", b_ammonite, a_ammonite),
    ("amber", "호박 속 곤충", b_amber, a_amber),
    ("raptor", "벨로키랍토르", b_raptor, a_raptor),
    ("smilodon", "검치호", b_smilodon, a_smilodon),
    ("mammoth", "매머드", b_mammoth, a_mammoth),
    ("stego", "스테고사우루스", b_stego, a_stego),
    ("trike", "트리케라톱스", b_trike, a_trike),
    ("ptera", "프테라노돈", b_ptera, a_ptera),
    ("archaeo", "시조새", b_archaeo, a_archaeo),
    ("ankylo", "안킬로사우루스", b_ankylo, a_ankylo),
    ("eggs", "공룡알 둥지", b_eggs, a_eggs),
    ("spino", "스피노사우루스", b_spino, a_spino),
    ("mosa", "모사사우루스", b_mosa, a_mosa),
    ("trex", "티라노사우루스", b_trex, a_trex),
    ("brachio", "브라키오사우루스", b_brachio, a_brachio),
]

if __name__ == "__main__":
    import os
    os.makedirs("out/Relic", exist_ok=True)
    bs, al = build_sheets(ENTRIES, "a")
    bs.save("out/Relic/relic_buried_a.png")
    al.save("out/Relic/relic_alive_a.png")
    print("ok", bs.size, al.size)
