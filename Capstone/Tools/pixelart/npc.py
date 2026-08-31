"""마을 주민 NPC — 16x16 옆모습 걷기 4프레임, 색 변형 6종."""
from pixlib import Canvas, sheet, preview
from PIL import Image

W = 16

# 오른쪽을 보는 주민 (rows 0..11). 'H' 머리, 'T' 상의, 'S' 피부
NPC_BODY = [
    "................",
    "................",
    "....KKKKKK......",
    "...KHHHHHHK.....",
    "...KHHHHHHK.....",
    "...KHSSSSSK.....",
    "...KHSSESSs.....",
    "....KsSSSsK.....",
    "...KTTTTTTK.....",
    "..KTTTuuTTTK....",
    "..KTTTTTTTSK....",
    "...KTtttTKSK....",
]

# 챙모자 쓴 변형
NPC_HAT = [
    "................",
    "....KKKKKKK.....",
    "..KKuuuuuuuKK...",
    "...KHHHHHHK.....",
    "...KHHHHHHK.....",
    "...KHSSSSSK.....",
    "...KHSSESSs.....",
    "....KsSSSsK.....",
    "...KTTTTTTK.....",
    "..KTTTuuTTTK....",
    "..KTTTTTTTSK....",
    "...KTtttTKSK....",
]


def legs(c, phase):
    c.row(12, (3, 3, 'K'), (4, 10, 'N'), (11, 11, 'K'))
    poses = {0: ((4, 6), (8, 10)), 1: ((3, 5), (9, 11)),
             2: ((5, 7), (7, 9)), 3: ((8, 10), (4, 6))}
    for (x0, x1), boot, pants in ((poses[phase][0], 'o', 'n'), (poses[phase][1], 'O', 'N')):
        c.row(13, (x0, x1, pants))
        c.row(14, (x0, x1, boot))
        c.row(15, (x0, x1, 'K'))
        c.set(x0 - 1, 13, 'K')
        c.set(x0 - 1, 14, 'K')
        c.set(x1 + 1, 14, 'K')


def frame(body, phase, kid=False):
    c = Canvas(W, W)
    c.blit_rows(body, 0, (-1 if phase in (1, 3) else 0) + (2 if kid else 0))
    if kid:
        legs_kid(c, phase)
    else:
        legs(c, phase)
    return c


def legs_kid(c, phase):
    c.row(13, (4, 4, 'K'), (5, 9, 'N'), (10, 10, 'K'))
    poses = {0: ((5, 6), (8, 9)), 1: ((4, 5), (9, 10)),
             2: ((6, 7), (7, 8)), 3: ((8, 9), (5, 6))}
    for (x0, x1), boot in ((poses[phase][0], 'o'), (poses[phase][1], 'O')):
        c.row(14, (x0, x1, 'N'))
        c.row(15, (x0, x1, boot))
        c.set(x0 - 1, 14, 'K')
        c.set(x1 + 1, 14, 'K')


# 머리(H) / 상의(T,t,u) / 바지(N,n) / 신발(O,o) 색 변형
VARIANTS = [
    dict(name='villager_a', hat=False, kid=False, pal={
        'H': (92, 62, 42, 255), 'T': (176, 96, 84, 255), 't': (130, 66, 58, 255),
        'u': (208, 132, 116, 255), 'N': (72, 74, 96, 255), 'n': (50, 52, 70, 255)}),
    dict(name='villager_b', hat=False, kid=False, pal={
        'H': (48, 42, 44, 255), 'T': (96, 122, 168, 255), 't': (66, 88, 126, 255),
        'u': (140, 166, 208, 255), 'N': (94, 84, 68, 255), 'n': (66, 58, 46, 255)}),
    dict(name='villager_c', hat=True, kid=False, pal={
        'H': (154, 128, 72, 255), 'T': (128, 152, 100, 255), 't': (92, 114, 70, 255),
        'u': (176, 196, 140, 255), 'N': (108, 92, 74, 255), 'n': (76, 64, 50, 255)}),
    dict(name='villager_d', hat=False, kid=False, pal={
        'H': (168, 152, 128, 255), 'T': (146, 116, 164, 255), 't': (104, 80, 120, 255),
        'u': (186, 158, 200, 255), 'N': (62, 66, 74, 255), 'n': (44, 46, 54, 255)}),
    dict(name='villager_e', hat=True, kid=False, pal={
        'H': (76, 56, 46, 255), 'T': (196, 158, 92, 255), 't': (146, 112, 62, 255),
        'u': (222, 194, 136, 255), 'N': (84, 96, 92, 255), 'n': (58, 68, 66, 255)}),
    dict(name='villager_kid', hat=False, kid=True, pal={
        'H': (188, 148, 84, 255), 'T': (108, 168, 152, 255), 't': (74, 122, 112, 255),
        'u': (156, 204, 190, 255), 'N': (150, 96, 86, 255), 'n': (108, 66, 58, 255)}),
]


def build():
    out = {}
    for v in VARIANTS:
        body = NPC_HAT if v['hat'] else NPC_BODY
        frames = [frame(body, p, v['kid']) for p in range(4)]
        out[v['name']] = sheet(frames, overrides=v['pal'])
    return out


if __name__ == '__main__':
    import os
    os.makedirs('out/NPC', exist_ok=True)
    imgs = build()
    for name, img in imgs.items():
        img.save(f'out/NPC/{name}.png')
    plate = Image.new('RGBA', (16 * 4, 16 * len(imgs)), (0, 0, 0, 0))
    for i, n in enumerate(imgs):
        plate.paste(imgs[n], (0, i * 16))
    preview(plate, scale=10, path='out/preview_npc.png')
    print('ok', list(imgs))
