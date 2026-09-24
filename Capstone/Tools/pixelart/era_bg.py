"""시대별 원경 배경 — 먼 층(산·거대 덩어리) / 가까운 층(건물·나무) 2장 x 5시대.

256x64, bottom-center 기준으로 지평선에 붙는다. 좌우로 끝없이 이어지도록
x=0 과 x=256 의 실루엣이 맞물리게 그린다 (wrap 함수로 좌표를 감아서 그림).

시트: era_far_{era}.png / era_near_{era}.png (각 256x64, 1프레임)
"""
from PIL import Image, ImageDraw
from pixbox import ERA, C

W, H = 256, 64


def rgba(c):
    return c if len(c) == 4 else (c[0], c[1], c[2], 255)


def mix(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3)) + (255,)


class Strip:
    """가로로 감기는 캔버스. x 좌표는 자동으로 0~W 로 모듈러 된다."""

    def __init__(self):
        self.img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
        self.d = ImageDraw.Draw(self.img)

    def px(self, x, y, c):
        if 0 <= y < H:
            self.img.putpixel((int(x) % W, int(y)), rgba(c))

    def vline(self, x, y0, y1, c):
        for y in range(min(y0, y1), max(y0, y1) + 1):
            self.px(x, y, c)

    def rect(self, x0, y0, x1, y1, c):
        for x in range(x0, x1 + 1):
            self.vline(x, y0, y1, c)

    def tri(self, cx, half, ytop, ybase, c, shade=None):
        """좌우 대칭 삼각 (산). shade 를 주면 오른쪽 면을 어둡게."""
        for x in range(cx - half, cx + half + 1):
            t = abs(x - cx) / max(1, half)
            y = int(ytop + (ybase - ytop) * t)
            self.vline(x, y, ybase, shade if (shade and x > cx) else c)

    def blob(self, cx, half, ytop, ybase, c, shade=None):
        """둥근 언덕."""
        for x in range(cx - half, cx + half + 1):
            t = abs(x - cx) / max(1, half)
            y = int(ybase - (ybase - ytop) * (1 - t * t) ** 0.6)
            self.vline(x, y, ybase, shade if (shade and x > cx) else c)


BASE = H - 1          # 지평선 (이 아래는 땅에 가려진다)


# ================================================================ 먼 층
def far(e):
    p = ERA[e]
    s = Strip()
    sky_far = mix(p['s3'], (40, 44, 58), 0.45)        # 멀수록 하늘에 섞인 색
    sky_far2 = mix(sky_far, (20, 24, 36), 0.30)

    if e == 0:                                         # 화산 + 연기 + 능선
        for (cx, half, top) in ((30, 46, 22), (118, 38, 26), (206, 44, 20)):
            s.tri(cx, half, top, BASE, sky_far, sky_far2)
        s.tri(118, 26, 14, BASE, mix(sky_far, (120, 60, 40), 0.30),
              mix(sky_far2, (90, 40, 30), 0.30))
        for (x, y) in ((116, 12), (120, 9), (114, 7), (122, 5), (118, 3)):
            s.rect(x, y, x + 2, y + 1, mix(sky_far, (160, 150, 150), 0.5))
        s.rect(112, 13, 124, 14, mix(sky_far, (220, 120, 60), 0.6))
    elif e == 1:                                       # 완만한 산 + 토성
        for (cx, half, top) in ((20, 52, 30), (100, 44, 26), (180, 50, 32), (246, 40, 34)):
            s.blob(cx, half, top, BASE, sky_far, sky_far2)
        s.rect(88, 30, 140, BASE, mix(sky_far, (150, 130, 100), 0.35))
        for x in range(88, 141, 8):
            s.rect(x, 27, x + 4, 31, mix(sky_far, (170, 150, 118), 0.35))
    elif e == 2:                                       # 첩첩 산등성이
        for (cx, half, top, t) in ((14, 60, 24, 0.0), (96, 52, 18, 0.0),
                                   (176, 58, 26, 0.0), (240, 48, 22, 0.0)):
            s.tri(cx, half, top, BASE, sky_far, sky_far2)
        for (cx, half, top) in ((54, 40, 34), (136, 44, 32), (210, 38, 36)):
            s.blob(cx, half, top, BASE, mix(sky_far, p['g3'], 0.45),
                   mix(sky_far2, p['g3'], 0.45))
    elif e == 3:                                       # 매연 낀 낮은 구릉 + 굴뚝 무리
        for (cx, half, top) in ((40, 70, 40), (150, 66, 38), (240, 56, 42)):
            s.blob(cx, half, top, BASE, sky_far, sky_far2)
        for (x, h, w) in ((64, 30, 5), (74, 24, 4), (150, 32, 6), (162, 26, 4), (210, 28, 5)):
            s.rect(x, BASE - h, x + w, BASE, sky_far2)
            s.rect(x - 1, BASE - h - 2, x + w + 1, BASE - h, sky_far)
            for k in range(3):
                s.rect(x + 1 - k, BASE - h - 6 - k * 4, x + w - 1 + k, BASE - h - 4 - k * 4,
                       mix(sky_far, (170, 160, 150), 0.35))
    else:                                              # 미래 — 거대 구조물 실루엣
        for (cx, half, top) in ((30, 54, 34), (140, 48, 30), (224, 44, 36)):
            s.blob(cx, half, top, BASE, sky_far, sky_far2)
        for (x, w, h) in ((70, 18, 46), (96, 12, 34), (178, 16, 40), (200, 10, 28)):
            s.rect(x, BASE - h, x + w, BASE, sky_far2)
            s.rect(x, BASE - h, x + w // 2, BASE, sky_far)
            for y in range(BASE - h + 4, BASE, 7):
                s.rect(x + 2, y, x + w - 2, y, mix(sky_far, p['a1'], 0.35))
        # 떠 있는 고리
        for (cx, cy, r) in ((124, 14, 13), (216, 10, 9)):
            for x in range(cx - r, cx + r + 1):
                t = abs(x - cx) / r
                y = int(cy + 4 * (1 - t * t))
                s.px(x, y, mix(sky_far, p['a1'], 0.5))
                s.px(x, y + 1, sky_far2)
    return s.img


# ================================================================ 가까운 층
def near(e):
    p = ERA[e]
    s = Strip()
    c1 = mix(p['s3'], (34, 38, 50), 0.22)
    c2 = mix(c1, (18, 22, 32), 0.30)
    leaf = mix(p['g3'], (32, 40, 48), 0.25)
    leaf2 = mix(leaf, (16, 22, 30), 0.30)

    if e == 0:                                         # 빽빽한 고사리 숲
        for i in range(0, W, 13):
            x = i + (i * 7) % 4
            h = 13 + (i * 13) % 10
            s.rect(x, BASE - h, x + 1, BASE, c2)
            for side in (-1, 1):
                for k in range(1, 7):
                    yy = BASE - h - 1 + (k * k) // 6
                    s.px(x + side * k, yy, leaf)
                    s.px(x + side * k, yy + 1, leaf2)
            s.px(x, BASE - h - 5, leaf)
            s.px(x, BASE - h - 4, leaf)
        for i in range(6, W, 41):                      # 낮은 덤불
            s.blob(i, 9, BASE - 7, BASE, leaf2, mix(leaf2, (14, 20, 26), 0.3))
    elif e == 1:                                       # 초가 마을 + 나무
        straw = mix(c2, p['r1'], 0.50)
        wallc = mix(c1, (60, 52, 44), 0.35)
        for i in range(0, W, 34):
            x = i + (i * 5) % 7
            w = 18 + (i * 3) % 8
            h = 11 + (i * 11) % 4
            rh = 10
            s.rect(x, BASE - h, x + w, BASE, wallc)
            cx = x + w // 2
            for k in range(rh):                        # 둥근 초가지붕
                t = (k / (rh - 1)) ** 0.55
                half = int((w // 2 + 5) * t) + 1
                s.rect(cx - half, BASE - h - rh + k, cx + half, BASE - h - rh + k, straw)
            s.rect(cx - w // 2 - 6, BASE - h - 1, cx + w // 2 + 6, BASE - h, mix(straw, (30, 26, 24), 0.4))
        for i in range(17, W, 34):
            s.rect(i, BASE - 10, i + 1, BASE, c2)
            s.blob(i, 7, BASE - 22, BASE - 9, leaf, leaf2)
    elif e == 2:                                       # 기와 지붕 겹침 + 소나무
        wallc = mix(c1, (238, 232, 218), 0.62)         # 흰 회벽은 멀리서도 밝다
        roofc = mix(c2, (88, 108, 124), 0.45)          # 기와는 확실히 어둡게
        ridge = mix(roofc, (206, 222, 232), 0.40)
        for i in range(0, W, 30):
            x = i + (i * 3) % 6
            w = 22 + (i * 7) % 8
            h = 11 + (i * 5) % 5
            rh = 9
            s.rect(x, BASE - h, x + w, BASE, wallc)
            for k in range(rh):                        # 사다리꼴 기와
                t = k / (rh - 1)
                left = int(x + 5 - 11 * t)
                right = int(x + w - 5 + 11 * t)
                s.rect(left, BASE - h - rh + k, right, BASE - h - rh + k, roofc)
            s.rect(x + 4, BASE - h - rh, x + w - 4, BASE - h - rh, ridge)
            s.rect(x - 6, BASE - h - 1, x + w + 6, BASE - h, mix(roofc, (26, 30, 36), 0.35))
        for i in range(24, W, 60):
            s.rect(i, BASE - 16, i + 1, BASE, c2)
            for (y, half) in ((BASE - 30, 9), (BASE - 24, 7), (BASE - 19, 5)):
                s.tri(i, half, y, y + 5, leaf, leaf2)
    elif e == 3:                                       # 연립주택 + 전봇대 스카이라인
        for i in range(0, W, 21):
            x = i
            w = 15 + (i * 7) % 5
            h = 16 + (i * 13) % 16
            s.rect(x, BASE - h, x + w, BASE, c1 if (i // 21) % 2 else c2)
            s.rect(x - 1, BASE - h - 2, x + w + 1, BASE - h, c2)
            for wy in range(BASE - h + 3, BASE - 3, 6):
                for wx in range(x + 3, x + w - 2, 5):
                    s.rect(wx, wy, wx + 1, wy + 2, mix(c1, p['glow'], 0.45))
        for i in range(10, W, 52):                     # 전봇대 + 전선
            s.rect(i, BASE - 34, i + 1, BASE, c2)
            s.rect(i - 4, BASE - 32, i + 5, BASE - 31, c2)
            for k in range(52):
                s.px(i + k, BASE - 31 + (k * (52 - k)) // 260, c2)
    else:                                              # 미래 도시 실루엣
        for i in range(0, W, 19):
            x = i
            w = 11 + (i * 5) % 7
            h = 20 + (i * 17) % 30
            s.rect(x, BASE - h, x + w, BASE, c1 if (i // 19) % 2 else c2)
            s.rect(x + 1, BASE - h - 3, x + w - 1, BASE - h, c2)
            for wy in range(BASE - h + 4, BASE - 2, 5):
                s.rect(x + 2, wy, x + w - 2, wy, mix(c1, p['a1'], 0.40))
            if (i // 19) % 3 == 0:
                s.rect(x + w // 2, BASE - h - 10, x + w // 2, BASE - h - 3, c2)
                s.px(x + w // 2, BASE - h - 11, p['a1'])
        for (cx, cy, w) in ((60, 12, 16), (170, 8, 12)):   # 부유 플랫폼
            s.rect(cx - w, cy, cx + w, cy + 3, c2)
            s.rect(cx - w + 2, cy - 3, cx + w - 2, cy, c1)
            s.rect(cx - w + 3, cy + 3, cx + w - 3, cy + 3, mix(c1, p['a1'], 0.5))
    return s.img


if __name__ == "__main__":
    import os
    os.makedirs("out/Era", exist_ok=True)
    for e in range(5):
        far(e).save(f"out/Era/era_far_{e}.png")
        near(e).save(f"out/Era/era_near_{e}.png")
    print("ok")
