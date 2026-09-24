"""D. 한국 역사 유물 10종 — 대부분 전시형. 좌대 위에 올려 박물관처럼 보이게."""
import math
from relic_lib import *

CEL1, CEL2, CEL3 = C(154, 204, 184), C(104, 160, 144), C(64, 112, 102)     # 청자 비색
WHT1, WHT2, WHT3 = C(248, 246, 238), C(216, 212, 200), C(168, 164, 152)    # 백자
BRZ1, BRZ2, BRZ3 = C(186, 156, 96), C(132, 106, 58), C(88, 68, 36)         # 청동
VERD1, VERD2 = C(120, 186, 158), C(74, 134, 116)                           # 청동 녹
WOOD1, WOOD2, WOOD3 = C(166, 122, 78), C(120, 84, 52), C(80, 54, 34)       # 목재
CLAY1, CLAY2, CLAY3 = C(198, 146, 104), C(154, 106, 72), C(110, 72, 48)    # 토기


def plinth(b, half=12, h=5):
    """전시 좌대 — 유물마다 폭만 달리 쓴다."""
    pedestal(b, half, h)
    return b.ground - h


# ============================================================ 31 신라 토우
def b_tou():
    b = buried()
    matrix(b, 41)
    # 부서진 흙 인형 — 몸통과 머리가 따로 굴러다닌다
    b.ellipse(13, 5, 23, 15, CLAY2)
    b.ellipse(14, 6, 20, 12, CLAY1)
    b.px(16, 9, DARK); b.px(19, 9, DARK)
    b.poly([(8, 17), (20, 15), (22, 27), (10, 28)], CLAY2)
    b.poly([(10, 18), (18, 17), (19, 24), (11, 25)], CLAY1)
    b.rect(20, 19, 25, 21, CLAY2)                              # 떨어진 팔
    crack(b, [(11, 20), (16, 23), (13, 28)], CLAY3)
    crack(b, [(15, 7), (18, 13)], CLAY3)
    grime(b, 41, 9)
    return fin(b)


def a_tou(f):
    b = alive(); g = b.ground
    t = bob(f, 0, -1)
    step = wig(f, 2)
    b.rect(19 - step, g - 7, 22 - step, g, CLAY2)              # 다리
    b.rect(26 + step, g - 7, 29 + step, g, CLAY1)
    b.poly([(17, g - 26 + t), (31, g - 26 + t), (33, g - 6 + t), (15, g - 6 + t)], CLAY2)
    b.poly([(19, g - 24 + t), (28, g - 24 + t), (29, g - 10 + t), (18, g - 10 + t)], CLAY1)
    b.rect(11, g - 24 + t, 17, g - 20 + t, CLAY2)              # 팔
    b.rect(31, g - 24 + t, 37, g - 20 + t, CLAY2)
    b.ellipse(16, g - 42 + t, 32, g - 25 + t, CLAY2)           # 머리
    b.ellipse(18, g - 40 + t, 29, g - 30 + t, CLAY1)
    b.ellipse(19, g - 37 + t, 22, g - 34 + t, DARK)            # 푹 파인 눈
    b.ellipse(26, g - 37 + t, 29, g - 34 + t, DARK)
    b.ellipse(22, g - 31 + t, 27, g - 28 + t, DARK)            # 벌린 입
    b.rect(18, g - 43 + t, 30, g - 41 + t, CLAY3)              # 상투 띠
    b.ellipse(21, g - 47 + t, 27, g - 42 + t, CLAY2)
    return fin(b)


# ============================================================ 32 고려청자
def b_shard():
    b = buried()
    matrix(b, 42)
    # 비색 도편 세 조각 — 한 조각에 학 무늬가 남아 있다
    b.poly([(5, 12), (17, 7), (20, 18), (8, 22)], CEL2)
    b.poly([(7, 13), (15, 10), (17, 17), (10, 19)], CEL1)
    b.poly([(11, 13), (14, 12), (13, 15), (15, 16), (11, 16)], WHT1)    # 학
    b.poly([(19, 20), (28, 17), (27, 26), (20, 27)], CEL2)
    b.poly([(20, 21), (26, 19), (25, 24), (21, 25)], CEL1)
    b.poly([(6, 24), (13, 23), (12, 29), (7, 28)], CEL3)
    crack(b, [(8, 14), (14, 19), (9, 21)], CEL3)
    grime(b, 42, 8)
    return fin(b)


def a_celadon(f):
    b = alive(); g = b.ground
    t = bob(f, 0, 0)
    y = plinth(b, 11, 5)
    # 매병 — 어깨가 넓고 아래가 좁은 실루엣
    # 굽은 좁고, 어깨가 넓고, 목은 짧다 — 전형적인 매병
    prof = ([6, 6, 7, 8] + [9, 10, 11, 12, 13, 13, 14, 14] +
            [14, 13, 13, 12, 12, 11, 10, 10, 9, 8, 8, 7] + [5, 4, 4, 4] + [6, 7])
    for k, half in enumerate(prof):
        yy = y - 1 - k
        b.rect(24 - half, yy, 24 + half, yy, CEL2)
        b.rect(24 - half + 1, yy, 24 - half + 3, yy, CEL1)
        b.px(24 + half - 1, yy, CEL3)
    b.rect(17, y - 1 - len(prof), 31, y - len(prof), CEL1)      # 아가리
    # 상감 학 두 마리 + 구름
    for (hx, hy) in ((18, y - 20), (28, y - 15)):
        b.poly([(hx, hy), (hx + 5, hy - 2), (hx + 3, hy + 2), (hx + 6, hy + 3), (hx, hy + 3)], WHT1)
        b.px(hx + 6, hy - 3, WHT1)
    for (cx2, cy2) in ((26, y - 23), (17, y - 12)):
        b.rect(cx2, cy2, cx2 + 4, cy2, WHT2)
        b.rect(cx2 + 1, cy2 - 1, cx2 + 3, cy2 - 1, WHT2)
    return fin(b, [(24, y - 18, 16, C(180, 240, 220))])


# ============================================================ 33 백자 달항아리
def b_white():
    b = buried()
    matrix(b, 43)
    for (pts) in (
        [(6, 9), (18, 6), (21, 15), (9, 19)],
        [(18, 17), (27, 14), (26, 24), (19, 25)],
        [(8, 21), (17, 20), (16, 28), (9, 27)],
    ):
        b.poly(pts, WHT2)
        b.poly([(x + 1, y + 1) for (x, y) in pts[:3]] + [(pts[3][0] + 1, pts[3][1] - 1)], WHT1)
    crack(b, [(10, 11), (16, 16), (12, 22)], WHT3)
    grime(b, 43, 9)
    return fin(b)


def a_moonjar(f):
    b = alive(); g = b.ground
    t = bob(f, 0, 0)
    y = plinth(b, 12, 5)
    cy = y - 17
    b.ellipse(7, cy - 16, 41, cy + 16, WHT2)                    # 둥근 몸
    b.ellipse(10, cy - 14, 30, cy + 6, WHT1)
    b.ellipse(14, cy - 12, 24, cy - 2, C(255, 253, 248))
    b.rect(17, y - 1, 31, y, WHT3)                              # 굽
    b.rect(18, y - 3, 30, y - 1, WHT2)
    b.rect(18, cy - 17, 30, cy - 15, WHT2)                      # 아가리
    b.rect(19, cy - 18, 29, cy - 17, WHT1)
    b.line(9, cy + 2, 39, cy + 1, WHT3)                         # 붙인 자국
    return fin(b, [(24, cy, 20, C(246, 248, 255))])


# ============================================================ 34 앙부일구
def b_bowl():
    b = buried()
    matrix(b, 44)
    # 반구가 세로로 쪼개져 반쪽만 남았다
    b.ellipse(5, 8, 27, 26, BRZ2)
    b.ellipse(7, 10, 25, 22, BRZ3)
    b.ellipse(9, 11, 23, 20, BRZ1)
    b.rect(16, 8, 27, 27, MTX1)                                 # 깨져 나간 절반
    for k in range(4):                                          # 시각선
        b.line(10 + k * 2, 12, 15, 20, VERD2)
    b.line(9, 16, 16, 16, VERD1)
    crack(b, [(8, 12), (14, 18), (11, 25)], MTX3)
    grime(b, 44, 8)
    return fin(b)


def a_sundial(f):
    b = alive(); g = b.ground
    t = bob(f, 0, 0)
    y = plinth(b, 13, 5)
    # 네 다리 받침
    for x in (12, 34):
        b.rect(x, y - 9, x + 3, y, BRZ3)
    b.rect(10, y - 12, 38, y - 9, BRZ2)
    b.rect(10, y - 12, 38, y - 11, BRZ1)
    # 반구 솥 — 위가 열린 그릇
    for k in range(13):
        half = int(5 + 10 * (k / 12.0) ** 0.55)                  # 아래가 좁은 사발
        b.rect(24 - half, y - 13 - k, 24 + half, y - 13 - k, BRZ2)
        b.rect(24 - half + 1, y - 13 - k, 24 - half + 2, y - 13 - k, BRZ1)
        b.px(24 + half - 1, y - 13 - k, BRZ3)
    b.rect(9, y - 26, 39, y - 25, BRZ1)                         # 테두리
    b.ellipse(11, y - 26, 37, y - 20, BRZ3)                     # 안쪽 그늘
    b.ellipse(13, y - 25, 35, y - 21, C(60, 46, 26))
    for k in range(5):                                          # 시각선
        b.line(15 + k * 4, y - 24, 24, y - 15, VERD2)
    b.line(11, y - 22, 37, y - 22, VERD1)
    # 영침 + 그림자 (프레임마다 움직인다)
    sx = cyc(f, 20, 22, 25, 27)
    b.line(24, y - 14, sx, y - 24, GOLD2)
    b.px(sx, y - 24, GOLD1)
    b.line(24, y - 14, 48 - sx, y - 22, C(48, 38, 22))
    return fin(b, [(24, y - 22, 12, C(255, 232, 170))])


# ============================================================ 35 측우기
def b_cyl():
    b = buried()
    matrix(b, 45)
    # 찌그러진 청동 원통
    b.poly([(9, 6), (23, 8), (21, 27), (11, 25)], BRZ2)
    b.poly([(11, 8), (20, 10), (19, 24), (13, 23)], BRZ1)
    b.ellipse(9, 4, 23, 9, BRZ3)
    b.ellipse(11, 5, 21, 8, C(66, 52, 28))
    b.rect(14, 14, 21, 17, MTX1)                                # 뚫린 자리
    for (x, y) in ((12, 12), (19, 20), (14, 22)):
        b.ellipse(x, y, x + 2, y + 2, VERD2)
    crack(b, [(11, 11), (16, 18), (13, 24)], MTX3)
    grime(b, 45, 8)
    return fin(b)


def a_raingauge(f):
    b = alive(); g = b.ground
    y = plinth(b, 12, 5)
    # 돌 받침
    b.rect(15, y - 10, 33, y, STONE2)
    b.rect(15, y - 10, 33, y - 9, STONE1)
    b.rect(13, y - 13, 35, y - 10, STONE2)
    b.rect(13, y - 13, 35, y - 12, STONE1)
    b.rect(21, y - 4, 27, y - 3, STONE3)
    # 청동 원통
    b.rect(17, y - 37, 31, y - 13, BRZ2)
    b.rect(18, y - 37, 22, y - 13, BRZ1)
    b.rect(30, y - 37, 31, y - 13, BRZ3)
    for yy in (y - 33, y - 24, y - 16):
        b.rect(16, yy, 32, yy + 1, BRZ3)
        b.rect(16, yy, 32, yy, BRZ1)
    b.ellipse(16, y - 40, 32, y - 35, BRZ1)                     # 열린 아가리
    b.ellipse(18, y - 39, 30, y - 36, C(58, 46, 26))
    for (x, yy) in ((19, y - 30), (28, y - 22), (22, y - 18)):  # 푸른 녹
        b.ellipse(x, yy, x + 2, yy + 2, VERD1)
    # 빗방울 (프레임마다 다른 높이)
    for k in range(3):
        yy = y - 48 + ((k * 5 + f * 3) % 8)
        b.px(20 + k * 4, yy, C(180, 226, 246))
        b.px(20 + k * 4, yy + 1, C(130, 190, 226))
    return fin(b, [(24, y - 30, 14, C(198, 232, 244))])


# ============================================================ 36 비파형 동검
def b_blade():
    b = buried()
    matrix(b, 46)
    # 녹슨 칼날 — 가운데가 볼록한 비파형
    for k in range(20):
        half = 1 + int(3.2 * math.sin(math.radians(k * 9 + 20)))
        b.rect(16 - half, 6 + k, 16 + half, 6 + k, BRZ2)
        b.rect(16 - half, 6 + k, 16 - half + 1, 6 + k, BRZ1)
    b.rect(15, 26, 17, 29, BRZ3)                                # 부러진 슴베
    b.rect(14, 4, 18, 6, BRZ3)
    for (x, y) in ((13, 11), (19, 16), (15, 21), (18, 24)):
        b.ellipse(x, y, x + 2, y + 2, VERD2)
    crack(b, [(15, 14), (17, 19)], MTX3)
    grime(b, 46, 8)
    return fin(b)


def a_dagger(f):
    b = alive(); g = b.ground
    y = plinth(b, 10, 5)
    t = bob(f, 0, 0)
    # 받침에 꽂아 세운 동검
    b.rect(20, y - 6, 28, y, STONE2)
    b.rect(20, y - 6, 28, y - 5, STONE1)
    b.rect(22, y - 14, 26, y - 6, WOOD2)                        # 자루
    b.rect(22, y - 14, 23, y - 6, WOOD1)
    b.rect(19, y - 17, 29, y - 14, BRZ2)                        # 검코
    b.rect(19, y - 17, 29, y - 16, BRZ1)
    for k in range(30):                                          # 비파형 날
        half = 2 + int(4.4 * math.sin(math.radians(k * 6 + 16)))
        yy = y - 18 - k
        b.rect(24 - half, yy, 24 + half, yy, BRZ2)
        b.rect(24 - half + 1, yy, 24 - 1, yy, BRZ1)
        b.px(24 + half - 1, yy, BRZ3)
    b.rect(23, y - 48, 25, y - 18, GOLD3)                       # 등날
    b.poly([(21, y - 47), (24, y - 53), (27, y - 47)], BRZ1)
    for (x, yy) in ((20, y - 34), (28, y - 26), (22, y - 42)):
        b.ellipse(x, yy, x + 1, yy + 2, VERD1)
    return fin(b, [(24, y - 34, 14, C(168, 232, 208))])


# ============================================================ 37 금관
def b_gold():
    b = buried()
    matrix(b, 47)
    # 짓눌려 찌그러진 금띠
    b.poly([(5, 14), (14, 10), (26, 13), (27, 20), (14, 18), (6, 21)], GOLD2)
    b.poly([(7, 15), (14, 12), (24, 15), (24, 18), (14, 16), (8, 19)], GOLD1)
    b.poly([(11, 10), (13, 4), (16, 9)], GOLD2)                 # 꺾인 장식
    b.poly([(19, 9), (23, 5), (22, 12)], GOLD2)
    for (x, y) in ((9, 16), (16, 14), (22, 16), (13, 19)):      # 달개
        b.ellipse(x, y, x + 1, y + 1, GOLD1)
    b.rect(4, 22, 12, 26, MTX1)
    crack(b, [(8, 13), (17, 17), (25, 14)], GOLD3)
    grime(b, 47, 7)
    return fin(b)


def a_crown(f):
    b = alive(); g = b.ground
    y = plinth(b, 11, 5)
    t = bob(f, 0, 0)
    # 받침 기둥
    b.rect(21, y - 10, 27, y, STONE2)
    b.rect(21, y - 10, 23, y, STONE1)
    base = y - 10
    # 금테
    b.rect(11, base - 8, 37, base, GOLD2)
    b.rect(11, base - 8, 37, base - 7, GOLD1)
    b.rect(11, base - 1, 37, base, GOLD3)
    for x in range(13, 37, 4):
        b.px(x, base - 5, GOLD1)
        b.px(x, base - 3, GOLD3)
    # 出 자 나뭇가지 장식 세 개
    for (cx2, h) in ((16, 16), (24, 22), (32, 16)):
        b.rect(cx2 - 1, base - 8 - h, cx2 + 1, base - 8, GOLD2)
        b.rect(cx2 - 1, base - 8 - h, cx2, base - 8, GOLD1)
        for k in (0, 1):
            yy = base - 12 - k * 7
            b.rect(cx2 - 5, yy, cx2 + 5, yy + 1, GOLD2)
            b.rect(cx2 - 5, yy, cx2 - 4, yy + 1, GOLD1)
            b.rect(cx2 - 5, yy - 3, cx2 - 4, yy, GOLD2)
            b.rect(cx2 + 4, yy - 3, cx2 + 5, yy, GOLD2)
        b.ellipse(cx2 - 2, base - 11 - h, cx2 + 2, base - 7 - h, GOLD1)
    # 곡옥 + 달개
    for (x, yy) in ((14, base - 14), (34, base - 14), (20, base - 5), (28, base - 5)):
        b.ellipse(x, yy, x + 2, yy + 3, JADE1)
        b.px(x + 1, yy + 1, C(196, 234, 220))
    return fin(b, [(24, base - 14, 20, C(255, 232, 160))])


# ============================================================ 38 거북선
def b_plank():
    b = buried()
    matrix(b, 48)
    # 물에 삭은 판자 두 장 + 못
    b.poly([(4, 10), (26, 6), (27, 12), (5, 16)], WOOD2)
    b.poly([(5, 11), (25, 8), (25, 11), (6, 14)], WOOD1)
    b.poly([(6, 18), (24, 16), (25, 23), (7, 25)], WOOD3)
    b.poly([(8, 19), (22, 18), (22, 21), (9, 22)], WOOD2)
    for (x, y) in ((9, 12), (20, 9), (12, 21), (21, 19)):
        b.px(x, y, RUST2); b.px(x + 1, y, RUST1)
    for k in range(6):                                           # 나뭇결
        b.px(7 + k * 3, 13 - k // 3, WOOD3)
    crack(b, [(14, 7), (16, 15)], WOOD3)
    grime(b, 48, 9)
    return fin(b)


def a_turtleship(f):
    b = alive(); g = b.ground
    y = plinth(b, 18, 5)
    t = bob(f, 0, -1)
    # 물결 받침
    for x in range(8, 41, 3):
        b.px(x, y - 1 + (0 if (x // 3 + f) % 2 else 1), C(120, 176, 206))
    # 선체
    b.poly([(6, y - 13 + t), (42, y - 13 + t), (38, y - 2 + t), (10, y - 2 + t)], WOOD2)
    b.poly([(9, y - 12 + t), (39, y - 12 + t), (36, y - 5 + t), (12, y - 5 + t)], WOOD1)
    b.rect(6, y - 14 + t, 42, y - 13 + t, WOOD3)
    for x in range(11, 38, 5):                                   # 포혈
        b.rect(x, y - 10 + t, x + 2, y - 8 + t, C(46, 34, 24))
    # 등딱지 지붕 + 쇠못
    for k in range(9):
        half = 17 - k * 2
        yy = y - 15 - k + t
        b.rect(24 - half, yy, 24 + half, yy, WOOD3 if k % 2 else WOOD2)
        if k % 2 == 0:
            for x in range(24 - half + 2, 24 + half - 1, 5):
                b.px(x, yy, IRON1)
    b.rect(20, y - 26 + t, 28, y - 24 + t, WOOD3)
    # 용머리
    b.poly([(40, y - 16 + t), (48, y - 20 + t), (49, y - 12 + t), (40, y - 10 + t)], C(176, 128, 72))
    b.poly([(42, y - 16 + t), (47, y - 18 + t), (47, y - 13 + t), (42, y - 12 + t)], C(210, 162, 96))
    b.px(46, y - 16 + t, DARK)
    b.poly([(49, y - 16 + t), (53, y - 15 + t), (49, y - 13 + t)], FIRE2)   # 뿜는 연기
    b.px(52, y - 14 + t, FIRE1)
    return fin(b, [(24, y - 18, 18, C(238, 214, 170))])


# ============================================================ 39 첨성대
def b_stones():
    b = buried()
    matrix(b, 49)
    # 다듬은 돌 세 덩이 — 쌓았던 자국이 남아 있다
    for (x, y, w, h) in ((5, 8, 11, 8), (17, 6, 10, 7), (9, 19, 13, 8)):
        b.rect(x, y, x + w, y + h, STONE2)
        b.rect(x + 1, y + 1, x + w - 2, y + h - 3, STONE1)
        b.rect(x, y + h - 1, x + w, y + h, STONE3)
        for k in range(3):
            b.px(x + 2 + k * 3, y + 2, STONE3)
    crack(b, [(7, 12), (14, 10)], MTX3)
    grime(b, 49, 9)
    return fin(b)


def a_observatory(f):
    b = alive(); g = b.ground
    t = bob(f, 0, 0)
    # 기단
    b.rect(6, g - 5, 42, g, STONE3)
    b.rect(6, g - 5, 42, g - 4, STONE2)
    b.rect(9, g - 8, 39, g - 5, STONE2)
    b.rect(9, g - 8, 39, g - 7, STONE1)
    # 병 모양 몸통 — 27단 돌
    for k in range(38):
        yy = g - 9 - k
        tn = k / 37.0
        half = int(8 + 7 * math.cos(tn * math.pi / 2))           # 아래가 넓은 병 모양
        c = STONE2 if k % 2 else STONE1
        b.rect(24 - half, yy, 24 + half, yy, c)
        b.px(24 - half, yy, STONE3)
        b.px(24 + half, yy, STONE3)
        if k % 3 == 1:                                            # 돌 이음매
            for x in range(24 - half + 2, 24 + half - 1, 5):
                b.px(x, yy, STONE3)
    # 가운데 창
    b.rect(21, g - 26, 27, g - 20, STONE3)
    b.rect(22, g - 25, 26, g - 21, C(40, 36, 40))
    b.rect(20, g - 27, 28, g - 26, STONE1)
    # 정자석 (井)
    b.rect(14, g - 48, 34, g - 46, STONE1)
    b.rect(14, g - 48, 34, g - 47, C(196, 192, 182))
    b.rect(18, g - 50, 20, g - 45, STONE2)
    b.rect(28, g - 50, 30, g - 45, STONE2)
    # 별
    for (x, yy) in ((10, g - 54), (38, g - 50), (24, g - 57)):
        b.px(x, yy + wig(f, 1), C(255, 250, 210))
        b.px(x - 1, yy + 1, C(200, 206, 230))
        b.px(x + 1, yy + 1, C(200, 206, 230))
    return fin(b)


# ============================================================ 40 에밀레종
def b_bellshard():
    b = buried()
    matrix(b, 50)
    # 종 파편 — 비천상 무늬가 반쯤 남았다
    b.poly([(6, 8), (24, 5), (27, 24), (10, 27)], BRZ2)
    b.poly([(9, 10), (22, 8), (24, 21), (12, 24)], BRZ1)
    b.line(8, 14, 26, 12, BRZ3)
    b.line(9, 20, 26, 18, BRZ3)
    b.poly([(14, 14), (18, 13), (16, 18), (19, 19), (13, 19)], C(228, 206, 150))  # 비천상
    b.poly([(19, 13), (23, 11), (22, 16)], C(228, 206, 150))
    for (x, y) in ((11, 12), (21, 22), (16, 24)):
        b.ellipse(x, y, x + 2, y + 2, VERD2)
    crack(b, [(11, 9), (17, 16), (13, 26)], MTX3)
    grime(b, 50, 9)
    return fin(b)


def a_bell(f):
    b = alive(); g = b.ground
    sway = wig(f, 1)
    # 종각 — 기둥 둘과 가로보만. 종이 주인공이라 최대한 가볍게 짠다
    for x in (2, 44):
        b.rect(x, g - 50, x + 3, g, WOOD2)
        b.rect(x, g - 50, x + 1, g, WOOD1)
        b.rect(x - 2, g - 3, x + 5, g, WOOD3)
    b.rect(0, g - 54, 48, g - 50, WOOD3)
    b.rect(0, g - 54, 48, g - 53, WOOD2)
    b.rect(0, g - 57, 48, g - 55, C(112, 96, 84))
    # 종 — 위가 좁고 아래가 넓게 퍼지는 한국 종
    top = g - 46
    H = 38
    for k in range(H):
        tn = k / (H - 1.0)
        half = int(7 + 10 * (tn ** 1.7))
        yy = top + k
        b.rect(24 - half + sway, yy, 24 + half + sway, yy, BRZ2)
        b.rect(24 - half + 1 + sway, yy, 24 - half + 4 + sway, yy, BRZ1)
        b.px(24 + half - 1 + sway, yy, BRZ3)
    b.rect(24 - 17 + sway, g - 10, 24 + 17 + sway, g - 8, BRZ3)       # 아랫단
    b.rect(24 - 17 + sway, g - 10, 24 + 17 + sway, g - 9, BRZ1)
    b.rect(24 - 8 + sway, top + 5, 24 + 8 + sway, top + 6, BRZ3)      # 상대
    b.rect(24 - 12 + sway, top + 20, 24 + 12 + sway, top + 21, BRZ3)  # 하대
    # 비천상 + 당좌
    b.poly([(20 + sway, top + 11), (24 + sway, top + 9), (22 + sway, top + 15),
            (26 + sway, top + 16), (18 + sway, top + 16)], C(224, 200, 142))
    b.ellipse(18 + sway, top + 24, 30 + sway, top + 32, BRZ3)
    b.ellipse(20 + sway, top + 26, 28 + sway, top + 30, C(216, 186, 118))
    b.px(24 + sway, top + 28, C(240, 218, 160))
    # 용뉴 + 음통
    b.poly([(20 + sway, top - 7), (24 + sway, top - 11), (28 + sway, top - 7),
            (28 + sway, top), (20 + sway, top)], BRZ2)
    b.rect(21 + sway, top - 9, 25 + sway, top - 7, BRZ1)
    b.rect(29 + sway, top - 9, 31 + sway, top, BRZ1)
    b.px(26 + sway, top - 8, DARK)
    # 당목 + 울림
    b.rect(38, g - 22, 46, g - 19, WOOD2)
    b.rect(38, g - 22, 46, g - 21, WOOD1)
    if f % 2 == 1:
        for k in range(3):
            b.px(4, g - 26 - k * 4, C(238, 216, 158))
            b.px(44, g - 30 - k * 4, C(238, 216, 158))
    return fin(b, [(24, g - 24, 22, C(246, 224, 160))])


# ============================================================
ENTRIES = [
    ("tou", "신라 토우", b_tou, a_tou),
    ("celadon", "고려청자", b_shard, a_celadon),
    ("moonjar", "백자 달항아리", b_white, a_moonjar),
    ("sundial", "앙부일구", b_bowl, a_sundial),
    ("raingauge", "측우기", b_cyl, a_raingauge),
    ("dagger", "비파형 동검", b_blade, a_dagger),
    ("crown", "금관", b_gold, a_crown),
    ("turtleship", "거북선", b_plank, a_turtleship),
    ("observatory", "첨성대", b_stones, a_observatory),
    ("bell", "에밀레종", b_bellshard, a_bell),
]

if __name__ == "__main__":
    import os
    os.makedirs("out/Relic", exist_ok=True)
    bs, al = build_sheets(ENTRIES, "d")
    bs.save("out/Relic/relic_buried_d.png")
    al.save("out/Relic/relic_alive_d.png")
    print("ok", bs.size, al.size)
