"""통로 타일 찌꺼기 버그 검증 — 수정 전(4방향) vs 수정 후(8방향)를 렌더링해 비교한다."""
import math
from PIL import Image

G = 16          # 타일 픽셀
DIRT = (138, 104, 72, 255)
DIRT2 = (112, 84, 58, 255)
VOID = (198, 96, 128, 255)   # 파낸 자리(스크린샷의 분홍 배경 대용)


def dist_seg(px, py, ax, ay, bx, by):
    vx, vy = bx - ax, by - ay
    l2 = vx * vx + vy * vy
    if l2 <= 1e-4:
        return math.hypot(px - ax, py - ay)
    t = max(0.0, min(1.0, ((px - ax) * vx + (py - ay) * vy) / l2))
    return math.hypot(px - (ax + vx * t), py - (ay + vy * t))


def is_dug(mask, x, y, pattern, eight):
    full = 255 if eight else 15
    if (mask & full) == full:
        return True
    c = (G - 1) * 0.5
    far, near = G + 1.0, -2.0
    d = math.hypot(x - c, y - c)
    # PIL 은 y 가 아래로 증가하므로 위/아래를 뒤집어 그린다 (유니티 텍스처는 반대)
    if mask & 1:
        d = min(d, dist_seg(x, y, c, c, c, near))
    if mask & 2:
        d = min(d, dist_seg(x, y, c, c, far, c))
    if mask & 4:
        d = min(d, dist_seg(x, y, c, c, c, far))
    if mask & 8:
        d = min(d, dist_seg(x, y, c, c, near, c))
    if eight:
        if mask & 16:
            d = min(d, dist_seg(x, y, c, c, far, near))
        if mask & 32:
            d = min(d, dist_seg(x, y, c, c, far, far))
        if mask & 64:
            d = min(d, dist_seg(x, y, c, c, near, far))
        if mask & 128:
            d = min(d, dist_seg(x, y, c, c, near, near))
    wobble = (math.sin((x + pattern * 3) * 0.77 + y * 0.31) * 0.32
              + math.sin(x * 0.29 - (y + pattern * 5) * 0.67) * 0.22)
    return d <= 5.25 + wobble


def passage_mask(grid, x, y, eight):
    def op(gx, gy):
        return 0 <= gy < len(grid) and 0 <= gx < len(grid[0]) and grid[gy][gx] == 0
    up, right, down, left = op(x, y - 1), op(x + 1, y), op(x, y + 1), op(x - 1, y)
    m = (1 if up else 0) | (2 if right else 0) | (4 if down else 0) | (8 if left else 0)
    if eight:
        if up and right and op(x + 1, y - 1):
            m |= 16
        if down and right and op(x + 1, y + 1):
            m |= 32
        if down and left and op(x - 1, y + 1):
            m |= 64
        if up and left and op(x - 1, y - 1):
            m |= 128
    return m


def render(grid, eight):
    h, w = len(grid), len(grid[0])
    img = Image.new('RGBA', (w * G, h * G), VOID)
    p = img.load()
    for gy in range(h):
        for gx in range(w):
            if grid[gy][gx] != 0:
                for y in range(G):
                    for x in range(G):
                        p[gx * G + x, gy * G + y] = DIRT if (x + y) % 5 else DIRT2
                continue
            mask = passage_mask(grid, gx, gy, eight)
            pat = (gx % 2) + (gy % 2) * 2
            for y in range(G):
                for x in range(G):
                    if not is_dug(mask, x, y, pat, eight):
                        p[gx * G + x, gy * G + y] = DIRT if (x + y) % 5 else DIRT2


    return img


def floating_chunks(grid, eight):
    """벽에 닿지 않은 흙 픽셀(=찌꺼기) 개수. 4-이웃 연결로 벽에서 퍼뜨려 센다."""
    h, w = len(grid), len(grid[0])
    solid = [[False] * (w * G) for _ in range(h * G)]
    for gy in range(h):
        for gx in range(w):
            if grid[gy][gx] != 0:
                for y in range(G):
                    for x in range(G):
                        solid[gy * G + y][gx * G + x] = True
                continue
            mask = passage_mask(grid, gx, gy, eight)
            pat = (gx % 2) + (gy % 2) * 2
            for y in range(G):
                for x in range(G):
                    if not is_dug(mask, x, y, pat, eight):
                        solid[gy * G + y][gx * G + x] = True

    seen = [[False] * (w * G) for _ in range(h * G)]
    stack = []
    for gy in range(h):
        for gx in range(w):
            if grid[gy][gx] == 0:
                continue
            for y in range(G):
                for x in range(G):
                    py, px = gy * G + y, gx * G + x
                    if not seen[py][px]:
                        seen[py][px] = True
                        stack.append((px, py))
    while stack:
        x, y = stack.pop()
        for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            nx, ny = x + dx, y + dy
            if 0 <= nx < w * G and 0 <= ny < h * G and solid[ny][nx] and not seen[ny][nx]:
                seen[ny][nx] = True
                stack.append((nx, ny))
    return sum(1 for y in range(h * G) for x in range(w * G) if solid[y][x] and not seen[y][x])


# 1 = 안 판 땅, 0 = 파낸 통로
def make_grid():
    w, h = 14, 10
    g = [[1] * w for _ in range(h)]
    for y in (3, 4):                       # 가로 2칸 터널
        for x in range(2, 12):
            g[y][x] = 0
    for y in range(1, 9):                  # 세로 2칸 수직굴
        for x in (6, 7):
            g[y][x] = 0
    for y in range(6, 8):                  # 넓은 공터
        for x in range(3, 6):
            g[y][x] = 0
    return g


if __name__ == '__main__':
    grid = make_grid()
    before = render(grid, False)
    after = render(grid, True)
    print('수정 전 떠 있는 흙 픽셀:', floating_chunks(grid, False))
    print('수정 후 떠 있는 흙 픽셀:', floating_chunks(grid, True))

    pad = 12
    plate = Image.new('RGBA', (before.width, before.height * 2 + pad), (30, 26, 34, 255))
    plate.paste(before, (0, 0))
    plate.paste(after, (0, before.height + pad))
    plate = plate.resize((plate.width * 3, plate.height * 3), Image.NEAREST)
    plate.save('out/verify_passage.png')
    print('saved out/verify_passage.png')
