"""5시대 1순위 에셋 — 시대별 집 1채 + 시간 복원소 (48x48, bottom-center, 16px 그리드 3칸)."""
from PIL import Image, ImageDraw

S = 48
GROUND = 46                     # 건물이 닿는 마지막 줄 (47 = 그림자)
K = (26, 20, 28)                # 외곽선 (기존 에셋과 동일)
SHADOW = (58, 54, 52, 170)

# 복원 에너지 — 모든 시대 복원소가 공유하는 시안 빛
CY1, CY2, CY3 = (170, 250, 244), (86, 214, 222), (44, 140, 160)


def rgba(c):
    return c if len(c) == 4 else (c[0], c[1], c[2], 255)


class B:
    def __init__(self):
        self.img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
        self.d = ImageDraw.Draw(self.img)

    def rect(self, x0, y0, x1, y1, c):
        x0, x1 = min(x0, x1), max(x0, x1)
        y0, y1 = min(y0, y1), max(y0, y1)
        self.d.rectangle([x0, y0, x1, y1], fill=rgba(c))

    def poly(self, pts, c):
        self.d.polygon(pts, fill=rgba(c))

    def line(self, x0, y0, x1, y1, c):
        self.d.line([x0, y0, x1, y1], fill=rgba(c))

    def ellipse(self, x0, y0, x1, y1, c):
        self.d.ellipse([x0, y0, x1, y1], fill=rgba(c))

    def px(self, x, y, c):
        if 0 <= x < S and 0 <= y < S:
            self.img.putpixel((int(x), int(y)), rgba(c))

    def solid(self, x, y):
        return 0 <= x < S and 0 <= y < S and self.img.getpixel((x, y))[3] >= 200

    def outline(self):
        """불투명 실루엣 바깥에 1px 외곽선."""
        add = []
        for y in range(S):
            for x in range(S):
                if self.img.getpixel((x, y))[3] > 0:
                    continue
                if any(self.solid(x + dx, y + dy) for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                    add.append((x, y))
        for (x, y) in add:
            self.px(x, y, K)

    def shadow(self, x0, x1):
        for x in range(x0, x1 + 1):
            if self.img.getpixel((x, 47))[3] == 0:
                self.px(x, 47, SHADOW)

    def glow(self, cx, cy, r, col, strength=0.55):
        """투명 영역에만 은은한 빛 번짐 (외곽선 뒤에 그린다)."""
        for y in range(int(cy - r), int(cy + r) + 1):
            for x in range(int(cx - r), int(cx + r) + 1):
                if not (0 <= x < S and 0 <= y < S):
                    continue
                d = ((x - cx) ** 2 + (y - cy) ** 2) ** 0.5
                if d >= r:
                    continue
                a = int(255 * strength * (1 - d / r) ** 2)
                old = self.img.getpixel((x, y))
                if old[3] == 0 and a > 8:
                    self.px(x, y, (col[0], col[1], col[2], a))


def dither_rows(b, x0, x1, y0, y1, c_light, c_dark, period=3, phase=0):
    """가로 결 텍스처 (짚·판자·기와 줄)."""
    for y in range(y0, y1 + 1):
        if (y + phase) % period == 0:
            for x in range(x0, x1 + 1):
                if b.img.getpixel((x, y))[3] >= 200:
                    b.px(x, y, c_dark)


# ================================================================== 1. 원시
STRAW1, STRAW2, STRAW3 = (206, 170, 104), (166, 128, 72), (118, 88, 50)
WOOD1, WOOD2 = (124, 88, 56), (84, 58, 36)
HIDE1, HIDE2 = (190, 146, 104), (146, 104, 70)
STONE1, STONE2, STONE3 = (156, 152, 142), (118, 114, 106), (86, 82, 78)
FIRE1, FIRE2, FIRE3 = (255, 236, 150), (250, 164, 64), (212, 84, 44)


def house_primitive():
    """원시 — 움집. 짚을 덮은 낮은 원뿔, 꼭대기로 삐져나온 나뭇가지 뼈대."""
    b = B()
    apex = (24, 13)
    b.poly([(5, GROUND), (43, GROUND), apex], STRAW2)
    b.poly([(5, GROUND), (24, GROUND), apex], STRAW1)          # 왼쪽 밝은 면
    # 짚 결 — 사선
    for i in range(-40, 40, 4):
        for t in range(0, 34):
            x, y = 24 + i * t / 34.0 * 0.5 + (t * 0.0), 13 + t
            xx = int(24 + (i / 40.0) * t * 0.55)
            if b.solid(xx, 13 + t):
                b.px(xx, 13 + t, STRAW3 if xx > 24 else STRAW2)
    # 나뭇가지 뼈대 (꼭대기 X자)
    for (x0, y0, x1, y1) in ((24, 13, 18, 5), (24, 13, 30, 5), (24, 13, 24, 6)):
        b.line(x0, y0, x1, y1, WOOD2)
        b.line(x0 + 1, y0, x1 + 1, y1, WOOD1)
    b.rect(20, 14, 28, 15, WOOD2)                               # 묶은 끈
    # 입구 — 어두운 구멍 + 걷어 올린 가죽
    b.poly([(18, GROUND), (30, GROUND), (24, 31)], (40, 28, 26))
    b.poly([(16, GROUND), (19, GROUND), (22, 33), (20, 32)], HIDE1)
    b.px(20, 36, HIDE2); b.px(18, 42, HIDE2)
    # 바닥 돌
    for (x, y) in ((6, 44), (40, 44), (11, 45), (35, 45)):
        b.rect(x, y, x + 2, y + 1, STONE2)
        b.px(x, y, STONE1)
    b.outline()
    b.shadow(4, 44)
    return b.img


def restorer_primitive():
    """원시 — 돌 제단. 고인돌 모양 제단 위에서 시안빛 불꽃이 타오른다."""
    b = B()
    # 기단
    b.rect(8, 40, 39, GROUND, STONE2)
    b.rect(8, 40, 39, 41, STONE1)
    for x in (14, 23, 32):
        b.line(x, 42, x, GROUND, STONE3)
    # 기둥 2개
    b.rect(12, 28, 17, 39, STONE2)
    b.rect(12, 28, 13, 39, STONE1)
    b.rect(30, 28, 35, 39, STONE2)
    b.rect(30, 28, 31, 39, STONE1)
    # 판석
    b.poly([(6, 27), (41, 25), (42, 28), (7, 30)], STONE2)
    b.line(6, 27, 41, 25, STONE1)
    b.line(7, 30, 42, 28, STONE3)
    # 불 — 바깥은 주황, 속은 시안(복원 에너지)
    b.poly([(18, 25), (30, 24), (28, 14), (24, 7), (21, 14)], FIRE2)
    b.poly([(20, 24), (28, 23), (26, 15), (24, 10), (22, 16)], FIRE1)
    b.poly([(22, 23), (26, 23), (25, 17), (24, 14), (23, 18)], CY1)
    # 룬 새김
    for (x, y) in ((14, 32), (15, 35), (32, 33), (33, 36)):
        b.px(x, y, CY2)
    b.outline()
    b.glow(24, 16, 14, CY2, 0.45)
    b.shadow(6, 41)
    return b.img


# ================================================================== 2. 고대
EARTH1, EARTH2, EARTH3 = (184, 128, 86), (148, 98, 64), (110, 70, 46)
POT1, POT2 = (182, 96, 66), (134, 64, 44)
ROPE = (112, 84, 48)


def house_ancient():
    """고대 — 토담집. 흙벽 + 두툼한 짚 지붕, 낮고 옆으로 넓다. 기와 없음."""
    b = B()
    # 흙벽
    b.rect(7, 30, 40, GROUND, EARTH2)
    b.rect(7, 30, 12, GROUND, EARTH1)
    for (x, y) in ((16, 34), (30, 38), (22, 42), (36, 33), (10, 40), (27, 32)):
        b.px(x, y, EARTH3)
        b.px(x + 1, y, EARTH3)
    # 짚 지붕 — 둥근 반원형, 처마가 짧다
    b.ellipse(2, 13, 45, 40, STRAW2)
    b.rect(2, 30, 45, 40, (0, 0, 0, 0))                          # 아래 반은 잘라낸다
    b.d.rectangle([2, 30, 45, 40], fill=(0, 0, 0, 0))
    b.ellipse(4, 14, 30, 36, STRAW1)
    b.rect(3, 29, 44, 31, STRAW3)                                # 처마 끝
    b.rect(4, 30, 43, 31, STRAW2)
    # 새끼줄 격자 (초가 지붕을 묶는 줄)
    for x in (12, 20, 28, 36):
        for y in range(15, 30):
            if b.solid(x, y):
                b.px(x, y, ROPE)
    for y in (20, 25):
        for x in range(3, 45):
            if b.solid(x, y) and b.img.getpixel((x, y))[:3] != ROPE:
                b.px(x, y, ROPE)
    # 흙벽 다시 (지붕이 덮은 부분 정리)
    b.rect(7, 32, 40, GROUND, EARTH2)
    b.rect(7, 32, 12, GROUND, EARTH1)
    for (x, y) in ((16, 35), (30, 39), (22, 43), (36, 34), (10, 41)):
        b.px(x, y, EARTH3)
    # 나무 문
    b.rect(18, 35, 25, GROUND, WOOD2)
    b.rect(19, 36, 24, GROUND, WOOD1)
    b.line(21, 36, 21, GROUND, WOOD2)
    # 작은 창
    b.rect(30, 36, 35, 40, (44, 30, 26))
    b.line(32, 36, 32, 40, WOOD1)
    # 토기 항아리
    b.ellipse(40, 38, 46, GROUND, POT1)
    b.rect(42, 37, 44, 38, POT2)
    b.px(41, 40, (214, 132, 96))
    b.outline()
    b.shadow(5, 47)
    return b.img


def restorer_ancient():
    """고대 — 토기 가마. 붉은 흙을 둥글게 쌓은 가마, 아궁이 속이 시안빛."""
    b = B()
    b.ellipse(5, 16, 43, 58, EARTH2)
    b.d.rectangle([0, GROUND + 1, 47, 47], fill=(0, 0, 0, 0))
    b.ellipse(7, 18, 26, 44, EARTH1)
    # 흙 쌓은 결
    for y in (24, 30, 36, 42):
        for x in range(5, 44):
            if b.solid(x, y) and (x + y) % 3:
                b.px(x, y, EARTH3)
    # 굴뚝 구멍 + 연기
    b.rect(21, 14, 27, 18, EARTH3)
    b.rect(22, 15, 26, 17, (40, 28, 26))
    for i, (x, y) in enumerate(((24, 11), (26, 8), (23, 5), (25, 2))):
        b.rect(x, y, x + 2, y + 1, (214, 212, 216) if i % 2 == 0 else (170, 168, 176))
    # 아궁이 — 불 + 시안 코어
    b.ellipse(16, 32, 32, 52, (38, 24, 24))
    b.d.rectangle([0, GROUND + 1, 47, 47], fill=(0, 0, 0, 0))
    b.ellipse(18, 35, 30, 50, FIRE3)
    b.d.rectangle([0, GROUND + 1, 47, 47], fill=(0, 0, 0, 0))
    b.ellipse(20, 38, 28, 50, FIRE2)
    b.d.rectangle([0, GROUND + 1, 47, 47], fill=(0, 0, 0, 0))
    b.ellipse(22, 40, 26, 48, CY1)
    b.d.rectangle([0, GROUND + 1, 47, 47], fill=(0, 0, 0, 0))
    # 옆에 굽는 중인 토기
    b.ellipse(37, 38, 45, GROUND, POT1)
    b.rect(39, 37, 43, 38, POT2)
    b.px(38, 40, (214, 132, 96))
    b.outline()
    b.glow(24, 42, 10, CY2, 0.35)
    b.shadow(5, 45)
    return b.img


# ================================================================== 3. 조선
WALL1, WALL2 = (238, 232, 218), (200, 192, 176)
PILLAR1, PILLAR2 = (116, 78, 50), (80, 52, 34)
TILE1, TILE2, TILE3 = (140, 152, 166), (100, 112, 128), (66, 74, 90)
BASE1, BASE2 = (170, 166, 156), (126, 122, 114)
PAPER = (244, 234, 204)
CELADON1, CELADON2 = (160, 214, 190), (100, 164, 144)


def giwa_roof(b, x0, x1, y_ridge, y_eave, lift=4):
    """양끝이 올라간 곡선 기와지붕."""
    cx = (x0 + x1) / 2.0
    half = (x1 - x0) / 2.0
    for x in range(x0, x1 + 1):
        t = abs(x - cx) / half
        eave = int(round(y_eave - lift * t ** 3))                # 처마 곡선
        top = int(round(y_ridge + 3 * t ** 1.4))                 # 지붕면
        for y in range(top, eave + 1):
            b.px(x, y, TILE2)
        # 기와 골 (세로줄)
        if x % 3 == 0:
            for y in range(top + 1, eave):
                b.px(x, y, TILE3)
        b.px(x, eave, TILE3)                                     # 처마 끝선
        b.px(x, top, TILE1)
    # 용마루 (위쪽 두꺼운 선) + 양끝 치켜 올림
    b.rect(int(cx - half * 0.55), y_ridge - 2, int(cx + half * 0.55), y_ridge, TILE3)
    b.rect(int(cx - half * 0.55), y_ridge - 2, int(cx + half * 0.55), y_ridge - 2, TILE1)
    for s in (-1, 1):
        ex = int(cx + s * half * 0.55)
        b.rect(ex, y_ridge - 4, ex + s, y_ridge - 1, TILE3)


def house_joseon():
    """조선 — 기와 한옥. 흰 회벽 + 나무 기둥 + 곡선 기와지붕, 돌 기단."""
    b = B()
    # 기단
    b.rect(5, 42, 42, GROUND, BASE1)
    b.rect(5, 42, 42, 42, (196, 192, 182))
    for x in (12, 21, 30, 38):
        b.line(x, 43, x, GROUND, BASE2)
    # 벽 + 기둥
    b.rect(9, 27, 38, 41, WALL1)
    b.rect(34, 27, 38, 41, WALL2)
    for x in (9, 18, 29, 38):
        b.rect(x, 26, x + 1, 41, PILLAR2)
        b.px(x, 26, PILLAR1)
    b.rect(9, 26, 39, 27, PILLAR1)                               # 도리
    # 창호 (한지 격자)
    for (x0, x1) in ((12, 16), (31, 35)):
        b.rect(x0, 30, x1, 38, PAPER)
        for x in range(x0, x1 + 1, 2):
            b.line(x, 30, x, 38, PILLAR1)
        for y in (32, 35):
            b.line(x0, y, x1, y, PILLAR1)
    # 대청 문 (가운데)
    b.rect(20, 30, 27, 41, PILLAR1)
    b.rect(21, 31, 23, 41, PAPER)
    b.rect(24, 31, 26, 41, PAPER)
    for y in (33, 36, 39):
        b.line(21, y, 26, y, PILLAR1)
    # 기와지붕
    giwa_roof(b, 1, 46, 13, 25, lift=5)
    b.outline()
    b.shadow(4, 43)
    return b.img


def restorer_joseon():
    """조선 — 오름가마. 비탈을 따라 오르는 긴 가마 + 앞에 놓인 청자."""
    b = B()
    # 가마 몸체 — 왼쪽 아래에서 오른쪽 위로 오르는 긴 경사
    body = [(12, GROUND), (45, GROUND), (45, 22), (12, 38)]
    b.poly(body, WALL2)
    # 윗면 둥근 칸 3개 (경사선 위에 혹처럼)
    for (cx, cy, r) in ((18, 35, 5), (28, 30, 6), (38, 25, 6)):
        b.ellipse(cx - r, cy - r, cx + r, cy + r, WALL2)
        b.ellipse(cx - r + 1, cy - r + 1, cx, cy, WALL1)
    # 칸 사이 경계선
    for x in (23, 33):
        for y in range(0, GROUND + 1):
            if b.solid(x, y):
                b.px(x, y, BASE2)
    # 아래쪽 그늘
    b.rect(12, GROUND - 2, 45, GROUND, BASE2)
    # 불창 (아치) — 주황 + 시안 코어
    for (x, y) in ((16, 39), (26, 35), (36, 30)):
        b.rect(x, y, x + 4, y + 5, (40, 26, 24))
        b.rect(x + 1, y + 1, x + 3, y + 5, FIRE2)
        b.px(x + 2, y + 3, CY1)
        b.px(x + 2, y + 4, CY1)
    # 굴뚝 + 연기
    b.rect(40, 9, 43, 21, BASE2)
    b.rect(40, 9, 41, 21, BASE1)
    b.rect(39, 9, 44, 10, PILLAR2)
    for i, (x, y) in enumerate(((41, 6), (43, 3), (40, 0))):
        b.rect(x, y, x + 2, y + 1, (214, 212, 216) if i % 2 == 0 else (170, 168, 176))
    # 앞에 놓인 청자 (나무 받침 위)
    b.rect(1, 42, 10, 43, PILLAR1)
    b.rect(2, 44, 3, GROUND, PILLAR2)
    b.rect(8, 44, 9, GROUND, PILLAR2)
    b.ellipse(2, 31, 9, 41, CELADON2)             # 몸통
    b.ellipse(3, 32, 6, 38, CELADON1)
    b.rect(4, 28, 7, 31, CELADON2)                 # 목
    b.rect(3, 27, 8, 27, CELADON2)                 # 입
    b.px(5, 29, CELADON1)
    b.outline()
    b.glow(5, 35, 8, CY2, 0.40)
    b.shadow(1, 46)
    return b.img


# ================================================================== 4. 근대
BRICK1, BRICK2, BRICK3 = (176, 92, 70), (140, 68, 52), (104, 48, 40)
TIN1, TIN2 = (150, 156, 160), (104, 110, 116)
FRAME = (232, 228, 214)
GLASS1, GLASS2 = (148, 178, 196), (96, 124, 146)
ANT = (60, 60, 66)


def bricks(b, x0, y0, x1, y1):
    b.rect(x0, y0, x1, y1, BRICK2)
    for y in range(y0, y1 + 1):
        if (y - y0) % 3 == 2:
            b.line(x0, y, x1, y, BRICK3)
        else:
            off = 0 if ((y - y0) // 3) % 2 == 0 else 3
            for x in range(x0 + off, x1 + 1, 6):
                b.px(x, y, BRICK3)
    b.rect(x0, y0, x0 + 3, y1, BRICK1)
    for y in range(y0, y1 + 1):
        if (y - y0) % 3 == 2:
            b.line(x0, y, x0 + 3, y, BRICK3)


def house_modern():
    """근대 — 2층 붉은 벽돌 양옥. 함석 지붕 + TV 안테나 = 60~70년대."""
    b = B()
    bricks(b, 8, 17, 39, GROUND)
    b.rect(8, 31, 39, 32, (196, 190, 176))                       # 층 구분 띠
    # 함석 지붕 (낮은 박공)
    b.poly([(4, 17), (43, 17), (37, 10), (10, 10)], TIN1)
    for x in range(6, 42, 3):
        b.line(x, 11, x, 16, TIN2)
    b.line(4, 17, 43, 17, TIN2)
    # TV 안테나
    b.line(33, 10, 33, 1, ANT)
    for (y, w) in ((2, 5), (4, 4), (6, 3)):
        b.line(33 - w, y, 33 + w, y, ANT)
    # 굴뚝
    b.rect(13, 5, 16, 11, BRICK2)
    b.rect(13, 5, 14, 11, BRICK1)
    # 창문 (흰 틀 + 유리)
    for (x, y) in ((11, 20), (29, 20), (11, 35), (29, 35)):
        b.rect(x, y, x + 7, y + 7, FRAME)
        b.rect(x + 1, y + 1, x + 6, y + 6, GLASS2)
        b.rect(x + 1, y + 1, x + 3, y + 3, GLASS1)
        b.line(x + 4, y + 1, x + 4, y + 6, FRAME)
        b.line(x + 1, y + 4, x + 6, y + 4, FRAME)
    # 현관 + 차양
    b.rect(20, 36, 27, GROUND, WOOD2)
    b.rect(21, 37, 26, GROUND, WOOD1)
    b.px(25, 41, FIRE1)
    b.rect(18, 33, 29, 34, TIN2)
    b.outline()
    b.shadow(6, 41)
    return b.img


def restorer_modern():
    """근대 — 기계 작업장. 벽돌 공방 + 톱니바퀴 + 시안 액체가 든 복원 탱크."""
    b = B()
    bricks(b, 4, 22, 30, GROUND)
    # 톱니 지붕 (공장 지붕)
    for i in range(3):
        x = 4 + i * 9
        b.poly([(x, 22), (x + 9, 22), (x + 9, 15)], TIN1)
        b.line(x + 9, 15, x + 9, 22, TIN2)
    # 굴뚝 + 연기
    b.rect(8, 4, 12, 16, BRICK2)
    b.rect(8, 4, 9, 16, BRICK1)
    b.rect(7, 4, 13, 5, BRICK3)
    for i, (x, y) in enumerate(((9, 1), (11, -1))):
        b.rect(x, max(0, y), x + 2, max(0, y) + 1, (170, 168, 176))
    # 톱니바퀴
    cx, cy = 17, 34
    for a in range(0, 360, 45):
        import math
        tx = cx + int(round(6 * math.cos(math.radians(a))))
        ty = cy + int(round(6 * math.sin(math.radians(a))))
        b.rect(tx - 1, ty - 1, tx + 1, ty + 1, TIN2)
    b.ellipse(cx - 5, cy - 5, cx + 5, cy + 5, TIN1)
    b.ellipse(cx - 2, cy - 2, cx + 2, cy + 2, TIN2)
    # 복원 탱크 (유리관 + 시안 액체)
    b.rect(32, 14, 44, 16, TIN2)                                  # 뚜껑
    b.rect(33, 17, 43, 40, (70, 96, 110))                         # 유리 테두리
    b.rect(34, 18, 42, 39, (120, 160, 176))
    b.rect(34, 24, 42, 39, CY2)                                   # 액체
    b.rect(35, 24, 36, 39, CY1)
    for (x, y) in ((38, 30), (40, 26), (37, 34)):                 # 기포
        b.px(x, y, CY1)
    b.rect(31, 40, 45, GROUND, TIN2)                              # 받침
    b.rect(31, 40, 45, 41, TIN1)
    b.line(30, 28, 32, 28, TIN2)                                  # 배관
    b.line(30, 29, 32, 29, TIN2)
    b.outline()
    b.glow(38, 30, 12, CY2, 0.35)
    b.shadow(3, 46)
    return b.img


# ================================================================== 5. 미래
WHITE1, WHITE2, WHITE3 = (244, 246, 250), (206, 212, 222), (158, 166, 180)
DARKMETAL1, DARKMETAL2 = (84, 92, 110), (54, 60, 76)
NEON = (255, 110, 196)


def house_future():
    """미래 — 부유 캡슐 주택. 공중에 뜬 흰 캡슐 + 통유리 + 아래 빛기둥."""
    b = B()
    # 착륙 패드
    b.rect(12, 43, 35, GROUND, DARKMETAL2)
    b.rect(12, 43, 35, 43, DARKMETAL1)
    b.rect(16, 44, 31, 44, CY2)
    # 캡슐 본체 (둥근 사각)
    b.ellipse(5, 8, 42, 34, WHITE2)
    b.ellipse(6, 8, 30, 30, WHITE1)
    b.rect(5, 22, 42, 28, WHITE2)
    b.ellipse(5, 20, 42, 34, WHITE3)
    b.ellipse(6, 19, 41, 31, WHITE2)
    # 통유리 띠
    b.rect(9, 16, 38, 23, (40, 72, 96))
    b.rect(10, 17, 37, 22, (74, 146, 180))
    b.rect(10, 17, 18, 19, (170, 226, 240))
    for x in (19, 28):
        b.line(x, 16, x, 23, WHITE3)
    # 네온 라인
    b.line(8, 26, 39, 26, CY2)
    b.px(12, 12, NEON); b.px(13, 12, NEON)
    # 안테나 비콘
    b.line(24, 3, 24, 8, WHITE3)
    b.rect(23, 1, 25, 2, CY1)
    b.outline()
    # 부양 빛기둥 (반투명)
    for y in range(35, 43):
        for x in range(17, 31):
            if b.img.getpixel((x, y))[3] == 0:
                a = int(120 * (1 - abs(x - 23.5) / 7.5))
                b.px(x, y, (CY2[0], CY2[1], CY2[2], max(20, a)))
    b.glow(24, 2, 5, CY1, 0.5)
    b.shadow(10, 37)
    return b.img


def restorer_future():
    """미래 — 홀로그램 복원소. 세로 링 안에 유물 홀로그램이 떠 있다."""
    import math
    b = B()
    # 받침 플랫폼
    b.poly([(6, GROUND), (41, GROUND), (37, 39), (10, 39)], DARKMETAL1)
    b.rect(10, 39, 37, 40, WHITE3)
    b.rect(12, 43, 35, 43, CY2)
    # 링 (두꺼운 원)
    cx, cy, r = 24, 22, 15
    for y in range(cy - r - 1, cy + r + 2):
        for x in range(cx - r - 1, cx + r + 2):
            d = math.hypot(x - cx, y - cy)
            if r - 3 <= d <= r:
                b.px(x, y, WHITE1 if (x - cx) < -(y - cy) * 0.3 else WHITE3)
            if r - 3 <= d <= r - 2 and (x + y) % 2 == 0:
                b.px(x, y, CY2)
    # 링 지지대
    b.rect(10, 34, 13, 39, WHITE3)
    b.rect(34, 34, 37, 39, WHITE3)
    b.outline()
    # 홀로그램 — 링 안에 떠 있는 유물(항아리) 실루엣, 스캔라인
    vase = {11: (23, 25), 12: (22, 26), 13: (23, 25), 14: (23, 25),
            15: (21, 27), 16: (20, 28), 17: (19, 29), 18: (19, 29),
            19: (19, 29), 20: (19, 29), 21: (20, 28), 22: (20, 28),
            23: (21, 27), 24: (21, 27), 25: (22, 26), 26: (20, 28)}
    for y, (x0, x1) in vase.items():
        for x in range(x0, x1 + 1):
            edge = x in (x0, x1) or y in (11, 26)
            a = 235 if edge else (170 if y % 2 == 0 else 95)
            b.px(x, y, (CY1[0], CY1[1], CY1[2], a))
    for (x, y) in ((16, 14), (32, 18), (15, 24), (33, 27)):          # 떠다니는 입자
        b.px(x, y, (CY1[0], CY1[1], CY1[2], 200))
    # 바닥에서 올라오는 빛
    for y in range(28, 39):
        for x in range(18, 30):
            if b.img.getpixel((x, y))[3] == 0:
                b.px(x, y, (CY2[0], CY2[1], CY2[2], 60))
    b.glow(24, 22, 20, CY2, 0.22)
    b.shadow(5, 42)
    return b.img


ERAS = [
    ("원시", "움집", house_primitive, "돌 제단", restorer_primitive),
    ("고대", "토담집", house_ancient, "토기 가마", restorer_ancient),
    ("조선", "기와 한옥", house_joseon, "오름가마", restorer_joseon),
    ("근대", "벽돌 양옥", house_modern, "기계 작업장", restorer_modern),
    ("미래", "부유 캡슐", house_future, "홀로그램 복원소", restorer_future),
]


def sheet(imgs):
    out = Image.new("RGBA", (S * len(imgs), S), (0, 0, 0, 0))
    for i, im in enumerate(imgs):
        out.alpha_composite(im, (i * S, 0))
    return out


if __name__ == "__main__":
    import os
    os.makedirs("out/Era", exist_ok=True)
    houses = [e[2]() for e in ERAS]
    restorers = [e[4]() for e in ERAS]
    sheet(houses).save("out/Era/era_houses.png")
    sheet(restorers).save("out/Era/era_restorers.png")
    print("ok")
