"""32x32 픽셀 아트용 최소 도구."""
from PIL import Image

T = 32

class Box:
    def __init__(self, w=T, h=T):
        self.w, self.h = w, h
        self.px = [[None]*w for _ in range(h)]

    def set(self, x, y, c):
        if c is not None and 0 <= x < self.w and 0 <= y < self.h:
            self.px[int(y)][int(x)] = c

    def rect(self, x0, y0, x1, y1, c):
        for y in range(int(y0), int(y1)+1):
            for x in range(int(x0), int(x1)+1):
                self.set(x, y, c)

    def line(self, x0, y0, x1, y1, c):
        x0,y0,x1,y1 = map(int,(x0,y0,x1,y1))
        dx, dy = abs(x1-x0), -abs(y1-y0)
        sx = 1 if x0 < x1 else -1
        sy = 1 if y0 < y1 else -1
        err = dx+dy
        while True:
            self.set(x0, y0, c)
            if x0 == x1 and y0 == y1: break
            e2 = 2*err
            if e2 >= dy: err += dy; x0 += sx
            if e2 <= dx: err += dx; y0 += sy

    def ellipse(self, cx, cy, rx, ry, c, fill=True):
        for y in range(int(cy-ry), int(cy+ry)+1):
            for x in range(int(cx-rx), int(cx+rx)+1):
                d = ((x-cx)/max(.5,rx))**2 + ((y-cy)/max(.5,ry))**2
                if d <= 1.0:
                    if fill or d > 0.55: self.set(x, y, c)

    def poly(self, pts, c):
        """볼록 다각형 채우기 (스캔라인)."""
        ys = [p[1] for p in pts]
        for y in range(int(min(ys)), int(max(ys))+1):
            xs = []
            for i in range(len(pts)):
                (x0,y0),(x1,y1) = pts[i], pts[(i+1) % len(pts)]
                if y0 == y1: continue
                if min(y0,y1) <= y <= max(y0,y1):
                    xs.append(x0 + (x1-x0)*(y-y0)/(y1-y0))
            if xs: self.rect(min(xs), y, max(xs), y, c)

    def outline(self, c):
        """실루엣 바깥에 1px 테두리 — 흙 위에서 형태가 또렷해진다."""
        add = []
        for y in range(self.h):
            for x in range(self.w):
                if self.px[y][x] is not None: continue
                for dx,dy in ((1,0),(-1,0),(0,1),(0,-1)):
                    nx,ny = x+dx, y+dy
                    if 0 <= nx < self.w and 0 <= ny < self.h and self.px[ny][nx] is not None:
                        add.append((x,y)); break
        for x,y in add: self.set(x,y,c)

    def shadow(self, c=(0,0,0,70)):
        """바닥 그림자 한 줄."""
        cols = [x for x in range(self.w)
                if any(self.px[y][x] is not None for y in range(self.h))]
        if not cols: return
        lo, hi = min(cols)+1, max(cols)-1
        for x in range(lo, hi+1):
            self.set(x, self.h-2, c)

    def image(self):
        im = Image.new("RGBA", (self.w, self.h), (0,0,0,0))
        for y in range(self.h):
            for x in range(self.w):
                if self.px[y][x]: im.putpixel((x,y), self.px[y][x])
        return im
