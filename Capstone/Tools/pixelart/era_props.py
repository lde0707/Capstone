"""시대별 장식물 — 10종 x 5시대 = 50개. 32x32, bottom-center, 16px 그리드 2칸.

시트: era_props_{era}.png (320x32, 10프레임). 인덱스 = Prop enum 순서.
"""
from PIL import Image
from pixbox import Box, ERA, K, C, sheet

S = 32


def new():
    return Box(S, S)


def fin(b, glow_at=None):
    b.outline()
    if glow_at:
        cx, cy, r, col = glow_at
        b.glow(cx, cy, r, col, 0.5)
    b.auto_shadow()
    return b.img


# ================================================================ 1. 나무
def tree(e):
    p = ERA[e]
    b = new()
    g = b.ground
    if e == 0:                                   # 고사리 나무
        b.rect(14, 12, 17, g, p['w2'])
        b.rect(14, 12, 15, g, p['w1'])
        for (dx, dy) in ((-10, -2), (10, -2), (-8, -8), (8, -8), (0, -12)):
            b.poly([(16, 13), (16 + dx, 13 + dy), (16 + dx // 2, 17 + dy)], p['g1'])
            b.poly([(16, 13), (16 + dx, 14 + dy), (16 + dx // 2, 16 + dy)], p['g2'])
    elif e == 1:                                 # 둥근 활엽수
        b.rect(14, 16, 17, g, p['w2'])
        b.rect(14, 16, 15, g, p['w1'])
        b.line(15, 20, 10, 16, p['w2'])
        b.line(16, 20, 22, 16, p['w2'])
        b.ellipse(4, 2, 28, 20, p['g2'])
        b.ellipse(6, 3, 22, 16, p['g1'])
        b.ellipse(10, 5, 18, 11, C(146, 182, 106))
    elif e == 2:                                 # 굽은 소나무
        b.poly([(13, g), (18, g), (20, 18), (17, 10), (14, 10), (12, 18)], p['w2'])
        b.poly([(13, g), (15, g), (14, 18), (15, 11)], p['w1'])
        for (y, half) in ((9, 11), (14, 9), (19, 7)):
            b.poly([(16 - half, y + 4), (16 + half, y + 4), (16, y - 2)], p['g2'])
            b.poly([(16 - half + 2, y + 3), (16 + half - 3, y + 3), (16, y - 1)], p['g1'])
    elif e == 3:                                 # 가로수
        b.rect(14, 14, 18, g, p['w2'])
        b.rect(14, 14, 15, g, p['w1'])
        b.rect(12, g - 2, 20, g, p['s3'])
        b.ellipse(5, 1, 27, 17, p['g2'])
        b.ellipse(7, 2, 22, 13, p['g1'])
        b.streak(6, 26, 3, 16, p['g3'], period=4)
    else:                                        # 홀로 나무
        b.rect(14, 12, 17, g, p['w2'])
        b.rect(14, 12, 15, g, p['w1'])
        b.rect(11, g - 2, 21, g, p['s2'])
        b.ellipse(5, 1, 27, 17, p['g2'])
        b.ellipse(8, 3, 23, 14, p['g1'])
        for y in range(3, 17, 3):
            b.line(6, y, 26, y, p['a1'])
    return fin(b, (16, 9, 12, ERA[e]['a1']) if e == 4 else None)


# ================================================================ 2. 덤불
def bush(e):
    p = ERA[e]
    b = new()
    g = b.ground
    if e == 0:                                   # 거친 풀 뭉치
        for (x, h) in ((6, 12), (11, 17), (16, 20), (21, 15), (26, 10)):
            b.poly([(x - 2, g), (x + 2, g), (x, g - h)], p['g2'])
            b.poly([(x - 1, g), (x + 1, g), (x, g - h + 2)], p['g1'])
    elif e == 1:
        b.ellipse(4, 14, 28, g, p['g2'])
        b.ellipse(6, 15, 20, g - 3, p['g1'])
        b.ellipse(16, 12, 27, 24, p['g2'])
    elif e == 2:                                 # 다듬은 관목 + 꽃
        b.ellipse(4, 13, 28, g, p['g2'])
        b.ellipse(6, 14, 21, g - 4, p['g1'])
        for (x, y) in ((10, 17), (19, 15), (24, 21), (14, 22)):
            b.rect(x, y, x + 1, y + 1, p['a1'])
    elif e == 3:                                 # 화단
        b.rect(2, 22, 29, g, p['s2'])
        b.rect(2, 22, 29, 23, p['s1'])
        b.rect(4, 15, 27, 22, p['g2'])
        b.ellipse(5, 13, 26, 22, p['g1'])
        for (x, y, c) in ((8, 15, p['a1']), (15, 13, C(236, 232, 240)), (22, 16, p['a1'])):
            b.rect(x, y, x + 2, y + 2, c)
    else:                                        # 발광 이끼
        b.ellipse(4, 15, 28, g, p['g2'])
        b.ellipse(7, 16, 20, g - 3, p['g1'])
        for (x, y) in ((10, 19), (16, 17), (22, 21), (13, 24)):
            b.rect(x, y, x + 1, y + 1, p['a1'])
    return fin(b, (16, 21, 11, ERA[e]['a1']) if e == 4 else None)


# ================================================================ 3. 바위
def rock(e):
    p = ERA[e]
    b = new()
    g = b.ground
    if e == 4:                                   # 부유 결정
        b.rect(6, g - 2, 26, g, p['s2'])
        b.poly([(16, 4), (24, 14), (16, 24), (8, 14)], p['a2'])
        b.poly([(16, 4), (24, 14), (16, 24)], p['a1'])
        b.poly([(16, 8), (20, 14), (16, 20), (12, 14)], C(232, 252, 255))
        return fin(b, (16, 14, 14, p['a1']))
    if e == 3:                                   # 콘크리트 블록
        for (x, y, w, h) in ((3, 20, 12, 10), (16, 17, 13, 13), (8, 13, 11, 7)):
            b.rect(x, y, x + w, y + h, p['s2'] if e != 3 else C(150, 146, 140))
            b.rect(x, y, x + w, y + 1, C(184, 180, 172))
            b.rect(x, y + h - 1, x + w, y + h, C(104, 100, 96))
        return fin(b)
    big = (4, 12, 27, g) if e != 1 else (7, 14, 26, g)
    b.ellipse(*big, p['s2'])
    b.ellipse(big[0] + 2, big[1] + 2, big[0] + (big[2] - big[0]) // 2, big[3] - 3, p['s1'])
    b.rect(big[0] + 1, g - 2, big[2] - 1, g, p['s3'])
    if e == 1:
        b.ellipse(2, 21, 12, g, p['s2'])
        b.ellipse(3, 22, 8, g - 2, p['s1'])
    if e == 2:                                   # 이끼 낀 정원석
        for (x, y) in ((9, 14), (18, 13), (13, 17), (22, 19)):
            b.rect(x, y, x + 2, y + 1, p['g2'])
    return fin(b)


# ================================================================ 4. 울타리
def fence(e):
    p = ERA[e]
    b = new()
    g = b.ground
    if e == 4:                                   # 에너지 펜스
        for x in (4, 27):
            b.rect(x - 2, 10, x + 2, g, p['w2'])
            b.rect(x - 2, 10, x - 1, g, p['w1'])
            b.rect(x - 3, 8, x + 3, 11, p['s3'])
        for y in (14, 19, 24):
            b.line(4, y, 27, y, p['a1'])
        return fin(b, (16, 19, 14, p['a1']))
    if e == 3:                                   # 철제 난간
        for x in (3, 15, 28):
            b.rect(x - 1, 12, x + 1, g, p['s3'] if e != 3 else C(96, 102, 110))
        for y in (15, 22):
            b.rect(2, y, 29, y + 1, C(140, 146, 154))
            b.rect(2, y, 29, y, C(178, 184, 192))
        return fin(b)
    posts = (4, 12, 20, 28) if e != 0 else (5, 13, 21, 28)
    for x in posts:
        top = 11 + ((x * 5) % 3)
        b.rect(x - 2, top, x + 1, g, p['w2'])
        b.rect(x - 2, top, x - 1, g, p['w1'])
        if e == 0:
            b.rect(x - 3, top - 2, x + 2, top + 1, C(222, 214, 194))   # 뼈 끝
    for y in ((16, 23) if e != 2 else (15, 20, 25)):
        b.rect(2, y, 29, y + 1, p['w2'])
        b.rect(2, y, 29, y, p['w1'])
    return fin(b)


# ================================================================ 5. 조명
def lamp(e):
    p = ERA[e]
    b = new()
    g = b.ground
    if e == 0:                                   # 모닥불
        b.rect(5, g - 5, 27, g, p['w3'])
        b.rect(7, g - 6, 25, g - 3, p['w2'])
        for (x, y, w) in ((3, g - 3, 6), (23, g - 3, 6)):
            b.ellipse(x, y, x + w, g, p['s2'])
        b.poly([(16, 8), (23, g - 5), (9, g - 5)], C(226, 104, 56))
        b.poly([(16, 12), (21, g - 5), (11, g - 5)], p['glow'])
        b.poly([(16, 16), (19, g - 5), (13, g - 5)], C(255, 240, 180))
        return fin(b, (16, 18, 15, p['glow']))
    if e == 1:                                   # 기름 등잔
        b.rect(11, 22, 21, g, p['s2'])
        b.rect(11, 22, 15, g, p['s1'])
        b.rect(9, 20, 23, 23, p['s3'])
        b.rect(12, 14, 20, 21, p['a2'])
        b.ellipse(11, 12, 21, 17, p['a1'])
        b.ellipse(13, 13, 19, 16, p['dark'])
        b.poly([(16, 6), (19, 13), (13, 13)], p['glow'])
        return fin(b, (16, 11, 12, p['glow']))
    if e == 2:                                   # 석등
        b.rect(9, g - 3, 23, g, p['s3'])
        b.rect(11, g - 4, 21, g - 1, p['s2'])
        b.rect(13, 16, 19, g - 3, p['s2'])
        b.rect(13, 16, 15, g - 3, p['s1'])
        b.rect(9, 11, 23, 17, p['s2'])
        b.rect(11, 12, 21, 16, p['glow'])
        b.rect(13, 13, 19, 15, C(255, 244, 196))
        b.poly([(6, 11), (26, 11), (22, 5), (10, 5)], p['r2'])
        b.poly([(6, 11), (16, 11), (16, 5), (10, 5)], p['r1'])
        b.rect(15, 2, 17, 5, p['s2'])
        return fin(b, (16, 14, 13, p['glow']))
    if e == 3:                                   # 가로등
        b.rect(12, g - 2, 20, g, p['s3'])
        b.rect(14, 8, 18, g - 1, C(92, 98, 106))
        b.rect(14, 8, 15, g - 1, C(138, 144, 152))
        b.poly([(16, 8), (28, 8), (28, 11), (16, 11)], C(112, 118, 126))
        b.rect(24, 11, 31, 15, C(138, 144, 152))
        b.rect(25, 12, 30, 15, p['glow'])
        b.rect(26, 13, 29, 15, C(255, 246, 208))
        return fin(b, (27, 15, 14, p['glow']))
    # 미래 — 부유 네온 조명
    b.rect(10, g - 2, 22, g, p['s2'])
    b.rect(14, 14, 18, g - 2, p['w2'])
    b.ellipse(6, 4, 26, 14, p['s1'])
    b.ellipse(8, 6, 24, 12, p['a1'])
    b.ellipse(11, 7, 21, 11, C(232, 253, 255))
    return fin(b, (16, 10, 16, p['a1']))


# ================================================================ 6. 물
def well(e):
    p = ERA[e]
    b = new()
    g = b.ground
    if e == 0:                                   # 물웅덩이
        b.ellipse(2, 20, 29, g, p['s2'])
        b.ellipse(5, 22, 26, g - 1, C(78, 128, 150))
        b.ellipse(7, 23, 18, 27, C(120, 176, 196))
        for (x, y, w) in ((0, 21, 7), (24, 19, 8)):
            b.ellipse(x, y, x + w, y + 7, p['s2'])
            b.ellipse(x + 1, y + 1, x + w - 3, y + 4, p['s1'])
        return fin(b)
    if e == 4:                                   # 정수 탱크
        b.rect(6, g - 3, 26, g, p['s2'])
        b.rect(8, 8, 24, g - 3, p['s1'])
        b.rect(8, 8, 12, g - 3, C(236, 242, 250))
        b.rect(10, 12, 22, 24, p['a2'])
        b.rect(11, 13, 21, 23, p['a1'])
        for y in range(14, 23, 3):
            b.line(11, y, 21, y, C(226, 252, 255))
        b.rect(5, 5, 27, 9, p['s2'])
        return fin(b, (16, 18, 15, p['a1']))
    # 우물 몸통
    if e == 2:
        b.rect(6, 18, 26, g, p['s2'])
        b.rect(6, 18, 13, g, p['s1'])
        for y in range(20, g, 4):
            b.line(6, y, 26, y, p['s3'])
    elif e == 3:                                 # 수도 펌프
        b.rect(10, 18, 22, g, C(150, 146, 140))
        b.rect(10, 18, 14, g, C(184, 180, 172))
        b.rect(13, 10, 19, 19, C(120, 126, 134))
        b.rect(13, 10, 14, 19, C(168, 174, 182))
        b.poly([(19, 12), (26, 12), (26, 15), (19, 15)], C(150, 156, 164))
        b.rect(24, 15, 26, 18, C(120, 126, 134))
        b.line(13, 13, 6, 9, C(150, 156, 164))
        b.line(13, 14, 6, 10, C(110, 116, 124))
        return fin(b)
    else:
        b.rect(7, 18, 25, g, p['w2'])
        b.rect(7, 18, 13, g, p['w1'])
        for x in range(8, 26, 4):
            b.line(x, 18, x, g, p['w3'])
    b.rect(5, 16, 27, 19, p['s3'] if e == 2 else p['w3'])
    b.ellipse(7, 15, 25, 20, p['dark'])
    b.rect(8, 6, 10, 17, p['w2'])
    b.rect(22, 6, 24, 17, p['w2'])
    b.poly([(4, 8), (28, 8), (24, 3), (8, 3)], p['r2'])
    b.poly([(4, 8), (16, 8), (16, 3), (8, 3)], p['r1'])
    b.rect(15, 9, 17, 14, p['w3'])
    b.rect(13, 13, 19, 17, p['w1'])
    return fin(b)


# ================================================================ 7. 표지 기둥
def sign(e):
    p = ERA[e]
    b = new()
    g = b.ground
    if e == 0:                                   # 토템 기둥
        b.rect(12, 5, 20, g, p['w2'])
        b.rect(12, 5, 14, g, p['w1'])
        for y in (8, 15, 22):
            b.rect(11, y, 21, y + 4, p['a2'])
            b.rect(11, y, 21, y + 1, p['a1'])
            b.rect(13, y + 2, 15, y + 3, p['dark'])
            b.rect(17, y + 2, 19, y + 3, p['dark'])
        b.poly([(16, 0), (23, 6), (9, 6)], C(222, 214, 194))
        return fin(b)
    if e == 1:                                   # 솟대
        b.rect(14, 8, 17, g, p['w2'])
        b.rect(14, 8, 15, g, p['w1'])
        b.rect(10, g - 3, 22, g, p['s2'])
        b.ellipse(10, 3, 22, 9, p['w2'])
        b.poly([(22, 5), (28, 7), (22, 8)], p['w2'])
        b.ellipse(10, 3, 16, 8, p['w1'])
        b.px(12, 5, p['dark'])
        return fin(b)
    if e == 2:                                   # 장승
        b.rect(9, 6, 23, g, p['w2'])
        b.rect(9, 6, 14, g, p['w1'])
        b.poly([(9, 6), (23, 6), (16, 1)], p['w3'])
        b.rect(11, 10, 14, 12, p['dark'])        # 눈
        b.rect(18, 10, 21, 12, p['dark'])
        b.rect(14, 14, 18, 16, p['a2'])          # 코
        b.rect(11, 19, 21, 22, p['dark'])        # 입
        b.rect(12, 20, 20, 21, C(238, 232, 214))
        b.rect(7, 24, 25, 27, p['w3'])
        return fin(b)
    if e == 3:                                   # 전봇대
        b.rect(13, 2, 18, g, p['w2'])
        b.rect(13, 2, 14, g, p['w1'])
        b.rect(4, 6, 27, 8, p['w3'])
        b.rect(6, 12, 25, 14, p['w3'])
        for x in (6, 16, 25):
            b.rect(x - 1, 4, x + 1, 6, C(210, 214, 220))
        b.line(0, 5, 31, 3, C(56, 54, 58))
        b.line(0, 11, 31, 10, C(56, 54, 58))
        return fin(b)
    # 미래 — 홀로 표지
    b.rect(11, g - 2, 21, g, p['s2'])
    b.rect(14, 12, 18, g - 2, p['w2'])
    b.rect(5, 2, 27, 13, p['s3'])
    b.rect(6, 3, 26, 12, p['a2'])
    for y in range(4, 12, 2):
        b.line(7, y, 25, y, p['a1'])
    return fin(b, (16, 8, 16, p['a1']))


# ================================================================ 8. 저장물
def storage(e):
    p = ERA[e]
    b = new()
    g = b.ground
    if e == 0:                                   # 가죽 더미 + 뼈
        b.ellipse(2, 18, 24, g, p['a2'])
        b.ellipse(4, 19, 16, g - 3, p['a1'])
        b.ellipse(12, 14, 28, 26, p['a2'])
        b.ellipse(14, 15, 23, 21, p['a1'])
        b.rect(4, 12, 18, 14, C(222, 214, 194))
        b.rect(3, 11, 5, 15, C(222, 214, 194))
        b.rect(17, 11, 19, 15, C(222, 214, 194))
        return fin(b)
    if e == 1:                                   # 항아리 2개
        for (cx, h) in ((9, 16), (23, 13)):
            b.ellipse(cx - 8, g - h, cx + 8, g, p['a2'])
            b.ellipse(cx - 6, g - h + 2, cx + 1, g - 3, p['a1'])
            b.rect(cx - 4, g - h - 3, cx + 4, g - h + 2, C(120, 78, 50))
            b.rect(cx - 3, g - h - 3, cx + 3, g - h - 1, p['dark'])
        return fin(b)
    if e == 2:                                   # 볏단·가마니 (짚은 기와색이 아니라 누런 색)
        ST1, ST2, ST3 = C(212, 178, 108), C(168, 134, 74), C(120, 94, 48)
        for (cx, h) in ((9, 14), (23, 11)):
            b.ellipse(cx - 8, g - h, cx + 8, g, ST2)
            b.ellipse(cx - 6, g - h + 2, cx + 1, g - 3, ST1)
            b.line(cx - 8, g - h // 2, cx + 8, g - h // 2, ST3)
            b.line(cx - 8, g - h // 2 + 1, cx + 8, g - h // 2 + 1, C(146, 112, 60))
        b.poly([(13, 13), (20, 13), (18, 2), (15, 2)], ST1)
        b.poly([(17, 13), (20, 13), (18, 3)], ST2)
        b.line(16, 4, 16, 12, ST3)
        b.rect(13, 8, 20, 9, ST3)
        return fin(b)
    if e == 3:                                   # 나무 상자 더미
        for (x, y, w, h) in ((2, 18, 13, 12), (17, 16, 13, 14), (7, 8, 12, 9)):
            b.rect(x, y, x + w, y + h, p['w2'])
            b.rect(x, y, x + w, y + 1, p['w1'])
            b.rect(x, y + h - 1, x + w, y + h, p['w3'])
            b.line(x, y + h // 2, x + w, y + h // 2, p['w3'])
            b.rect(x + w // 2 - 1, y, x + w // 2 + 1, y + h, p['w3'])
        return fin(b)
    # 미래 — 컨테이너
    b.rect(2, 14, 29, g, p['s2'])
    b.rect(2, 14, 12, g, p['s1'])
    b.rect(2, 14, 29, 15, C(236, 242, 250))
    for x in range(5, 29, 4):
        b.line(x, 16, x, g - 1, p['s3'])
    b.rect(2, 21, 29, 23, p['a2'])
    b.rect(3, 21, 28, 22, p['a1'])
    return fin(b, (16, 22, 13, p['a1']))


# ================================================================ 9. 앉는 것
def seat(e):
    p = ERA[e]
    b = new()
    g = b.ground
    if e == 0:                                   # 통나무
        b.rect(2, 20, 29, g, p['w2'])
        b.rect(2, 20, 29, 22, p['w1'])
        b.ellipse(0, 19, 7, g, p['w3'])
        b.ellipse(1, 21, 5, g - 2, p['a2'])
        b.streak(2, 29, 23, g, p['w3'], period=4)
        return fin(b)
    if e == 1:                                   # 돌 의자
        b.rect(3, 20, 28, 25, p['s2'])
        b.rect(3, 20, 28, 21, p['s1'])
        b.rect(5, 25, 10, g, p['s3'])
        b.rect(21, 25, 26, g, p['s3'])
        return fin(b)
    if e == 2:                                   # 평상
        b.rect(2, 18, 29, 22, p['w1'])
        for x in range(4, 29, 5):
            b.line(x, 18, x, 21, p['w2'])
        b.rect(2, 22, 29, 23, p['w3'])
        for x in (4, 25):
            b.rect(x, 23, x + 3, g, p['w2'])
        return fin(b)
    if e == 3:                                   # 벤치
        b.rect(3, 19, 28, 22, p['w1'])
        b.rect(3, 22, 28, 23, p['w3'])
        b.rect(3, 12, 28, 15, p['w1'])
        b.rect(3, 15, 28, 16, p['w3'])
        for x in (5, 24):
            b.rect(x, 12, x + 2, g, C(96, 102, 110))
        return fin(b)
    # 미래 — 부유 벤치
    b.rect(4, 16, 28, 20, p['s1'])
    b.rect(4, 16, 28, 17, C(240, 246, 252))
    b.rect(4, 20, 28, 21, p['s3'])
    b.rect(8, 24, 24, 26, p['a1'])
    return fin(b, (16, 24, 13, p['a1']))


# ================================================================ 10. 기념물
def monument(e):
    p = ERA[e]
    b = new()
    g = b.ground
    if e == 0:                                   # 고인돌
        for x in (4, 22):
            b.rect(x, 14, x + 6, g, p['s2'])
            b.rect(x, 14, x + 2, g, p['s1'])
        b.poly([(1, 13), (30, 11), (29, 6), (3, 8)], p['s2'])
        b.poly([(1, 13), (16, 12), (15, 7), (3, 8)], p['s1'])
        return fin(b)
    if e == 1:                                   # 돌탑
        for i, (w, h) in enumerate(((11, 5), (9, 5), (7, 4), (5, 4), (3, 3))):
            y = g - (i + 1) * 5
            b.ellipse(16 - w, y, 16 + w, y + 6, p['s2'])
            b.ellipse(16 - w + 1, y + 1, 16 - 1, y + 4, p['s1'])
        return fin(b)
    if e == 2:                                   # 비석
        b.ellipse(3, 24, 29, g, p['s2'])         # 거북 받침
        b.ellipse(5, 25, 17, g - 1, p['s1'])
        b.rect(10, 6, 22, 25, p['s1'])
        b.rect(10, 6, 13, 25, C(230, 226, 216))
        b.rect(10, 5, 22, 7, p['s3'])
        b.poly([(9, 6), (23, 6), (16, 1)], p['r2'])
        for y in range(11, 23, 4):
            b.line(13, y, 19, y, p['s3'])
        return fin(b)
    if e == 3:                                   # 분수 + 시계판
        b.ellipse(1, 22, 30, g, C(150, 146, 140))
        b.ellipse(4, 24, 27, g - 1, C(90, 140, 160))
        b.rect(13, 12, 19, 24, C(178, 174, 166))
        b.ellipse(9, 3, 23, 15, C(196, 192, 184))
        b.ellipse(11, 5, 21, 13, C(240, 236, 226))
        b.line(16, 9, 16, 6, C(60, 58, 60))
        b.line(16, 9, 20, 10, C(60, 58, 60))
        return fin(b)
    # 미래 — 홀로 조각
    b.ellipse(4, 25, 28, g, p['s2'])
    b.ellipse(6, 26, 26, g - 1, p['s1'])
    b.poly([(16, 2), (24, 16), (16, 25), (8, 16)], p['a2'])
    b.poly([(16, 5), (21, 16), (16, 22), (11, 16)], p['a1'])
    b.poly([(16, 9), (18, 16), (16, 19), (14, 16)], C(236, 253, 255))
    return fin(b, (16, 15, 16, p['a1']))


# ================================================================
PROPS = [
    ("tree", "나무", tree),
    ("bush", "덤불", bush),
    ("rock", "바위", rock),
    ("fence", "울타리", fence),
    ("lamp", "조명", lamp),
    ("well", "물", well),
    ("sign", "표지", sign),
    ("storage", "저장물", storage),
    ("seat", "앉을 자리", seat),
    ("monument", "기념물", monument),
]

PROP_NAMES = [
    ["고사리 나무", "억새 덤불", "바위", "뼈 울타리", "모닥불", "물웅덩이", "토템 기둥", "가죽 더미", "통나무 의자", "고인돌"],
    ["활엽수", "관목", "돌무더기", "말뚝 울타리", "기름 등잔", "두레박 우물", "솟대", "항아리", "돌 의자", "돌탑"],
    ["굽은 소나무", "다듬은 관목", "정원석", "싸리 울타리", "석등", "돌 우물", "장승", "볏단", "평상", "비석"],
    ["가로수", "화단", "콘크리트 더미", "철제 난간", "가로등", "수도 펌프", "전봇대", "나무 상자", "벤치", "분수 시계"],
    ["홀로 나무", "발광 이끼", "부유 결정", "에너지 펜스", "부유 조명", "정수 탱크", "홀로 표지", "컨테이너", "부유 벤치", "홀로 조각"],
]


def build():
    out = {}
    for e in range(5):
        out[f"era_props_{e}"] = sheet([fn(e) for _, _, fn in PROPS], S, S)
    return out


if __name__ == "__main__":
    import os
    os.makedirs("out/Era", exist_ok=True)
    for name, img in build().items():
        img.save(f"out/Era/{name}.png")
        print(name, img.size)
