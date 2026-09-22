"""쓰레기 집게 소년 — 픽셀 로고 3종."""
import math
from PIL import Image, ImageDraw, ImageFont

FONT_BLACK = "/usr/share/fonts/opentype/noto/NotoSansCJK-Black.ttc"
FONT_BOLD = "/usr/share/fonts/opentype/noto/NotoSansCJK-Bold.ttc"

# 인게임 팔레트 계열
K = (26, 20, 28, 255)
Y1 = (255, 226, 136, 255)
Y2 = (233, 186, 74, 255)
Y3 = (172, 126, 42, 255)
TEAL1 = (156, 212, 200, 255)
TEAL2 = (100, 160, 154, 255)
TEAL3 = (58, 106, 106, 255)
GREEN1 = (150, 218, 138, 255)
GREEN2 = (92, 176, 108, 255)
GREEN3 = (50, 118, 74, 255)
METAL1 = (192, 196, 200, 255)
METAL2 = (142, 146, 152, 255)
METAL3 = (92, 96, 102, 255)
RUST = (166, 102, 66, 255)
BROWN1 = (152, 114, 76, 255)
BROWN2 = (104, 74, 48, 255)
CREAM = (246, 238, 214, 255)
NIGHT = (32, 30, 42, 255)
SKIN = (222, 172, 128, 255)
PANTS = (98, 94, 122, 255)
WHITE = (255, 255, 255, 255)
T = (0, 0, 0, 0)

W, H = 224, 96


class Px:
    def __init__(self, w, h):
        self.w, self.h = w, h
        self.img = Image.new("RGBA", (w, h), T)
        self.d = ImageDraw.Draw(self.img)

    def set(self, x, y, c):
        if 0 <= x < self.w and 0 <= y < self.h and c[3]:
            self.img.putpixel((int(x), int(y)), c)

    def rect(self, x0, y0, x1, y1, c):
        self.d.rectangle([x0, y0, x1, y1], fill=c)

    def ellipse(self, x0, y0, x1, y1, c):
        self.d.ellipse([x0, y0, x1, y1], fill=c)

    def paste(self, img, x, y):
        self.img.alpha_composite(img, (x, y))


# ------------------------------------------------------------------ 글자
def text_mask(text, size, font_path=FONT_BLACK, spacing=0):
    """안티에일리어싱 없는 1비트 마스크 → 픽셀 폰트처럼 보인다."""
    f = ImageFont.truetype(font_path, size)
    probe = ImageDraw.Draw(Image.new("1", (8, 8)))
    boxes = [probe.textbbox((0, 0), ch, font=f) for ch in text]
    widths = [(b[2] - b[0]) if ch != " " else int(size * 0.30) for ch, b in zip(text, boxes)]
    top = min(b[1] for b in boxes)
    bot = max(b[3] for b in boxes)
    total = max(1, sum(widths) + spacing * (len(text) - 1))
    img = Image.new("1", (total + 2, bot - top + 2), 0)
    d = ImageDraw.Draw(img)
    x = 1
    for ch, b, w in zip(text, boxes, widths):
        if ch != " ":
            d.text((x - b[0], 1 - top), ch, font=f, fill=1)
        x += w + spacing
    return img


def mask_pixels(mask):
    p = mask.load()
    return {(x, y) for y in range(mask.height) for x in range(mask.width) if p[x, y]}


def text_size(text, size, font_path=FONT_BLACK, spacing=0):
    pts = mask_pixels(text_mask(text, size, font_path, spacing))
    if not pts:
        return 0, 0
    xs, ys = [p[0] for p in pts], [p[1] for p in pts]
    return max(xs) - min(xs) + 1, max(ys) - min(ys) + 1


def draw_text(canvas, text, size, ox, oy, ramp, outline=K, shadow=None,
              font_path=FONT_BLACK, spacing=0, top_light=None):
    """ramp = 위→아래 색. 외곽선 1px + 드롭섀도."""
    pts = mask_pixels(text_mask(text, size, font_path, spacing))
    if not pts:
        return 0, 0
    xs, ys = [p[0] for p in pts], [p[1] for p in pts]
    x0, y0, y1 = min(xs), min(ys), max(ys)
    span = max(1, y1 - y0)
    ox -= x0
    oy -= y0

    if shadow:
        sdx, sdy, sc = shadow
        for (x, y) in pts:
            for dx in (-1, 0, 1):
                for dy in (-1, 0, 1):
                    if (x + dx, y + dy) not in pts:
                        canvas.set(ox + x + dx + sdx, oy + y + dy + sdy, sc)
        for (x, y) in pts:
            canvas.set(ox + x + sdx, oy + y + sdy, sc)
    if outline:
        for (x, y) in pts:
            for dx in (-1, 0, 1):
                for dy in (-1, 0, 1):
                    if (x + dx, y + dy) not in pts:
                        canvas.set(ox + x + dx, oy + y + dy, outline)
    for (x, y) in pts:
        t = (y - y0) / span
        c = ramp[min(len(ramp) - 1, int(t * len(ramp)))]
        if top_light is not None and (x, y - 1) not in pts:
            c = top_light
        canvas.set(ox + x, oy + y, c)
    return max(xs) - x0 + 1, span + 1


# ------------------------------------------------------------------ 아이콘
def pixelate(size, paint, threshold=110):
    """8배로 그린 뒤 내려서 깨끗한 픽셀 도형으로 만든다."""
    S = 8
    big = Image.new("RGBA", (size * S, size * S), T)
    paint(ImageDraw.Draw(big), size * S)
    small = big.resize((size, size), Image.BOX)
    out = Image.new("RGBA", (size, size), T)
    sp, op = small.load(), out.load()
    for y in range(size):
        for x in range(size):
            r, g, b, a = sp[x, y]
            if a > threshold:
                op[x, y] = (r, g, b, 255)
    return out


def recycle_symbol(size, colors=(GREEN1, GREEN2, GREEN3)):
    def paint(d, S):
        c = S / 2
        R, w = S * 0.34, S * 0.15
        for i, col in enumerate(colors):
            a0 = math.radians(-95 + i * 120)
            out, inn = [], []
            for t in range(0, 76, 2):
                a = a0 + math.radians(t)
                out.append((c + (R + w / 2) * math.cos(a), c + (R + w / 2) * math.sin(a)))
                inn.append((c + (R - w / 2) * math.cos(a), c + (R - w / 2) * math.sin(a)))
            d.polygon(out + inn[::-1], fill=col)
            ae = a0 + math.radians(74)
            tx, ty = c + R * math.cos(ae), c + R * math.sin(ae)
            tan = ae + math.pi / 2
            hx, hy = math.cos(tan), math.sin(tan)
            nx, ny = math.cos(ae), math.sin(ae)
            d.polygon([(tx + hx * w * 1.9, ty + hy * w * 1.9),
                       (tx - nx * w * 1.35, ty - ny * w * 1.35),
                       (tx + nx * w * 1.35, ty + ny * w * 1.35)], fill=col)
    return pixelate(size, paint)


def tongs_px(c, x, y, light=METAL1, dark=METAL3, outline=K):
    """쓰레기 집게 12x20. 손잡이 + 벌어진 집게발이 확실히 읽히게 손으로 찍었다."""
    rows = [
        (0, [(5, 6, 'L')]),
        (1, [(5, 6, 'L')]),
        (2, [(4, 7, 'D')]),
        (3, [(4, 7, 'D')]),
        (4, [(5, 6, 'L')]),
        (5, [(5, 6, 'L')]),
        (6, [(5, 6, 'L')]),
        (7, [(5, 6, 'L')]),
        (8, [(5, 6, 'L')]),
        (9, [(5, 6, 'L')]),
        (10, [(5, 6, 'L')]),
        (11, [(4, 7, 'L')]),
        (12, [(3, 8, 'D')]),
        (13, [(2, 4, 'L'), (7, 9, 'L')]),
        (14, [(1, 3, 'L'), (8, 10, 'L')]),
        (15, [(1, 2, 'L'), (9, 10, 'L')]),
        (16, [(1, 2, 'L'), (9, 10, 'L')]),
        (17, [(2, 3, 'L'), (8, 9, 'L')]),
        (18, [(3, 5, 'L'), (6, 8, 'L')]),
        (19, [(4, 5, 'D'), (6, 7, 'D')]),
    ]
    px = {}
    for ry, spans in rows:
        for (a, b, kind) in spans:
            for rx in range(a, b + 1):
                px[(rx, ry)] = light if kind == 'L' else dark
    if outline:
        for (rx, ry) in list(px):
            for dx in (-1, 0, 1):
                for dy in (-1, 0, 1):
                    if (rx + dx, ry + dy) not in px:
                        c.set(x + rx + dx, y + ry + dy, outline)
    for (rx, ry), col in px.items():
        c.set(x + rx, y + ry, col)


def hero(c, x, y, s=1):
    """로고용 소년. s=1 이면 20x26."""
    def R(x0, y0, x1, y1, col):
        c.rect(x + x0 * s, y + y0 * s, x + (x1 + 1) * s - 1, y + (y1 + 1) * s - 1, col)
    R(3, 0, 16, 1, K)            # 헬멧 위
    R(2, 1, 17, 4, Y2)
    R(3, 1, 16, 2, Y1)
    R(1, 5, 18, 6, Y3)           # 챙
    R(0, 5, 19, 5, K)
    R(0, 7, 19, 7, K)
    R(4, 7, 15, 11, SKIN)
    R(3, 7, 3, 11, K)
    R(16, 7, 16, 11, K)
    R(6, 9, 7, 10, K)            # 눈
    R(12, 9, 13, 10, K)
    R(4, 12, 15, 12, K)
    R(2, 13, 17, 19, TEAL2)      # 상의
    R(3, 13, 16, 14, TEAL1)
    R(2, 19, 17, 19, TEAL3)
    R(1, 13, 1, 19, K)
    R(18, 13, 18, 19, K)
    R(2, 20, 17, 20, K)
    R(4, 21, 8, 24, PANTS)       # 바지
    R(11, 21, 15, 24, PANTS)
    R(4, 25, 8, 25, K)
    R(11, 25, 15, 25, K)
    R(3, 21, 3, 25, K)
    R(16, 21, 16, 25, K)


def bolt(c, x, y):
    c.rect(x, y, x + 2, y + 2, METAL3)
    c.rect(x, y, x + 1, y + 1, METAL1)
    c.set(x + 1, y + 1, K)


def bit(c, x, y, col):
    c.rect(x, y, x + 2, y + 2, col)
    c.set(x, y, K)
    c.set(x + 2, y + 2, K)


# ================================================================== A
def logo_sign():
    """A — 낡은 철판 간판. 작업장·현장 느낌."""
    c = Px(W, H)
    px0, py0, px1, py1 = 10, 16, W - 11, H - 10

    for x in (46, W - 49):       # 걸이 체인
        for i in range(0, py0 - 3, 3):
            c.rect(x, i, x + 1, i + 1, METAL2)
            c.set(x, i, METAL1)
        c.rect(x, py0 - 4, x + 1, py0 - 1, METAL2)
        c.rect(x - 2, py0 - 3, x + 3, py0 - 1, K)
        c.rect(x - 1, py0 - 2, x + 2, py0 - 2, METAL1)

    c.rect(px0 - 1, py0 - 1, px1 + 1, py1 + 1, K)
    c.rect(px0, py0, px1, py1, METAL2)
    c.rect(px0, py0, px1, py0 + 3, METAL1)
    c.rect(px0, py1 - 4, px1, py1, METAL3)
    for (rx, ry, rw) in ((16, 60, 11), (188, 24, 9), (120, 68, 14), (28, 22, 6), (166, 62, 8)):
        c.rect(rx, ry, rx + rw, ry + 2, RUST)
    for bx in (px0 + 3, px1 - 5):
        for by in (py0 + 3, py1 - 5):
            bolt(c, bx, by)

    tw, _ = text_size("쓰레기", 15, spacing=2)
    draw_text(c, "쓰레기", 15, (W - tw) // 2, py0 + 6,
              [CREAM, CREAM, (200, 192, 172, 255)], shadow=(1, 2, (34, 30, 34, 255)), spacing=2)
    tw2, _ = text_size("집게 소년", 28, spacing=-1)
    draw_text(c, "집게 소년", 28, (W - tw2) // 2, py0 + 24,
              [Y1, Y1, Y2, Y3], shadow=(2, 3, (32, 26, 30, 255)), top_light=WHITE, spacing=-1)

    tongs_px(c, px1 - 26, py0 + 8)
    c.rect(px0 + 8, py1 + 4, px1 - 8, py1 + 5, (24, 20, 26, 110))
    return c


# ================================================================== B
def logo_character():
    """B — 캐릭터 조합형. 가장 캐주얼하고 눈에 잘 띈다."""
    c = Px(W, H)
    base = H - 14                                    # 글자·캐릭터가 함께 서는 선

    draw_text(c, "쓰레기", 19, 14, 12, [GREEN1, GREEN1, GREEN2, GREEN3],
              shadow=(2, 3, (28, 44, 34, 255)), top_light=WHITE, spacing=2)
    draw_text(c, "집게소년", 36, 10, 34, [Y1, Y1, Y2, Y3],
              shadow=(3, 4, (32, 26, 30, 255)), top_light=WHITE, spacing=-2)

    hx, hy = W - 42, base - 26
    c.ellipse(hx - 2, base - 1, hx + 21, base + 3, (24, 20, 26, 120))   # 발밑 그림자
    tongs_px(c, hx - 14, hy + 5)                                        # 손에 든 집게
    hero(c, hx, hy)

    bit(c, 8, 20, TEAL1)                             # 흩날리는 쓰레기 조각
    bit(c, W - 12, 12, Y2)
    bit(c, W - 18, 34, GREEN1)
    bit(c, 200, 66, TEAL2)
    return c


# ================================================================== C
def logo_badge():
    """C — 리사이클 엠블럼. 앱 아이콘·워터마크로도 쓸 수 있다."""
    c = Px(W, H)
    bs = 84
    bx, by = 6, (H - bs) // 2
    c.ellipse(bx - 1, by - 1, bx + bs + 1, by + bs + 1, K)
    c.ellipse(bx, by, bx + bs, by + bs, NIGHT)
    c.ellipse(bx + 4, by + 4, bx + bs - 4, by + bs - 4, (26, 42, 38, 255))
    c.paste(recycle_symbol(74), bx + 5, by + 5)
    hero(c, bx + bs // 2 - 10, by + bs // 2 - 14)

    tx = bx + bs + 12
    draw_text(c, "쓰레기", 16, tx, 12, [TEAL1, TEAL1, TEAL2],
              shadow=(1, 2, (26, 34, 36, 255)), spacing=2)
    draw_text(c, "집게소년", 26, tx, 32, [Y1, Y1, Y2, Y3],
              shadow=(2, 3, (32, 26, 30, 255)), top_light=WHITE, spacing=-1)
    c.rect(tx, 66, W - 10, 66, METAL3)
    c.rect(tx, 67, W - 10, 67, K)
    draw_text(c, "7일 안에 도시를 세워라", 11, tx, 72,
              [(176, 186, 190, 255)], outline=None, font_path=FONT_BOLD, spacing=1)
    return c


LOGOS = [("A. 낡은 철판 간판", logo_sign),
         ("B. 캐릭터 조합형", logo_character),
         ("C. 리사이클 엠블럼", logo_badge)]

if __name__ == "__main__":
    import os
    os.makedirs("out/logo", exist_ok=True)
    scale, made = 4, []
    for i, (label, fn) in enumerate(LOGOS, 1):
        img = fn().img
        img.save(f"out/logo/logo_{chr(64 + i)}.png")
        made.append((label, img))
    pad = 16
    plate = Image.new("RGBA", (W * scale + pad * 2,
                               sum(H * scale + 34 for _ in made) + pad), (22, 20, 26, 255))
    d = ImageDraw.Draw(plate)
    f = ImageFont.truetype(FONT_BOLD, 16)
    y = pad
    for label, im in made:
        d.text((pad, y), label, font=f, fill=(228, 222, 212, 255))
        y += 24
        plate.alpha_composite(im.resize((W * scale, H * scale), Image.NEAREST), (pad, y))
        y += H * scale + 10
    plate.save("out/preview_logo.png")
    print("ok", plate.size)
