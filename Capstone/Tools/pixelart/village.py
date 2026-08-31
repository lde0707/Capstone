"""마을/도시 에셋 — 48x48 건물, 32x32 소품, 16x16 길·잔디."""
from pixlib import Canvas, sheet, preview
from PIL import Image

TILE = 48          # 건물 캔버스 (16px 그리드 3칸)
PROP = 32          # 소품 캔버스 (2칸)
ROAD = 16          # 길 타일 (1칸)


# ------------------------------------------------------------------ 건물
def outline_box(c, x0, y0, x1, y1, fill, dark):
    c.rect(x0, y0, x1, y1, fill)
    for x in range(x0, x1 + 1):          # 아래쪽 그림자
        c.set(x, y1, dark)
    for y in range(y0, y1 + 1):          # 오른쪽 그림자
        c.set(x1, y, dark)
    c.rect(x0 - 1, y0 - 1, x0 - 1, y1 + 1, 'K')
    c.rect(x1 + 1, y0 - 1, x1 + 1, y1 + 1, 'K')
    c.row(y1 + 1, (x0 - 1, x1 + 1, 'K'))


def plank_texture(c, x0, y0, x1, y1, dark, step=4):
    for y in range(y0 + step - 1, y1, step):
        c.row(y, (x0, x1 - 1, dark))


def gable_roof(c, x0, y0, x1, over, light, dark, height):
    """삼각 지붕. y0 은 지붕 바닥줄."""
    left, right = x0 - over, x1 + over
    a = b = 0
    for i in range(height):
        y = y0 - i
        a = left + int(i * (right - left) / (2.0 * height))
        b = right - int(i * (right - left) / (2.0 * height))
        if a > b:
            a = b = (a + b) // 2
        c.row(y, (a, b, light if i % 2 == 0 else dark))
        c.set(a - 1, y, 'K')
        c.set(b + 1, y, 'K')
    c.row(y0 + 1, (left - 1, right + 1, 'K'))
    c.row(y0 - height, (a - 1, b + 1, 'K'))


def hip_roof(c, x0, y0, x1, over, light, dark, height):
    left, right = x0 - over, x1 + over
    for i in range(height):
        y = y0 - i
        a, b = left + i, right - i
        if a > b:
            break
        c.row(y, (a, b, light if i % 2 == 0 else dark))
        c.set(a - 1, y, 'K')
        c.set(b + 1, y, 'K')
    c.row(y0 + 1, (left - 1, right + 1, 'K'))


def window(c, x, y, w=4, h=4, glass='f'):
    c.rect(x - 1, y - 1, x + w, y + h, 'K')
    c.rect(x, y, x + w - 1, y + h - 1, glass)
    for yy in range(y, y + h):
        c.set(x + w // 2, yy, 'K')
    for xx in range(x, x + w):
        c.set(xx, y + h // 2, 'K')


def door(c, cx, ybottom, w=6, h=9):
    x0 = cx - w // 2
    c.rect(x0 - 1, ybottom - h, x0 + w, ybottom, 'K')
    c.rect(x0, ybottom - h + 1, x0 + w - 1, ybottom, 'i')
    c.rect(x0, ybottom - h + 1, x0, ybottom, 'I')
    c.set(x0 + w - 2, ybottom - h // 2, 'L')


def chimney(c, x, ytop, h=6, smoke=True):
    c.rect(x - 1, ytop - h - 1, x + 3, ytop, 'K')
    c.rect(x, ytop - h, x + 2, ytop, '6')
    c.row(ytop - h, (x, x + 2, '5'))
    if smoke:
        for i, (dx, dy) in enumerate(((1, -3), (2, -5), (0, -7), (2, -9))):
            c.set(x + dx, ytop - h + dy, 'q' if i % 2 == 0 else 'Q')
            c.set(x + dx + 1, ytop - h + dy, 'Q')


def base_shadow(c, cx, y, half):
    c.row(y, (cx - half, cx + half, 'Z'))


def house(spec):
    c = Canvas(TILE, TILE)
    bw, bh = spec['w'], spec['h']
    ground = TILE - 2
    cx = TILE // 2
    x0, x1 = cx - bw // 2, cx - bw // 2 + bw - 1
    y1 = ground
    y0 = y1 - bh + 1

    base_shadow(c, cx, ground + 1, bw // 2 + 2)
    outline_box(c, x0, y0, x1, y1, spec['wall'], spec['wallDark'])
    if spec.get('plank'):
        plank_texture(c, x0, y0, x1, y1, spec['wallDark'], spec['plank'])

    roof = spec.get('roof', 'gable')
    rh = spec.get('roofH', 8)
    if roof == 'gable':
        gable_roof(c, x0, y0 - 1, x1, spec.get('over', 2),
                   spec['roofC'], spec['roofD'], rh)
    elif roof == 'hip':
        hip_roof(c, x0, y0 - 1, x1, spec.get('over', 2),
                 spec['roofC'], spec['roofD'], rh)
    elif roof == 'flat':
        c.rect(x0 - 2, y0 - 3, x1 + 2, y0 - 1, spec['roofC'])
        c.row(y0 - 4, (x0 - 2, x1 + 2, 'K'))
        c.row(y0 - 1, (x0 - 2, x1 + 2, spec['roofD']))
        c.set(x0 - 3, y0 - 3, 'K'); c.set(x1 + 3, y0 - 3, 'K')
        c.set(x0 - 3, y0 - 2, 'K'); c.set(x1 + 3, y0 - 2, 'K')
        c.set(x0 - 3, y0 - 1, 'K'); c.set(x1 + 3, y0 - 1, 'K')
    elif roof == 'spire':
        hip_roof(c, x0, y0 - 1, x1, 2, spec['roofC'], spec['roofD'], 6)
        tx = cx
        c.rect(tx - 3, y0 - 16, tx + 3, y0 - 7, spec['wall'])
        c.rect(tx - 4, y0 - 17, tx + 4, y0 - 17, 'K')
        c.rect(tx - 4, y0 - 16, tx - 4, y0 - 7, 'K')
        c.rect(tx + 4, y0 - 16, tx + 4, y0 - 7, 'K')
        window(c, tx - 2, y0 - 14, 4, 4, 'f')
        for i in range(6):
            a = tx - 3 + i // 2
            b = tx + 3 - i // 2
            c.row(y0 - 18 - i, (a, b, spec['roofC'] if i % 2 == 0 else spec['roofD']))
        c.set(tx, y0 - 25, 'L')

    for (wx, wy) in spec.get('windows', []):
        window(c, x0 + wx, y0 + wy, spec.get('winW', 4), spec.get('winH', 4))
    if spec.get('door', True):
        door(c, cx + spec.get('doorDx', 0), ground - 1,
             spec.get('doorW', 6), spec.get('doorH', 9))
    if spec.get('chimney'):
        chimney(c, x0 + spec['chimney'], y0 - 1, spec.get('chimneyH', 6))
    if spec.get('holes'):
        for (hx, hy) in spec['holes']:
            c.rect(x0 + hx, y0 + hy, x0 + hx + 1, y0 + hy + 1, 'K')
    return c


HOUSE_TIERS = [
    # 1 — 다 쓰러져가는 판잣집
    dict(w=18, h=12, wall='3', wallDark='4', plank=3, roof='gable',
         roofC='4', roofD='Z', roofH=5, over=2,
         windows=[(3, 4)], winW=3, winH=3, doorW=5, doorH=7, doorDx=2,
         holes=[(12, 2), (5, 8)]),
    # 2 — 초가 오두막
    dict(w=24, h=15, wall='1', wallDark='2', plank=4, roof='gable',
         roofC='a', roofD='A', roofH=8, over=3,
         windows=[(3, 4), (17, 4)], doorW=6, doorH=9, chimney=3, chimneyH=5),
    # 3 — 벽돌집 + 굴뚝
    dict(w=28, h=18, wall='5', wallDark='6', roof='gable',
         roofC='c', roofD='C', roofH=9, over=3,
         windows=[(3, 4), (21, 4), (12, 4)], doorW=6, doorH=9,
         chimney=22, chimneyH=7),
    # 4 — 2층 회벽집
    dict(w=30, h=26, wall='7', wallDark='8', roof='hip',
         roofC='c', roofD='C', roofH=8, over=3,
         windows=[(4, 4), (13, 4), (22, 4), (4, 13), (22, 13)],
         doorW=6, doorH=9, chimney=24, chimneyH=8),
    # 5 — 3층 석조 저택
    dict(w=36, h=32, wall='9', wallDark='0', roof='hip',
         roofC='d', roofD='D', roofH=7, over=4,
         windows=[(3, 3), (11, 3), (19, 3), (28, 3),
                  (3, 12), (11, 12), (19, 12), (28, 12),
                  (3, 21), (28, 21)],
         doorW=8, doorH=10, chimney=30, chimneyH=8),
    # 6 — 시계탑이 선 시청
    dict(w=38, h=22, wall='7', wallDark='8', roof='spire',
         roofC='e', roofD='F',
         windows=[(3, 4), (11, 4), (26, 4), (33, 4),
                  (3, 13), (33, 13)],
         doorW=8, doorH=11),
]


# ------------------------------------------------------------------ 소품
def tree(big=True, seed=0):
    c = Canvas(PROP, PROP)
    cx, ground = PROP // 2, PROP - 2
    base_shadow(c, cx, ground + 1, 5 if big else 3)
    th = 10 if big else 7
    c.rect(cx - 2, ground - th, cx + 2, ground, 'K')
    c.rect(cx - 1, ground - th, cx + 1, ground - 1, 'r')
    c.rect(cx - 1, ground - th, cx - 1, ground - 1, 'R')
    r = 9 if big else 6
    top = ground - th - r + 2
    for y in range(top - r, top + r + 1):
        for x in range(cx - r - 1, cx + r + 2):
            dx, dy = (x - cx) / (r + 1.0), (y - top) / (r * 0.95)
            d = dx * dx + dy * dy
            if d > 1.15:
                continue
            if d > 0.92:
                c.set(x, y, 'K')
            elif (x * 7 + y * 13 + seed * 5) % 11 == 0:
                c.set(x, y, 'l')
            elif dx + dy < -0.35:
                c.set(x, y, 'j')
            else:
                c.set(x, y, 'J')
    return c


def bush():
    c = Canvas(PROP, PROP)
    cx, g = PROP // 2, PROP - 2
    base_shadow(c, cx, g + 1, 5)
    for (ox, r) in ((-4, 5), (4, 5), (0, 7)):
        for y in range(g - r * 2, g + 1):
            for x in range(cx + ox - r, cx + ox + r + 1):
                dx, dy = (x - cx - ox) / float(r), (y - (g - r)) / float(r)
                d = dx * dx + dy * dy
                if d > 1.0:
                    continue
                c.set(x, y, 'j' if dy < -0.35 else ('l' if d > 0.7 else 'J'))
    c.outline()
    c.row(g + 1, (cx - 8, cx + 8, 'Z'))
    return c


def lamp_post():
    c = Canvas(PROP, PROP)
    cx, g = PROP // 2, PROP - 2
    base_shadow(c, cx, g + 1, 3)
    c.rect(cx - 2, g - 1, cx + 2, g, 'K')
    c.rect(cx - 1, g - 2, cx + 1, g - 1, '0')
    c.rect(cx - 1, 10, cx + 1, g - 2, 'K')
    c.rect(cx, 10, cx, g - 2, '9')
    c.rect(cx - 3, 5, cx + 3, 10, 'K')
    c.rect(cx - 2, 6, cx + 2, 9, 'L')
    c.rect(cx - 2, 3, cx + 2, 4, 'K')
    c.set(cx, 2, 'K')
    return c


def fence():
    c = Canvas(PROP, PROP)
    g = PROP - 2
    c.row(g + 1, (0, PROP - 1, 'Z'))
    for x in (2, 10, 18, 26):
        c.rect(x, g - 11, x + 3, g, 'K')
        c.rect(x + 1, g - 10, x + 2, g - 1, '1')
        c.set(x + 1, g - 10, '2')
    for y in (g - 8, g - 3):
        c.row(y, (0, PROP - 1, '2'))
        c.row(y - 1, (0, PROP - 1, '1'))
        c.row(y - 2, (0, PROP - 1, 'K'))
        c.row(y + 1, (0, PROP - 1, 'K'))
    return c


def well():
    c = Canvas(PROP, PROP)
    cx, g = PROP // 2, PROP - 2
    base_shadow(c, cx, g + 1, 8)
    c.rect(cx - 8, g - 8, cx + 8, g, 'K')
    c.rect(cx - 7, g - 7, cx + 7, g - 1, '9')
    for y in range(g - 7, g, 3):
        c.row(y, (cx - 7, cx + 7, '0'))
    c.rect(cx - 7, g - 9, cx + 7, g - 8, '0')
    for x in (cx - 6, cx + 5):
        c.rect(x, g - 20, x + 1, g - 9, 'K')
        c.set(x, g - 19, 'r')
    for i in range(9):
        c.row(g - 21 - i // 2, (cx - 8 + i, cx + 8 - i, 'c' if i % 2 == 0 else 'C'))
    c.rect(cx - 1, g - 16, cx + 1, g - 12, 'K')
    c.rect(cx, g - 15, cx, g - 13, 'r')
    return c


def crates():
    c = Canvas(PROP, PROP)
    g = PROP - 2
    c.row(g + 1, (4, 27, 'Z'))
    for (x, y, w) in ((4, g - 9, 10), (16, g - 11, 12), (8, g - 20, 11)):
        c.rect(x, y, x + w, y + 9, 'K')
        c.rect(x + 1, y + 1, x + w - 1, y + 8, '1')
        c.rect(x + 1, y + 4, x + w - 1, y + 5, '2')
        c.rect(x + w // 2, y + 1, x + w // 2, y + 8, '2')
    return c


def sign():
    c = Canvas(PROP, PROP)
    cx, g = PROP // 2, PROP - 2
    base_shadow(c, cx, g + 1, 3)
    c.rect(cx - 1, g - 12, cx + 1, g, 'K')
    c.set(cx, g - 11, 'r')
    c.rect(cx - 10, g - 24, cx + 10, g - 12, 'K')
    c.rect(cx - 9, g - 23, cx + 9, g - 13, '1')
    c.rect(cx - 9, g - 19, cx + 9, g - 19, '2')
    for y in (g - 21, g - 17, g - 15):
        c.row(y, (cx - 6, cx + 5, '2'))
    return c


def flowerbed():
    c = Canvas(PROP, PROP)
    g = PROP - 2
    c.row(g + 1, (2, 29, 'Z'))
    c.rect(2, g - 5, 29, g, 'K')
    c.rect(3, g - 4, 28, g - 1, 'V')
    c.row(g - 4, (3, 28, 'v'))
    for i, x in enumerate(range(5, 28, 5)):
        c.rect(x, g - 9, x, g - 5, 'V')
        col = ('c', 'f', 'q', 'L', 'j')[i % 5]
        c.rect(x - 1, g - 12, x + 1, g - 10, col)
        c.set(x, g - 11, 'K')
    return c


def water_tower():
    c = Canvas(PROP, PROP)
    cx, g = PROP // 2, PROP - 2
    base_shadow(c, cx, g + 1, 7)
    for x in (cx - 7, cx + 6):
        c.rect(x, g - 13, x + 1, g, 'K')
        c.set(x, g - 12, '0')
    c.row(g - 8, (cx - 7, cx + 7, '0'))
    c.rect(cx - 9, g - 27, cx + 9, g - 14, 'K')
    c.rect(cx - 8, g - 26, cx + 8, g - 15, '3')
    c.rect(cx - 8, g - 22, cx + 8, g - 22, '4')
    c.rect(cx - 8, g - 18, cx + 8, g - 18, '4')
    for i in range(5):
        c.row(g - 28 - i, (cx - 9 + i, cx + 9 - i, 'c' if i % 2 == 0 else 'C'))
    return c


def bench():
    c = Canvas(PROP, PROP)
    g = PROP - 2
    c.row(g + 1, (5, 27, 'Z'))
    for x in (6, 24):
        c.rect(x, g - 6, x + 2, g, 'K')
        c.set(x + 1, g - 5, '2')
    c.rect(5, g - 9, 27, g - 7, 'K')
    c.rect(6, g - 8, 26, g - 8, '1')
    c.rect(5, g - 17, 7, g - 9, 'K')
    c.rect(25, g - 17, 27, g - 9, 'K')
    c.rect(6, g - 16, 26, g - 14, 'K')
    c.rect(7, g - 15, 25, g - 15, '1')
    return c


PROPS = [
    ('tree_big', lambda: tree(True, 1)),
    ('tree_small', lambda: tree(False, 4)),
    ('bush', bush),
    ('lamp_post', lamp_post),
    ('fence', fence),
    ('well', well),
    ('crates', crates),
    ('sign', sign),
    ('flowerbed', flowerbed),
    ('water_tower', water_tower),
    ('bench', bench),
]


# ------------------------------------------------------------------ 길 / 잔디
def road_tile(kind, seed):
    c = Canvas(ROAD, ROAD)
    if kind == 'dirt':
        c.rect(0, 0, 15, 15, 'A')
        for y in range(16):
            for x in range(16):
                h = (x * 7 + y * 13 + seed * 29) % 9
                if h == 0:
                    c.set(x, y, '2')
                elif h in (1, 2):
                    c.set(x, y, 'a')
    elif kind == 'cobble':
        c.rect(0, 0, 15, 15, 'z')
        for by in range(0, 16, 4):
            off = 2 if (by // 4 + seed) % 2 else 0
            for bx in range(-2, 16, 4):
                x0 = bx + off
                c.rect(x0, by, x0 + 2, by + 2, 'x')
                c.row(by, (x0, x0 + 2, 'w'))
    else:  # brick
        c.rect(0, 0, 15, 15, 'C')
        for by in range(0, 16, 4):
            off = 2 if (by // 4 + seed) % 2 else 0
            for bx in range(-2, 16, 4):
                x0 = bx + off
                c.rect(x0, by, x0 + 2, by + 2, 'c')
                c.set(x0, by, '5')
    return c


def grass_tile(seed):
    c = Canvas(ROAD, ROAD)
    c.rect(0, 0, 15, 15, 'V')
    c.rect(0, 0, 15, 3, 'v')
    for x in range(16):
        h = (x * 11 + seed * 17) % 3
        c.set(x, 3 + h, 'v')
        if (x + seed) % 4 == 0:
            c.set(x, 1, 'j')
    for y in range(6, 16):
        for x in range(16):
            if (x * 5 + y * 9 + seed * 3) % 13 == 0:
                c.set(x, y, 'l')
    return c


if __name__ == '__main__':
    import os
    os.makedirs('out/Village', exist_ok=True)

    houses = sheet([house(t) for t in HOUSE_TIERS])
    houses.save('out/Village/houses.png')

    props = sheet([fn() for _, fn in PROPS])
    props.save('out/Village/props.png')

    roads = sheet([road_tile(k, s) for k in ('dirt', 'cobble', 'brick') for s in range(2)])
    roads.save('out/Village/roads.png')

    grass = sheet([grass_tile(s) for s in range(4)])
    grass.save('out/Village/grass.png')

    plate = Image.new('RGBA', (32 * 11, 48 + 32 + 16 + 16), (0, 0, 0, 0))
    plate.paste(houses, (0, 0))
    plate.paste(props, (0, 48))
    plate.paste(roads, (0, 80))
    plate.paste(grass, (0, 96))
    preview(plate, scale=5, path='out/preview_village.png')
    print('ok', [n for n, _ in PROPS])
