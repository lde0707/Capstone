"""F. 미스터리 · SF 8종 — 시안 빛과 금속. 마을에서 제일 '수상한' 유물들."""
import math
from relic_lib import *

SIL1, SIL2, SIL3 = C(226, 230, 238), C(172, 178, 192), C(112, 120, 140)    # 은빛 금속
XTL1, XTL2, XTL3 = C(226, 250, 252), C(160, 222, 236), C(96, 160, 190)     # 수정
ALG1, ALG2, ALG3 = C(160, 236, 144), C(104, 188, 96), C(60, 130, 62)       # 외계 초록
EGG1, EGG2, EGG3 = C(236, 224, 200), C(196, 180, 152), C(140, 126, 104)    # 알 껍데기
MET1, MET2, MET3 = C(96, 92, 96), C(62, 58, 62), C(36, 34, 38)             # 운석


def plinth(b, half=12, h=5):
    pedestal(b, half, h)
    return b.ground - h


# ============================================================ 51 타임캡슐
def b_capsule():
    b = buried()
    matrix(b, 61)
    # 녹슨 금속 원통 — 양 끝이 둥글고 가운데 이음 띠
    b.ellipse(4, 11, 28, 23, RUST2)
    b.ellipse(6, 13, 26, 21, RUST1)
    b.ellipse(4, 11, 12, 23, RUST1)
    b.rect(14, 10, 18, 24, IRON3)
    b.rect(14, 10, 15, 24, IRON2)
    for (x, y) in ((9, 15), (22, 19), (19, 14)):
        b.ellipse(x, y, x + 2, y + 2, C(88, 44, 22))
    b.rect(24, 20, 29, 24, MTX1)
    crack(b, [(8, 13), (13, 18), (10, 22)], DARK)
    grime(b, 61, 9)
    return fin(b)


def a_timecapsule(f):
    b = alive(); g = b.ground
    y = plinth(b, 15, 5)
    t = bob(f, 0, 0)
    # 눕혀서 연 캡슐 — 뚜껑은 옆으로 젖혀져 있다
    b.ellipse(6, y - 16, 36, y, SIL2)
    b.ellipse(8, y - 14, 30, y - 3, SIL1)
    b.ellipse(6, y - 16, 16, y, SIL3)                           # 열린 입구 안쪽
    b.ellipse(8, y - 14, 14, y - 2, C(46, 46, 54))
    b.rect(24, y - 16, 26, y, SIL3)
    b.rect(14, y - 16, 15, y, SIL3)
    for x in range(8, 37, 6):                                   # 리벳 줄
        b.px(x, y - 15, SIL3)
        b.px(x, y - 1, SIL3)
    # 벗겨 낸 뚜껑 — 캡슐 오른쪽에 비스듬히 기대 놓았다
    b.poly([(38, y), (40, y - 18), (46, y - 20), (46, y)], SIL2)
    b.poly([(39, y - 2), (41, y - 16), (44, y - 17), (44, y - 2)], SIL1)
    b.poly([(40, y - 18), (46, y - 20), (46, y - 18), (40, y - 16)], SIL3)
    # 쏟아져 나온 옛날 물건들
    b.rect(4, y - 8, 14, y, C(200, 182, 146))                   # 쏟아진 편지 뭉치
    b.rect(4, y - 8, 14, y - 6, C(240, 228, 196))
    b.rect(5, y - 6, 13, y - 1, C(226, 214, 184))
    b.line(6, y - 4, 12, y - 4, C(158, 142, 112))
    b.line(6, y - 2, 11, y - 2, C(158, 142, 112))
    b.ellipse(13, y - 12, 23, y - 2, C(150, 116, 52))           # 회중시계
    b.ellipse(15, y - 10, 21, y - 4, C(244, 240, 226))
    b.line(18, y - 7, 18, y - 9, C(90, 76, 60))
    b.line(18, y - 7, 20, y - 6, C(90, 76, 60))
    b.rect(17, y - 14, 19, y - 12, C(196, 158, 78))
    b.rect(23, y - 10, 33, y, C(86, 116, 158))                  # 사진
    b.rect(24, y - 9, 32, y - 2, C(222, 228, 240))
    b.ellipse(26, y - 8, 30, y - 4, C(160, 140, 124))
    b.rect(25, y - 3, 31, y - 2, C(180, 190, 206))
    # 명패
    b.rect(16, y + 1, 32, y + 1, GOLD2)
    if f % 2 == 1:
        b.px(9, y - 20, C(250, 244, 210)); b.px(41, y - 22, C(250, 244, 210))
    return fin(b, [(20, y - 8, 16, C(246, 232, 190))])


# ============================================================ 52 운석
def b_meteor():
    b = buried()
    matrix(b, 62)
    # 표면이 탄 검은 돌
    b.ellipse(4, 6, 28, 27, MET2)
    b.ellipse(6, 8, 26, 25, MET1)
    b.ellipse(8, 9, 20, 18, C(118, 114, 118))
    for (x, y, r) in ((10, 13, 3), (19, 11, 2), (15, 20, 3), (22, 19, 2), (12, 22, 2)):
        b.ellipse(x - r, y - r, x + r, y + r, MET2)              # 충돌 구덩이
        b.ellipse(x - r + 1, y - r + 1, x + r - 1, y + r - 1, MET3)
    for i in range(32):                                          # 반짝이는 금속 알갱이
        for j in range(32):
            if b.solid(i, j) and ((i * 73856093) ^ (j * 19349663)) % 23 == 0:
                b.px(i, j, C(198, 190, 178))
    b.rect(3, 22, 9, 28, MTX1)
    grime(b, 62, 6)
    return fin(b)


def a_meteorite(f):
    b = alive(); g = b.ground
    y = plinth(b, 13, 5)
    # 받침 고리
    b.rect(14, y - 4, 34, y, IRON3)
    b.rect(14, y - 4, 34, y - 3, IRON2)
    for x in (16, 30):
        b.rect(x, y - 8, x + 2, y - 3, IRON2)
    # 운석 덩어리
    b.ellipse(8, y - 32, 40, y - 6, MET2)
    b.ellipse(11, y - 30, 33, y - 10, MET1)
    b.ellipse(14, y - 28, 26, y - 19, C(122, 118, 122))
    for (x, yy, r) in ((16, y - 24, 4), (28, y - 26, 3), (22, y - 13, 4), (33, y - 16, 3), (12, y - 15, 2)):
        b.ellipse(x - r, yy - r, x + r, yy + r, MET2)
        b.ellipse(x - r + 1, yy - r + 1, x + r - 1, yy + r - 1, MET3)
    for i in range(6, 44):                                       # 금속 반짝임
        for j in range(y - 34, y - 4):
            if b.solid(i, j) and ((i * 73856093) ^ ((j + f) * 19349663)) % 27 == 0:
                b.px(i, j, C(228, 220, 204))
    # 아직 식지 않은 균열
    for pts in (((13, y - 22), (20, y - 20), (26, y - 24)),
                ((22, y - 12), (28, y - 15), (35, y - 13))):
        crack(b, pts, cyc(f, C(226, 118, 56), C(240, 150, 70), C(250, 176, 84), C(240, 150, 70)))
    b.rect(16, y + 1, 32, y + 1, GOLD2)
    return fin(b, [(24, y - 18, 16, C(255, 176, 110))])


# ============================================================ 53 태엽 자동인형
def b_gears():
    b = buried()
    matrix(b, 63)
    # 엉킨 톱니 뭉치
    for (cx2, cy2, r) in ((11, 12, 7), (22, 18, 6), (13, 24, 5)):
        b.ellipse(cx2 - r, cy2 - r, cx2 + r, cy2 + r, BRZ_ := C(148, 118, 66))
        b.ellipse(cx2 - r + 2, cy2 - r + 2, cx2 + r - 2, cy2 + r - 2, C(190, 158, 92))
        b.ellipse(cx2 - 2, cy2 - 2, cx2 + 2, cy2 + 2, MTX2)
        for k in range(8):
            ang = math.radians(k * 45)
            b.rect(cx2 + int(r * math.cos(ang)) - 1, cy2 + int(r * math.sin(ang)) - 1,
                   cx2 + int(r * math.cos(ang)) + 1, cy2 + int(r * math.sin(ang)) + 1, BRZ_)
    for (x, y) in ((9, 9), (24, 21), (15, 26)):
        b.px(x, y, C(96, 128, 108))
    b.rect(24, 5, 27, 12, C(148, 118, 66))
    grime(b, 63, 8)
    return fin(b)


def a_automaton(f):
    b = alive(); g = b.ground
    m1, m2, m3 = C(198, 166, 100), C(146, 114, 62), C(96, 72, 38)
    lean = wig(f, 1)
    t = bob(f, 0, -1)
    # 다리 — 관절이 드러난 황동
    for (x, c, h) in (((18 + lean), m2, 13), ((29 - lean), m1, 13)):
        b.rect(x - 2, g - h, x + 2, g, c)
        b.ellipse(x - 3, g - h - 2, x + 3, g - h + 2, m3)
        b.rect(x - 4, g - 2, x + 4, g, m3)
    # 몸통 — 앞면이 열려 톱니가 보인다
    b.rect(13 + lean, g - 34 + t, 35 + lean, g - 12 + t, m2)
    b.rect(14 + lean, g - 33 + t, 24 + lean, g - 14 + t, m1)
    b.rect(13 + lean, g - 34 + t, 35 + lean, g - 33 + t, m3)
    b.ellipse(18 + lean, g - 29 + t, 30 + lean, g - 17 + t, C(52, 44, 34))
    for (cx2, r) in ((22 + lean, 4), (28 + lean, 3)):
        cy2 = g - 23 + t
        b.ellipse(cx2 - r, cy2 - r, cx2 + r, cy2 + r, m1)
        b.ellipse(cx2 - r + 2, cy2 - r + 2, cx2 + r - 2, cy2 + r - 2, m3)
        for k in range(8):
            ang = math.radians(k * 45 + f * 11)
            b.px(cx2 + int(r * math.cos(ang)), cy2 + int(r * math.sin(ang)), m1)
    # 팔
    b.rect(6 + lean, g - 31 + t, 13 + lean, g - 28 + t, m2)
    b.rect(6 + lean, g - 28 + t, 9 + lean, g - 19 + t, m1)
    b.ellipse(4 + lean, g - 21 + t, 11 + lean, g - 15 + t, m2)
    b.rect(35 + lean, g - 31 + t, 42 + lean, g - 28 + t, m2)
    b.rect(39 + lean, g - 28 + t, 42 + lean, g - 19 + t, m1)
    b.ellipse(37 + lean, g - 21 + t, 44 + lean, g - 15 + t, m2)
    # 머리 — 도자기 얼굴
    b.ellipse(16 + lean, g - 50 + t, 32 + lean, g - 34 + t, m2)
    b.ellipse(18 + lean, g - 48 + t, 30 + lean, g - 36 + t, PORC1)
    b.ellipse(19 + lean, g - 47 + t, 26 + lean, g - 41 + t, C(255, 253, 248))
    eyes(b, 20 + lean, g - 45 + t, 6)
    b.ellipse(23 + lean, g - 39 + t, 26 + lean, g - 38 + t, C(206, 128, 132))
    b.rect(16 + lean, g - 51 + t, 32 + lean, g - 49 + t, m3)     # 이마 밴드
    b.rect(22 + lean, g - 55 + t, 26 + lean, g - 51 + t, m1)     # 태엽 꼭지
    b.ellipse(21 + lean, g - 58 + t, 27 + lean, g - 53 + t, m2)
    b.px(24 + lean, g - 56 + t, m3)
    return fin(b, [(24, g - 30, 16, C(246, 216, 150))])


# ============================================================ 54 수정 해골
def b_crystal():
    b = buried()
    matrix(b, 64)
    # 금 간 투명 수정 덩이 — 안쪽에 두개골 실루엣이 어렴풋
    b.poly([(8, 7), (22, 5), (27, 15), (22, 27), (9, 26), (5, 14)], C(120, 168, 190))
    b.poly([(10, 9), (21, 8), (24, 15), (20, 24), (11, 23), (8, 14)], XTL2)
    b.poly([(12, 11), (19, 10), (21, 16), (13, 15)], XTL1)
    b.ellipse(12, 13, 21, 22, C(150, 198, 216))                  # 안쪽 두개골
    b.ellipse(13, 15, 16, 18, C(88, 132, 160))
    b.ellipse(18, 15, 21, 18, C(88, 132, 160))
    b.rect(15, 20, 19, 22, C(88, 132, 160))
    crack(b, [(9, 10), (16, 16), (12, 25)], C(236, 250, 252))
    crack(b, [(24, 12), (18, 19)], C(236, 250, 252))
    grime(b, 64, 6)
    return fin(b)


def a_skull(f):
    b = alive(); g = b.ground
    y = plinth(b, 12, 5)
    t = bob(f, 0, 0)
    pulse = swing(f, 0, 2)
    # 두개골 — 면을 각지게 깎아 수정처럼
    b.poly([(11, y - 20), (14, y - 34), (24, y - 38), (34, y - 34), (37, y - 20),
            (32, y - 12), (16, y - 12)], XTL3)
    b.poly([(13, y - 20), (16, y - 32), (24, y - 36), (32, y - 32), (35, y - 20),
            (30, y - 13), (18, y - 13)], XTL2)
    b.poly([(16, y - 24), (18, y - 32), (24, y - 34), (26, y - 26), (20, y - 22)], XTL1)
    # 눈구멍 — 안에서 빛난다
    for ex in (16, 26):
        b.poly([(ex, y - 28), (ex + 6, y - 28), (ex + 5, y - 21), (ex + 1, y - 21)], C(24, 60, 82))
        b.poly([(ex + 1, y - 27), (ex + 5, y - 27), (ex + 4, y - 22), (ex + 2, y - 22)],
               cyc(f, C(120, 236, 252), C(156, 244, 254), C(196, 252, 255), C(156, 244, 254)))
    b.poly([(22, y - 20), (26, y - 20), (24, y - 16)], C(24, 60, 82))   # 코
    # 이빨
    b.rect(16, y - 15, 32, y - 12, XTL2)
    for k in range(6):
        b.rect(17 + k * 3, y - 15, 17 + k * 3, y - 12, XTL3)
    b.rect(16, y - 16, 32, y - 15, XTL1)
    # 아래턱
    b.poly([(16, y - 12), (32, y - 12), (29, y - 7), (19, y - 7)], XTL3)
    b.poly([(18, y - 11), (30, y - 11), (28, y - 8), (20, y - 8)], XTL2)
    b.rect(16, y + 1, 32, y + 1, GOLD2)
    return fin(b, [(19, y - 24, 10 + pulse, C(150, 246, 255)),
                   (29, y - 24, 10 + pulse, C(150, 246, 255)),
                   (24, y - 22, 16, C(120, 220, 250))])


# ============================================================ 55 고대 기계
def b_cog():
    b = buried()
    matrix(b, 65)
    # 이가 빠진 청동 톱니 원반
    cx2, cy2, r = 15, 17, 10
    b.ellipse(cx2 - r, cy2 - r, cx2 + r, cy2 + r, C(132, 106, 58))
    b.ellipse(cx2 - r + 2, cy2 - r + 2, cx2 + r - 2, cy2 + r - 2, C(178, 148, 86))
    b.ellipse(cx2 - 4, cy2 - 4, cx2 + 4, cy2 + 4, C(120, 96, 52))
    b.ellipse(cx2 - 2, cy2 - 2, cx2 + 2, cy2 + 2, MTX2)
    for k in range(14):
        if k in (3, 4, 9):                                       # 빠진 이
            continue
        ang = math.radians(k * 25.7)
        x = cx2 + int((r + 2) * math.cos(ang))
        yy = cy2 + int((r + 2) * math.sin(ang))
        b.rect(x - 1, yy - 1, x + 1, yy + 1, C(150, 122, 68))
    for k in range(8):                                           # 눈금
        ang = math.radians(k * 45 + 12)
        b.px(cx2 + int(7 * math.cos(ang)), cy2 + int(7 * math.sin(ang)), C(96, 128, 108))
    crack(b, [(8, 12), (16, 18), (12, 25)], MTX3)
    grime(b, 65, 8)
    return fin(b)


def a_mechanism(f):
    b = alive(); g = b.ground
    m1, m2, m3 = C(196, 164, 98), C(142, 112, 62), C(92, 70, 36)
    y = plinth(b, 15, 5)
    # 나무 케이스
    b.rect(8, y - 40, 40, y, C(120, 84, 52))
    b.rect(10, y - 38, 38, y - 2, C(78, 54, 34))
    b.rect(8, y - 40, 40, y - 38, C(166, 122, 78))
    b.rect(8, y - 2, 40, y, C(78, 54, 34))
    # 큰 톱니 셋이 맞물려 돈다
    for (cx2, cy2, r, sp) in ((18, y - 25, 9, 1), (31, y - 28, 6, -1), (28, y - 14, 5, 1)):
        b.ellipse(cx2 - r, cy2 - r, cx2 + r, cy2 + r, m2)
        b.ellipse(cx2 - r + 2, cy2 - r + 2, cx2 + r - 2, cy2 + r - 2, m1)
        b.ellipse(cx2 - 2, cy2 - 2, cx2 + 2, cy2 + 2, m3)
        n = 12 if r > 7 else 9
        for k in range(n):
            ang = math.radians(k * (360.0 / n) + f * sp * 14)
            x = cx2 + int((r + 1) * math.cos(ang))
            yy = cy2 + int((r + 1) * math.sin(ang))
            b.rect(x - 1, yy - 1, x + 1, yy + 1, m2)
        for k in range(4):
            ang = math.radians(k * 90 + f * sp * 14)
            b.px(cx2 + int((r - 3) * math.cos(ang)), cy2 + int((r - 3) * math.sin(ang)), m3)
    # 눈금 원반 + 바늘
    b.ellipse(11, y - 37, 25, y - 29, m1)
    b.ellipse(12, y - 36, 24, y - 30, C(64, 52, 30))
    for k in range(10):
        ang = math.radians(k * 36)
        b.px(18 + int(6 * math.cos(ang)), y - 33 + int(3 * math.sin(ang)), m1)
    ang = math.radians(210 + f * 40)
    b.line(18, y - 33, 18 + int(5 * math.cos(ang)), y - 33 + int(3 * math.sin(ang)), C(232, 208, 140))
    # 새겨진 글자
    for k in range(6):
        b.px(29 + k * 2, y - 35, C(180, 206, 190))
        b.px(29 + k * 2, y - 33, C(140, 172, 158))
    b.rect(14, y + 1, 34, y + 1, GOLD2)
    return fin(b, [(24, y - 24, 18, C(246, 220, 150))])


# ============================================================ 56 UFO 파편
def b_plate():
    b = buried()
    matrix(b, 66)
    # 휘어진 은빛 금속판 — 긁힌 자국과 낯선 기호
    b.poly([(4, 13), (16, 7), (28, 12), (26, 20), (14, 25), (5, 21)], SIL3)
    b.poly([(6, 14), (16, 9), (25, 13), (23, 19), (14, 23), (7, 19)], SIL2)
    b.poly([(8, 14), (16, 11), (21, 14), (14, 19)], SIL1)
    for k in range(5):
        b.line(7 + k * 4, 20, 9 + k * 4, 12, C(146, 154, 170))
    for (x, y) in ((11, 15), (18, 16), (15, 19)):                # 낯선 기호
        b.px(x, y, CY2); b.px(x + 1, y + 1, CY3); b.px(x + 2, y - 1, CY2)
    b.rect(24, 20, 29, 25, MTX1)
    grime(b, 66, 7)
    return fin(b)


def a_ufo(f):
    b = alive(); g = b.ground
    hover = swing(f, 0, -3)
    y = g - 26 + hover
    # 빛기둥
    for k in range(14):
        half = 3 + k
        yy = y + 8 + k
        if yy >= g:
            break
        b.rect(24 - half, yy, 24 + half, yy, (120, 220, 236, 70) if k % 2 else (150, 234, 246, 50))
    b.ellipse(12, g - 4, 36, g, (120, 220, 236, 60))
    # 접시 몸체
    b.ellipse(4, y - 2, 44, y + 10, SIL3)
    b.ellipse(6, y - 2, 42, y + 6, SIL2)
    b.ellipse(9, y - 1, 39, y + 2, SIL1)
    b.rect(4, y + 3, 44, y + 4, SIL3)
    # 돔
    for k in range(11):
        half = int(11 * math.sin(math.acos(k / 11.0)))
        b.rect(24 - half, y - 2 - k, 24 + half, y - 2 - k, C(150, 228, 240))
        b.rect(24 - half + 1, y - 2 - k, 24 - half + 3, y - 2 - k, C(206, 246, 250))
    b.ellipse(18, y - 10, 30, y - 3, C(96, 178, 200))            # 안에 비치는 그림자
    b.ellipse(21, y - 9, 27, y - 5, ALG2)
    b.px(22, y - 8, DARK); b.px(26, y - 8, DARK)
    # 돌아가는 불빛
    cols = [CY1, C(240, 220, 120), C(240, 130, 150), ALG1]
    for k in range(7):
        x = 8 + k * 5
        b.ellipse(x, y + 5, x + 2, y + 7, cols[(k + f) % 4])
    return fin(b, [(24, y + 4, 24, C(150, 234, 246)), (24, y - 8, 14, C(200, 250, 255))],
               shade=False)


# ============================================================ 57 외계 생물 표본
def b_vial():
    b = buried()
    matrix(b, 67)
    # 초록 액체가 남은 깨진 캡슐
    b.rect(10, 6, 22, 27, C(146, 176, 186))
    b.rect(11, 7, 17, 26, C(190, 216, 224))
    b.rect(12, 16, 20, 25, ALG2)                                  # 남은 액체
    b.rect(12, 16, 20, 17, ALG1)
    b.ellipse(13, 19, 18, 24, ALG3)                               # 웅크린 표본
    b.px(14, 21, DARK); b.px(17, 21, DARK)
    b.poly([(10, 6), (22, 6), (20, 2), (12, 2)], IRON3)           # 찌그러진 마개
    b.poly([(12, 3), (18, 3), (18, 5), (12, 5)], IRON2)
    b.poly([(18, 8), (24, 10), (22, 16), (19, 14)], MTX1)         # 깨져 나간 유리
    crack(b, [(11, 9), (17, 13), (13, 18)], C(236, 244, 248))
    grime(b, 67, 7)
    return fin(b)


def a_alien(f):
    b = alive(); g = b.ground
    hop = cyc(f, 0, -5, -8, -3)
    y = g + hop
    squash = cyc(f, 0, 1, 1, 0)
    # 말랑한 몸 — 튀어오를 때 길쭉해진다
    b.ellipse(12, y - 24 - squash * 3, 36, y, ALG2)
    b.ellipse(15, y - 22 - squash * 3, 29, y - 8, ALG1)
    b.ellipse(18, y - 20 - squash * 3, 25, y - 14, C(206, 250, 190))
    # 더듬이 둘
    for (sx, d) in ((18, -1), (30, 1)):
        for k in range(7):
            b.px(sx + d * k // 2, y - 25 - squash * 3 - k, ALG3 if k % 2 else ALG2)
        b.ellipse(sx + d * 3 - 2, y - 35 - squash * 3, sx + d * 3 + 2, y - 31 - squash * 3, CY2)
        b.px(sx + d * 3, y - 34 - squash * 3, CY1)
    # 큰 눈 하나 + 작은 눈 둘
    b.ellipse(18, y - 20 - squash * 3, 30, y - 11, C(246, 250, 246))
    b.ellipse(21, y - 18 - squash * 3, 27, y - 13, C(38, 44, 52))
    b.ellipse(22, y - 17 - squash * 3, 24, y - 15, C(160, 236, 252))
    for ex in (15, 31):
        b.ellipse(ex, y - 24 - squash * 3, ex + 3, y - 21 - squash * 3, C(246, 250, 246))
        b.px(ex + 1, y - 23 - squash * 3, DARK)
    # 작은 팔 + 발
    b.ellipse(8, y - 16, 14, y - 11, ALG2)
    b.ellipse(34, y - 16, 40, y - 11, ALG2)
    b.ellipse(14, y - 3, 22, y, ALG3)
    b.ellipse(26, y - 3, 34, y, ALG3)
    # 점무늬
    for (x, yy) in ((16, y - 8), (30, y - 6), (22, y - 4), (33, y - 12)):
        b.ellipse(x, yy, x + 2, yy + 1, ALG3)
    if hop < 0:                                                   # 뛰어오른 자리 먼지
        b.ellipse(16, g - 1, 32, g, (160, 200, 160, 90))
    return fin(b, [(24, y - 16, 20, C(180, 250, 170))])


# ============================================================ 58 미지의 알
def b_egg():
    b = buried()
    matrix(b, 68)
    # 금 간 거대한 알 — 안에서 뭔가 비친다
    b.ellipse(6, 4, 26, 28, EGG2)
    b.ellipse(8, 6, 22, 23, EGG1)
    b.ellipse(10, 8, 18, 16, C(250, 244, 226))
    for (x, y, r) in ((11, 20, 2), (19, 13, 2), (15, 25, 2)):     # 반점
        b.ellipse(x - r, y - r, x + r, y + r, EGG3)
    crack(b, [(9, 9), (14, 14), (11, 20), (16, 26)], C(102, 92, 76))
    crack(b, [(23, 12), (18, 17), (22, 23)], C(102, 92, 76))
    b.rect(13, 14, 19, 18, C(62, 84, 78))                         # 갈라진 틈 사이 어둠
    b.px(15, 16, CY2); b.px(17, 16, CY2)
    grime(b, 68, 8)
    return fin(b)


def a_hatchling(f):
    b = alive(); g = b.ground
    t = bob(f, 0, -1)
    step = wig(f, 2)
    s1, s2, s3 = C(140, 206, 196), C(92, 158, 152), C(56, 108, 106)
    # 아직 엉덩이에 붙어 있는 알 껍데기
    b.ellipse(8, g - 18, 26, g, EGG2)
    b.ellipse(10, g - 16, 21, g - 3, EGG1)
    for k in range(7):                                            # 깨진 윗단
        b.rect(8 + k * 3, g - 18 - (k % 2) * 2, 10 + k * 3, g - 17, EGG2)
    # 다리
    b.rect(24 - step, g - 8, 28 - step, g, s2)
    b.rect(31 + step, g - 8, 35 + step, g, s1)
    b.rect(22 - step, g - 2, 29 - step, g, s3)
    b.rect(30 + step, g - 2, 37 + step, g, s3)
    # 몸통
    b.ellipse(19, g - 28 + t, 41, g - 6 + t, s2)
    b.ellipse(22, g - 26 + t, 35, g - 12 + t, s1)
    for k in range(5):                                            # 등 돌기
        b.poly([(23 + k * 4, g - 28 + t), (25 + k * 4, g - 33 + t), (27 + k * 4, g - 28 + t)], s3)
    b.poly([(19, g - 20 + t), (10, g - 26 + t), (14, g - 14 + t)], s2)    # 꼬리
    # 머리
    b.ellipse(30, g - 42 + t, 46, g - 26 + t, s2)
    b.ellipse(32, g - 40 + t, 42, g - 31 + t, s1)
    b.poly([(42, g - 36 + t), (47, g - 34 + t), (46, g - 29 + t), (41, g - 30 + t)], s1)
    b.px(46, g - 32 + t, s3)
    b.poly([(33, g - 41 + t), (34, g - 47 + t), (38, g - 40 + t)], s3)    # 작은 뿔
    eyes(b, 34, g - 38 + t, 6)
    b.rect(38, g - 31 + t, 44, g - 30 + t, s3)
    # 머리에 얹힌 껍데기 조각
    b.poly([(33, g - 43 + t), (43, g - 43 + t), (41, g - 47 + t), (35, g - 47 + t)], EGG1)
    b.poly([(34, g - 44 + t), (42, g - 44 + t), (42, g - 43 + t), (34, g - 43 + t)], EGG2)
    return fin(b, [(32, g - 34, 14, C(180, 246, 238))])


# ============================================================
ENTRIES = [
    ("timecapsule", "타임캡슐", b_capsule, a_timecapsule),
    ("meteorite", "운석", b_meteor, a_meteorite),
    ("automaton", "태엽 자동인형", b_gears, a_automaton),
    ("crystalskull", "수정 해골", b_crystal, a_skull),
    ("mechanism", "고대 기계", b_cog, a_mechanism),
    ("ufo", "UFO", b_plate, a_ufo),
    ("alien", "외계 생물", b_vial, a_alien),
    ("hatchling", "미지의 알", b_egg, a_hatchling),
]

if __name__ == "__main__":
    import os
    os.makedirs("out/Relic", exist_ok=True)
    bs, al = build_sheets(ENTRIES, "f")
    bs.save("out/Relic/relic_buried_f.png")
    al.save("out/Relic/relic_alive_f.png")
    print("ok", bs.size, al.size)
