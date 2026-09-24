"""시대별 지상 시설 2종 — 쓰레기 투입소(분리수거장) / 강화소(처리장).

48x48, bottom-center, 16px 그리드 3칸. era_art 와 같은 규격·외곽선.

읽히는 규칙
  · 투입소는 시대가 달라도 **초록 표식 + 어두운 투입구**를 유지한다.
  · 강화소는 시대가 달라도 **주황 불빛/에너지**를 유지한다.
"""
from PIL import Image
from era_art import (B, S, GROUND, K, dither_rows,
                     STRAW1, STRAW2, STRAW3, WOOD1, WOOD2, HIDE1, HIDE2,
                     STONE1, STONE2, STONE3, FIRE1, FIRE2, FIRE3,
                     CY1, CY2, CY3)

# 공통 상징색
GR1, GR2, GR3 = (154, 226, 132), (96, 172, 92), (54, 112, 62)     # 재활용 초록
HOLE = (34, 26, 30)                                                # 투입구 어둠
GIWA1, GIWA2, GIWA3 = (146, 160, 168), (104, 120, 132), (72, 86, 98)
CLAY1, CLAY2, CLAY3 = (188, 132, 90), (146, 98, 64), (104, 68, 44)
BRICK1, BRICK2, BRICK3 = (176, 96, 76), (134, 68, 54), (94, 46, 38)
IRON1, IRON2, IRON3 = (176, 182, 190), (124, 130, 140), (78, 84, 94)
RUST1, RUST2 = (166, 106, 62), (118, 70, 40)
PLASTER1, PLASTER2 = (226, 220, 204), (182, 176, 160)
GLASS1, GLASS2 = (198, 232, 238), (132, 178, 196)


def recycle_mark(b, cx, cy, r=4):
    """어느 시대에나 붙는 초록 재활용 표식 (삼각 화살표 느낌)."""
    b.poly([(cx, cy - r), (cx + r, cy + r), (cx - r, cy + r)], GR2)
    b.poly([(cx, cy - r + 1), (cx + r - 2, cy + r - 1), (cx, cy + r - 1)], GR1)
    b.poly([(cx, cy - 1), (cx + 2, cy + 2), (cx - 2, cy + 2)], GR3)


def ember(b, x0, y0, x1, y1):
    """강화소가 공유하는 주황 불빛."""
    b.rect(x0, y0, x1, y1, FIRE2)
    b.rect(x0 + 1, y0 + 1, x1 - 1, y1, FIRE1)
    b.glow((x0 + x1) // 2, (y0 + y1) // 2, 9, FIRE1, 0.5)


# ============================================================ 1. 원시
def dropoff_primitive():
    """돌을 둥글게 두른 구덩이. 옆에 뼈와 나뭇가지 더미."""
    b = B()
    # 구덩이 테두리 돌
    b.ellipse(8, 30, 40, GROUND, STONE2)
    b.ellipse(10, 32, 38, GROUND - 1, STONE1)
    b.ellipse(14, 35, 34, GROUND - 2, HOLE)
    for (x, y, w) in ((8, 33, 4), (36, 33, 4), (12, 30, 5), (30, 29, 6), (20, 28, 6)):
        b.rect(x, y, x + w, y + 4, STONE2)
        b.rect(x, y, x + w, y + 1, STONE1)
        b.rect(x, y + 4, x + w, y + 4, STONE3)
    # 세워 놓은 나뭇가지 (표식 기둥)
    b.rect(35, 12, 37, 34, WOOD2)
    b.rect(35, 12, 35, 34, WOOD1)
    b.line(36, 16, 42, 12, WOOD2)
    recycle_mark(b, 36, 10, 5)
    # 던져 넣은 뼈
    b.rect(16, 33, 24, 35, (222, 214, 194))
    b.px(15, 32, (222, 214, 194)); b.px(25, 35, (222, 214, 194))
    b.outline()
    b.shadow(6, 42)
    return b.img


def shop_primitive():
    """숫돌과 모닥불. 평평한 돌 위에 날을 가는 자리."""
    b = B()
    # 모닥불
    b.rect(6, 40, 20, 44, WOOD2)
    b.rect(8, 39, 18, 41, WOOD1)
    ember(b, 9, 33, 17, 40)
    b.poly([(13, 27), (17, 36), (9, 36)], FIRE3)
    b.poly([(13, 30), (16, 36), (10, 36)], FIRE2)
    # 숫돌 받침
    b.rect(24, 38, 45, GROUND, STONE3)
    b.rect(24, 36, 45, 40, STONE2)
    b.rect(25, 36, 44, 37, STONE1)
    # 세워 갈아 놓은 큰 돌칼
    b.poly([(31, 36), (41, 36), (39, 20), (35, 16), (33, 20)], STONE1)
    b.poly([(35, 36), (40, 36), (38, 21), (36, 17)], STONE3)
    b.line(34, 22, 34, 35, (216, 212, 200))
    b.px(35, 15, FIRE1); b.px(37, 18, FIRE1); b.px(33, 24, FIRE1)
    b.rect(32, 34, 41, 37, HIDE2)                                # 손잡이 가죽끈
    # 가죽 차양 — 한쪽으로만 기울여 친다 (돌칼을 가두지 않게)
    b.poly([(20, 14), (47, 20), (46, 23), (21, 17)], HIDE1)
    b.poly([(20, 14), (34, 17), (33, 20), (21, 17)], (210, 168, 124))
    b.rect(19, 13, 22, 42, WOOD2)
    b.rect(19, 13, 20, 42, WOOD1)
    b.px(46, 21, HIDE2)
    b.outline()
    b.shadow(4, 46)
    return b.img


# ============================================================ 2. 고대
def dropoff_ancient():
    """입 벌린 짚 광주리와 토기 항아리들."""
    b = B()
    # 표식 장대 (뒤)
    b.rect(39, 14, 41, 34, WOOD2)
    b.rect(39, 14, 39, 34, WOOD1)
    # 가운데 큰 짚 광주리 — 위가 넓은 사다리꼴
    b.poly([(15, GROUND), (33, GROUND), (37, 27), (11, 27)], STRAW2)
    b.poly([(15, GROUND), (24, GROUND), (24, 27), (11, 27)], STRAW1)
    dither_rows(b, 11, 37, 29, GROUND, STRAW1, STRAW3, period=4)
    # 엮은 테 두 줄
    for y in (33, 40):
        b.line(12, y, 36, y, STRAW3)
        b.line(12, y + 1, 36, y + 1, (146, 110, 62))
    # 크게 벌어진 입
    b.ellipse(9, 21, 39, 32, STRAW3)
    b.ellipse(11, 23, 37, 30, STRAW1)
    b.ellipse(13, 24, 35, 29, HOLE)
    # 삐져나온 짚단
    b.poly([(28, 26), (33, 26), (35, 16), (32, 16)], STRAW1)
    b.line(33, 17, 34, 25, STRAW3)
    # 앞쪽 토기 항아리 2개
    for (cx, h) in ((7, 15), (42, 13)):
        b.ellipse(cx - 7, GROUND - h, cx + 7, GROUND, CLAY2)
        b.ellipse(cx - 6, GROUND - h + 2, cx + 1, GROUND - 3, CLAY1)
        b.rect(cx - 4, GROUND - h - 3, cx + 4, GROUND - h + 2, CLAY3)
        b.rect(cx - 3, GROUND - h - 3, cx + 3, GROUND - h - 1, HOLE)
        b.line(cx - 7, GROUND - h + 5, cx + 7, GROUND - h + 5, CLAY3)
    recycle_mark(b, 40, 11, 5)
    b.outline()
    b.shadow(0, 47)
    return b.img


def shop_ancient():
    """대장간 화덕. 돌 화덕에 불이 붙고 옆에 모루와 망치."""
    b = B()
    # 화덕 본체
    b.rect(6, 24, 30, GROUND, STONE2)
    b.rect(6, 24, 18, GROUND, STONE1)
    for y in range(26, GROUND, 4):
        b.line(6, y, 30, y, STONE3)
    b.rect(5, 21, 31, 25, STONE3)
    b.rect(6, 21, 30, 22, STONE1)
    # 화구
    b.rect(12, 30, 24, 40, HOLE)
    ember(b, 13, 33, 23, 39)
    b.poly([(18, 27), (22, 36), (14, 36)], FIRE3)
    # 굴뚝 + 연기
    b.rect(22, 10, 29, 22, STONE2)
    b.rect(22, 10, 25, 22, STONE1)
    b.rect(21, 8, 30, 11, STONE3)
    for (x, y, r) in ((26, 5, 3), (30, 2, 2)):
        b.ellipse(x - r, y - r, x + r, y + r, (188, 184, 176, 190))
    # 모루
    b.rect(34, 36, 44, 39, IRON2)
    b.rect(34, 36, 44, 37, IRON1)
    b.rect(37, 39, 41, 43, IRON3)
    b.rect(35, 43, 43, GROUND, IRON2)
    # 망치
    b.line(40, 35, 44, 26, WOOD1)
    b.line(41, 35, 45, 26, WOOD2)
    b.rect(41, 23, 47, 27, IRON1)
    b.rect(41, 26, 47, 27, IRON3)
    b.outline()
    b.shadow(3, 46)
    return b.img


# ============================================================ 3. 조선
def giwa_small(b, x0, x1, y, c1=GIWA1, c2=GIWA2, c3=GIWA3):
    """작은 기와 지붕 한 겹 (곡선 처마)."""
    b.poly([(x0 - 3, y + 5), (x1 + 3, y + 5), (x1 - 2, y), (x0 + 2, y)], c2)
    b.poly([(x0 - 3, y + 5), (x0 + 12, y + 5), (x0 + 9, y), (x0 + 2, y)], c1)
    b.rect(x0 - 4, y + 5, x1 + 4, y + 7, c3)
    b.rect(x0 - 4, y + 5, x1 + 4, y + 5, c1)
    for x in range(x0 - 2, x1 + 3, 4):
        b.line(x, y + 1, x - 1, y + 6, c3)


def dropoff_joseon():
    """곳간. 기와 얹은 나무 창고, 널문이 열려 있고 앞에 가마니."""
    b = B()
    # 몸체
    b.rect(8, 22, 40, GROUND, WOOD1)
    b.rect(8, 22, 22, GROUND, (146, 106, 70))
    for x in range(8, 41, 5):
        b.line(x, 22, x, GROUND, WOOD2)
    b.rect(8, GROUND - 2, 40, GROUND, STONE3)
    # 열린 널문 = 투입구
    b.rect(18, 28, 32, GROUND - 2, HOLE)
    b.rect(18, 28, 20, GROUND - 2, WOOD2)
    b.rect(30, 28, 32, GROUND - 2, WOOD2)
    b.rect(17, 27, 33, 29, WOOD2)
    # 기와 지붕
    giwa_small(b, 8, 40, 14)
    b.rect(6, 19, 42, 21, WOOD2)
    # 가마니 2개
    for cx in (11, 38):
        b.ellipse(cx - 5, GROUND - 9, cx + 5, GROUND, STRAW2)
        b.ellipse(cx - 4, GROUND - 8, cx + 1, GROUND - 2, STRAW1)
        b.line(cx - 5, GROUND - 5, cx + 5, GROUND - 5, STRAW3)
    recycle_mark(b, 24, 24, 4)
    b.outline()
    b.shadow(4, 44)
    return b.img


def shop_joseon():
    """저잣거리 좌판. 무명 차양 아래 물건을 벌여 놓고 화로가 놓였다."""
    b = B()
    # 기둥
    b.rect(7, 16, 10, GROUND, WOOD1)
    b.rect(38, 16, 41, GROUND, WOOD1)
    b.rect(9, 16, 10, GROUND, WOOD2)
    b.rect(40, 16, 41, GROUND, WOOD2)
    # 차양 (줄무늬 천)
    b.poly([(4, 18), (44, 18), (41, 12), (7, 12)], PLASTER1)
    for x in range(6, 44, 6):
        b.poly([(x, 18), (x + 3, 18), (x + 4, 12), (x + 1, 12)], (176, 108, 96))
    b.rect(3, 18, 45, 20, PLASTER2)
    b.rect(3, 18, 45, 18, PLASTER1)
    # 좌판
    b.rect(8, 32, 40, 36, WOOD1)
    b.rect(8, 32, 40, 33, (168, 126, 84))
    b.rect(8, 35, 40, 36, WOOD2)
    b.rect(10, 36, 13, GROUND, WOOD2)
    b.rect(35, 36, 38, GROUND, WOOD2)
    # 벌여 놓은 물건 — 옹기·연장
    b.ellipse(11, 26, 19, 32, CLAY2)
    b.ellipse(12, 27, 16, 31, CLAY1)
    b.rect(13, 24, 17, 27, CLAY3)
    b.rect(22, 27, 26, 32, IRON2)
    b.rect(22, 27, 26, 28, IRON1)
    b.line(24, 24, 24, 27, WOOD1)
    # 화로 (주황 불빛)
    b.ellipse(29, 26, 39, 33, IRON3)
    b.ellipse(30, 27, 38, 32, IRON2)
    ember(b, 31, 27, 37, 31)
    b.outline()
    b.shadow(5, 43)
    return b.img


# ============================================================ 4. 근대
def bricks(b, x0, y0, x1, y1, c1, c2, c3):
    b.rect(x0, y0, x1, y1, c1)
    for i, y in enumerate(range(y0, y1 + 1, 4)):
        b.line(x0, y, x1, y, c3)
        off = 0 if i % 2 == 0 else 3
        for x in range(x0 + off, x1, 6):
            b.line(x, y, x, min(y + 3, y1), c2)


def dropoff_modern():
    """고물상. 녹슨 드럼통 옆에 철제 수거함, 오른쪽에 함석 간판."""
    b = B()
    # 간판 기둥 (뒤)
    b.rect(39, 16, 42, 44, IRON3)
    b.rect(39, 16, 40, 44, IRON2)
    b.rect(30, 6, 47, 18, IRON1)
    b.rect(30, 6, 47, 7, (218, 222, 228))
    b.rect(30, 17, 47, 18, IRON3)
    recycle_mark(b, 38, 12, 5)
    b.rect(31, 9, 35, 10, IRON3)
    b.rect(31, 12, 34, 13, IRON3)
    # 왼쪽 드럼통 2개
    for (cx, top) in ((7, 30), (16, 34)):
        b.rect(cx - 6, top, cx + 6, GROUND, RUST2)
        b.rect(cx - 6, top, cx, GROUND, RUST1)
        for y in range(top + 3, GROUND, 5):
            b.line(cx - 6, y, cx + 6, y, (86, 52, 32))
        b.ellipse(cx - 6, top - 2, cx + 6, top + 2, IRON2)
        b.ellipse(cx - 5, top - 1, cx + 5, top + 1, HOLE)
    # 가운데 철제 수거함
    b.rect(20, 26, 40, GROUND, IRON2)
    b.rect(20, 26, 28, GROUND, IRON1)
    b.rect(20, GROUND - 3, 40, GROUND, IRON3)
    for x in range(23, 40, 5):
        b.line(x, 29, x, GROUND - 4, IRON3)
    # 투입구 + 왼쪽으로 젖힌 뚜껑
    b.rect(23, 29, 37, 37, HOLE)
    b.rect(22, 27, 38, 30, IRON3)
    b.poly([(20, 28), (30, 26), (29, 23), (19, 25)], IRON1)
    b.poly([(20, 28), (25, 27), (24, 24), (19, 25)], (206, 212, 220))
    b.rect(19, 25, 20, 29, IRON3)
    b.outline()
    b.shadow(0, 46)
    return b.img


def shop_modern():
    """철공소. 붉은 벽돌 건물에 굴뚝이 서고 창에 용광로 불빛이 비친다."""
    b = B()
    bricks(b, 6, 20, 42, GROUND, BRICK1, BRICK2, BRICK3)
    b.rect(6, GROUND - 3, 42, GROUND, (86, 82, 78))
    # 지붕 (함석)
    b.poly([(3, 20), (45, 20), (41, 14), (7, 14)], IRON2)
    b.poly([(3, 20), (24, 20), (24, 14), (7, 14)], IRON1)
    b.rect(2, 20, 46, 22, IRON3)
    for x in range(6, 43, 5):
        b.line(x, 15, x, 19, IRON3)
    # 굴뚝
    b.rect(33, 4, 39, 16, BRICK2)
    b.rect(33, 4, 35, 16, BRICK1)
    b.rect(32, 2, 40, 5, BRICK3)
    b.ellipse(33, -3, 39, 3, (180, 176, 170, 190))
    # 커다란 창 — 용광로 불빛
    b.rect(10, 26, 22, 38, IRON3)
    b.rect(11, 27, 21, 37, HOLE)
    ember(b, 12, 30, 20, 36)
    b.line(16, 27, 16, 37, IRON3)
    b.line(11, 32, 21, 32, IRON3)
    # 셔터 문
    b.rect(27, 28, 39, GROUND - 3, IRON2)
    for y in range(29, GROUND - 3, 3):
        b.line(27, y, 39, y, IRON3)
    b.rect(27, 28, 39, 29, IRON1)
    # 톱니 간판
    b.ellipse(28, 20, 38, 30, IRON1)
    b.ellipse(30, 22, 36, 28, BRICK2)
    for (dx, dy) in ((0, -6), (0, 6), (-6, 0), (6, 0), (-4, -4), (4, 4), (-4, 4), (4, -4)):
        b.rect(33 + dx - 1, 25 + dy - 1, 33 + dx + 1, 25 + dy + 1, IRON1)
    b.outline()
    b.shadow(2, 46)
    return b.img


# ============================================================ 5. 미래
def dropoff_future():
    """분해 포털. 떠 있는 시안 링 아래로 쓰레기가 빨려 들어간다."""
    b = B()
    # 바닥 발판
    b.poly([(8, GROUND), (40, GROUND), (35, 38), (13, 38)], IRON2)
    b.poly([(8, GROUND), (24, GROUND), (24, 38), (13, 38)], IRON1)
    b.rect(8, GROUND - 1, 40, GROUND, IRON3)
    # 빨려드는 빛기둥
    for i, y in enumerate(range(20, 39)):
        w = 3 + i // 2
        b.line(24 - w, y, 24 + w, y, CY3 if i % 3 == 0 else (60, 170, 190))
    b.ellipse(16, 34, 32, 42, HOLE)
    b.ellipse(18, 36, 30, 41, CY3)
    # 떠 있는 링 2개
    for (y, r, c) in ((20, 15, CY2), (13, 10, CY1)):
        b.ellipse(24 - r, y - 4, 24 + r, y + 4, c)
        b.ellipse(24 - r + 3, y - 2, 24 + r - 3, y + 2, (0, 0, 0, 0))
    b.ellipse(9, 16, 39, 24, (0, 0, 0, 0))
    for (y, r, c) in ((20, 15, CY2), (13, 10, CY1)):
        b.ellipse(24 - r, y - 4, 24 + r, y + 4, c)
        b.ellipse(24 - r + 3, y - 2, 24 + r - 3, y + 2, (0, 0, 0, 0))
    # 기둥 2개
    b.rect(6, 24, 10, 40, IRON2)
    b.rect(38, 24, 42, 40, IRON2)
    b.rect(6, 24, 8, 40, IRON1)
    b.rect(38, 24, 40, 40, IRON1)
    b.rect(5, 22, 11, 25, IRON3)
    b.rect(37, 22, 43, 25, IRON3)
    b.outline()
    b.glow(24, 26, 20, CY1, 0.45)
    b.shadow(6, 42)
    recycle_mark(b, 24, 8, 5)
    return b.img


def shop_future():
    """자동 강화 포드. 유리 캡슐 안에서 장비가 재조립된다."""
    b = B()
    # 받침
    b.rect(6, 40, 42, GROUND, IRON2)
    b.rect(6, 40, 42, 41, IRON1)
    b.rect(6, GROUND - 1, 42, GROUND, IRON3)
    b.rect(10, 36, 38, 41, IRON3)
    # 캡슐
    b.ellipse(12, 8, 36, 40, IRON2)
    b.ellipse(14, 10, 34, 38, GLASS2)
    b.ellipse(15, 12, 31, 36, GLASS1)
    b.ellipse(17, 14, 25, 28, (232, 250, 252))
    # 안에 떠 있는 장비 + 주황 에너지
    b.rect(22, 20, 26, 32, IRON1)
    b.poly([(24, 14), (28, 22), (20, 22)], IRON1)
    ember(b, 21, 28, 27, 33)
    for y in range(16, 36, 4):
        b.line(15, y, 33, y, (200, 236, 244, 120))
    # 캡슐 테두리
    b.ellipse(12, 8, 36, 40, (0, 0, 0, 0))
    b.ellipse(13, 9, 35, 39, IRON1)
    b.ellipse(15, 11, 33, 37, GLASS2)
    b.ellipse(16, 13, 30, 34, GLASS1)
    b.rect(20, 18, 28, 32, (0, 0, 0, 0))
    b.rect(22, 20, 26, 32, IRON1)
    b.poly([(24, 14), (28, 22), (20, 22)], IRON2)
    ember(b, 21, 26, 27, 33)
    # 옆 기둥 + 홀로 패널
    b.rect(4, 18, 10, 42, IRON2)
    b.rect(4, 18, 6, 42, IRON1)
    b.rect(38, 18, 44, 42, IRON2)
    b.rect(38, 18, 40, 42, IRON1)
    b.rect(39, 22, 44, 30, FIRE2)
    b.rect(40, 23, 43, 29, FIRE1)
    b.rect(4, 22, 9, 30, FIRE2)
    b.rect(5, 23, 8, 29, FIRE1)
    b.outline()
    b.glow(24, 26, 18, FIRE1, 0.35)
    b.shadow(3, 45)
    return b.img


# ============================================================
ERA_DROPOFFS = [
    ("원시", "돌 구덩이", dropoff_primitive),
    ("고대", "짚 광주리", dropoff_ancient),
    ("조선", "곳간", dropoff_joseon),
    ("근대", "고물상", dropoff_modern),
    ("미래", "분해 포털", dropoff_future),
]
ERA_SHOPS = [
    ("원시", "숫돌 터", shop_primitive),
    ("고대", "대장간", shop_ancient),
    ("조선", "저잣거리 좌판", shop_joseon),
    ("근대", "철공소", shop_modern),
    ("미래", "강화 포드", shop_future),
]


def strip(entries):
    img = Image.new("RGBA", (S * len(entries), S), (0, 0, 0, 0))
    for i, (_, _, fn) in enumerate(entries):
        img.paste(fn(), (i * S, 0))
    return img


if __name__ == "__main__":
    import os
    os.makedirs("out/Era", exist_ok=True)
    strip(ERA_DROPOFFS).save("out/Era/era_dropoffs.png")
    strip(ERA_SHOPS).save("out/Era/era_shops.png")
    print("ok")
