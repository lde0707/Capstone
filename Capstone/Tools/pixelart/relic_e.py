"""E. 근현대 물건 10종 — 매립지에 진짜로 묻혀 있을 법한 것들. 대부분 전시형."""
import math
from relic_lib import *

PL1, PL2, PL3 = C(206, 206, 210), C(156, 158, 166), C(104, 106, 116)      # 낡은 플라스틱
CRM1, CRM2, CRM3 = C(232, 222, 192), C(192, 180, 148), C(140, 128, 100)   # 아이보리 껍데기
GRN1, GRN2 = C(120, 214, 150), C(64, 156, 104)                            # 액정 초록
RED1, RED2, RED3 = C(214, 88, 78), C(160, 54, 50), C(106, 32, 32)         # 도장·양철
BLU1, BLU2 = C(120, 170, 224), C(72, 116, 174)
TIN1, TIN2, TIN3 = C(212, 200, 172), C(158, 146, 122), C(104, 94, 76)     # 양철
SCR1, SCR2 = C(150, 166, 176), C(88, 104, 118)                            # 브라운관


def plinth(b, half=12, h=5):
    pedestal(b, half, h)
    return b.ground - h


def screen_noise(b, x0, y0, x1, y1, frame, c1, c2):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            if (x * 7 + y * 13 + frame * 11) % 9 < 2:
                b.px(x, y, c1)
            elif (x * 3 + y * 5 + frame * 7) % 11 == 0:
                b.px(x, y, c2)


# ============================================================ 41 삐삐
def b_pager():
    b = buried()
    matrix(b, 51)
    b.poly([(8, 9), (22, 7), (24, 20), (10, 23)], PL2)
    b.poly([(10, 11), (20, 9), (21, 17), (12, 19)], PL1)
    b.rect(12, 12, 19, 15, C(72, 92, 80))                     # 꺼진 액정
    b.rect(12, 12, 19, 12, C(52, 68, 60))
    for k in range(3):
        b.rect(12 + k * 3, 17, 13 + k * 3, 18, PL3)
    b.rect(20, 21, 25, 25, MTX1)                              # 떨어져 나간 조각
    crack(b, [(11, 10), (17, 16), (13, 22)], PL3)
    grime(b, 51, 8)
    return fin(b)


def a_pager(f):
    b = alive(); g = b.ground
    y = plinth(b, 9, 4)
    b.rect(15, y - 26, 33, y, PL2)                             # 몸체
    b.rect(16, y - 26, 26, y - 1, PL1)
    b.rect(15, y - 26, 33, y - 25, PL3)
    b.rect(18, y - 23, 30, y - 15, C(96, 128, 106))            # 액정
    b.rect(18, y - 23, 30, y - 22, C(64, 88, 74))
    for k in range(5):                                         # 숫자
        b.rect(20 + k * 2, y - 21, 20 + k * 2, y - 17, GRN1 if (k + f) % 2 else GRN2)
    for (x, yy) in ((18, y - 12), (23, y - 12), (28, y - 12), (18, y - 8), (23, y - 8), (28, y - 8)):
        b.rect(x, yy, x + 3, yy + 2, PL1)
        b.rect(x, yy + 2, x + 3, yy + 2, PL3)
    b.rect(20, y - 28, 22, y - 26, PL3)                        # 클립
    if f % 2 == 1:
        for k in range(3):
            b.px(11 - k, y - 24 - k * 2, C(246, 232, 160))
            b.px(37 + k, y - 24 - k * 2, C(246, 232, 160))
    return fin(b, [(24, y - 20, 12, C(180, 246, 200))])


# ============================================================ 42 다이얼 전화기
def b_handset():
    b = buried()
    matrix(b, 52)
    # 부서진 수화기 — 양끝이 굵은 바 모양
    b.poly([(5, 14), (11, 10), (21, 12), (26, 9), (27, 15), (21, 17), (11, 16), (6, 20)], RED2)
    b.ellipse(3, 12, 11, 21, RED2)
    b.ellipse(5, 14, 9, 19, RED3)
    b.ellipse(22, 7, 29, 16, RED2)
    b.ellipse(24, 9, 28, 14, RED3)
    b.poly([(8, 16), (18, 14), (18, 16), (9, 18)], RED1)
    for k in range(4):                                         # 끊긴 전화선
        b.px(6 + k * 2, 23 + (k % 2), PL3)
    crack(b, [(12, 12), (19, 15)], DARK)
    grime(b, 52, 8)
    return fin(b)


def a_phone(f):
    b = alive(); g = b.ground
    y = plinth(b, 13, 5)
    ring = wig(f, 1)
    # 본체 — 아래가 넓은 사다리꼴
    b.poly([(11 + ring, y - 16), (37 + ring, y - 16), (39 + ring, y), (9 + ring, y)], RED2)
    b.poly([(13 + ring, y - 15), (35 + ring, y - 15), (36 + ring, y - 12), (12 + ring, y - 12)], RED1)
    b.rect(9 + ring, y - 2, 39 + ring, y, RED3)
    # 다이얼 — 손가락 구멍 열 개가 도는 원판
    dx, dy = 24 + ring, y - 8
    b.ellipse(dx - 10, dy - 7, dx + 10, dy + 7, RED3)
    b.ellipse(dx - 9, dy - 6, dx + 9, dy + 6, C(236, 232, 226))
    b.ellipse(dx - 8, dy - 5, dx + 6, dy + 3, C(252, 250, 246))
    for k in range(10):
        ang = math.radians(200 + k * 30 + f * 9)
        hx = dx + int(7 * math.cos(ang))
        hy = dy + int(5 * math.sin(ang))
        b.ellipse(hx - 1, hy - 1, hx + 1, hy + 1, C(58, 50, 50))
        b.px(hx, hy, C(120, 108, 108))
    b.ellipse(dx - 3, dy - 2, dx + 3, dy + 2, RED2)
    b.ellipse(dx - 2, dy - 1, dx + 1, dy + 1, RED1)
    # 수화기 — 본체 위에 가로로 얹혀 있다
    b.rect(12 + ring, y - 22, 36 + ring, y - 18, RED2)
    b.rect(13 + ring, y - 22, 34 + ring, y - 21, RED1)
    for (ex) in (9, 33):
        b.ellipse(ex + ring, y - 25, ex + 7 + ring, y - 16, RED2)
        b.ellipse(ex + 1 + ring, y - 24, ex + 6 + ring, y - 17, RED1)
        b.ellipse(ex + 2 + ring, y - 23, ex + 5 + ring, y - 19, RED3)
    b.rect(20 + ring, y - 24, 28 + ring, y - 22, RED1)            # 손잡이 잘록한 부분
    # 꼬인 전화선
    for k in range(10):
        b.px(40 + (k % 2) * 2, y - 17 + k, C(120, 40, 40))
        b.px(41 + (k % 2) * 2, y - 17 + k, C(160, 60, 58))
    if f % 2 == 1:
        for k in range(3):
            b.px(4, y - 26 - k * 3, C(250, 224, 150))
            b.px(44, y - 28 - k * 3, C(250, 224, 150))
    return fin(b, [(24, y - 14, 14, C(250, 190, 170))])


# ============================================================ 43 자전거
def b_frame():
    b = buried()
    matrix(b, 53)
    # 휘어진 프레임 — 삼각 구조가 찌그러졌다
    b.line(5, 24, 16, 10, RUST2); b.line(6, 24, 17, 10, RUST1)
    b.line(16, 10, 26, 18, RUST2); b.line(16, 11, 26, 19, RUST1)
    b.line(5, 24, 24, 22, RUST2); b.line(5, 25, 24, 23, RUST1)
    b.line(16, 10, 13, 24, RUST2)
    b.ellipse(3, 22, 8, 27, RUST2)                             # 크랭크
    b.ellipse(4, 23, 7, 26, RUST1)
    b.rect(14, 6, 19, 9, RUST2)                                # 안장 자리
    for (x, y) in ((10, 17), (20, 15), (8, 25)):
        b.px(x, y, C(88, 44, 22))
    grime(b, 53, 9)
    return fin(b)


def a_bicycle(f):
    b = alive(); g = b.ground
    t = bob(f, 0, 0)
    fr1, fr2 = C(72, 104, 88), C(44, 72, 62)
    # 바퀴
    for cx2 in (11, 37):
        b.ellipse(cx2 - 10, g - 20, cx2 + 10, g, DARK)
        b.ellipse(cx2 - 8, g - 18, cx2 + 8, g - 2, C(96, 100, 108))
        b.ellipse(cx2 - 6, g - 16, cx2 + 6, g - 4, (0, 0, 0, 0))
        for k in range(6):                                      # 살
            ang = math.radians(k * 30 + f * 15)
            b.line(cx2, g - 10, cx2 + int(7 * math.cos(ang)), g - 10 + int(7 * math.sin(ang)),
                   C(180, 184, 192))
        b.ellipse(cx2 - 2, g - 12, cx2 + 2, g - 8, C(150, 156, 166))
    # 프레임
    b.line(11, g - 10, 22, g - 26, fr1); b.line(12, g - 10, 23, g - 26, fr2)
    b.line(22, g - 26, 33, g - 24, fr1); b.line(22, g - 25, 33, g - 23, fr2)
    b.line(11, g - 10, 30, g - 12, fr1); b.line(11, g - 9, 30, g - 11, fr2)
    b.line(30, g - 12, 33, g - 24, fr1)
    b.line(37, g - 10, 34, g - 28, fr1); b.line(38, g - 10, 35, g - 28, fr2)
    # 안장 / 핸들
    b.rect(18, g - 30, 27, g - 27, C(60, 48, 44))
    b.rect(18, g - 30, 25, g - 29, C(92, 74, 66))
    b.rect(31, g - 32, 40, g - 30, C(60, 48, 44))
    b.rect(34, g - 30, 36, g - 28, fr2)
    # 앞 짐받이
    b.rect(38, g - 28, 48, g - 26, C(148, 152, 160))
    b.rect(38, g - 26, 40, g - 20, C(148, 152, 160))
    b.rect(46, g - 26, 48, g - 20, C(148, 152, 160))
    b.rect(39, g - 32, 47, g - 28, C(174, 138, 92))            # 나무 상자
    b.rect(39, g - 32, 47, g - 31, C(202, 168, 120))
    # 페달
    px_ = swing(f, 30, 28)
    b.ellipse(28, g - 14, 32, g - 10, C(150, 156, 166))
    b.rect(px_ - 2, g - 8, px_ + 2, g - 7, DARK)
    return fin(b)


# ============================================================ 44 재봉틀
def b_pedal():
    b = buried()
    matrix(b, 54)
    # 녹슨 무쇠 발판 — 덩굴 무늬 주물
    b.poly([(4, 12), (27, 9), (28, 21), (5, 24)], RUST2)
    b.poly([(7, 14), (25, 11), (25, 19), (7, 22)], MTX1)
    for k in range(5):                                         # 주물 살
        b.line(8 + k * 4, 13, 10 + k * 4, 22, RUST1)
    b.line(6, 18, 27, 15, RUST1)
    b.ellipse(3, 10, 8, 16, RUST2)
    b.ellipse(24, 17, 29, 23, RUST2)
    for (x, y) in ((11, 16), (20, 13), (16, 21)):
        b.px(x, y, C(88, 44, 22)); b.px(x + 1, y, C(126, 68, 34))
    grime(b, 54, 9)
    return fin(b)


def a_sewing(f):
    b = alive(); g = b.ground
    wd1, wd2, wd3 = C(168, 122, 78), C(122, 84, 52), C(82, 54, 34)
    t = bob(f, 0, 0)
    # 무쇠 다리 + 발판
    for x in (8, 38):
        b.rect(x, g - 22, x + 2, g, C(66, 62, 60))
        b.rect(x - 2, g - 2, x + 5, g, C(48, 44, 44))
    for k in range(7):
        b.line(10, g - 20 + k * 2, 40, g - 8 + k, C(78, 74, 72))
    b.rect(12, g - 8, 36, g - 5, C(58, 54, 52))                 # 발판
    b.rect(12, g - 8, 36, g - 7, C(92, 88, 84))
    b.ellipse(34, g - 22, 44, g - 12, C(78, 74, 72))            # 큰 바퀴
    b.ellipse(36, g - 20, 42, g - 14, C(48, 44, 44))
    b.line(39, g - 17, 30, g - 7 + swing(f, 0, 3), C(110, 104, 100))
    # 나무 상판
    b.rect(5, g - 27, 43, g - 22, wd2)
    b.rect(5, g - 27, 43, g - 26, wd1)
    b.rect(5, g - 23, 43, g - 22, wd3)
    # 재봉틀 머리 — 검은 몸체에 금색 무늬
    b.poly([(11, g - 34), (33, g - 34), (33, g - 27), (11, g - 27)], C(44, 42, 44))
    b.poly([(12, g - 33), (31, g - 33), (31, g - 30), (12, g - 30)], C(70, 66, 70))
    b.rect(11, g - 45, 17, g - 34, C(44, 42, 44))               # 세로 기둥
    b.rect(12, g - 45, 14, g - 34, C(70, 66, 70))
    b.rect(11, g - 48, 36, g - 45, C(44, 42, 44))               # 팔
    b.rect(12, g - 48, 34, g - 47, C(70, 66, 70))
    b.rect(32, g - 45, 36, g - 38, C(44, 42, 44))               # 바늘대
    b.rect(33, g - 38, 34, g - 34 + cyc(f, 0, 2, 3, 1), IRON1)
    b.ellipse(14, g - 52, 20, g - 47, C(60, 56, 58))            # 실패
    b.rect(16, g - 56, 18, g - 51, C(214, 96, 104))
    for (x, yy) in ((15, g - 32), (22, g - 31), (28, g - 32)):  # 금장식
        b.px(x, yy, GOLD1); b.px(x + 1, yy + 1, GOLD2)
    b.line(34, g - 34, 34, g - 27, C(228, 226, 220))            # 실
    return fin(b)


# ============================================================ 45 태엽 로봇 장난감
def b_spring():
    b = buried()
    matrix(b, 55)
    # 녹슨 태엽 뭉치
    for k in range(22):
        ang = math.radians(k * 52)
        r = 2 + k * 0.42
        x = 15 + int(r * math.cos(ang))
        y = 17 + int(r * math.sin(ang))
        b.px(x, y, RUST1 if k % 2 else RUST2)
        b.px(x + 1, y, RUST2)
        b.px(x, y + 1, RUST2)
    b.rect(14, 16, 17, 19, IRON3)
    b.rect(21, 5, 24, 11, RUST2)                               # 부러진 태엽 손잡이
    b.rect(21, 5, 22, 11, RUST1)
    for (x, y) in ((8, 22), (23, 20)):
        b.px(x, y, C(88, 44, 22))
    grime(b, 55, 8)
    return fin(b)


def a_robot(f):
    b = alive(); g = b.ground
    lean = wig(f, 1)
    t = bob(f, 0, -1)
    # 다리 — 뒤뚱
    b.rect(17 + lean, g - 9, 22 + lean, g, TIN2)
    b.rect(26 - lean, g - 9, 31 - lean, g, TIN1)
    b.rect(15 + lean, g - 3, 23 + lean, g, TIN3)
    b.rect(25 - lean, g - 3, 33 - lean, g, TIN3)
    # 몸통
    b.rect(13 + lean, g - 30 + t, 35 + lean, g - 9 + t, TIN2)
    b.rect(14 + lean, g - 29 + t, 26 + lean, g - 11 + t, TIN1)
    b.rect(13 + lean, g - 30 + t, 35 + lean, g - 29 + t, TIN3)
    b.rect(18 + lean, g - 25 + t, 30 + lean, g - 16 + t, RED2)    # 가슴판
    b.rect(18 + lean, g - 25 + t, 30 + lean, g - 24 + t, RED1)
    for k in range(3):
        b.rect(20 + k * 4 + lean, g - 22 + t, 22 + k * 4 + lean, g - 20 + t,
               GRN1 if (k + f) % 2 else GRN2)
    b.rect(20 + lean, g - 14 + t, 28 + lean, g - 12 + t, TIN3)
    # 팔
    b.rect(6 + lean, g - 28 + t, 13 + lean, g - 24 + t, TIN2)
    b.rect(6 + lean, g - 24 + t, 9 + lean, g - 16 + t, TIN1)
    b.rect(35 + lean, g - 28 + t, 42 + lean, g - 24 + t, TIN2)
    b.rect(39 + lean, g - 24 + t, 42 + lean, g - 16 + t, TIN1)
    # 머리
    b.rect(17 + lean, g - 44 + t, 31 + lean, g - 30 + t, TIN2)
    b.rect(18 + lean, g - 43 + t, 26 + lean, g - 32 + t, TIN1)
    b.rect(17 + lean, g - 44 + t, 31 + lean, g - 43 + t, TIN3)
    eyes(b, 19 + lean, g - 40 + t, 7)
    b.rect(21 + lean, g - 34 + t, 27 + lean, g - 33 + t, TIN3)
    b.rect(23 + lean, g - 48 + t, 25 + lean, g - 44 + t, IRON2)   # 안테나
    b.ellipse(22 + lean, g - 51 + t, 26 + lean, g - 47 + t, RED1)
    # 등 태엽
    kx, ky = 38 + lean, g - 18 + t
    b.rect(35 + lean, ky - 1, kx, ky + 1, IRON2)                  # 태엽 축
    # 태엽 열쇠 — 프레임마다 45도씩 돈다
    ang = math.radians(f * 45)
    ex, ey = int(6 * math.cos(ang)), int(4 * math.sin(ang))
    b.line(kx - ex, ky - ey, kx + ex, ky + ey, IRON1)
    for (hx2, hy2) in ((kx + ex, ky + ey), (kx - ex, ky - ey)):
        b.rect(hx2 - 1, hy2 - 1, hx2 + 1, hy2 + 1, IRON2)
    b.ellipse(kx - 2, ky - 2, kx + 2, ky + 2, IRON3)
    return fin(b)


# ============================================================ 46 흑백 TV
def b_crt():
    b = buried()
    matrix(b, 56)
    # 깨진 브라운관 — 둥근 유리에 방사형 금
    b.ellipse(4, 8, 28, 26, C(86, 96, 104))
    b.ellipse(6, 10, 26, 24, C(52, 62, 70))
    b.ellipse(8, 11, 20, 19, C(112, 126, 136))
    for ang in (20, 80, 150, 215, 300):
        a = math.radians(ang)
        b.line(16, 17, 16 + int(12 * math.cos(a)), 17 + int(9 * math.sin(a)), C(214, 226, 234))
    b.poly([(16, 17), (26, 12), (27, 20)], C(38, 46, 54))       # 떨어져 나간 유리
    b.rect(5, 25, 12, 28, PL3)                                   # 껍데기 조각
    grime(b, 56, 9)
    return fin(b)


def a_tv(f):
    b = alive(); g = b.ground
    wd1, wd2, wd3 = C(164, 118, 74), C(118, 82, 50), C(78, 52, 32)
    # 다리
    for x in (11, 33):
        b.rect(x, g - 6, x + 4, g, wd3)
        b.rect(x + 1, g - 6, x + 2, g, wd2)
    # 나무 캐비닛
    b.rect(5, g - 38, 43, g - 5, wd2)
    b.rect(5, g - 38, 43, g - 36, wd1)
    b.rect(5, g - 7, 43, g - 5, wd3)
    for k in range(4):                                           # 나뭇결
        b.line(7, g - 34 + k * 8, 41, g - 33 + k * 8, wd3)
    # 화면
    b.rect(8, g - 35, 32, g - 12, C(40, 44, 50))
    b.ellipse(8, g - 35, 32, g - 12, SCR2)
    b.ellipse(10, g - 33, 30, g - 14, C(52, 58, 66))
    screen_noise(b, 11, g - 32, 29, g - 15, f, C(206, 214, 220), SCR1)
    for k in range(4):                                           # 주사선
        b.rect(10, g - 31 + k * 5 + f, 30, g - 31 + k * 5 + f, C(70, 78, 86))
    b.rect(9, g - 34, 31, g - 33, C(132, 146, 156))
    # 다이얼 두 개 + 스피커
    for yy in (g - 32, g - 24):
        b.ellipse(35, yy, 41, yy + 6, C(58, 50, 44))
        b.ellipse(36, yy + 1, 40, yy + 5, C(198, 190, 172))
        b.rect(38, yy + 1, 38, yy + 3, C(58, 50, 44))
    for k in range(4):
        b.rect(35, g - 15 + k * 2, 41, g - 15 + k * 2, wd3)
    b.rect(20, g - 44, 22, g - 38, IRON2)                        # 안테나
    b.line(21, g - 44, 12, g - 54, IRON1)
    b.line(21, g - 44, 32, g - 52, IRON1)
    return fin(b, [(20, g - 24, 16, C(190, 214, 230))])


# ============================================================ 47 오락실 기판
def b_board():
    b = buried()
    matrix(b, 57)
    # 부식된 회로 기판
    b.poly([(4, 9), (27, 7), (28, 23), (5, 25)], C(58, 96, 72))
    b.poly([(6, 11), (25, 9), (26, 21), (7, 23)], C(78, 124, 92))
    for k in range(6):                                           # 배선
        b.line(7, 13 + k * 2, 24, 12 + k * 2, C(158, 178, 120))
    b.rect(10, 12, 17, 17, C(40, 38, 42))                        # 칩
    b.rect(10, 12, 17, 13, C(70, 66, 70))
    for k in range(4):
        b.px(9, 13 + k, IRON2); b.px(18, 13 + k, IRON2)
    b.rect(20, 18, 24, 21, C(40, 38, 42))
    for (x, y) in ((8, 20), (22, 11), (14, 22)):                 # 부식
        b.ellipse(x, y, x + 2, y + 2, VERD_ := C(120, 176, 148))
    b.rect(24, 22, 28, 26, MTX1)
    grime(b, 57, 8)
    return fin(b)


def a_arcade(f):
    b = alive(); g = b.ground
    c1, c2, c3 = C(70, 96, 176), C(44, 62, 128), C(28, 38, 84)
    # 캐비닛
    b.poly([(11, g - 52), (37, g - 52), (37, g), (11, g)], c2)
    b.poly([(13, g - 50), (26, g - 50), (26, g - 2), (13, g - 2)], c1)
    b.rect(9, g - 2, 39, g, c3)
    b.poly([(11, g - 30), (37, g - 30), (37, g - 22), (11, g - 22)], c3)   # 조작대
    b.poly([(11, g - 30), (37, g - 30), (37, g - 28), (11, g - 28)], c1)
    # 화면
    b.rect(13, g - 49, 35, g - 32, C(24, 22, 30))
    b.rect(15, g - 47, 33, g - 34, C(16, 16, 24))
    for k in range(9):                                            # 픽셀 게임 화면
        x = 16 + ((k * 5 + f * 3) % 17)
        yy = g - 46 + ((k * 3) % 11)
        b.rect(x, yy, x + 1, yy + 1, (GRN1, C(240, 220, 100), C(230, 110, 120))[k % 3])
    b.rect(18, g - 36, 22, g - 35, C(120, 200, 240))              # 플레이어
    b.rect(19, g - 38, 21, g - 36, C(240, 220, 100))
    b.rect(13, g - 49, 35, g - 48, C(120, 140, 200))
    # 조이스틱 + 버튼
    b.rect(17, g - 33, 18, g - 29, C(48, 44, 48))
    b.ellipse(15 + wig(f, 1), g - 36, 20 + wig(f, 1), g - 32, RED1)
    for (x, c) in ((25, C(240, 200, 90)), (30, GRN1)):
        b.ellipse(x, g - 28, x + 4, g - 25, c)
        b.px(x + 1, g - 27, C(255, 255, 240))
    # 마퀴
    b.rect(9, g - 58, 39, g - 52, C(236, 232, 226))
    b.rect(9, g - 58, 39, g - 57, C(180, 176, 172))
    for k in range(6):
        b.rect(12 + k * 4, g - 56, 14 + k * 4, g - 54,
               (RED1, C(240, 200, 90), GRN1, C(120, 200, 240))[k % 4])
    b.rect(10, g - 20, 38, g - 19, c3)
    return fin(b, [(24, g - 40, 18, C(150, 190, 250))])


# ============================================================ 48 주크박스
def b_cab():
    b = buried()
    matrix(b, 58)
    # 찌그러진 캐비닛 모서리 — 둥근 아치와 크롬 띠
    b.poly([(6, 10), (24, 7), (26, 25), (8, 27)], C(126, 74, 56))
    b.poly([(8, 12), (22, 9), (23, 22), (10, 24)], C(168, 104, 76))
    for k in range(9):                                            # 아치
        x = 9 + k
        yy = 12 + abs(k - 4) // 2
        b.px(x, yy, C(216, 200, 160)); b.px(x, yy + 1, C(160, 146, 116))
    b.rect(10, 17, 22, 19, C(96, 54, 42))
    b.rect(11, 21, 21, 22, C(206, 190, 150))
    b.rect(22, 23, 27, 27, MTX1)
    crack(b, [(9, 11), (16, 18), (12, 26)], DARK)
    grime(b, 58, 8)
    return fin(b)


def a_jukebox(f):
    b = alive(); g = b.ground
    w1, w2, w3 = C(186, 116, 82), C(140, 78, 56), C(94, 48, 34)
    chr1, chr2 = C(232, 222, 190), C(168, 158, 132)
    # 몸체
    b.rect(9, g - 34, 39, g, w2)
    b.rect(11, g - 32, 22, g - 2, w1)
    b.rect(7, g - 3, 41, g, w3)
    # 둥근 돔
    for k in range(18):
        tn = k / 17.0
        half = int(15 * math.sin(math.acos(tn * 0.95)))
        yy = g - 35 - k
        b.rect(24 - half, yy, 24 + half, yy, w2)
        b.rect(24 - half + 1, yy, 24 - half + 3, yy, w1)
    # 크롬 아치 두 줄
    for off in (0, 2):
        for k in range(31):
            x = 9 + k
            tn = abs(x - 24) / 15.0
            yy = g - 35 - int(17 * math.sqrt(max(0.0, 1 - tn * tn))) + off
            b.px(x, yy, chr1 if off == 0 else chr2)
    # 유리창 + 레코드
    b.rect(13, g - 44, 35, g - 30, C(40, 34, 40))
    b.rect(15, g - 42, 33, g - 32, C(58, 50, 58))
    b.ellipse(17, g - 41, 27, g - 33, C(28, 26, 30))
    b.ellipse(20, g - 39, 24, g - 35, C(214, 96, 104))
    b.px(22, g - 37, C(58, 50, 58))
    for k in range(4):                                             # 음표
        b.px(30, g - 40 + k * 2, chr1)
    b.rect(29, g - 34, 33, g - 33, chr1)
    # 색색 튜브 (프레임마다 색이 돈다)
    cols = [RED1, C(240, 200, 90), GRN1, C(120, 200, 240), C(200, 140, 230)]
    for k in range(5):
        c = cols[(k + f) % 5]
        b.rect(11, g - 28 + k * 2, 13, g - 28 + k * 2, c)
        b.rect(35, g - 28 + k * 2, 37, g - 28 + k * 2, c)
    # 선곡 버튼 + 스피커
    b.rect(15, g - 27, 33, g - 21, C(52, 46, 48))
    for k in range(6):
        b.rect(16 + k * 3, g - 26, 17 + k * 3, g - 24, chr2)
    for k in range(5):
        b.rect(14, g - 18 + k * 3, 34, g - 18 + k * 3, w3)
    if f % 2 == 1:
        b.px(5, g - 44, chr1); b.px(43, g - 46, chr1)
    return fin(b, [(24, g - 34, 22, C(255, 214, 170))])


# ============================================================ 49 영사기
def b_lens():
    b = buried()
    matrix(b, 59)
    # 금 간 렌즈 — 동심원 유리 + 황동 테
    b.ellipse(5, 8, 27, 26, C(142, 112, 62))
    b.ellipse(7, 10, 25, 24, C(198, 164, 96))
    b.ellipse(9, 12, 23, 22, C(64, 78, 92))
    b.ellipse(11, 13, 21, 20, C(104, 126, 142))
    b.ellipse(14, 15, 19, 19, C(178, 202, 214))
    crack(b, [(10, 13), (16, 18), (14, 23)], C(230, 240, 246))
    crack(b, [(22, 12), (17, 17)], C(230, 240, 246))
    b.rect(4, 22, 10, 27, MTX1)
    grime(b, 59, 8)
    return fin(b)


def a_projector(f):
    b = alive(); g = b.ground
    m1, m2, m3 = C(92, 90, 96), C(60, 58, 66), C(38, 36, 44)
    y = plinth(b, 13, 5)
    # 삼각대
    b.rect(23, y - 12, 26, y, m2)
    b.rect(16, y - 2, 33, y, m3)
    # 본체
    b.rect(9, y - 26, 31, y - 12, m2)
    b.rect(10, y - 25, 21, y - 14, m1)
    b.rect(9, y - 26, 31, y - 25, m3)
    # 필름 릴 두 개
    for (cx2, cy2, r) in ((14, y - 34, 8), (28, y - 31, 6)):
        b.ellipse(cx2 - r, cy2 - r, cx2 + r, cy2 + r, m3)
        b.ellipse(cx2 - r + 1, cy2 - r + 1, cx2 + r - 1, cy2 + r - 1, m1)
        for k in range(5):
            ang = math.radians(k * 72 + f * 18)
            b.px(cx2 + int((r - 3) * math.cos(ang)), cy2 + int((r - 3) * math.sin(ang)), m3)
        b.ellipse(cx2 - 2, cy2 - 2, cx2 + 2, cy2 + 2, C(178, 174, 182))
        b.px(cx2, cy2, m3)
    b.line(14, y - 27, 28, y - 25, m3)                            # 필름
    b.rect(17, y - 30, 25, y - 27, m2)
    # 렌즈 + 쏘는 빛
    b.rect(31, y - 24, 39, y - 17, C(166, 134, 78))
    b.rect(31, y - 24, 39, y - 23, C(206, 172, 104))
    b.ellipse(38, y - 25, 42, y - 16, C(120, 142, 156))
    b.ellipse(39, y - 24, 41, y - 18, C(204, 226, 236))
    for k in range(8):
        yy0 = y - 23 - k // 2
        yy1 = y - 18 + k // 2
        b.rect(42 + k // 2, yy0, 42 + k // 2, yy1, C(250, 244, 210) if (k + f) % 3 else C(224, 216, 180))
    return fin(b, [(40, y - 20, 9, C(255, 250, 220))])


# ============================================================ 50 우주복 헬멧
def b_visor():
    b = buried()
    matrix(b, 60)
    # 금 간 바이저 — 둥근 유리에 금색 코팅이 벗겨졌다
    b.ellipse(4, 7, 28, 27, C(206, 202, 194))
    b.ellipse(6, 9, 26, 25, C(160, 156, 150))
    b.ellipse(8, 11, 24, 22, C(150, 120, 52))
    b.ellipse(10, 12, 22, 20, C(196, 162, 78))
    crack(b, [(9, 13), (16, 17), (13, 22)], C(236, 240, 244))
    crack(b, [(22, 12), (17, 16), (21, 21)], C(236, 240, 244))
    b.rect(20, 23, 27, 27, MTX1)
    for k in range(4):
        b.px(7 + k * 5, 26, C(180, 176, 170))
    grime(b, 60, 8)
    return fin(b)


def a_helmet(f):
    b = alive(); g = b.ground
    s1, s2, s3 = C(238, 238, 242), C(196, 198, 206), C(142, 146, 158)
    y = plinth(b, 12, 5)
    # 목 링
    b.rect(15, y - 8, 33, y, s3)
    b.rect(15, y - 8, 33, y - 7, s2)
    b.rect(13, y - 12, 35, y - 8, IRON2)
    b.rect(13, y - 12, 35, y - 11, IRON1)
    # 헬멧 셸
    b.ellipse(9, y - 42, 39, y - 10, s2)
    b.ellipse(11, y - 40, 31, y - 22, s1)
    b.ellipse(9, y - 42, 39, y - 38, s1)
    # 바이저
    b.ellipse(13, y - 38, 35, y - 16, C(46, 52, 62))
    b.ellipse(15, y - 36, 33, y - 18, C(120, 96, 36))
    b.ellipse(16, y - 35, 31, y - 20, C(196, 160, 72))
    b.ellipse(18, y - 33, 25, y - 26, C(238, 208, 120))
    if f % 2 == 1:                                                # 반사광이 미끄러진다
        gx = 26 if f == 1 else 30
        b.line(gx, y - 34, gx + 4, y - 24, C(252, 238, 190))
    b.rect(12, y - 30, 14, y - 24, s3)                            # 측면 장치
    b.rect(34, y - 30, 36, y - 24, s3)
    b.ellipse(33, y - 40, 39, y - 34, C(214, 96, 96))             # 라이트
    b.px(35, y - 38, C(252, 220, 210))
    b.rect(20, y - 45, 28, y - 41, s3)                            # 상단 커넥터
    b.rect(20, y - 45, 28, y - 44, s2)
    return fin(b, [(24, y - 28, 18, C(255, 236, 180))])


# ============================================================
ENTRIES = [
    ("pager", "삐삐", b_pager, a_pager),
    ("phone", "다이얼 전화기", b_handset, a_phone),
    ("bicycle", "자전거", b_frame, a_bicycle),
    ("sewing", "재봉틀", b_pedal, a_sewing),
    ("tinrobot", "태엽 로봇", b_spring, a_robot),
    ("tv", "흑백 TV", b_crt, a_tv),
    ("arcade", "오락실 기판", b_board, a_arcade),
    ("jukebox", "주크박스", b_cab, a_jukebox),
    ("projector", "영사기", b_lens, a_projector),
    ("helmet", "우주복 헬멧", b_visor, a_helmet),
]

if __name__ == "__main__":
    import os
    os.makedirs("out/Relic", exist_ok=True)
    bs, al = build_sheets(ENTRIES, "e")
    bs.save("out/Relic/relic_buried_e.png")
    al.save("out/Relic/relic_alive_e.png")
    print("ok", bs.size, al.size)
