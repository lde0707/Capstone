"""유물 아트 공용 — 발굴본(32x32) / 복원본(48x48, 2프레임).

발굴본  : 흙때 낀 화석·깨진 물건. 뼈·돌·녹 색으로 톤을 눌러 "오래됐다"를 만든다.
복원본  : 제 색을 되찾은 모습. 배회형은 2프레임이 걷기, 전시형은 2프레임이 숨쉬기.
"""
from PIL import Image
from pixbox import Box, K, C

BW, AW = 32, 64          # buried / alive 캔버스 (복원본은 사방 8px 여백을 둔다)

# ---------------------------------------------------------------- 공용 색
BONE1, BONE2, BONE3 = C(236, 230, 210), C(190, 180, 154), C(134, 124, 102)
STONE1, STONE2, STONE3 = C(160, 156, 146), C(120, 116, 106), C(84, 80, 74)
DIRT1, DIRT2 = C(126, 100, 68), C(88, 68, 46)
RUST1, RUST2 = C(168, 104, 58), C(112, 66, 34)
IRON1, IRON2, IRON3 = C(198, 204, 214), C(140, 148, 160), C(88, 94, 106)
GOLD1, GOLD2, GOLD3 = C(248, 214, 110), C(198, 158, 56), C(134, 102, 30)
JADE1, JADE2, JADE3 = C(148, 196, 176), C(98, 148, 132), C(62, 102, 92)
PORC1, PORC2 = C(244, 240, 230), C(198, 194, 182)
CY1, CY2, CY3 = C(190, 252, 246), C(96, 220, 226), C(40, 132, 156)
FIRE1, FIRE2, FIRE3 = C(255, 236, 150), C(250, 164, 64), C(212, 84, 44)
DARK = C(38, 30, 34)
BLOOD = C(176, 62, 58)

# 복원 생물 살색 (공룡·괴수 공용)
SK = [C(150, 176, 124), C(112, 136, 92), C(78, 98, 66)]        # 초록 계열
SB = [C(176, 140, 104), C(134, 104, 74), C(94, 72, 50)]        # 갈색 계열
SG = [C(168, 168, 178), C(126, 126, 138), C(88, 88, 100)]      # 회색 계열
SW = [C(240, 238, 232), C(198, 196, 190), C(148, 146, 142)]    # 흰색 계열


MTX1, MTX2, MTX3 = C(106, 84, 62), C(76, 58, 42), C(48, 36, 28)


def matrix(b, seed=0):
    """화석이 박힌 어두운 암석 덩어리. 밝은 뼈가 확실히 도드라지게 어둡게 깐다."""
    w, h = b.w, b.h
    b.ellipse(2, 3, w - 3, h - 3, MTX2)
    b.ellipse(4, 5, w - 5, h - 5, MTX1)
    for (x, y, rw, rh) in ((2, 9, 5, 7), (w - 9, 6, 6, 6), (10, h - 9, 8, 6), (w - 12, h - 8, 7, 5)):
        b.ellipse(x, y, x + rw, y + rh, MTX2)
        b.ellipse(x + 1, y + 1, x + rw - 2, y + rh - 2, MTX1)
    for i in range(w):
        for j in range(h):
            if b.solid(i, j) and (i * 5 + j * 11 + seed * 17) % 17 == 0:
                b.px(i, j, MTX3)


def buried():
    return Box(BW, BW)


class AliveBox(Box):
    """복원본 전용 캔버스. 그리는 좌표는 그대로 쓰되 사방에 여백을 둬서 잘리지 않게 한다.

    기존 48x48 기준으로 짠 좌표(x 0~48, y 는 ground 기준)를 그대로 받아
    64x64 캔버스 가운데로 밀어 넣는다. ground 가 62 라 위쪽 여유도 16px 늘어난다.
    """

    def __init__(self):
        super().__init__(AW, AW)
        self.OX = 8

    def _raw(self, fn, *args):
        """외곽선·그림자는 실제 캔버스 좌표로 돌아야 하므로 오프셋을 잠시 끈다."""
        keep, self.OX = self.OX, 0
        try:
            return fn(*args)
        finally:
            self.OX = keep

    def outline(self, c=K):
        return self._raw(super().outline, c)

    def shadow(self, x0=None, x1=None):
        return self._raw(super().shadow, x0, x1)

    def auto_shadow(self, pad=1):
        return self._raw(super().auto_shadow, pad)

    def rect(self, x0, y0, x1, y1, c):
        super().rect(x0 + self.OX, y0, x1 + self.OX, y1, c)

    def poly(self, pts, c):
        super().poly([(x + self.OX, y) for (x, y) in pts], c)

    def line(self, x0, y0, x1, y1, c):
        super().line(x0 + self.OX, y0, x1 + self.OX, y1, c)

    def ellipse(self, x0, y0, x1, y1, c):
        super().ellipse(x0 + self.OX, y0, x1 + self.OX, y1, c)

    def px(self, x, y, c):
        super().px(x + self.OX, y, c)

    def solid(self, x, y):
        return super().solid(x + self.OX, y)

    def glow(self, cx, cy, r, col, strength=0.5):
        super().glow(cx + self.OX, cy, r, col, strength)

    def streak(self, x0, x1, y0, y1, c, period=3, phase=0):
        super().streak(x0 + self.OX, x1 + self.OX, y0, y1, c, period, phase)


def alive():
    return AliveBox()


def clipped(img):
    """스프라이트가 캔버스 테두리에 닿았는지 — 닿으면 잘린 것이다.

    맨 아랫줄은 그림자를 일부러 까는 자리라 검사에서 뺀다.
    """
    w, h = img.size
    p = img.load()
    sides = set()
    for x in range(w):
        if p[x, 0][3] > 0:
            sides.add("위")
    for y in range(h - 1):
        if p[0, y][3] > 0:
            sides.add("왼쪽")
        if p[w - 1, y][3] > 0:
            sides.add("오른쪽")
    return sorted(sides)


def fin(b, glow=None, shade=True):
    b.outline()
    if glow:
        for (cx, cy, r, col) in glow:
            b.glow(cx, cy, r, col, 0.45)
    if shade:
        b.auto_shadow()
    return b.img


def grime(b, seed=0, amount=10):
    """발굴본에 흙때를 찍는다 — 실루엣 위에만."""
    n = 0
    for y in range(b.h):
        for x in range(b.w):
            if not b.solid(x, y):
                continue
            if (x * 7 + y * 13 + seed * 29) % 23 == 0:
                b.px(x, y, DIRT2 if n % 2 else DIRT1)
                n += 1
                if n >= amount:
                    return


def crack(b, pts, c=DARK):
    """금 간 선."""
    for i in range(len(pts) - 1):
        b.line(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1], c)


def bone_rod(b, x0, y0, x1, y1, thick=2, c1=BONE1, c2=BONE2):
    """양 끝이 굵은 뼈대."""
    b.line(x0, y0, x1, y1, c2)
    for t in range(1, thick):
        b.line(x0, y0 + t, x1, y1 + t, c2)
    b.line(x0, y0, x1, y1, c1)
    for (x, y) in ((x0, y0), (x1, y1)):
        b.ellipse(x - 1, y - 1, x + 1, y + 1, c1)


def pedestal(b, half=13, h=5, c1=STONE1, c2=STONE2, c3=STONE3):
    """전시형 좌대."""
    cx, g = b.w // 2, b.ground
    b.rect(cx - half, g - h, cx + half, g, c2)
    b.rect(cx - half, g - h, cx + half, g - h + 1, c1)
    b.rect(cx - half, g - 1, cx + half, g, c3)
    b.rect(cx - half + 2, g - h - 2, cx + half - 2, g - h, c1)


def bob(frame, a=0, b_=1):
    """전시형 숨쉬기 / 배회형 위아래 흔들림."""
    return a if frame == 0 else b_


def legs2(b, cx, g, span, frame, c1, c2, thick=3, h=8):
    """네발 짐승 다리 2쌍 — 프레임마다 앞뒤가 바뀐다."""
    front = cx + span if frame == 0 else cx + span - 3
    back = cx - span if frame == 0 else cx - span + 3
    for (x, c) in ((back, c2), (front, c1)):
        b.rect(x - thick // 2, g - h, x + thick // 2, g, c)
        b.rect(x - thick // 2, g - 1, x + thick // 2, g, c2)


def sheet_row(images, cw, ch):
    out = Image.new("RGBA", (cw * len(images), ch), (0, 0, 0, 0))
    for i, im in enumerate(images):
        out.paste(im, (i * cw, 0))
    return out


def build_sheets(entries, tag):
    """entries: [(key, name, buried_fn, alive_fn)] → (buried 시트, alive 시트)"""
    bs = sheet_row([e[2]() for e in entries], BW, BW)
    al = []
    for e in entries:
        al.append(e[3](0))
        al.append(e[3](1))
    return bs, sheet_row(al, AW, AW)
