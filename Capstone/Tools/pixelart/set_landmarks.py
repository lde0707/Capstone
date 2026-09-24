"""세트 보너스 랜드마크 6종 — 유물 한 분류를 전부 복원하면 마을에 들어서는 건물.

96 x 80, bottom-center 피벗. 일반 이웃 건물(64x64)보다 확실히 크고 화려해서
"저건 내가 도감을 다 채워서 생긴 것"이라는 게 멀리서도 읽혀야 한다.
시대와 무관하게 한 벌만 쓴다 (박물관·공원은 시대를 타지 않는다).

시트: set_landmarks.png (576 x 80 = 96 x 80 x 6)
순서: 공룡공원 · 요괴마을 · 기담관 · 박물관 · 추억의거리 · 미확인구역
"""
import math
from PIL import Image
from pixbox import Box, K, C

W, H = 96, 80

# ---------------------------------------------------------------- 팔레트
WOOD1, WOOD2, WOOD3 = C(176, 132, 84), C(126, 90, 54), C(84, 58, 34)
STONE1, STONE2, STONE3 = C(198, 194, 184), C(148, 144, 136), C(98, 94, 90)
ROOF1, ROOF2, ROOF3 = C(128, 148, 166), C(86, 104, 124), C(52, 66, 84)
LEAF1, LEAF2, LEAF3 = C(140, 194, 110), C(92, 148, 76), C(56, 100, 54)
BONE1, BONE2 = C(238, 232, 212), C(186, 178, 152)
RED1, RED2, RED3 = C(206, 88, 72), C(158, 56, 46), C(104, 34, 30)
GOLD1, GOLD2, GOLD3 = C(248, 216, 120), C(198, 158, 60), C(132, 100, 32)
CLOTH1, CLOTH2 = C(238, 234, 224), C(196, 190, 178)
CY1, CY2, CY3 = C(196, 250, 250), C(104, 214, 226), C(44, 132, 158)
METAL1, METAL2, METAL3 = C(206, 210, 220), C(148, 154, 168), C(92, 98, 114)
DARK = C(38, 32, 38)
NEON = C(250, 128, 96)


def box():
    return Box(W, H)


def fin(b, glows=None):
    b.outline()
    if glows:
        for (cx, cy, r, col) in glows:
            b.glow(cx, cy, r, col, 0.42)
    b.auto_shadow()
    return b.img


def base(b, half, h, c1=STONE1, c2=STONE2, c3=STONE3):
    """돌 기단 — 랜드마크는 전부 기단 위에 선다."""
    g = b.ground
    b.rect(48 - half, g - h, 48 + half, g, c2)
    b.rect(48 - half, g - h, 48 + half, g - h + 1, c1)
    b.rect(48 - half, g - 1, 48 + half, g, c3)
    b.rect(48 - half - 2, g - 2, 48 + half + 2, g, c2)
    b.rect(48 - half - 2, g - 2, 48 + half + 2, g - 2, c1)
    return g - h


def hanok_roof(b, cx, y, half, rows=9, c1=ROOF1, c2=ROOF2, c3=ROOF3):
    """끝이 살짝 올라간 기와 지붕."""
    for k in range(rows):
        t = k / (rows - 1.0)
        w = int(half * (0.42 + 0.58 * t))
        yy = y - rows + k + 1
        b.rect(cx - w, yy, cx + w, yy, c2 if k % 2 else c1)
    # 처마 — 양 끝을 한 칸씩 들어올린다
    b.rect(cx - half - 3, y - 1, cx + half + 3, y, c3)
    b.rect(cx - half - 3, y - 1, cx + half + 3, y - 1, c1)
    for d in (-1, 1):
        b.rect(cx + d * (half + 4), y - 3, cx + d * (half + 5), y - 1, c2)
        b.px(cx + d * (half + 5), y - 4, c1)
    b.rect(cx - half + 2, y - rows, cx + half - 2, y - rows + 1, c1)   # 용마루


def plaque(b, cx, y, text_color=GOLD1, w=13, h=5):
    """현판 — 글씨 대신 금색 획 세 줄."""
    b.rect(cx - w, y, cx + w, y + h, WOOD3)
    b.rect(cx - w + 1, y + 1, cx + w - 1, y + h - 1, C(58, 40, 26))
    for k in range(3):
        x = cx - 7 + k * 7
        b.rect(x, y + 2, x + 3, y + 2, text_color)
        b.px(x + 1, y + 3, text_color)


# ============================================================ 1 공룡 공원
def dino_park():
    b = box(); g = b.ground

    # 울타리 — 게이트 좌우로 낮게
    for (x0, x1) in ((1, 26), (70, 94)):
        for x in range(x0, x1, 9):
            b.rect(x, g - 15, x + 2, g, WOOD2)
            b.rect(x, g - 15, x, g, WOOD1)
            b.px(x + 1, g - 16, WOOD3)
        for y in (g - 13, g - 7):
            b.rect(x0, y, x1, y + 1, WOOD2)
            b.rect(x0, y, x1, y, WOOD1)

    # 게이트 기둥
    for d in (-1, 1):
        x = 48 + d * 21
        b.rect(x - 5, g - 44, x + 5, g, WOOD2)
        b.rect(x - 5, g - 44, x - 1, g, WOOD1)
        b.rect(x - 7, g - 4, x + 7, g, WOOD3)
        b.streak(x - 5, x + 5, g - 42, g - 6, WOOD3, 8)

    # 게이트 안쪽 — 숲과 공룡 실루엣이 보인다 (x 33~63 안에 가둔다)
    b.rect(33, g - 40, 63, g, C(126, 152, 116))
    b.rect(33, g - 40, 63, g - 33, C(158, 184, 146))
    b.ellipse(37, g - 23, 53, g - 11, LEAF3)                   # 몸통
    b.poly([(40, g - 19), (33, g - 25), (36, g - 13)], LEAF3)  # 꼬리
    b.poly([(48, g - 21), (51, g - 32), (56, g - 31), (54, g - 15)], LEAF3)  # 목
    b.ellipse(50, g - 36, 60, g - 29, LEAF3)                   # 머리
    b.px(57, g - 34, C(232, 240, 220))
    b.rect(40, g - 12, 43, g, LEAF3)                           # 다리
    b.rect(48, g - 12, 51, g, LEAF3)
    for (x, r) in ((35, 5), (60, 5)):                          # 수풀
        b.ellipse(max(33, x - r), g - 7 - r, min(63, x + r), g, LEAF2)

    # 상단 보 + 현판
    b.rect(20, g - 52, 76, g - 42, WOOD2)
    b.rect(20, g - 52, 76, g - 50, WOOD1)
    b.rect(20, g - 43, 76, g - 42, WOOD3)
    plaque(b, 48, g - 51, GOLD1, 19, 7)

    # 보 위의 육식공룡 두개골 (왼쪽을 보는 옆모습)
    sx, sy = 48, g - 57
    # 위턱 — 뒤통수는 높고 주둥이로 갈수록 낮아진다
    b.poly([(sx - 21, sy + 2), (sx - 14, sy - 3), (sx - 2, sy - 8),
            (sx + 9, sy - 10), (sx + 17, sy - 4), (sx + 17, sy + 3),
            (sx - 20, sy + 4)], BONE1)
    b.poly([(sx - 17, sy + 1), (sx - 8, sy - 3), (sx + 3, sy - 6),
            (sx + 12, sy - 6), (sx + 13, sy + 1), (sx - 16, sy + 2)], BONE2)
    b.ellipse(sx + 4, sy - 5, sx + 12, sy + 1, DARK)           # 큰 눈구멍
    b.ellipse(sx + 6, sy - 3, sx + 9, sy - 1, C(128, 108, 102))
    b.ellipse(sx - 3, sy - 3, sx + 1, sy, DARK)                # 콧구멍 앞 구멍
    b.ellipse(sx - 18, sy, sx - 15, sy + 2, DARK)              # 콧구멍
    b.rect(sx + 13, sy - 3, sx + 17, sy + 2, BONE2)            # 뒤통수 그늘
    # 아래턱
    b.poly([(sx - 20, sy + 4), (sx + 15, sy + 5), (sx + 13, sy + 9), (sx - 18, sy + 8)], BONE1)
    b.poly([(sx - 16, sy + 6), (sx + 11, sy + 6), (sx + 10, sy + 8), (sx - 15, sy + 7)], BONE2)
    for k in range(8):                                         # 이빨 — 위아래 맞물림
        x = sx - 18 + k * 4
        b.rect(x, sy + 4, x + 1, sy + 6, C(250, 248, 238))
    return fin(b, [(sx + 8, sy - 2, 16, C(210, 240, 190))])


# ============================================================ 2 요괴 마을
def yokai_shrine():
    b = box(); g = b.ground
    y = base(b, 32, 5, STONE1, STONE2, STONE3)

    # 장승 둘 — 사당보다 낮게 둬서 지붕을 가리지 않는다
    for d in (-1, 1):
        x = 48 + d * 40
        b.rect(x - 4, g - 30, x + 4, g, WOOD2)
        b.rect(x - 4, g - 30, x - 2, g, WOOD1)
        b.rect(x - 6, g - 34, x + 6, g - 29, RED2)
        b.rect(x - 6, g - 34, x + 6, g - 33, RED1)
        b.ellipse(x - 4, g - 33, x - 2, g - 31, C(246, 244, 238))
        b.ellipse(x + 2, g - 33, x + 4, g - 31, C(246, 244, 238))
        b.px(x - 3, g - 32, DARK); b.px(x + 3, g - 32, DARK)
        b.poly([(x - 6, g - 35), (x, g - 41), (x + 6, g - 35)], WOOD3)
        b.rect(x - 3, g - 26, x + 3, g - 24, WOOD3)

    # 사당 본채
    b.rect(22, y - 22, 74, y, WOOD2)
    b.rect(22, y - 22, 74, y - 20, WOOD1)
    for x in (22, 71):                                        # 모서리 기둥
        b.rect(x, y - 22, x + 3, y, WOOD3)
    b.rect(38, y - 18, 58, y, DARK)                           # 열린 문
    b.rect(38, y - 18, 58, y - 17, WOOD3)
    for x in range(41, 58, 5):
        b.rect(x, y - 16, x + 1, y - 2, C(70, 56, 48))
    plaque(b, 48, y - 22, GOLD1, 13, 5)                       # 현판은 처마 아래

    # 기와 지붕 — 본채보다 훨씬 넓고 높게
    hanok_roof(b, 48, y - 23, 36, 12)

    # 청사초롱 — 처마 밑에 매달린 도깨비불
    for (x, c) in ((30, C(150, 240, 200)), (48, C(180, 200, 255)), (66, C(240, 200, 140))):
        b.rect(x, y - 21, x + 1, y - 18, WOOD3)
        b.ellipse(x - 4, y - 18, x + 5, y - 10, c)
        b.ellipse(x - 2, y - 16, x + 3, y - 12, C(255, 255, 240))
    return fin(b, [(30, y - 14, 13, C(150, 240, 200)),
                   (48, y - 14, 13, C(180, 200, 255)),
                   (66, y - 14, 13, C(240, 200, 140))])


# ============================================================ 3 세계 기담관
def curio_hall():
    b = box(); g = b.ground
    y = base(b, 34, 4)

    # 줄무늬 천막 — 서커스풍 원뿔
    cx, top = 48, y - 52
    for k in range(40):
        t = k / 39.0
        half = int(4 + 32 * t)
        yy = top + k
        stripe = (int((yy - top) * 0.6) + (half // 6)) % 2 == 0
        b.rect(cx - half, yy, cx + half, yy, RED1 if stripe else CLOTH1)
    # 천막 아랫단 물결
    for k in range(9):
        x0 = cx - 36 + k * 8
        b.ellipse(x0, y - 15, x0 + 8, y - 9, CLOTH2 if k % 2 else RED2)
    b.rect(cx - 36, y - 13, cx + 36, y - 11, RED3)

    # 입구
    b.poly([(cx - 11, y), (cx - 9, y - 22), (cx + 9, y - 22), (cx + 11, y)], DARK)
    b.poly([(cx - 9, y), (cx - 7, y - 20), (cx - 2, y - 20), (cx - 3, y)], C(70, 56, 70))
    b.poly([(cx + 2, y - 20), (cx + 7, y - 20), (cx + 9, y), (cx + 3, y)], C(70, 56, 70))
    b.rect(cx - 13, y - 25, cx + 13, y - 21, GOLD2)
    b.rect(cx - 13, y - 25, cx + 13, y - 24, GOLD1)
    for k in range(3):
        b.rect(cx - 7 + k * 6, y - 24, cx - 5 + k * 6, y - 23, DARK)

    # 꼭대기 깃발 + 별
    b.rect(cx - 1, top - 10, cx + 1, top + 2, WOOD3)
    b.poly([(cx + 1, top - 10), (cx + 14, top - 6), (cx + 1, top - 2)], GOLD1)
    for (sx, sy) in ((18, y - 44), (78, y - 40), (48, top - 14)):
        b.px(sx, sy, C(255, 250, 210))
        b.px(sx - 1, sy + 1, GOLD1); b.px(sx + 1, sy + 1, GOLD1)
        b.px(sx, sy + 2, C(255, 250, 210))
    return fin(b, [(cx, y - 24, 18, C(255, 214, 150))])


# ============================================================ 4 국립 박물관
def museum():
    b = box(); g = b.ground
    # 넓은 돌계단
    for k in range(4):
        half = 40 - k * 3
        yy = g - k * 2
        b.rect(48 - half, yy - 2, 48 + half, yy, STONE2)
        b.rect(48 - half, yy - 2, 48 + half, yy - 2, STONE1)
    y = g - 8

    # 흰 회벽 본채
    b.rect(14, y - 30, 82, y, C(236, 232, 222))
    b.rect(14, y - 30, 82, y - 28, C(250, 248, 242))
    b.rect(14, y - 2, 82, y, STONE3)
    # 돌기둥 여섯
    for k in range(6):
        x = 18 + k * 12
        b.rect(x, y - 28, x + 6, y - 3, STONE1)
        b.rect(x + 4, y - 28, x + 6, y - 3, STONE2)
        b.rect(x - 1, y - 30, x + 7, y - 28, STONE1)
        b.rect(x - 1, y - 4, x + 7, y - 2, STONE1)
        b.streak(x, x + 6, y - 26, y - 6, STONE2, 4)
    # 가운데 입구
    b.rect(42, y - 22, 54, y - 2, DARK)
    b.rect(42, y - 22, 54, y - 21, STONE3)
    b.rect(47, y - 22, 49, y - 2, C(72, 66, 64))

    # 기와 지붕 + 현판
    hanok_roof(b, 48, y - 30, 38, 11)
    plaque(b, 48, y - 37, GOLD1, 16, 6)
    # 좌우 석사자
    for d in (-1, 1):
        x = 48 + d * 42
        b.rect(x - 4, g - 10, x + 4, g - 6, STONE2)
        b.ellipse(x - 5, g - 20, x + 5, g - 9, STONE1)
        b.ellipse(x - 3, g - 18, x + 3, g - 13, STONE3)
        b.px(x - 2, g - 16, DARK); b.px(x + 2, g - 16, DARK)
    return fin(b, [(48, y - 20, 16, C(255, 240, 200))])


# ============================================================ 5 추억의 거리
def retro_street():
    b = box(); g = b.ground
    # 양옆 낡은 상가
    for (x0, x1, c1, c2) in ((2, 30, C(196, 160, 118), C(150, 116, 80)),
                             (66, 94, C(168, 172, 160), C(122, 126, 116))):
        b.rect(x0, g - 40, x1, g, c2)
        b.rect(x0, g - 40, x1, g - 38, c1)
        b.rect(x0 + 3, g - 34, x1 - 3, g - 22, C(96, 120, 132))   # 유리창
        b.rect(x0 + 3, g - 34, x1 - 3, g - 33, C(150, 180, 190))
        for k in range(3):
            b.rect(x0 + 5 + k * 8, g - 33, x0 + 6 + k * 8, g - 23, C(140, 168, 180))
        b.rect(x0 + 6, g - 18, x1 - 6, g, C(72, 60, 54))          # 출입구
        b.rect(x0 + 6, g - 18, x1 - 6, g - 17, c1)
        # 차양
        for k in range((x1 - x0) // 4):
            xx = x0 + k * 4
            b.rect(xx, g - 22, xx + 3, g - 19, RED1 if k % 2 else CLOTH1)

    # 가운데 아치 간판
    for d in (-1, 1):
        x = 48 + d * 24
        b.rect(x - 3, g - 48, x + 3, g, METAL2)
        b.rect(x - 3, g - 48, x - 1, g, METAL1)
        b.rect(x - 5, g - 2, x + 5, g, METAL3)
    b.rect(20, g - 58, 76, g - 46, C(70, 58, 62))
    b.rect(20, g - 58, 76, g - 56, C(112, 96, 98))
    b.rect(23, g - 56, 73, g - 48, C(44, 36, 40))
    for k in range(6):                                            # 네온 글자
        x = 27 + k * 8
        b.rect(x, g - 54, x + 1, g - 50, NEON)
        b.rect(x, g - 54, x + 4, g - 53, NEON)
        b.rect(x, g - 51, x + 4, g - 50, C(255, 190, 150))
    for k in range(14):                                           # 전구 줄
        x = 22 + k * 4
        b.px(x, g - 45, GOLD1)
        b.px(x, g - 44, GOLD2)

    # 전봇대 + 전선
    b.rect(8, g - 62, 11, g - 40, WOOD3)
    b.rect(8, g - 62, 9, g - 40, WOOD2)
    b.rect(3, g - 58, 16, g - 56, WOOD3)
    for k in range(80):
        b.px(11 + k, g - 57 + int(4 * math.sin(k / 80.0 * math.pi)), C(58, 50, 52))
    return fin(b, [(48, g - 52, 20, C(255, 170, 130)),
                   (48, g - 44, 14, C(255, 224, 150))])


# ============================================================ 6 미확인 구역
def anomaly_zone():
    b = box(); g = b.ground
    # 철조망
    for x in range(4, 93, 14):
        b.rect(x, g - 26, x + 2, g, METAL3)
        b.rect(x, g - 26, x, g, METAL2)
    for y in (g - 24, g - 17, g - 10):
        for x in range(4, 94, 2):
            b.px(x, y, METAL2)
            b.px(x + 1, y + 1, METAL3)
    b.rect(4, g - 28, 94, g - 26, METAL2)

    # 경고 표지판
    b.rect(12, g - 40, 30, g - 28, C(232, 200, 72))
    b.rect(12, g - 40, 30, g - 39, C(252, 228, 120))
    b.poly([(21, g - 38), (27, g - 30), (15, g - 30)], DARK)
    b.rect(20, g - 36, 22, g - 33, C(232, 200, 72))
    b.px(21, g - 32, C(232, 200, 72))

    # 관측 돔
    y = g - 22
    b.rect(34, y - 16, 74, y, METAL2)
    b.rect(34, y - 16, 74, y - 14, METAL1)
    b.rect(34, y - 2, 74, y, METAL3)
    for k in range(16):                                   # 반구 돔
        t = k / 15.0
        half = int(20 * math.sin(math.acos(t * 0.96)))
        b.rect(54 - half, y - 16 - k, 54 + half, y - 16 - k, METAL2 if k % 3 else METAL1)
    b.ellipse(44, y - 30, 64, y - 20, CY3)
    b.ellipse(46, y - 29, 58, y - 23, CY2)
    b.px(49, y - 27, CY1)
    b.rect(40, y - 10, 50, y, DARK)                       # 입구
    b.rect(40, y - 10, 50, y - 9, METAL3)
    for k in range(4):                                    # 상태등
        b.rect(60 + k * 3, y - 8, 61 + k * 3, y - 6, CY2 if k % 2 else C(240, 130, 120))

    # 안테나
    b.rect(80, y - 40, 82, y, METAL3)
    b.rect(80, y - 40, 81, y, METAL2)
    for k in range(3):
        w = 9 - k * 2
        b.rect(81 - w, y - 36 + k * 8, 81 + w, y - 35 + k * 8, METAL2)
    b.ellipse(74, y - 48, 88, y - 38, METAL1)
    b.ellipse(77, y - 46, 85, y - 41, CY3)
    b.px(81, y - 50, CY1)

    # 떠 있는 빛 (UFO 는 따로 배회하므로 여기선 광선만)
    for k in range(5):
        b.px(54 - 8 + k * 4, y - 38 - (k % 3) * 3, CY1)
    return fin(b, [(54, y - 26, 20, C(120, 230, 240)),
                   (81, y - 44, 12, C(140, 240, 250))])


# ============================================================
ENTRIES = [
    ("dino", "공룡 공원", dino_park),
    ("yokai", "요괴 마을", yokai_shrine),
    ("beast", "세계 기담관", curio_hall),
    ("history", "국립 박물관", museum),
    ("modern", "추억의 거리", retro_street),
    ("mystery", "미확인 구역", anomaly_zone),
]

if __name__ == "__main__":
    import os
    os.makedirs("out/Era", exist_ok=True)
    sheet = Image.new("RGBA", (W * len(ENTRIES), H), (0, 0, 0, 0))
    for i, (_, _, fn) in enumerate(ENTRIES):
        sheet.paste(fn(), (i * W, 0))
    sheet.save("out/Era/set_landmarks.png")
    print("ok", sheet.size)
