# -*- coding: utf-8 -*-
"""분리수거 5칸 x 3종 = 15장. 2초 안에 칸이 읽히는 게 유일한 목표다.

칸마다 '실루엣 + 색' 두 축을 모두 다르게 잡았다:
  캔      세로 원통 + 가로 리브 + 컬러 라벨 띠     (라벨이 있다 = 캔)
  알루미늄 라벨 없는 순은색 + 구김/주름           (라벨이 없다 = 알루미늄)
  페트     병 실루엣(좁은 목) + 반투명 하늘빛 + 컬러 뚜껑
  플라스틱 각진 통/상자 + 불투명 원색             (병이 아니다 = 플라스틱)
  종이     납작한 묶음 / 골판지 단면
"""
import os
from recycle_box import Box, T

OUT = "../../Assets/Resources/TrashSprites/Recycle"
LINE = (26, 24, 34, 255)          # 공통 외곽선

# ---- 캔: 푸른 강철 + 컬러 라벨 ----
S_D, S_M, S_L, S_H = (84,96,112,255), (146,160,178,255), (198,210,224,255), (242,248,253,255)
# ---- 알루미늄: 라벨 없는 순은색 (더 밝고 더 차갑다) ----
F_D, F_M, F_L, F_H = (118,128,140,255), (182,194,206,255), (228,238,248,255), (255,255,255,255)
# ---- 페트: 반투명 하늘빛 ----
P_D, P_M, P_L, P_H = (120,168,186,255), (172,214,230,255), (214,240,248,255), (250,254,255,255)
# ---- 종이 ----
K_D, K_M, K_L = (138,100,58,255), (186,146,92,255), (222,196,148,255)
W_D, W_M, W_L = (176,172,160,255), (222,220,212,255), (248,247,242,255)


def save(b, name):
    b.outline(LINE); b.shadow()
    os.makedirs(OUT, exist_ok=True)
    b.image().save(f"{OUT}/{name}.png")
    return name


def cylinder(b, x0, x1, y0, y1, dark, mid, light, hi):
    """세로 원통 + 위아래 타원. 캔 계열 공통 몸통."""
    cx = (x0 + x1) / 2
    rx = (x1 - x0) / 2
    b.rect(x0, y0, x1, y1, mid)
    for x in range(int(x0), int(x1) + 1):
        t = (x - x0) / max(1, x1 - x0)
        if t < 0.18 or t > 0.93: b.rect(x, y0, x, y1, dark)
        elif 0.28 < t < 0.44:    b.rect(x, y0, x, y1, light)
        elif 0.44 <= t < 0.52:   b.rect(x, y0, x, y1, hi)
    b.ellipse(cx, y0, rx, 2.2, light)          # 윗면
    b.ellipse(cx, y0, rx - 1.4, 1.4, mid)
    b.ellipse(cx, y1, rx, 2.0, dark)           # 아랫면


# ===================== 캔 =====================
def can_steel():
    """통조림 캔 — 짧고 뚱뚱 + 빨간 라벨 띠."""
    b = Box()
    cylinder(b, 7, 24, 9, 26, S_D, S_M, S_L, S_H)
    b.rect(7, 14, 24, 21, (186, 58, 54, 255))                 # 라벨
    for x in range(8, 24, 1):
        if (x - 8) % 2 == 0: b.set(x, 17, (228, 112, 96, 255))
    b.rect(7, 13, 24, 13, (120, 34, 34, 255))
    b.rect(7, 22, 24, 22, (120, 34, 34, 255))
    b.rect(11, 15, 12, 20, (248, 226, 200, 255))              # 라벨 하이라이트
    return save(b, "can_steel")


def can_drink():
    """음료 캔 — 길쭉 + 목 좁힘 + 풀탭 + 파란 라벨."""
    b = Box()
    cylinder(b, 10, 22, 6, 28, S_D, S_M, S_L, S_H)
    b.rect(10, 6, 22, 7, S_D)                                  # 넥
    b.rect(11, 5, 21, 6, S_L)
    b.ellipse(16, 5, 5, 1.8, S_H)
    b.rect(15, 4, 18, 5, (96, 110, 126, 255))                  # 풀탭
    b.rect(10, 12, 22, 23, (48, 104, 180, 255))                # 라벨
    b.rect(10, 11, 22, 11, (28, 62, 118, 255))
    b.rect(10, 24, 22, 24, (28, 62, 118, 255))
    b.rect(13, 13, 14, 22, (134, 190, 240, 255))
    for y in (15, 19): b.rect(11, y, 21, y, (150, 200, 244, 255))
    return save(b, "can_drink")


def can_crushed():
    """찌그러진 캔 — 옆으로 누운 원통. 둥근 입구와 가로 리브로 '캔'을 붙잡는다."""
    b = Box()
    # 누운 몸통 (가운데가 움푹)
    b.poly([(6,12),(26,11),(27,23),(5,24)], S_M)
    for x in range(6, 27):
        t = (x - 6) / 20.0
        top = 12 - int(1.5 * (1 - abs(t - .5) * 2))
        bot = 23 + int(1.5 * (1 - abs(t - .5) * 2))
        b.rect(x, top, x, top + 2, S_L)
        b.rect(x, bot - 1, x, bot, S_D)
    b.rect(8, 16, 25, 20, (186, 58, 54, 255))          # 라벨 띠
    b.rect(8, 15, 25, 15, (120, 34, 34, 255))
    b.rect(8, 21, 25, 21, (120, 34, 34, 255))
    for x in range(9, 25, 3): b.set(x, 18, (236, 124, 106, 255))
    for x in (13, 19):                                  # 찌그러진 접힘선
        b.rect(x, 12, x, 23, S_D)
        b.rect(x + 1, 12, x + 1, 23, S_H)
    b.ellipse(26, 17, 2.6, 6.5, S_L)                    # 둥근 입구
    b.ellipse(26, 17, 1.6, 5.0, (52, 58, 70, 255))
    b.ellipse(6, 17, 2.2, 6.0, S_D)                     # 바닥
    return save(b, "can_crushed")


# ===================== 알루미늄 =====================
def foil_ball():
    """구겨진 은박 공 — 각진 면이 제각각 빛난다. 라벨이 없다."""
    b = Box()
    b.ellipse(16, 17, 10, 9, F_M)
    facets = [([(7,16),(12,9),(16,13),(11,20)], F_L),
              ([(12,9),(19,8),(21,14),(16,13)], F_H),
              ([(19,8),(25,15),(21,19),(21,14)], F_M),
              ([(11,20),(16,13),(21,19),(17,25)], F_D),
              ([(17,25),(21,19),(24,22),(20,26)], F_L),
              ([(7,16),(11,20),(17,25),(10,24)], F_M)]
    for pts, c in facets: b.poly(pts, c)
    for (x, y) in ((13,12),(20,11),(15,21),(22,17)): b.set(x, y, F_H)
    b.line(12, 9, 16, 13, F_H); b.line(16, 13, 21, 19, F_H)
    b.line(11, 20, 16, 13, F_D)
    return save(b, "foil_ball")


def foil_tray():
    """은박 접시 — 얕은 타원 그릇. 부드러운 방사형 주름, 라벨은 없다."""
    b = Box()
    b.ellipse(16, 18, 13, 7.5, F_M)                     # 바깥 테
    b.ellipse(16, 18, 11.5, 6.2, F_L)
    b.ellipse(16, 19, 8.5, 4.4, F_D)                    # 움푹한 안쪽
    b.ellipse(16, 19, 7.2, 3.5, F_M)
    import math
    for a in range(0, 360, 30):                         # 방사형 주름 (밝게)
        rad = math.radians(a)
        x0, y0 = 16 + 7.6 * math.cos(rad), 19 + 4.0 * math.sin(rad)
        x1, y1 = 16 + 12.2 * math.cos(rad), 18 + 6.8 * math.sin(rad)
        b.line(x0, y0, x1, y1, F_H if a % 60 == 0 else F_L)
    b.ellipse(16, 16.6, 12.8, 6.4, F_H, fill=False)     # 림 반짝임
    b.rect(12, 12, 20, 12, F_H)
    return save(b, "foil_tray")


def foil_sheet():
    """구겨진 은박 뭉치 — 각진 면마다 밝기가 다르다. 큰 민짜 면을 두지 않는다."""
    b = Box()
    b.poly([(4,13),(11,7),(21,6),(28,12),(26,24),(14,27),(5,22)], F_M)
    facets = [([(4,13),(11,7),(14,14),(7,18)], F_L),
              ([(11,7),(21,6),(19,13),(14,14)], F_H),
              ([(21,6),(28,12),(23,16),(19,13)], F_M),
              ([(14,14),(19,13),(23,16),(18,21)], F_D),
              ([(7,18),(14,14),(18,21),(13,25)], F_L),
              ([(5,22),(7,18),(13,25)], F_M),
              ([(18,21),(23,16),(26,24),(14,27)], F_L),
              ([(23,16),(28,12),(26,24)], F_D)]
    for pts, c in facets: b.poly(pts, c)
    for (x0,y0,x1,y1) in ((11,7,14,14),(14,14,18,21),(19,13,23,16),(7,18,13,25)):
        b.line(x0, y0, x1, y1, F_H)
    for (x, y) in ((16,9),(10,12),(21,19),(13,22)): b.set(x, y, F_H)
    return save(b, "foil_sheet")


# ===================== 페트 =====================
def pet_bottle(b, cap, belly=9, name="pet"):
    b.rect(13, 4, 19, 9, P_M)                                   # 목
    b.rect(14, 4, 15, 9, P_L)
    b.rect(12, 8, 20, 10, P_M)                                  # 어깨 링
    b.poly([(12,10),(20,10),(16+belly-5,14),(16-belly+5,14)], P_M)
    b.rect(16-belly, 14, 16+belly-1, 26, P_M)
    b.rect(16-belly+1, 15, 16-belly+3, 25, P_L)                 # 반사
    b.rect(16-belly+4, 16, 16-belly+4, 24, P_H)
    b.rect(16+belly-3, 15, 16+belly-2, 25, P_D)
    b.poly([(16-belly,26),(16+belly-1,26),(16+belly-3,28),(16-belly+2,28)], P_D)
    b.rect(12, 2, 20, 4, cap)                                   # 뚜껑
    for x in range(12, 21, 2): b.rect(x, 2, x, 4, tuple(min(255,c+40) for c in cap[:3]) + (255,))
    return b


def pet_water():
    b = Box(); pet_bottle(b, (48, 118, 200, 255), 8)
    for y in (17, 21): b.rect(9, y, 22, y, P_D)                 # 몸통 링
    return save(b, "pet_water")


def pet_soda():
    b = Box(); pet_bottle(b, (56, 160, 92, 255), 10)
    b.rect(7, 16, 24, 22, (236, 246, 250, 255))                 # 라벨 자리(투명 강조)
    b.rect(7, 16, 24, 16, P_D); b.rect(7, 22, 24, 22, P_D)
    for x in range(8, 24, 3): b.rect(x, 17, x, 21, (190, 226, 238, 255))
    for x in range(8, 24, 4): b.rect(x, 26, x + 1, 28, P_D)     # 꽃잎 바닥
    return save(b, "pet_soda")


def pet_crushed():
    """압착 페트 — 목과 뚜껑은 그대로, 몸통만 납작하게 접혔다."""
    b = Box()
    b.rect(13, 6, 19, 11, P_M); b.rect(14, 6, 15, 11, P_L)
    b.rect(12, 4, 20, 6, (48, 118, 200, 255))
    for x in range(12, 21, 2): b.rect(x, 4, x, 6, (120, 176, 244, 255))
    b.poly([(6,12),(26,13),(27,20),(5,21)], P_M)                # 납작해진 몸통
    for i, y in enumerate((14, 16, 18)):                        # 접힘 주름
        b.line(6 + i, y, 26 - i, y + 1, P_D)
        b.line(6 + i, y + 1, 26 - i, y + 2, P_H)
    b.poly([(6,12),(13,12),(12,21),(5,21)], P_L)
    b.rect(5, 20, 27, 21, P_D)
    return save(b, "pet_crushed")


# ===================== 플라스틱 =====================
def plastic_tub():
    """반찬통 — 각진 불투명 초록 + 뚜껑."""
    b = Box()
    g_d, g_m, g_l = (36,116,78,255), (72,166,112,255), (134,212,158,255)
    b.poly([(5,12),(27,12),(24,27),(8,27)], g_m)
    b.poly([(5,12),(12,12),(11,27),(8,27)], g_l)
    b.poly([(22,12),(27,12),(24,27),(21,27)], g_d)
    b.rect(3, 8, 29, 12, (178,230,198,255))                     # 뚜껑
    b.rect(3, 8, 29, 9, (236,252,242,255))
    b.rect(3, 11, 29, 12, g_d)
    b.rect(13, 16, 15, 24, (168,228,190,255))                   # 하이라이트
    return save(b, "plastic_tub")


def plastic_basin():
    """바가지 — 입이 넓게 벌어진 주황 불투명. 병이 아니다."""
    b = Box()
    o_d, o_m, o_l = (178,88,24,255), (232,136,46,255), (252,190,116,255)
    b.poly([(3,11),(29,11),(23,26),(9,26)], o_m)
    b.poly([(3,11),(11,11),(11,26),(9,26)], o_l)
    b.poly([(23,11),(29,11),(23,26),(21,26)], o_d)
    b.ellipse(16, 11, 13, 2.6, o_l)                             # 입 테두리
    b.ellipse(16, 11, 11.4, 1.6, (120,58,16,255))               # 안쪽 그늘
    b.rect(10, 26, 22, 27, o_d)
    b.rect(12, 15, 14, 24, (254,214,160,255))
    return save(b, "plastic_basin")


def plastic_crate():
    """플라스틱 상자 — 격자 구멍. 실루엣이 네모다."""
    b = Box()
    y_d, y_m, y_l = (184,146,26,255), (236,196,54,255), (252,232,140,255)
    b.rect(4, 9, 27, 26, y_m)
    b.rect(4, 9, 27, 10, y_l)
    b.rect(4, 25, 27, 26, y_d)
    b.rect(4, 9, 5, 26, y_l); b.rect(26, 9, 27, 26, y_d)
    for gy in (13, 19):                                          # 격자 구멍
        for gx in (8, 14, 20):
            b.rect(gx, gy, gx + 3, gy + 3, (92,70,16,255))
            b.rect(gx, gy, gx + 3, gy, (60,46,10,255))
    b.rect(4, 17, 27, 18, y_l)
    return save(b, "plastic_crate")


# ===================== 종이 =====================
def paper_box():
    """골판지 상자 — 닫힌 날개가 만드는 십자 이음새 + 골 단면."""
    b = Box()
    b.rect(5, 14, 27, 27, K_M)                           # 앞면
    b.rect(5, 14, 27, 15, K_L)
    b.rect(5, 26, 27, 27, K_D)
    b.rect(5, 14, 6, 27, K_L); b.rect(26, 14, 27, 27, K_D)
    b.poly([(5,14),(10,9),(27,9),(27,14)], K_L)          # 윗면(원근)
    b.rect(10, 9, 27, 9, K_D)
    b.line(10, 9, 5, 14, K_D)
    b.rect(18, 9, 18, 14, K_D)                           # 십자 이음새
    b.rect(10, 11, 27, 11, K_D)
    for x in range(6, 27, 2): b.set(x, 18, K_D)          # 골 단면
    for x in range(7, 27, 2): b.set(x, 18, K_L)
    b.rect(6, 17, 26, 17, K_D); b.rect(6, 19, 26, 19, K_D)
    b.rect(11, 22, 21, 25, K_L)                          # 송장 라벨
    b.rect(11, 22, 21, 22, K_D)
    for y in (23, 24): b.rect(13, y, 19, y, (150,122,84,255))
    return save(b, "paper_box")


def paper_news():
    """신문지 묶음 — 납작하게 쌓이고 끈으로 묶였다."""
    b = Box()
    for i, y in enumerate((11, 15, 19)):
        b.rect(4 + (i % 2), y, 28 - (i % 2), y + 4, W_M)
        b.rect(4 + (i % 2), y, 28 - (i % 2), y, W_L)
        b.rect(4 + (i % 2), y + 4, 28 - (i % 2), y + 4, W_D)
        for ty in range(y + 1, y + 4):                           # 글자 줄
            for x in range(6, 26, 2):
                if (x + ty) % 3: b.set(x, ty, (150,146,138,255))
        b.rect(6, y + 1, 13, y + 1, (96,94,90,255))              # 헤드라인
    b.rect(13, 10, 15, 24, (150,98,52,255))                      # 끈
    b.rect(13, 10, 13, 24, (196,146,92,255))
    b.rect(3, 16, 29, 17, (120,116,108,255))
    return save(b, "paper_news")


def paper_books():
    """책·노트 더미 — 종이 단면이 보인다."""
    b = Box()
    covers = [((58,96,146,255),(96,142,196,255), 4, 22, 20),
              ((146,64,72,255),(198,104,106,255), 6, 26, 15),
              ((86,120,64,255),(132,174,98,255), 3, 21, 10)]
    for dark, light, x0, x1, y in covers:
        b.rect(x0, y, x1, y + 4, dark)
        b.rect(x0, y, x1, y, light)
        b.rect(x0 + 2, y + 1, x1, y + 3, W_L)                    # 종이 단면
        for py in range(y + 1, y + 4):
            for x in range(x0 + 3, x1, 2): b.set(x, py, W_D)
        b.rect(x0, y, x0 + 1, y + 4, dark)                        # 책등
        b.rect(x0, y + 1, x0, y + 3, light)
    return save(b, "paper_books")


ALL = {
    "Can":      [can_steel, can_drink, can_crushed],
    "Pet":      [pet_water, pet_soda, pet_crushed],
    "Paper":    [paper_box, paper_news, paper_books],
    "Plastic":  [plastic_tub, plastic_basin, plastic_crate],
    "Aluminum": [foil_ball, foil_tray, foil_sheet],
}

if __name__ == "__main__":
    for cat, fns in ALL.items():
        print(cat, [f() for f in fns])
