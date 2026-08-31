"""쓰레기 집게 소년 — 16x16 5방향 애니메이션."""
from pixlib import Canvas, sheet, preview

W = 16

# ---------------------------------------------------------------- 상체 (rows 0..11)
BODY_DOWN = [
    "................",
    "....KKKKKKKK....",
    "...KYYYYYYYYK...",
    "...KWYYYYYYYK...",
    "..KyyyyyyyyyyK..",
    "...KSSSSSSSSK...",
    "...KsESSSEsK....",
    "....KsSSSsK.....",
    "..KTTTTTTTTTTK..",
    "..KTTuTTTTuTTK..",
    "..KSTTTTTTTTSK..",
    "..KKKTtttTKKK...",
]

BODY_UP = [
    "................",
    "....KKKKKKKK....",
    "...KYYYYYYYYK...",
    "...KYYYYYYYYK...",
    "..KyyyyyyyyyyK..",
    "...KHHHHHHHHK...",
    "...KHHHHHHHHK...",
    "....KHHHHHHK....",
    "..KTTKBBBBKTTK..",
    "..KTTKBbbBKTTK..",
    "..KSTKBBBBKTSK..",
    "..KKKKBBBBKKK...",
]

# 오른쪽을 보는 옆모습
BODY_SIDE = [
    "................",
    "....KKKKKKK.....",
    "...KYYYYYYYK....",
    "...KYYYYYYYWK...",
    "..KyyyyyyyyyyK..",
    "...KHHSSSSSK....",
    "...KHSSSESSSK...",
    "...KHsSSSSsK....",
    "..KBBTTTTTTK....",
    "..KBbTTTTTTTK...",
    "..KBBTTTTTSSK...",
    "...KKTtttKSK....",
]


def legs(c, phase, back=False):
    """rows 12..15 정면/후면 걷기."""
    lifts = {0: (0, 0), 1: (1, 0), 2: (0, 0), 3: (0, 1)}[phase]
    c.row(12, (3, 3, 'K'), (4, 11, 'N'), (12, 12, 'K'))
    for (x0, x1, lift, left) in ((4, 6, lifts[0], True), (9, 11, lifts[1], False)):
        y = 13 - lift
        c.row(y, (x0, x1, 'N'))
        c.row(y + 1, (x0, x1, 'O'))
        c.row(y + 2, (x0, x1, 'o'))
        ox = x0 - 1 if left else x1 + 1
        c.set(ox, y, 'K')
        c.set(ox, y + 1, 'K')
        c.set(ox, y + 2, 'K')


def legs_side(c, phase):
    """옆모습 걷기 — 앞다리(밝음)/뒷다리(어두움)."""
    c.row(12, (3, 3, 'K'), (4, 10, 'N'), (11, 11, 'K'))
    poses = {
        0: ((4, 6), (8, 10)),
        1: ((3, 5), (9, 11)),
        2: ((5, 7), (7, 9)),
        3: ((8, 10), (4, 6)),
    }
    for (x0, x1), boot, pants in ((poses[phase][0], 'o', 'n'), (poses[phase][1], 'O', 'N')):
        c.row(13, (x0, x1, pants))
        c.row(14, (x0, x1, boot))
        c.row(15, (x0, x1, 'K'))
        c.set(x0 - 1, 13, 'K')
        c.set(x0 - 1, 14, 'K')
        c.set(x1 + 1, 14, 'K')


def make_walk(body, phase, back=False, side=False):
    c = Canvas(W, W)
    c.blit_rows(body, 0, -1 if phase in (1, 3) else 0)
    (legs_side(c, phase) if side else legs(c, phase, back))
    return c


# ---------------------------------------------------------------- 곡괭이
def pick_from(c, hx, hy, dx, dy, reach=4):
    """손 (hx,hy) 에서 (dx,dy) 방향으로 자루를 뻗고 끝에 쇠머리를 붙인다."""
    x, y = hx, hy
    for i in range(1, reach + 1):
        x, y = hx + dx * i, hy + dy * i
        c.set(x, y, 'P' if i % 2 else 'p')
    if dy == 0:                     # 수평
        c.row(y - 1, (x, x + dx, 'M'))
        c.row(y, (x, x + dx, 'M'))
        c.row(y + 1, (x, x + dx, 'm'))
    elif dx == 0:                   # 수직
        c.row(y + dy, (x - 1, x + 1, 'M'))
        c.row(y + dy * 2, (x - 1, x + 1, 'm'))
    else:                           # 대각
        c.row(y + dy, (x - 1, x + 1, 'M'))
        c.row(y + dy * 2, (x, x + 1, 'm'))


def make_dig_side(phase):
    c = Canvas(W, W)
    bob = 1 if phase == 2 else 0
    c.blit_rows(BODY_SIDE, 0, bob)
    legs_side(c, 0)
    hx, hy = 10, 10 + bob
    if phase == 0:
        pick_from(c, hx, hy - 1, 0, -1, 4)          # 치켜듦
    elif phase == 1:
        pick_from(c, hx, hy - 1, 1, -1, 3)          # 대각
    elif phase == 2:
        pick_from(c, hx, hy, 1, 0, 3)               # 강타
    else:
        pick_from(c, hx, hy, 1, 1, 3)               # 아래로
    return c


def make_dig_down(phase):
    c = Canvas(W, W)
    bob = 1 if phase in (1, 2) else 0
    c.blit_rows(BODY_DOWN, 0, bob)
    legs(c, 0)
    depth = (0, 1, 2, 1)[phase]
    pick_from(c, 12, 8 + bob + depth, 0, 1, 3)
    return c


def make_dig_up(phase):
    c = Canvas(W, W)
    bob = 1 if phase in (1, 2) else 0
    c.blit_rows(BODY_DOWN, 0, bob)
    legs(c, 0)
    rise = (0, 1, 2, 1)[phase]
    pick_from(c, 12, 10 + bob - rise, 0, -1, 3)
    return c


def build():
    return {
        'hero_down': sheet([make_walk(BODY_DOWN, p) for p in range(4)]),
        'hero_up': sheet([make_walk(BODY_UP, p, back=True) for p in range(4)]),
        'hero_side': sheet([make_walk(BODY_SIDE, p, side=True) for p in range(4)]),
        'hero_dig_side': sheet([make_dig_side(p) for p in range(4)]),
        'hero_dig_down': sheet([make_dig_down(p) for p in range(4)]),
        'hero_dig_up': sheet([make_dig_up(p) for p in range(4)]),
    }


if __name__ == '__main__':
    import os
    from PIL import Image
    os.makedirs('out/Player', exist_ok=True)
    imgs = build()
    for name, img in imgs.items():
        img.save(f'out/Player/{name}.png')
    order = list(imgs)
    plate = Image.new('RGBA', (16 * 4, 16 * len(order)), (0, 0, 0, 0))
    for i, n in enumerate(order):
        plate.paste(imgs[n], (0, i * 16))
    preview(plate, scale=10, path='out/preview_player.png')
    print('ok', order)
