"""시대별 이웃 건물. 64x64, bottom-center, 16px 그리드 4칸.

한 칸을 64x64 로 통일하되 그림이 차지하는 크기를 다르게 해서 실루엣에 변화를 준다.
  0 작은집  ~31x26   1 보통집 ~47x40   2 넓은건물 ~60x36   3 높은건물 ~35x56

이틀씩 머무는 원시·미래는 3종을 더 얹어 7종, 나머지 시대는 4종.
시트: era_buildings_{era}.png (4종 256x64 / 7종 448x64).
"""
from PIL import Image
from pixbox import Box, ERA, K, C, sheet

S = 64


def new():
    return Box(S, S)


def fin(b, glow=None):
    b.outline()
    if glow:
        for (cx, cy, r, col) in glow:
            b.glow(cx, cy, r, col, 0.45)
    b.auto_shadow()
    return b.img


# ================================================================ 공용 부품
def wall(b, x0, y0, x1, y1, e):
    """시대별 벽면."""
    p = ERA[e]
    if e == 0:                                   # 통나무 + 가죽
        b.rect(x0, y0, x1, y1, p['w2'])
        for x in range(x0, x1, 5):
            b.rect(x, y0, x + 1, y1, p['w1'])
            b.line(x + 3, y0, x + 3, y1, p['w3'])
    elif e == 1:                                 # 흙벽
        b.rect(x0, y0, x1, y1, C(186, 156, 112))
        b.rect(x0, y0, x0 + (x1 - x0) // 3, y1, C(208, 180, 134))
        for y in range(y0 + 3, y1, 6):
            b.line(x0 + 2, y, x1 - 2, y, C(160, 130, 90))
    elif e == 2:                                 # 흰 회벽 + 나무 기둥
        b.rect(x0, y0, x1, y1, C(232, 226, 210))
        b.rect(x0, y0, x0 + (x1 - x0) // 3, y1, C(244, 240, 228))
        b.rect(x0, y0, x0 + 2, y1, p['w2'])
        b.rect(x1 - 2, y0, x1, y1, p['w2'])
        b.rect(x0, y1 - 3, x1, y1, C(168, 162, 150))
    elif e == 3:                                 # 붉은 벽돌
        b.rect(x0, y0, x1, y1, p['s2'])
        for i, y in enumerate(range(y0, y1 + 1, 4)):
            b.line(x0, y, x1, y, p['s3'])
            off = 0 if i % 2 == 0 else 3
            for x in range(x0 + off, x1, 6):
                b.line(x, y, x, min(y + 3, y1), p['s1'])
    else:                                        # 흰 패널
        b.rect(x0, y0, x1, y1, p['s2'])
        b.rect(x0, y0, x0 + (x1 - x0) // 3, y1, p['s1'])
        for y in range(y0 + 4, y1, 7):
            b.line(x0 + 1, y, x1 - 1, y, C(122, 134, 152))


def roof_gable(b, x0, x1, ybase, h, e, eave=3):
    """박공 지붕 (원시·고대·근대·미래 공용)."""
    p = ERA[e]
    cx = (x0 + x1) // 2
    top = ybase - h
    b.poly([(x0 - eave, ybase), (x1 + eave, ybase), (cx, top)], p['r2'])
    b.poly([(x0 - eave, ybase), (cx, ybase), (cx, top)], p['r1'])
    b.rect(x0 - eave, ybase, x1 + eave, ybase + 2, p['r3'])
    if e in (0, 1):                              # 짚 결
        for x in range(x0 - eave, x1 + eave, 4):
            b.line(x, ybase + 1, cx + (x - cx) // 2, top + 2, p['r3'])
    if e == 3:                                   # 함석 골
        for x in range(x0 - eave, x1 + eave, 5):
            b.line(x, ybase, cx + (x - cx) // 2, top + 1, p['r3'])


def roof_giwa(b, x0, x1, ybase, h, e=2):
    """조선 기와 — 처마가 살짝 치켜 올라간 곡선."""
    p = ERA[e]
    cx = (x0 + x1) // 2
    top = ybase - h
    b.poly([(x0 - 6, ybase), (x1 + 6, ybase), (x1 - 3, top), (x0 + 3, top)], p['r2'])
    b.poly([(x0 - 6, ybase), (cx, ybase), (cx, top), (x0 + 3, top)], p['r1'])
    b.poly([(x0 - 6, ybase - 1), (x0 - 9, ybase - 4), (x0 - 3, ybase - 3)], p['r2'])
    b.poly([(x1 + 6, ybase - 1), (x1 + 9, ybase - 4), (x1 + 3, ybase - 3)], p['r2'])
    b.rect(x0 - 6, ybase, x1 + 6, ybase + 2, p['r3'])
    b.rect(x0 - 6, ybase, x1 + 6, ybase, C(190, 202, 208))
    for x in range(x0 - 4, x1 + 5, 4):
        b.line(x, top + 1, x - 2, ybase - 1, p['r3'])
    b.rect(x0 + 2, top - 1, x1 - 2, top + 1, p['r3'])


def roof_flat(b, x0, x1, y, e, lip=3):
    p = ERA[e]
    b.rect(x0 - lip, y - 3, x1 + lip, y, p['r2'])
    b.rect(x0 - lip, y - 3, x1 + lip, y - 2, p['r1'])
    b.rect(x0 - lip, y, x1 + lip, y + 1, p['r3'])


def door(b, cx, ybase, w, h, e):
    p = ERA[e]
    b.rect(cx - w // 2, ybase - h, cx + w // 2, ybase, p['dark'])
    if e == 0:
        b.poly([(cx - w // 2, ybase - h), (cx + w // 2, ybase - h), (cx, ybase - h - 4)], p['a2'])
    elif e == 1:
        b.rect(cx - w // 2 - 1, ybase - h - 2, cx + w // 2 + 1, ybase - h, p['w3'])
    elif e == 2:
        b.rect(cx - w // 2, ybase - h, cx + w // 2, ybase, p['w2'])
        for x in range(cx - w // 2 + 1, cx + w // 2, 3):
            b.line(x, ybase - h + 1, x, ybase - 1, C(214, 198, 160))
        b.rect(cx - 1, ybase - h, cx, ybase, p['w3'])
    elif e == 3:
        b.rect(cx - w // 2, ybase - h, cx + w // 2, ybase, p['w2'])
        b.rect(cx - w // 2, ybase - h, cx - w // 2 + 1, ybase, p['w1'])
        b.px(cx + w // 2 - 2, ybase - h // 2, p['a1'])
    else:
        b.rect(cx - w // 2, ybase - h, cx + w // 2, ybase, p['a2'])
        b.rect(cx - w // 2 + 1, ybase - h + 1, cx + w // 2 - 1, ybase - 1, p['dark'])
        b.rect(cx - w // 2, ybase - h, cx + w // 2, ybase - h + 1, p['a1'])


def window(b, x, y, w, h, e, lit=True):
    p = ERA[e]
    if e == 0:                                   # 창 없음 — 작은 구멍
        b.rect(x, y, x + w, y + h, p['dark'])
        return
    if e == 1:
        b.rect(x, y, x + w, y + h, p['w3'])
        b.rect(x + 1, y + 1, x + w - 1, y + h - 1, p['dark'])
        return
    if e == 2:                                   # 창살 문
        b.rect(x, y, x + w, y + h, p['w2'])
        b.rect(x + 1, y + 1, x + w - 1, y + h - 1, C(238, 226, 190) if lit else C(150, 140, 120))
        for gx in range(x + 2, x + w, 3):
            b.line(gx, y + 1, gx, y + h - 1, p['w2'])
        for gy in range(y + 2, y + h, 3):
            b.line(x + 1, gy, x + w - 1, gy, p['w2'])
        return
    if e == 3:
        b.rect(x, y, x + w, y + h, C(230, 226, 216))
        b.rect(x + 1, y + 1, x + w - 1, y + h - 1, p['glow'] if lit else C(96, 108, 126))
        b.line(x + w // 2, y, x + w // 2, y + h, C(230, 226, 216))
        b.line(x, y + h // 2, x + w, y + h // 2, C(230, 226, 216))
        return
    b.rect(x, y, x + w, y + h, p['r3'])
    b.rect(x + 1, y + 1, x + w - 1, y + h - 1, p['a2'])
    b.rect(x + 1, y + 1, x + w - 1, y + 1 + h // 3, p['a1'])


# ================================================================ 1. 작은 집
def hut(e):
    p = ERA[e]
    b = new()
    g = b.ground
    x0, x1 = 17, 47
    if e == 0:                                   # 짚 원뿔
        b.poly([(x0 - 3, g), (x1 + 3, g), (32, g - 26)], p['r2'])
        b.poly([(x0 - 3, g), (32, g), (32, g - 26)], p['r1'])
        for x in range(x0 - 2, x1 + 3, 4):
            b.line(x, g, 32 + (x - 32) // 3, g - 24, p['r3'])
        b.rect(30, g - 29, 34, g - 24, p['w2'])
        door(b, 32, g, 9, 10, e)
    elif e == 1:                                 # 흙벽 + 짚 박공
        wall(b, x0, g - 15, x1, g, e)
        roof_gable(b, x0, x1, g - 15, 11, e)
        door(b, 32, g, 9, 11, e)
        window(b, x0 + 3, g - 12, 5, 5, e)
    elif e == 2:                                 # 초가
        wall(b, x0, g - 15, x1, g, e)
        b.poly([(x0 - 5, g - 15), (x1 + 5, g - 15), (x1 - 2, g - 25), (x0 + 2, g - 25)], C(196, 162, 100))
        b.poly([(x0 - 5, g - 15), (32, g - 15), (32, g - 25), (x0 + 2, g - 25)], C(220, 190, 128))
        b.rect(x0 - 5, g - 15, x1 + 5, g - 13, C(150, 120, 68))
        for x in range(x0 - 4, x1 + 5, 4):
            b.line(x, g - 14, 32 + (x - 32) // 2, g - 24, C(160, 128, 74))
        door(b, 32, g, 11, 12, e)
    elif e == 3:                                 # 판잣집
        b.rect(x0, g - 17, x1, g, p['w2'])
        for x in range(x0, x1, 4):
            b.line(x, g - 17, x, g, p['w3'])
        b.rect(x0, g - 17, x0 + 6, g, p['w1'])
        roof_flat(b, x0, x1, g - 17, e, lip=2)
        door(b, 28, g, 9, 12, e)
        window(b, 38, g - 14, 7, 6, e)
        b.rect(44, g - 26, 46, g - 19, C(96, 102, 110))   # 연통
    else:                                        # 다리 달린 구형 캡슐
        for x in (21, 40):
            b.rect(x, g - 10, x + 3, g, p['w2'])
            b.rect(x, g - 10, x + 1, g, p['w1'])
            b.rect(x - 2, g - 1, x + 5, g, p['s3'])
        b.ellipse(19, g - 32, 45, g - 8, p['s2'])
        b.ellipse(21, g - 30, 33, g - 14, p['s1'])
        b.rect(23, g - 26, 41, g - 21, p['a2'])
        b.rect(24, g - 25, 40, g - 23, p['a1'])
        b.rect(28, g - 36, 30, g - 29, p['s2'])
        b.px(29, g - 38, p['a1'])
        door(b, 32, g - 9, 9, 9, e)
    return fin(b, [(32, g - 12, 16, p['a1'])] if e == 4 else None)


# ================================================================ 2. 보통 집
def house(e):
    p = ERA[e]
    b = new()
    g = b.ground
    x0, x1 = 9, 55
    if e == 0:                                   # 큰 움집 + 뼈 장식
        b.poly([(x0 - 4, g), (x1 + 4, g), (32, g - 36)], p['r2'])
        b.poly([(x0 - 4, g), (32, g), (32, g - 36)], p['r1'])
        for x in range(x0 - 3, x1 + 4, 5):
            b.line(x, g, 32 + (x - 32) // 3, g - 33, p['r3'])
        for (dx, dy) in ((-6, -6), (6, -6), (0, -9)):
            b.line(32, g - 36, 32 + dx, g - 36 + dy, p['w2'])
        b.rect(28, g - 40, 36, g - 35, p['w2'])
        door(b, 32, g, 13, 15, e)
        b.rect(14, g - 8, 20, g, p['a2'])        # 널어놓은 가죽
        b.rect(14, g - 8, 16, g, p['a1'])
    elif e == 1:                                 # 흙벽 우진각
        wall(b, x0, g - 22, x1, g, e)
        roof_gable(b, x0, x1, g - 22, 15, e, eave=4)
        door(b, 32, g, 12, 15, e)
        window(b, 15, g - 18, 7, 7, e)
        window(b, 44, g - 18, 7, 7, e)
    elif e == 2:                                 # 기와 한옥
        b.rect(x0 - 2, g - 5, x1 + 2, g, C(170, 166, 156))     # 기단
        b.rect(x0 - 2, g - 5, x1 + 2, g - 4, C(206, 202, 190))
        wall(b, x0, g - 24, x1, g - 5, e)
        roof_giwa(b, x0, x1, g - 24, 13)
        window(b, 14, g - 21, 12, 13, e)
        window(b, 38, g - 21, 12, 13, e)
        door(b, 32, g - 5, 10, 17, e)
    elif e == 3:                                 # 2층 벽돌 양옥
        wall(b, x0, g - 34, x1, g, e)
        roof_gable(b, x0, x1, g - 34, 10, e, eave=4)
        b.rect(40, g - 48, 46, g - 40, p['s2'])                # 굴뚝
        b.rect(40, g - 48, 42, g - 40, p['s1'])
        b.rect(38, g - 50, 48, g - 47, p['s3'])
        for (wx, wy) in ((14, g - 30), (32, g - 30), (14, g - 16), (40, g - 16)):
            window(b, wx, wy, 10, 9, e)
        door(b, 30, g, 11, 14, e)
    else:                                        # 패널 주택
        b.rect(16, g - 4, 48, g, p['s3'])
        wall(b, x0 + 2, g - 32, x1 - 2, g - 4, e)
        roof_flat(b, x0 + 2, x1 - 2, g - 32, e, lip=4)
        b.rect(x0 + 4, g - 30, x1 - 4, g - 29, p['a1'])
        window(b, 14, g - 26, 14, 8, e)
        window(b, 36, g - 26, 14, 8, e)
        door(b, 32, g - 4, 12, 14, e)
    return fin(b, [(32, g - 18, 20, p['a1'])] if e == 4 else None)


# ================================================================ 3. 넓은 건물
def hall(e):
    p = ERA[e]
    b = new()
    g = b.ground
    x0, x1 = 3, 60
    if e == 0:                                   # 가죽 천막 (긴 터널형)
        b.poly([(x0, g), (x1, g), (x1 - 6, g - 24), (x0 + 6, g - 24)], p['a2'])
        b.poly([(x0, g), (30, g), (30, g - 24), (x0 + 6, g - 24)], p['a1'])
        for x in range(x0 + 4, x1 - 2, 7):
            b.line(x, g, x + 3, g - 23, C(126, 72, 46))
        b.rect(x0 + 5, g - 26, x1 - 5, g - 23, p['w2'])
        for x in (x0 + 5, 32, x1 - 6):
            b.rect(x - 1, g - 29, x + 1, g - 24, p['w2'])
        door(b, 32, g, 14, 14, e)
    elif e == 1:                                 # 긴 창고 — 외쪽지붕 + 큰 두짝문
        wall(b, x0 + 2, g - 20, x1 - 2, g, e)
        b.poly([(x0 - 2, g - 18), (x1 + 2, g - 26), (x1 + 2, g - 22), (x0 - 2, g - 14)], p['r2'])
        b.poly([(x0 - 2, g - 18), (32, g - 22), (32, g - 18), (x0 - 2, g - 14)], p['r1'])
        b.rect(x0 - 2, g - 18, x1 + 2, g - 16, p['r3'])
        for x in range(x0, x1 + 2, 5):
            b.line(x, g - 17, x + 2, g - 24, p['r3'])
        b.rect(20, g - 16, 44, g, p['w2'])
        b.rect(20, g - 16, 44, g - 15, p['w3'])
        b.rect(31, g - 16, 33, g, p['w3'])
        for x in (24, 40):
            b.line(x, g - 14, x, g - 2, p['w3'])
        b.rect(8, g - 10, 16, g, C(186, 150, 94))
        b.line(8, g - 6, 16, g - 6, C(140, 108, 60))
    elif e == 2:                                 # 행랑채 (긴 기와)
        b.rect(x0, g - 4, x1, g, C(170, 166, 156))
        wall(b, x0 + 2, g - 20, x1 - 2, g - 4, e)
        roof_giwa(b, x0 + 4, x1 - 4, g - 20, 11)
        for wx in (9, 26, 43):
            window(b, wx, g - 17, 11, 11, e)
        door(b, 20, g - 4, 9, 13, e)
    elif e == 3:                                 # 벽돌 상가
        wall(b, x0 + 1, g - 26, x1 - 1, g, e)
        roof_flat(b, x0 + 1, x1 - 1, g - 26, e, lip=3)
        b.rect(x0 + 3, g - 24, x1 - 3, g - 20, C(206, 200, 190))   # 간판띠
        b.rect(x0 + 5, g - 23, x1 - 5, g - 21, p['a2'])
        for wx in (8, 26, 44):
            window(b, wx, g - 17, 12, 9, e)
        door(b, 32, g, 13, 13, e)
        b.rect(6, g - 13, 18, g, p['dark'])                        # 가게 입구
        b.rect(6, g - 13, 18, g - 11, p['w2'])
    else:                                        # 낮고 긴 회관 — 둥근 윗면 + 창 띠
        b.rect(x0, g - 4, x1, g, p['s3'])
        b.rect(x0 + 1, g - 22, x1 - 1, g - 4, p['s2'])
        b.rect(x0 + 1, g - 22, 24, g - 4, p['s1'])
        b.ellipse(x0 + 1, g - 32, x1 - 1, g - 14, p['s2'])
        b.ellipse(x0 + 4, g - 30, 30, g - 18, p['s1'])
        for y in (g - 26, g - 19, g - 12):
            b.rect(x0 + 5, y, x1 - 5, y + 3, p['a2'])
            b.rect(x0 + 6, y + 1, x1 - 6, y + 2, p['a1'])
        b.rect(30, g - 38, 34, g - 30, p['s2'])
        b.ellipse(28, g - 42, 36, g - 36, p['a1'])
        door(b, 32, g - 4, 13, 11, e)
    return fin(b, [(32, g - 18, 24, p['a1'])] if e == 4 else None)


# ================================================================ 4. 높은 건물
def tower(e):
    p = ERA[e]
    b = new()
    g = b.ground
    x0, x1 = 15, 49
    if e == 0:                                   # 통나무 망루
        for x in (x0 + 2, x1 - 4):
            b.rect(x, g - 40, x + 3, g, p['w2'])
            b.rect(x, g - 40, x + 1, g, p['w1'])
        for y in (g - 10, g - 22, g - 33):
            b.rect(x0, y, x1, y + 2, p['w3'])
        b.rect(x0 - 3, g - 46, x1 + 3, g - 39, p['w2'])
        b.rect(x0 - 3, g - 46, 32, g - 39, p['w1'])
        b.poly([(x0 - 5, g - 46), (x1 + 5, g - 46), (32, g - 58)], p['r2'])
        b.poly([(x0 - 5, g - 46), (32, g - 46), (32, g - 58)], p['r1'])
        for (x, y) in ((x0 - 1, g - 44), (x1 - 2, g - 44)):
            b.rect(x, y, x + 3, y + 2, C(222, 214, 194))
    elif e == 1:                                 # 목조 망루
        wall(b, x0 + 4, g - 30, x1 - 4, g, e)
        for x in (x0 + 4, x1 - 6):
            b.rect(x, g - 30, x + 2, g, p['w2'])
        b.rect(x0 - 2, g - 38, x1 + 2, g - 29, C(196, 168, 122))
        b.rect(x0 - 2, g - 38, 32, g - 29, C(216, 192, 148))
        b.rect(x0 - 2, g - 31, x1 + 2, g - 29, p['w3'])
        roof_gable(b, x0 - 1, x1 + 1, g - 38, 14, e, eave=4)
        window(b, 27, g - 36, 10, 6, e)
        door(b, 32, g, 11, 13, e)
    elif e == 2:                                 # 2층 누각
        b.rect(x0 - 4, g - 4, x1 + 4, g, C(170, 166, 156))
        for x in (x0, x1 - 3):
            b.rect(x, g - 22, x + 3, g - 4, p['w2'])
            b.rect(x, g - 22, x + 1, g - 4, p['w1'])
        b.rect(x0 + 4, g - 20, x1 - 4, g - 10, C(232, 226, 210))
        b.rect(x0 - 2, g - 24, x1 + 2, g - 21, p['w3'])
        roof_giwa(b, x0 + 1, x1 - 1, g - 24, 10)
        wall(b, x0 + 3, g - 44, x1 - 3, g - 30, e)
        window(b, 24, g - 41, 16, 9, e)
        b.rect(x0 - 1, g - 32, x1 + 1, g - 29, p['w2'])
        roof_giwa(b, x0 + 3, x1 - 3, g - 44, 11)
    elif e == 3:                                 # 시계탑 벽돌 건물
        wall(b, x0, g - 46, x1, g, e)
        for y in (g - 40, g - 28, g - 16):
            for wx in (19, 35):
                window(b, wx, y, 10, 8, e)
        b.rect(x0 - 2, g - 50, x1 + 2, g - 45, p['r2'])
        b.rect(x0 - 2, g - 50, 32, g - 45, p['r1'])
        b.ellipse(24, g - 62, 40, g - 50, C(206, 200, 190))
        b.ellipse(26, g - 60, 38, g - 52, C(244, 240, 230))
        b.line(32, g - 56, 32, g - 59, C(60, 58, 60))
        b.line(32, g - 56, 36, g - 55, C(60, 58, 60))
        door(b, 32, g, 12, 13, e)
    else:                                        # 미래 타워
        b.rect(20, g - 4, 44, g, p['s3'])
        wall(b, x0 + 4, g - 52, x1 - 4, g - 4, e)
        for y in range(g - 48, g - 10, 9):
            b.rect(x0 + 6, y, x1 - 6, y + 4, p['a2'])
            b.rect(x0 + 7, y + 1, x1 - 7, y + 2, p['a1'])
        roof_flat(b, x0 + 4, x1 - 4, g - 52, e, lip=3)
        b.rect(30, g - 62, 34, g - 52, p['s2'])
        b.ellipse(28, g - 66, 36, g - 60, p['a1'])
        door(b, 32, g - 4, 11, 12, e)
    return fin(b, [(32, g - 30, 24, p['a1'])] if e == 4 else None)


# ================================================================ 원시 보너스 3종
def rock_shelter(e=0):
    """바위에 기대 지은 가죽 움막."""
    p = ERA[0]
    b = new()
    g = b.ground
    b.ellipse(30, g - 34, 62, g, p['s2'])                   # 큰 바위
    b.ellipse(34, g - 31, 50, g - 8, p['s1'])
    b.rect(31, g - 3, 61, g, p['s3'])
    b.poly([(6, g), (36, g), (33, g - 24), (16, g - 26)], p['a2'])
    b.poly([(6, g), (21, g), (20, g - 25), (16, g - 26)], p['a1'])
    for x in range(9, 34, 5):
        b.line(x, g, x + 3, g - 23, C(126, 72, 46))
    b.rect(14, g - 29, 18, g - 24, p['w2'])                 # 지지 막대
    b.line(16, g - 29, 30, g - 34, p['w2'])
    door(b, 20, g, 11, 13, 0)
    for (x, y) in ((40, g - 8), (48, g - 6)):               # 주변 돌
        b.ellipse(x, y, x + 8, g, p['s2'])
        b.ellipse(x + 1, y + 1, x + 4, g - 2, p['s1'])
    return fin(b)


def drying_rack(e=0):
    """가죽과 고기를 널어 말리는 나무 틀. 넓고 낮다."""
    p = ERA[0]
    b = new()
    g = b.ground
    for x in (4, 30, 58):
        b.rect(x - 2, g - 34, x + 1, g, p['w2'])
        b.rect(x - 2, g - 34, x - 1, g, p['w1'])
        b.rect(x - 4, g - 1, x + 3, g, p['s3'])
    for y in (g - 32, g - 20):
        b.rect(2, y, 60, y + 2, p['w3'])
        b.rect(2, y, 60, y, p['w1'])
    for (x, w, h, c) in ((8, 9, 10, p['a1']), (22, 8, 13, p['a2']),
                         (36, 10, 9, p['a1']), (48, 8, 12, p['a2'])):
        b.rect(x, g - 30, x + w, g - 30 + h, c)
        b.rect(x, g - 30, x + 2, g - 30 + h, p['a2'] if c == p['a1'] else p['a1'])
    for x in (12, 40):                                      # 말리는 생선
        b.ellipse(x, g - 18, x + 11, g - 13, C(196, 176, 140))
        b.poly([(x + 11, g - 18), (x + 15, g - 20), (x + 15, g - 11), (x + 11, g - 13)], C(160, 142, 110))
        b.px(x + 3, g - 16, p['dark'])
    b.rect(44, g - 12, 56, g, C(186, 150, 94))              # 쌓아 둔 장작
    b.line(44, g - 8, 56, g - 8, p['w3'])
    return fin(b)


def cave_mouth(e=0):
    """바위 더미에 뚫린 동굴 입구. 넓다."""
    p = ERA[0]
    b = new()
    g = b.ground
    b.ellipse(0, g - 30, 40, g, p['s2'])
    b.ellipse(24, g - 38, 63, g, p['s2'])
    b.ellipse(28, g - 35, 48, g - 10, p['s1'])
    b.ellipse(4, g - 27, 22, g - 8, p['s1'])
    b.rect(0, g - 3, 63, g, p['s3'])
    b.poly([(18, g), (44, g), (40, g - 20), (31, g - 26), (22, g - 20)], p['dark'])
    b.poly([(21, g), (30, g), (28, g - 19), (24, g - 20)], C(56, 44, 42))
    for (x, y) in ((26, g - 14), (34, g - 10), (30, g - 5)):  # 안쪽 불빛
        b.px(x, y, p['glow'])
    b.rect(46, g - 26, 49, g - 6, p['w2'])                  # 표식 막대
    b.rect(44, g - 30, 51, g - 25, C(222, 214, 194))
    return fin(b, [(31, g - 12, 10, p['glow'])])


# ================================================================ 미래 보너스 3종
def pod_stack(e=4):
    """캡슐을 세 개 쌓아 올린 주거 탑."""
    p = ERA[4]
    b = new()
    g = b.ground
    b.rect(24, g - 4, 40, g, p['s3'])
    b.rect(29, g - 52, 35, g - 4, p['w3'])                  # 중심 기둥
    for i, y in enumerate((g - 14, g - 30, g - 46)):
        w = 17 - i * 2
        b.ellipse(32 - w, y - 12, 32 + w, y, p['s2'])
        b.ellipse(32 - w + 2, y - 10, 32, y - 3, p['s1'])
        b.rect(32 - w + 3, y - 9, 32 + w - 3, y - 5, p['a2'])
        b.rect(32 - w + 4, y - 8, 32 + w - 4, y - 7, p['a1'])
    b.rect(30, g - 58, 34, g - 50, p['s2'])
    b.ellipse(28, g - 62, 36, g - 56, p['a1'])
    door(b, 32, g - 4, 9, 9, 4)
    return fin(b, [(32, g - 30, 24, p['a1'])])


def greenhouse(e=4):
    """수경 재배 온실. 유리 돔 안이 온통 초록."""
    p = ERA[4]
    b = new()
    g = b.ground
    b.rect(6, g - 6, 58, g, p['s2'])
    b.rect(6, g - 6, 30, g, p['s1'])
    b.rect(6, g - 2, 58, g, p['s3'])
    b.ellipse(6, g - 40, 58, g - 4, C(186, 226, 232))
    b.ellipse(9, g - 37, 34, g - 8, C(214, 242, 246))
    for (x, w, h) in ((14, 9, 14), (26, 11, 20), (40, 9, 16)):   # 안의 식물
        b.poly([(x, g - 7), (x + w, g - 7), (x + w // 2, g - 7 - h)], p['g2'])
        b.poly([(x + 2, g - 7), (x + w - 2, g - 7), (x + w // 2, g - 5 - h)], p['g1'])
    b.rect(8, g - 12, 56, g - 9, p['a2'])                    # 수경 수조
    b.rect(9, g - 12, 55, g - 11, p['a1'])
    for x in range(10, 56, 8):                               # 유리 프레임
        b.line(x, g - 6, 32 + (x - 32) // 2, g - 37, C(108, 150, 172))
    b.rect(4, g - 44, 60, g - 40, p['s2'])
    b.rect(5, g - 44, 59, g - 43, p['s1'])
    door(b, 32, g - 6, 11, 11, 4)
    return fin(b, [(32, g - 20, 22, p['g1'])])


def landing_pad(e=4):
    """착륙장. 아주 낮고 넓은 패드 위에 셔틀이 떠 있다."""
    p = ERA[4]
    b = new()
    g = b.ground
    b.ellipse(0, g - 12, 63, g, p['s2'])
    b.ellipse(4, g - 10, 59, g - 2, p['s1'])
    b.ellipse(14, g - 8, 49, g - 4, p['s3'])
    for x in (2, 16, 32, 47, 60):                            # 유도등
        b.rect(x - 1, g - 12, x + 1, g - 10, p['a1'])
    b.poly([(16, g - 20), (48, g - 20), (42, g - 32), (22, g - 32)], p['s2'])
    b.poly([(16, g - 20), (32, g - 20), (32, g - 32), (22, g - 32)], p['s1'])
    b.rect(22, g - 29, 42, g - 25, p['a2'])
    b.rect(23, g - 28, 41, g - 27, p['a1'])
    b.poly([(12, g - 22), (20, g - 26), (20, g - 20)], p['s2'])   # 날개
    b.poly([(52, g - 22), (44, g - 26), (44, g - 20)], p['s2'])
    b.rect(24, g - 18, 28, g - 15, p['a1'])
    b.rect(36, g - 18, 40, g - 15, p['a1'])
    b.rect(58, g - 30, 61, g - 12, p['s2'])                  # 관제 기둥
    b.ellipse(56, g - 35, 63, g - 29, p['a1'])
    return fin(b, [(32, g - 16, 24, p['a1'])])


# ================================================================
BUILDINGS = [
    ("hut", hut), ("house", house), ("hall", hall), ("tower", tower),
]

# 이틀 머무는 시대(원시·미래)에만 붙는 추가 건물
BONUS = {
    0: [("rock_shelter", rock_shelter), ("drying_rack", drying_rack), ("cave_mouth", cave_mouth)],
    4: [("pod_stack", pod_stack), ("greenhouse", greenhouse), ("landing_pad", landing_pad)],
}

BLDG_NAMES = [
    ["움집", "큰 움집", "가죽 천막", "통나무 망루", "바위 그늘집", "건조대", "동굴 입구"],
    ["토담 오두막", "토담집", "흙벽 창고", "목조 망루"],
    ["초가집", "기와 한옥", "행랑채", "2층 누각"],
    ["판잣집", "벽돌 양옥", "벽돌 상가", "시계탑"],
    ["캡슐 주거", "패널 주택", "돔 회관", "타워", "캡슐 아파트", "수경 온실", "착륙장"],
]


def frames(e):
    out = [fn(e) for _, fn in BUILDINGS]
    for _, fn in BONUS.get(e, []):
        out.append(fn())
    return out


def build():
    return {f"era_buildings_{e}": sheet(frames(e), S, S) for e in range(5)}


if __name__ == "__main__":
    import os
    os.makedirs("out/Era", exist_ok=True)
    for name, img in build().items():
        img.save(f"out/Era/{name}.png")
        print(name, img.size)
