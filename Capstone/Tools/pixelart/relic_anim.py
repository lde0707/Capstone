"""4프레임 복원본 확인용 — 프레임을 가로로 늘어놓은 시트와 움직이는 GIF 를 만든다."""
from PIL import Image, ImageDraw, ImageFont
from relic_lib import BW, AW, FRAMES, cell

FONT = "/usr/share/fonts/opentype/noto/NotoSansCJK-Bold.ttc"
SKY = (78, 88, 104, 255)
GRASS = (72, 96, 62, 255)
DIRT = (50, 38, 30, 255)


def _stage(w, h, sc):
    cell = Image.new("RGBA", (w, h), SKY)
    d = ImageDraw.Draw(cell)
    d.rectangle([0, h - 3 * sc, w, h], fill=GRASS)
    return cell


def frames_sheet(entries, alive_sheet, title, path, sc=3):
    """한 줄에 유물 하나 — 발굴본 + 복원 4프레임."""
    CW, CH = AW * sc, AW * sc
    LAB, PAD, HEAD = 16, 8, 28
    W = PAD + BW * sc + PAD + FRAMES * (CW + 4) + PAD
    H = HEAD + len(entries) * (CH + LAB)
    out = Image.new("RGBA", (W, H), (26, 24, 30, 255))
    d = ImageDraw.Draw(out)
    fh = ImageFont.truetype(FONT, 18)
    fl = ImageFont.truetype(FONT, 13)
    d.text((PAD, 5), title, font=fh, fill=(244, 236, 218))
    for i, e in enumerate(entries):
        y = HEAD + i * (CH + LAB)
        for k in range(FRAMES):
            x = PAD + BW * sc + PAD + k * (CW + 4)
            stage = _stage(CW, CH, sc)
            im = cell(alive_sheet, i * FRAMES + k, AW)
            stage.alpha_composite(im.resize((CW, CH), Image.NEAREST), (0, 0))
            out.alpha_composite(stage, (x, y))
        d.text((PAD, y + CH // 2), f"{i + 1}. {e[1]}", font=fl, fill=(214, 208, 198))
    out.save(path)
    return out.size


def gif_row(entries, alive_sheet, path, sc=3, ms=180, cols=6):
    """전부 한 화면에서 동시에 움직이는 GIF."""
    CW, CH = AW * sc, AW * sc
    LAB, PAD = 14, 6
    rows = (len(entries) + cols - 1) // cols
    W = PAD + cols * (CW + PAD)
    H = PAD + rows * (CH + LAB + PAD)
    fl = ImageFont.truetype(FONT, 12)
    imgs = []
    for k in range(FRAMES):
        out = Image.new("RGBA", (W, H), (26, 24, 30, 255))
        d = ImageDraw.Draw(out)
        for i, e in enumerate(entries):
            r, c = divmod(i, cols)
            x = PAD + c * (CW + PAD)
            y = PAD + r * (CH + LAB + PAD)
            stage = _stage(CW, CH, sc)
            im = cell(alive_sheet, i * FRAMES + k, AW)
            stage.alpha_composite(im.resize((CW, CH), Image.NEAREST), (0, 0))
            out.alpha_composite(stage, (x, y))
            d.text((x + 2, y + CH + 1), e[1], font=fl, fill=(206, 200, 190))
        imgs.append(out.convert("P", palette=Image.ADAPTIVE, colors=255))
    imgs[0].save(path, save_all=True, append_images=imgs[1:], duration=ms, loop=0,
                 disposal=2, optimize=False)
    return (W, H)
