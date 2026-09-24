"""시대 에셋 공용 도구 — 크기 자유의 픽셀 캔버스 + 5시대 공통 팔레트.

era_art.B 는 48x48 고정이라, 32px 장식물과 64px 건물을 같이 다루려고 일반화했다.
바닥은 항상 (h-2) 줄, 마지막 줄은 그림자.
"""
from PIL import Image, ImageDraw

K = (26, 20, 28)                    # 외곽선 — 기존 에셋과 동일
SHADOW = (58, 54, 52, 170)


def rgba(c):
    return c if len(c) == 4 else (c[0], c[1], c[2], 255)


class Box:
    def __init__(self, w, h):
        self.w, self.h = w, h
        self.ground = h - 2
        self.img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
        self.d = ImageDraw.Draw(self.img)

    # ---------------- 기본 도형
    def rect(self, x0, y0, x1, y1, c):
        self.d.rectangle([min(x0, x1), min(y0, y1), max(x0, x1), max(y0, y1)], fill=rgba(c))

    def poly(self, pts, c):
        self.d.polygon(pts, fill=rgba(c))

    def line(self, x0, y0, x1, y1, c):
        self.d.line([x0, y0, x1, y1], fill=rgba(c))

    def ellipse(self, x0, y0, x1, y1, c):
        self.d.ellipse([min(x0, x1), min(y0, y1), max(x0, x1), max(y0, y1)], fill=rgba(c))

    def px(self, x, y, c):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.img.putpixel((int(x), int(y)), rgba(c))

    def solid(self, x, y):
        return (0 <= x < self.w and 0 <= y < self.h
                and self.img.getpixel((x, y))[3] >= 200)

    # ---------------- 마감
    def outline(self, c=K):
        add = []
        for y in range(self.h):
            for x in range(self.w):
                if self.img.getpixel((x, y))[3] > 0:
                    continue
                if any(self.solid(x + dx, y + dy) for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1))):
                    add.append((x, y))
        for (x, y) in add:
            self.px(x, y, c)

    def shadow(self, x0=None, x1=None):
        x0 = 0 if x0 is None else x0
        x1 = self.w - 1 if x1 is None else x1
        y = self.h - 1
        for x in range(max(0, x0), min(self.w, x1 + 1)):
            if self.img.getpixel((x, y))[3] == 0:
                self.px(x, y, SHADOW)

    def auto_shadow(self, pad=1):
        """실루엣이 닿는 x 범위에만 그림자를 깐다."""
        xs = [x for x in range(self.w)
              for y in range(self.h) if self.img.getpixel((x, y))[3] > 0]
        if not xs:
            return
        self.shadow(min(xs) - pad, max(xs) + pad)

    def glow(self, cx, cy, r, col, strength=0.5):
        for y in range(int(cy - r), int(cy + r) + 1):
            for x in range(int(cx - r), int(cx + r) + 1):
                if not (0 <= x < self.w and 0 <= y < self.h):
                    continue
                dist = ((x - cx) ** 2 + (y - cy) ** 2) ** 0.5
                if dist >= r:
                    continue
                a = int(255 * strength * (1 - dist / r) ** 2)
                if self.img.getpixel((x, y))[3] == 0 and a > 8:
                    self.px(x, y, (col[0], col[1], col[2], a))

    def streak(self, x0, x1, y0, y1, c, period=3, phase=0):
        """이미 칠한 면 위에 가로 결을 넣는다."""
        for y in range(y0, y1 + 1):
            if (y + phase) % period == 0:
                for x in range(x0, x1 + 1):
                    if self.solid(x, y):
                        self.px(x, y, c)


def C(r, g, b):
    return (r, g, b, 255)


# ==================================================================
# 5시대 공통 팔레트
#   w* 구조재(나무·기둥)   r* 지붕   s* 석재·기단   g* 잎·풀
#   a* 시대 시그니처 색     glow 빛   dark 어둠(입구·그늘)
# ==================================================================
ERA = [
    dict(  # 0 원시 — 통나무 / 짚 / 거친 돌 / 불
        name="원시",
        w1=C(160, 118, 78), w2=C(122, 86, 54), w3=C(84, 58, 36),
        r1=C(202, 166, 104), r2=C(158, 122, 70), r3=C(112, 84, 46),
        s1=C(154, 150, 140), s2=C(114, 110, 102), s3=C(80, 76, 72),
        g1=C(118, 152, 84), g2=C(80, 112, 60), g3=C(56, 80, 46),
        a1=C(198, 118, 78), a2=C(146, 80, 52),
        glow=C(250, 172, 68), dark=C(42, 32, 30),
    ),
    dict(  # 1 고대 — 흙벽 / 짚 / 토기
        name="고대",
        w1=C(150, 110, 70), w2=C(112, 80, 50), w3=C(78, 54, 34),
        r1=C(210, 178, 112), r2=C(166, 132, 76), r3=C(118, 90, 50),
        s1=C(176, 168, 150), s2=C(134, 126, 110), s3=C(94, 88, 78),
        g1=C(112, 148, 84), g2=C(76, 108, 60), g3=C(52, 78, 46),
        a1=C(186, 128, 86), a2=C(140, 92, 60),
        glow=C(248, 186, 96), dark=C(40, 32, 30),
    ),
    dict(  # 2 조선 — 흰 회벽 / 회청 기와 / 다듬은 돌
        name="조선",
        w1=C(150, 108, 68), w2=C(112, 78, 48), w3=C(76, 52, 32),
        r1=C(150, 164, 172), r2=C(106, 122, 134), r3=C(72, 86, 98),
        s1=C(206, 202, 190), s2=C(160, 156, 144), s3=C(112, 108, 100),
        g1=C(104, 146, 90), g2=C(70, 106, 64), g3=C(48, 76, 48),
        a1=C(178, 96, 80), a2=C(126, 62, 52),
        glow=C(250, 214, 140), dark=C(38, 32, 34),
    ),
    dict(  # 3 근대 — 붉은 벽돌 / 함석 / 콘크리트
        name="근대",
        w1=C(148, 112, 76), w2=C(108, 80, 52), w3=C(74, 54, 36),
        r1=C(158, 164, 172), r2=C(114, 120, 130), r3=C(78, 84, 94),
        s1=C(178, 96, 76), s2=C(134, 68, 54), s3=C(94, 46, 38),
        g1=C(108, 140, 86), g2=C(74, 102, 62), g3=C(50, 74, 46),
        a1=C(226, 190, 88), a2=C(168, 132, 52),
        glow=C(252, 220, 132), dark=C(36, 32, 34),
    ),
    dict(  # 4 미래 — 흰 패널 / 시안 네온 / 부유
        name="미래",
        w1=C(214, 222, 232), w2=C(150, 162, 178), w3=C(96, 106, 124),
        r1=C(196, 232, 240), r2=C(120, 172, 192), r3=C(64, 106, 130),
        s1=C(206, 214, 226), s2=C(146, 158, 176), s3=C(88, 98, 118),
        g1=C(126, 224, 210), g2=C(78, 168, 164), g3=C(48, 112, 118),
        a1=C(138, 240, 246), a2=C(72, 168, 190),
        glow=C(140, 242, 248), dark=C(26, 34, 46),
    ),
]


def sheet(images, cell_w, cell_h):
    out = Image.new("RGBA", (cell_w * len(images), cell_h), (0, 0, 0, 0))
    for i, im in enumerate(images):
        out.paste(im, (i * cell_w, 0))
    return out
