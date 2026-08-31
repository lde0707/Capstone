"""공용 픽셀 팔레트 / 캔버스 헬퍼."""
from PIL import Image

T = (0, 0, 0, 0)

PAL = {
    '.': T,
    'K': (28, 22, 30, 255),    # 진한 외곽선
    'k': (58, 46, 58, 255),    # 부드러운 외곽선
    # 헬멧
    'Y': (211, 173, 84, 255),
    'y': (152, 118, 50, 255),
    'W': (255, 243, 199, 255),  # 랜턴
    # 피부
    'S': (219, 170, 126, 255),
    's': (170, 120, 84, 255),
    'E': (44, 34, 46, 255),
    # 작업복
    'T': (100, 143, 140, 255),
    't': (66, 100, 102, 255),
    'u': (142, 180, 174, 255),
    # 장갑 / 장화
    'G': (78, 63, 68, 255),
    'g': (108, 90, 96, 255),
    # 배낭 (카키 캔버스)
    'B': (152, 138, 96, 255),
    'b': (108, 96, 62, 255),
    # 바지 / 장화
    'N': (98, 94, 122, 255),
    'n': (66, 62, 88, 255),
    'O': (122, 86, 58, 255),
    'o': (84, 58, 38, 255),
    # 금속 / 손잡이
    'M': (186, 192, 200, 255),
    'm': (122, 130, 140, 255),
    'P': (146, 102, 62, 255),
    'p': (104, 70, 42, 255),
    # 머리카락
    'H': (92, 62, 42, 255),
    # ---- 마을 팔레트 ----
    '1': (150, 118, 84, 255),   '2': (110, 84, 58, 255),      # 나무판자
    '3': (132, 128, 120, 255),  '4': (94, 90, 86, 255),       # 낡은 회색판자
    '5': (170, 106, 88, 255),   '6': (126, 74, 60, 255),      # 벽돌
    '7': (214, 204, 184, 255),  '8': (166, 156, 138, 255),    # 회벽
    '9': (152, 150, 142, 255),  '0': (110, 108, 102, 255),    # 석재
    'a': (178, 152, 94, 255),   'A': (132, 110, 64, 255),     # 초가지붕
    'c': (162, 86, 74, 255),    'C': (114, 58, 48, 255),      # 기와지붕
    'd': (96, 108, 128, 255),   'D': (66, 76, 94, 255),       # 슬레이트지붕
    'e': (110, 152, 130, 255),  'F': (76, 110, 94, 255),      # 청동지붕
    'f': (250, 216, 136, 255),  'L': (255, 238, 170, 255),    # 창문 불빛 / 램프
    'i': (124, 84, 54, 255),    'I': (86, 56, 34, 255),       # 문
    'j': (118, 154, 96, 255),   'J': (88, 126, 78, 255),      # 잎
    'l': (62, 96, 58, 255),
    'r': (108, 78, 52, 255),    'R': (74, 52, 34, 255),       # 줄기
    'v': (112, 146, 80, 255),   'V': (86, 118, 62, 255),      # 풀
    'w': (172, 166, 154, 255),  'x': (140, 134, 122, 255),    # 길
    'z': (106, 100, 90, 255),
    'q': (226, 226, 232, 255),  'Q': (176, 178, 186, 255),    # 연기
    'X': (200, 196, 186, 255),  'Z': (58, 54, 52, 255),
}


class Canvas:
    def __init__(self, w, h):
        self.w, self.h = w, h
        self.px = [['.'] * w for _ in range(h)]

    def set(self, x, y, c):
        if c == '.' or not (0 <= x < self.w and 0 <= y < self.h):
            return
        self.px[y][x] = c

    def rect(self, x0, y0, x1, y1, c):
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                self.set(x, y, c)

    def row(self, y, *segs):
        """segs: (x0, x1, char)"""
        for x0, x1, c in segs:
            for x in range(x0, x1 + 1):
                self.set(x, y, c)

    def blit_rows(self, rows, ox=0, oy=0):
        """rows: 문자열 리스트. '.' 은 투명(건너뜀)."""
        for j, line in enumerate(rows):
            for i, c in enumerate(line):
                self.set(ox + i, oy + j, c)

    def flip_x(self):
        out = Canvas(self.w, self.h)
        out.px = [list(reversed(r)) for r in self.px]
        return out

    def shift(self, dx, dy):
        out = Canvas(self.w, self.h)
        for y in range(self.h):
            for x in range(self.w):
                c = self.px[y][x]
                if c != '.':
                    out.set(x + dx, y + dy, c)
        return out

    def outline(self, char='K'):
        """투명 픽셀 중 채워진 칸과 맞닿은 곳에 외곽선을 그린다."""
        add = []
        for y in range(self.h):
            for x in range(self.w):
                if self.px[y][x] != '.':
                    continue
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < self.w and 0 <= ny < self.h and self.px[ny][nx] not in ('.', char):
                        add.append((x, y))
                        break
        for x, y in add:
            self.px[y][x] = char
        return self

    def copy(self):
        out = Canvas(self.w, self.h)
        out.px = [list(r) for r in self.px]
        return out

    def to_image(self, overrides=None):
        pal = dict(PAL)
        if overrides:
            pal.update(overrides)
        img = Image.new('RGBA', (self.w, self.h), T)
        p = img.load()
        for y in range(self.h):
            for x in range(self.w):
                p[x, y] = pal[self.px[y][x]]
        return img


def sheet(frames, cols=None, overrides=None):
    """Canvas 리스트를 가로(또는 격자) 스프라이트 시트 이미지로."""
    w, h = frames[0].w, frames[0].h
    cols = cols or len(frames)
    rows = (len(frames) + cols - 1) // cols
    img = Image.new('RGBA', (w * cols, h * rows), T)
    for i, f in enumerate(frames):
        img.paste(f.to_image(overrides), ((i % cols) * w, (i // cols) * h))
    return img


def preview(img, scale=8, bg=(70, 60, 80, 255), path=None):
    big = img.resize((img.width * scale, img.height * scale), Image.NEAREST)
    plate = Image.new('RGBA', big.size, bg)
    plate.alpha_composite(big)
    if path:
        plate.save(path)
    return plate
