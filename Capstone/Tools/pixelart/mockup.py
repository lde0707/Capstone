"""지상 마을이 자라는 모습 미리보기 — 1일차 vs 4일차 vs 7일차."""
from PIL import Image
import village
import npc
import player

CELL = 16
W_CELLS = 46
SKY = 10          # 하늘 칸
GROUND = 4        # 아래 땅 칸
H = (SKY + GROUND) * CELL
W = W_CELLS * CELL


def sky_gradient(top, bottom):
    img = Image.new('RGBA', (W, H), (0, 0, 0, 0))
    p = img.load()
    for y in range(SKY * CELL):
        t = y / float(SKY * CELL - 1)
        col = tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(3)) + (255,)
        for x in range(W):
            p[x, y] = col
    return img


def paste(base, img, cell_x, cell_y_bottom):
    """bottom-center 기준으로 붙인다."""
    x = int(cell_x * CELL + CELL * 0.5 - img.width * 0.5)
    y = int(cell_y_bottom * CELL - img.height)
    base.alpha_composite(img, (max(0, x), max(0, y)))


def scene(home_tier, houses, props, villagers, road_style, sky_top, sky_bottom):
    img = sky_gradient(sky_top, sky_bottom)

    ground_row = SKY
    grass = [village.grass_tile(s).to_image() for s in range(4)]
    road = [village.road_tile(k, s).to_image()
            for k in ('dirt', 'cobble', 'brick') for s in range(2)]
    dirt = village.Canvas(CELL, CELL)
    dirt.rect(0, 0, 15, 15, '2')
    for y in range(16):
        for x in range(16):
            if (x * 7 + y * 5) % 6 == 0:
                dirt.set(x, y, 'I')
    dirt_img = dirt.to_image()

    town_min, town_max = 23 - len(houses) * 2 - 6, 23 + len(houses) * 2 + 6
    for cx in range(W_CELLS):
        if town_min <= cx <= town_max and road_style >= 0:
            tile = road[road_style * 2 + cx % 2]
        else:
            tile = grass[(cx * 7 + 3) % 4]
        img.alpha_composite(tile, (cx * CELL, ground_row * CELL))
        for gy in range(ground_row + 1, SKY + GROUND):
            img.alpha_composite(dirt_img, (cx * CELL, gy * CELL))

    for (cx, tier) in houses:
        paste(img, village.house(village.HOUSE_TIERS[tier]).to_image(), cx, ground_row)
    for (cx, prop) in props:
        paste(img, prop().to_image(), cx, ground_row)
    paste(img, village.house(village.HOUSE_TIERS[home_tier]).to_image(), 23, ground_row)

    npcs = npc.build()
    names = list(npcs)
    for i, cx in enumerate(villagers):
        sprite = npcs[names[i % len(names)]].crop(((i % 4) * 16, 0, (i % 4) * 16 + 16, 16))
        if i % 3 == 0:
            sprite = sprite.transpose(Image.FLIP_LEFT_RIGHT)
        paste(img, sprite, cx, ground_row + 0.1)

    hero = player.build()['hero_side'].crop((0, 0, 16, 16))
    paste(img, hero, 20, ground_row + 0.1)
    return img


P = village
SCENES = [
    ("1일차 — 다 쓰러져가는 판잣집 한 채", dict(
        home_tier=0, houses=[], props=[(19, P.bush), (27, lambda: P.tree(False, 4))],
        villagers=[25], road_style=-1,
        sky_top=(96, 150, 200), sky_bottom=(190, 214, 226))),
    ("4일차 — 이웃이 모여 마을이 된다", dict(
        home_tier=2, houses=[(15, 1), (31, 1), (11, 0)],
        props=[(19, P.bush), (27, lambda: P.tree(True, 1)), (13, P.lamp_post),
               (35, lambda: P.tree(False, 4)), (9, P.fence), (37, P.well)],
        villagers=[13, 18, 26, 33, 30], road_style=1,
        sky_top=(72, 128, 190), sky_bottom=(214, 190, 160))),
    ("7일차 — 도시가 되었다", dict(
        home_tier=5, houses=[(15, 4), (31, 4), (11, 3), (35, 3), (7, 2), (39, 2)],
        props=[(19, P.lamp_post), (27, lambda: P.tree(True, 1)), (13, P.flowerbed),
               (37, P.water_tower), (9, P.bench), (41, lambda: P.tree(True, 7)),
               (5, P.sign), (43, P.crates), (23 - 6, P.bush)],
        villagers=[9, 13, 17, 21, 26, 29, 33, 37, 41, 6], road_style=2,
        sky_top=(40, 44, 96), sky_bottom=(232, 132, 92))),
]

if __name__ == '__main__':
    from PIL import ImageDraw
    pad = 10
    scale = 2
    tiles = [scene(**kw) for _, kw in SCENES]
    plate = Image.new('RGBA', (W * scale, (H * scale + 26) * len(tiles) + pad),
                      (24, 22, 28, 255))
    d = ImageDraw.Draw(plate)
    for i, ((label, _), t) in enumerate(zip(SCENES, tiles)):
        y = i * (H * scale + 26)
        plate.alpha_composite(t.resize((W * scale, H * scale), Image.NEAREST), (0, y + 22))
        d.text((6, y + 6), label, fill=(232, 226, 214, 255))
    plate.save('out/preview_town.png')
    print('saved out/preview_town.png', plate.size)
