from PIL import Image, ImageDraw, ImageFont
FONT = "/usr/share/fonts/opentype/noto/NotoSansCJK-Bold.ttc"
BW, AW = 32, 64

def make(entries, buried_sheet, alive_sheet, title, path, cols=6, sc=4):
    CELL_W = (BW + AW) * sc          # 발굴 + 복원 나란히
    CELL_H = AW * sc
    LAB, PAD, HEAD = 18, 10, 30
    rows = (len(entries) + cols - 1) // cols
    W = PAD + cols * (CELL_W + PAD)
    H = HEAD + rows * (CELL_H + LAB + PAD)
    out = Image.new("RGBA", (W, H), (26, 24, 30, 255)); d = ImageDraw.Draw(out)
    fh = ImageFont.truetype(FONT, 19); fl = ImageFont.truetype(FONT, 14)
    d.text((PAD, 6), title, font=fh, fill=(244, 236, 218))
    for i, e in enumerate(entries):
        r, c = divmod(i, cols)
        x = PAD + c * (CELL_W + PAD); y = HEAD + r * (CELL_H + LAB + PAD)
        cell = Image.new("RGBA", (CELL_W, CELL_H), (0, 0, 0, 255))
        cd = ImageDraw.Draw(cell)
        cd.rectangle([0, 0, BW * sc - 1, CELL_H], fill=(50, 38, 30, 255))     # 흙
        cd.rectangle([BW * sc, 0, CELL_W, CELL_H], fill=(78, 88, 104, 255))   # 지상
        cd.rectangle([BW * sc, CELL_H - 3 * sc, CELL_W, CELL_H], fill=(72, 96, 62, 255))
        bimg = buried_sheet.crop((i * BW, 0, (i + 1) * BW, BW)).resize((BW * sc, BW * sc), Image.NEAREST)
        cell.alpha_composite(bimg, (0, (CELL_H - BW * sc) // 2))
        aimg = alive_sheet.crop((i * 2 * AW, 0, (i * 2 + 1) * AW, AW)).resize((AW * sc, AW * sc), Image.NEAREST)
        cell.alpha_composite(aimg, (BW * sc, -2 * sc))
        out.alpha_composite(cell, (x, y))
        d.text((x + 2, y + CELL_H + 2), f"{i + 1}. {e[1]}", font=fl, fill=(214, 208, 198))
    out.save(path)
    return out.size
