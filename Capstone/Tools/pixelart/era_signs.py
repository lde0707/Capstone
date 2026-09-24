"""건물 간판 — 투입소 / 강화소 / 복원소를 한눈에 구분시키는 고정 표식.

64x26, bottom-center 피벗. 건물 꼭대기 바로 위에 떠 있다.
시트 = 3스타일 x 3종류 = 9프레임 (576x26), 인덱스 = style * 3 + kind.

스타일   0 나무판 (원시·고대·조선)  1 함석판 (근대)  2 홀로 패널 (미래)
아이콘   초록 ▲ 재활용 / 주황 ↑ 강화 / 시안 ⧗ 복원   — 시대가 바뀌어도 고정
"""
from PIL import Image, ImageDraw, ImageFont

W, H = 64, 26
FONT_BLACK = "/usr/share/fonts/opentype/noto/NotoSansCJK-Bold.ttc"
K = (22, 18, 24, 255)

GREEN = ((168, 236, 140), (96, 178, 92), (48, 106, 58))
ORANGE = ((255, 212, 128), (248, 156, 56), (176, 86, 26))
CYAN = ((190, 252, 246), (96, 220, 226), (40, 132, 156))


def rgba(c):
    return c if len(c) == 4 else (c[0], c[1], c[2], 255)


class Sign:
    def __init__(self):
        self.img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
        self.d = ImageDraw.Draw(self.img)

    def rect(self, x0, y0, x1, y1, c):
        self.d.rectangle([min(x0, x1), min(y0, y1), max(x0, x1), max(y0, y1)], fill=rgba(c))

    def poly(self, pts, c):
        self.d.polygon(pts, fill=rgba(c))

    def px(self, x, y, c):
        if 0 <= x < W and 0 <= y < H:
            self.img.putpixel((int(x), int(y)), rgba(c))

    def glow(self, x0, y0, x1, y1, col, strength=0.5):
        for y in range(max(0, y0), min(H, y1 + 1)):
            for x in range(max(0, x0), min(W, x1 + 1)):
                if self.img.getpixel((x, y))[3] == 0:
                    self.px(x, y, (col[0], col[1], col[2], int(255 * strength * 0.35)))


# ---------------------------------------------------------------- 1비트 한글
def text_mask(text, size, spacing=0):
    f = ImageFont.truetype(FONT_BLACK, size)
    probe = ImageDraw.Draw(Image.new("1", (8, 8)))
    boxes = [probe.textbbox((0, 0), ch, font=f) for ch in text]
    widths = [b[2] - b[0] for b in boxes]
    top = min(b[1] for b in boxes)
    bot = max(b[3] for b in boxes)
    img = Image.new("1", (sum(widths) + spacing * (len(text) - 1) + 2, bot - top + 2), 0)
    d = ImageDraw.Draw(img)
    x = 1
    for ch, b, w in zip(text, boxes, widths):
        d.text((x - b[0], 1 - top), ch, font=f, fill=1)
        x += w + spacing
    return img


def draw_text(s, text, size, ox, oy, fill, outline=K, spacing=1):
    m = text_mask(text, size, spacing)
    p = m.load()
    pts = [(x, y) for y in range(m.height) for x in range(m.width) if p[x, y]]
    if not pts:
        return
    x0 = min(q[0] for q in pts)
    y0 = min(q[1] for q in pts)
    pts = [(x - x0 + ox, y - y0 + oy) for (x, y) in pts]
    have = set(pts)
    for (x, y) in pts:                                   # 외곽선 먼저
        for dx in (-1, 0, 1):
            for dy in (-1, 0, 1):
                if (x + dx, y + dy) not in have:
                    s.px(x + dx, y + dy, outline)
    for (x, y) in pts:
        s.px(x, y, fill)
    return max(q[0] for q in pts) - ox + 1


# ---------------------------------------------------------------- 아이콘
def icon_recycle(s, ox, oy, pal):
    """▲ 재활용 화살표."""
    lo, mid, dk = pal
    s.poly([(ox + 6, oy), (ox + 12, oy + 11), (ox, oy + 11)], mid)
    s.poly([(ox + 6, oy + 1), (ox + 10, oy + 10), (ox + 6, oy + 10)], dk)
    s.poly([(ox + 6, oy + 2), (ox + 9, oy + 9), (ox + 3, oy + 9)], lo)
    s.rect(ox, oy + 11, ox + 12, oy + 12, dk)


def icon_upgrade(s, ox, oy, pal):
    """↑ 강화 화살표 (아래에 받침)."""
    lo, mid, dk = pal
    s.poly([(ox + 6, oy - 1), (ox + 12, oy + 6), (ox, oy + 6)], mid)
    s.poly([(ox + 6, oy), (ox + 10, oy + 5), (ox + 6, oy + 5)], dk)
    s.poly([(ox + 6, oy + 1), (ox + 9, oy + 5), (ox + 3, oy + 5)], lo)
    s.rect(ox + 3, oy + 6, ox + 9, oy + 10, mid)
    s.rect(ox + 3, oy + 6, ox + 5, oy + 10, lo)
    s.rect(ox + 7, oy + 6, ox + 9, oy + 10, dk)
    s.rect(ox, oy + 11, ox + 12, oy + 12, dk)


def icon_restore(s, ox, oy, pal):
    """⧗ 모래시계 (시간 복원)."""
    lo, mid, dk = pal
    s.rect(ox + 1, oy, ox + 11, oy + 1, dk)
    s.rect(ox + 1, oy + 11, ox + 11, oy + 12, dk)
    s.poly([(ox + 2, oy + 2), (ox + 10, oy + 2), (ox + 6, oy + 6)], mid)
    s.poly([(ox + 2, oy + 2), (ox + 7, oy + 2), (ox + 6, oy + 5)], lo)
    s.poly([(ox + 2, oy + 11), (ox + 10, oy + 11), (ox + 6, oy + 6)], mid)
    s.poly([(ox + 4, oy + 10), (ox + 8, oy + 10), (ox + 6, oy + 8)], lo)
    s.px(ox + 6, oy + 7, lo)
    s.px(ox + 6, oy + 9, lo)


# ---------------------------------------------------------------- 판
STYLES = [
    dict(key="wood",
         body=(142, 100, 62), light=(186, 142, 92), dark=(94, 62, 38),
         text=(248, 238, 212), hook=(122, 84, 52), holo=False),
    dict(key="tin",
         body=(132, 138, 148), light=(186, 192, 200), dark=(74, 80, 90),
         text=(246, 248, 250), hook=(96, 102, 112), holo=False),
    dict(key="holo",
         body=(30, 44, 62), light=(58, 88, 116), dark=(18, 26, 40),
         text=(200, 250, 252), hook=(96, 190, 204), holo=True),
]


def plate(style):
    s = Sign()
    st = STYLES[style]
    # 걸이 고리 2개
    for x in (16, 46):
        s.rect(x, 0, x + 1, 4, st["hook"])
        s.px(x, 0, st["light"])
    # 판
    s.rect(1, 3, W - 2, 24, st["dark"])
    s.rect(2, 4, W - 3, 23, st["body"])
    s.rect(2, 4, W - 3, 5, st["light"])
    s.rect(2, 22, W - 3, 23, st["dark"])
    s.rect(0, 5, 1, 22, st["dark"])
    s.rect(W - 2, 5, W - 1, 22, st["dark"])
    if style == 0:                                        # 나무 결
        for y in (9, 14, 19):
            s.rect(3, y, W - 4, y, (118, 82, 50))
    if style == 1:                                        # 함석 리벳
        for x in (4, W - 5):
            for y in (7, 20):
                s.px(x, y, st["dark"])
    if style == 2:                                        # 홀로 주사선
        for y in range(6, 22, 3):
            s.rect(2, y, W - 3, y, (54, 96, 124))
    # 외곽선
    s.rect(1, 2, W - 2, 2, K)
    s.rect(1, 25, W - 2, 25, K)
    s.rect(0, 3, 0, 24, K)
    s.rect(W - 1, 3, W - 1, 24, K)
    return s


KINDS = [
    ("dropoff", "투입소", icon_recycle, GREEN),
    ("shop", "강화소", icon_upgrade, ORANGE),
    ("restorer", "복원소", icon_restore, CYAN),
]


def build_sign(style, kind):
    s = plate(style)
    _, label, icon, pal = KINDS[kind]
    icon(s, 4, 6, pal)
    draw_text(s, label, 14, 20, 6, STYLES[style]["text"], spacing=0)
    if STYLES[style]["holo"]:
        s.glow(0, 0, W - 1, H - 1, pal[1], 0.9)
    return s.img


def build():
    sheet = Image.new("RGBA", (W * 9, H), (0, 0, 0, 0))
    for style in range(3):
        for kind in range(3):
            sheet.paste(build_sign(style, kind), ((style * 3 + kind) * W, 0))
    return sheet


if __name__ == "__main__":
    import os
    os.makedirs("out/Era", exist_ok=True)
    sheet = build()
    sheet.save("out/Era/era_signs.png")
    big = sheet.resize((sheet.width * 5, sheet.height * 5), Image.NEAREST)
    plate_bg = Image.new("RGBA", big.size, (58, 62, 74, 255))
    plate_bg.alpha_composite(big)
    plate_bg.save("out/preview_signs.png")
    print("ok", sheet.size)
