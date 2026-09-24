"""시대별 주민 NPC — 16x16 옆모습 걷기 4프레임.

5시대 x 5종 (+ 이틀 머무는 원시·미래는 3종 더) = 31명. 각 시대는 가로 한 줄 시트(320x16, 20프레임)로 나간다.
프레임 인덱스 = kind * 4 + phase.

머리 블록은 8행(y 0~7), 상체는 4행(y 8~11), 다리는 y 12~15.
머리 위 2~3행을 비워 두면 갓·삿갓·물동이 같은 장식을 얹을 수 있다.
"""
from PIL import Image
from pixlib import Canvas, sheet, preview

W = 16
C = lambda r, g, b: (r, g, b, 255)

# ============================================================== 머리 (8행)
HEAD_PLAIN = [
    "................",
    "................",
    "....KKKKKK......",
    "...KHHHHHHK.....",
    "...KHHHHHHK.....",
    "...KHSSSSSK.....",
    "...KHSSESSs.....",
    "....KsSSSsK.....",
]
HEAD_MESSY = [                      # 헝클어진 원시인 머리
    "................",
    "..K..K...K......",
    "..KHKKHHHKHK....",
    "...KHHHHHHK.....",
    "...KHHHHHHK.....",
    "...KHSSSSSK.....",
    "...KHSSESSs.....",
    "....KsSSSsK.....",
]
HEAD_LONG = [                       # 긴 머리 (뒤로 늘어뜨림)
    "................",
    "................",
    "....KKKKKK......",
    "...KHHHHHHK.....",
    "..KHHHHHHHK.....",
    "..KHHSSSSSK.....",
    "..KHHSSESSs.....",
    "..KHHsSSSsK.....",
]
HEAD_TOPKNOT = [                    # 상투 + 망건
    "................",
    ".....KK.........",
    "....KHHK........",
    "....KHHK........",
    "...KHHHHHHK.....",
    "...KHSSSSSK.....",
    "...KHSSESSs.....",
    "....KsSSSsK.....",
]
HEAD_GAT = [                        # 갓 — 좁은 모자통 + 넓고 평평한 챙
    "................",
    "....KKKK........",
    "...KHHHHK.......",
    "...KHHHHK.......",
    ".KKHHHHHHHKK....",
    "...KHSSSSSK.....",
    "...KHSSESSs.....",
    "....KsSSSsK.....",
]
HEAD_JEONRIP = [                    # 전립 — 꼭지에 붉은 술
    ".....c..........",
    ".....KK.........",
    "....KHHK........",
    "...KHHHHK.......",
    "..KHHHHHHHK.....",
    "...KHSSSSSK.....",
    "...KHSSESSs.....",
    "....KsSSSsK.....",
]
HEAD_SATGAT = [                     # 삿갓 — 짚으로 짠 원뿔
    "................",
    ".....KKK........",
    "...KKaaaKK......",
    "..KaaaaaaaK.....",
    ".KaaaaaaaaaK....",
    "...KHSSSSSK.....",
    "...KHSSESSs.....",
    "....KsSSSsK.....",
]
HEAD_HELM = [                       # 금속 투구
    "................",
    ".....KK.........",
    "....KMMK........",
    "...KMMMMMMK.....",
    "...KMMMMMMK.....",
    "...KMmSSSMK.....",
    "...KMSSESSs.....",
    "....KsSSSsK.....",
]
HEAD_MASK = [                       # 뼈 가면 + 뿔
    "................",
    "..K......K......",
    "..KW....WK......",
    "...KWHHHWK......",
    "...KHHHHHHK.....",
    "...KWWWWWK......",
    "...KWEWEWs......",
    "....KWWWK.......",
]
HEAD_HARDHAT = [                    # 안전모 (앞챙)
    "................",
    "................",
    "....KYYYK.......",
    "...KYYYYYK......",
    "..KYYYYYYYYKK...",
    "...KHSSSSSK.....",
    "...KHSSESSs.....",
    "....KsSSSsK.....",
]
HEAD_FEDORA = [                     # 중절모
    "................",
    "................",
    "....KHHHK.......",
    "...KHHHHHK......",
    "..KKHHHHHKKK....",
    "...KHSSSSSK.....",
    "...KHSSESSs.....",
    "....KsSSSsK.....",
]
HEAD_CAP = [                        # 교복 모자 (앞챙)
    "................",
    "................",
    "....KHHHHK......",
    "...KHHHHHHK.....",
    "...KHHHHHHKKK...",
    "...KHSSSSSK.....",
    "...KHSSESSs.....",
    "....KsSSSsK.....",
]
HEAD_PERM = [                       # 파마머리
    "................",
    "...KKKKKK.......",
    "..KHHHHHHK......",
    "..KHHHHHHK......",
    "..KHHHHHHK......",
    "..KHHSSSSSK.....",
    "...KHSSESSs.....",
    "....KsSSSsK.....",
]
HEAD_BUNDLE = [                     # 머리에 인 보따리
    ".KqqqqqqK.......",
    "..KQQQQK........",
    "...KKKK.........",
    "..KHHHHHHK......",
    "..KHHHHHHK......",
    "..KHHSSSSSK.....",
    "...KHSSESSs.....",
    "....KsSSSsK.....",
]
HEAD_WATERJAR = [                   # 머리에 인 물동이
    "..KccccccK......",
    "..KCccccCK......",
    "...KCCCCK.......",
    "...KHHHHHHK.....",
    "..KHHHHHHHK.....",
    "..KHHSSSSSK.....",
    "..KHHSSESSs.....",
    "..KHHsSSSsK.....",
]
HEAD_DUGEON = [                     # 두건
    "................",
    "................",
    "....KKKKKK......",
    "...KTTTTTTK.....",
    "...KTTTTTTKK....",
    "...KHSSSSSK.....",
    "...KHSSESSs.....",
    "....KsSSSsK.....",
]
HEAD_VISOR = [                      # 미래 바이저
    "................",
    "................",
    "....KKKKKK......",
    "...KHHHHHHK.....",
    "...KHHHHHHK.....",
    "...KMffffMK.....",
    "...KsSSSSSs.....",
    "....KsSSSsK.....",
]
HEAD_ROBOT = [                      # 로봇 헤드 + 안테나
    ".....K..........",
    ".....M..........",
    "...KKMKKKK......",
    "...KMMMMMMK.....",
    "...KMffffMK.....",
    "...KMMMMMMK.....",
    "....KMMMMK......",
    "....KmmmmK......",
]
HEAD_DRONE = [                      # 드론 본체 — 위에 로터, 아래에 짐칸
    "................",
    ".KMMK....KMMK...",
    "..KmK.KK.KmK....",
    "..KKMMMMMMKK....",
    "...KMffffMK.....",
    "...KMMMMMMK.....",
    "....KmmmmK......",
    "................",
]
HEAD_ELDER = [                      # 흰 머리 + 수염 노인
    "................",
    "................",
    "....KKKKKK......",
    "...KWWWWWWK.....",
    "...KWWSSSSK.....",
    "...KWSSESSs.....",
    "...KWWSSSsK.....",
    "....KWWWWK......",
]
HEAD_PILOT = [                      # 둥근 비행 헬멧 + 시안 바이저
    "................",
    "....KKKKKK......",
    "...KMMMMMMK.....",
    "..KMMMMMMMMK....",
    "..KMffffffMK....",
    "..KMffffffMK....",
    "...KMSSESSMK....",
    "....KsSSSsK.....",
]
HEAD_KID = [                        # 꼬마 (build_frame 이 2px 더 내려 그린다)
    "................",
    "................",
    "................",
    "....KKKKKK......",
    "...KHHHHHHK.....",
    "...KHSSSSSK.....",
    "...KHSSESSs.....",
    "....KsSSSsK.....",
]
HEAD_KID_DAENGI = [                 # 댕기머리 꼬마
    "................",
    "................",
    "................",
    "....KKKKKK......",
    "..KHHHHHHHK.....",
    "..KHHSSSSSK.....",
    "..KcHSSESSs.....",
    "..KcKsSSSsK.....",
]

# ============================================================== 상체 (4행)
TORSO_PLAIN = [
    "...KTTTTTTK.....",
    "..KTTTuuTTTK....",
    "..KTTTTTTTSK....",
    "...KTtttTKSK....",
]
TORSO_FUR = [                       # 털옷 — 어깨가 들쭉날쭉
    "..KTTKTTTTK.....",
    "..KTuTTTuTTK....",
    "..KTTTTTTTSK....",
    "..KTtKTtTKSK....",
]
TORSO_APRON = [                     # 앞치마·작업복
    "...KTTTTTTK.....",
    "..KTTuuuuTTK....",
    "..KTuuuuuuSK....",
    "...KTuuuTKSK....",
]
TORSO_ARMOR = [                     # 찰갑
    "...KMTTTTMK.....",
    "..KMTuuuTMK.....",
    "..KMTTTTTTSK....",
    "...KMtttMKSK....",
]
TORSO_HANBOK = [                    # 저고리 + 흰 고름
    "...KTTTTTTK.....",
    "..KTTuWWuTTK....",
    "..KTTuTTuTSK....",
    "...KttttttK.....",
]
TORSO_SUIT = [                      # 양복 + 흰 셔츠
    "...KTTTTTTK.....",
    "..KTTuWWuTTK....",
    "..KTTTWTTTSK....",
    "...KTtttTKSK....",
]
TORSO_ROBOT = [
    "..KMMMMMMK......",
    "..KMMffMMmK.....",
    "..KMffffMmK.....",
    "...KmmmmKmK.....",
]

# ============================================================== 다리
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


def legs_kid(c, phase):
    c.row(13, (4, 4, 'K'), (5, 9, 'N'), (10, 10, 'K'))
    poses = {0: ((5, 6), (8, 9)), 1: ((4, 5), (9, 10)),
             2: ((6, 7), (7, 8)), 3: ((8, 9), (5, 6))}
    for (x0, x1), boot in ((poses[phase][0], 'o'), (poses[phase][1], 'O')):
        c.row(14, (x0, x1, 'N'))
        c.row(15, (x0, x1, boot))
        c.set(x0 - 1, 14, 'K')
        c.set(x1 + 1, 14, 'K')


def legs_robe(c, phase):
    """도포·치마 — 걸을 때 옷자락이 좌우로 흔들린다."""
    sway = {0: 0, 1: 1, 2: 0, 3: -1}[phase]
    lo, hi = min(0, sway), max(0, sway)
    c.row(12, (3, 3, 'K'), (4, 10, 'T'), (11, 11, 'K'))
    c.row(13, (2, 2, 'K'), (3, 11, 'T'), (12, 12, 'K'))
    c.row(14, (2 + lo, 2 + lo, 'K'), (3 + lo, 11 + hi, 'T'), (12 + hi, 12 + hi, 'K'))
    c.row(15, (2 + lo, 2 + lo, 'K'), (3 + lo, 11 + hi, 't'), (12 + hi, 12 + hi, 'K'))


def legs_hover(c, phase):
    """부양 — 다리 대신 아래에 빛 고리."""
    bob = {0: 0, 1: 1, 2: 0, 3: 1}[phase]
    c.row(13 + bob, (5, 10, 'm'))
    c.row(14 + bob, (6, 9, 'f'))


def legs_drone(c, phase):
    """드론 — 짐칸 아래로 빛만 흘린다."""
    bob = -1 if phase in (1, 3) else 0
    c.row(14 + bob, (5, 10, 'f'))
    c.row(15 + bob, (6, 9, 'm')) if 15 + bob < 16 else None


LEGS = {'normal': legs, 'kid': legs_kid, 'robe': legs_robe,
        'hover': legs_hover, 'drone': legs_drone}


# ============================================================== 손·등에 든 것
def spear(c, phase):                # 창
    c.rect(12, 1, 12, 14, 'P')
    c.rect(11, 2, 13, 3, 'M')
    c.set(12, 0, 'M')


def torch(c, phase):                # 횃불
    f = {0: 0, 1: 1, 2: 0, 3: 1}[phase]
    c.rect(12, 7 - f, 12, 13, 'P')
    c.rect(11, 4 - f, 13, 6 - f, 'f')
    c.set(12, 5 - f, 'W')
    c.set(12, 3 - f, 'f')


def basket(c, phase):               # 채집 바구니
    c.rect(11, 10, 13, 12, 'B')
    c.row(10, (11, 13, 'b'))
    c.set(14, 11, 'K')
    c.set(10, 11, 'K')


def staff(c, phase):                # 주술 지팡이
    c.rect(12, 4, 12, 15, 'P')
    c.rect(11, 2, 13, 3, 'W')
    c.set(12, 1, 'W')


def pot(c, phase):                  # 항아리 (앞에 안음)
    c.rect(10, 9, 12, 12, 'c')
    c.row(9, (10, 12, 'C'))
    c.set(13, 10, 'K')
    c.set(13, 11, 'K')
    c.set(11, 11, 'C')


def hoe(c, phase):                  # 괭이
    c.rect(12, 5, 12, 14, 'P')
    c.rect(12, 5, 14, 6, 'M')


def bell(c, phase):                 # 제사 방울
    f = {0: 0, 1: 1, 2: 0, 3: -1}[phase]
    c.rect(12 + f, 9, 13 + f, 11, 'Y')
    c.set(12 + f, 8, 'y')


def fan(c, phase):                  # 부채
    c.rect(12, 7, 12, 10, 'P')
    c.rect(13, 6, 14, 9, 'q')
    c.set(13, 7, 'Q')
    c.set(14, 6, 'K')


def jige(c, phase):                 # 지게 + 짐
    c.rect(2, 8, 3, 13, 'P')
    c.rect(1, 5, 4, 9, 'B')
    c.row(5, (1, 4, 'b'))
    c.set(2, 7, 'b')
    c.set(4, 8, 'b')


def baton(c, phase):                # 육모방망이
    c.rect(12, 9, 13, 12, 'P')
    c.row(9, (12, 13, 'p'))


def ragbag(c, phase):               # 넝마 등짐
    c.rect(1, 6, 3, 12, 'B')
    c.row(6, (1, 3, 'b'))
    c.set(2, 8, 'b')
    c.set(3, 10, 'b')


def tongs(c, phase):                # 쓰레기 집게
    o = {0: 0, 1: 1, 2: 0, 3: 1}[phase]
    c.rect(10, 9, 11, 9, 'm')
    c.set(12, 9, 'M')
    c.rect(12, 8 - o, 13, 8 - o, 'M')
    c.rect(12, 10 + o, 13, 10 + o, 'M')


def toolbelt(c, phase):             # 공구 벨트
    c.row(11, (3, 10, 'G'))
    c.rect(4, 12, 5, 13, 'm')


def briefcase(c, phase):            # 서류 가방
    c.rect(11, 10, 13, 13, 'O')
    c.row(10, (11, 13, 'o'))
    c.set(12, 9, 'o')


def schoolbag(c, phase):            # 책가방
    c.rect(1, 7, 3, 12, 'O')
    c.row(7, (1, 3, 'o'))
    c.rect(1, 9, 3, 10, 'Y')


def tablet(c, phase):               # 홀로그램 태블릿
    f = {0: 0, 1: 1, 2: 0, 3: 1}[phase]
    c.rect(12, 6 - f, 14, 9 - f, 'f')
    c.rect(13, 7 - f, 14, 8 - f, 'W')


def clawarm(c, phase):              # 로봇 집게팔
    o = {0: 0, 1: 1, 2: 0, 3: 1}[phase]
    c.rect(10, 9, 11, 10, 'm')
    c.set(12, 9, 'M')
    c.set(12, 10, 'M')
    c.rect(12, 8 - o, 13, 8 - o, 'M')
    c.rect(12, 11 + o, 13, 11 + o, 'M')


def parcel(c, phase):               # 드론 배송 상자 (몸통 바로 아래에 매달림)
    bob = -1 if phase in (1, 3) else 0
    c.rect(6, 7 + bob, 9, 7 + bob, 'm')
    c.rect(4, 8 + bob, 11, 12 + bob, 'B')
    c.row(8 + bob, (4, 11, 'b'))
    c.rect(7, 8 + bob, 8, 12 + bob, 'b')
    c.set(3, 9 + bob, 'K')
    c.set(12, 9 + bob, 'K')


def neon(c, phase):                 # 네온 어깨·허리 라인
    c.rect(2, 9, 2, 11, 'f')
    c.rect(11, 9, 11, 10, 'f')
    c.row(12, (4, 10, 'f'))


def hoverboard(c, phase):           # 호버보드
    bob = {0: 0, 1: 1, 2: 0, 3: 1}[phase]
    c.row(14 + bob, (3, 12, 'M'))
    if 15 + bob < 16:
        c.row(15 + bob, (4, 11, 'f'))


def stick(c, phase):                # 꼬마 막대기
    c.rect(11, 7, 12, 13, 'P')
    c.set(12, 7, 'p')
    c.set(11, 13, 'p')


def harpoon(c, phase):              # 작살 + 매단 물고기
    c.rect(12, 2, 12, 14, 'P')
    c.set(12, 1, 'M')
    c.set(11, 3, 'M')
    c.set(13, 3, 'M')
    c.rect(13, 5, 15, 8, 'q')
    c.row(8, (13, 15, 'Q'))
    c.set(12, 6, 'Q')
    c.set(13, 6, 'K')
    c.set(15, 4, 'q')
    c.set(15, 9, 'q')


def bigpack(c, phase):              # 등에 진 큰 짐 (몸을 다 가리지 않게)
    c.rect(0, 4, 2, 12, 'B')
    c.row(4, (0, 2, 'b'))
    c.set(1, 7, 'b')
    c.set(2, 10, 'b')
    c.rect(0, 2, 2, 4, 'b')
    c.set(3, 6, 'b')
    c.set(3, 9, 'b')


def cart(c, phase):                 # 부유 노점 카트
    bob = {0: 0, 1: 1, 2: 0, 3: 1}[phase]
    c.rect(11, 10 + bob, 15, 13 + bob, 'B')
    c.row(10 + bob, (11, 15, 'b'))
    c.row(14 + bob, (12, 14, 'f'))
    c.set(10, 10, 'm')
    c.set(12, 9 + bob, 'b')


def wrench(c, phase):               # 렌치
    o = {0: 0, 1: 1, 2: 0, 3: 1}[phase]
    c.rect(11, 9, 12, 13, 'm')
    c.set(13, 8 - o, 'M')
    c.set(12, 8 - o, 'M')
    c.set(13, 10 - o, 'M')
    c.set(12, 9 - o, 'M')


def both(*fns):
    def run(c, phase):
        for f in fns:
            f(c, phase)
    return run


# ============================================================== 캐릭터
ERAS = [
    ("원시", [
        dict(key="hunter", name="사냥꾼", head=HEAD_MESSY, torso=TORSO_FUR, leg='normal', prop=spear,
             pal=dict(H=C(72, 48, 34), T=C(150, 110, 70), t=C(108, 78, 48), u=C(186, 148, 102),
                      N=C(122, 92, 60), n=C(88, 64, 42), O=C(96, 70, 46), o=C(68, 48, 32),
                      P=C(130, 92, 56), M=C(206, 200, 184))),
        dict(key="gatherer", name="채집인", head=HEAD_LONG, torso=TORSO_FUR, leg='normal', prop=basket,
             pal=dict(H=C(48, 36, 30), T=C(176, 142, 100), t=C(128, 100, 68), u=C(210, 182, 138),
                      N=C(134, 104, 70), n=C(96, 74, 50), O=C(104, 76, 50), o=C(74, 52, 34),
                      B=C(186, 158, 102), b=C(138, 114, 70))),
        dict(key="firekeeper", name="불지기", head=HEAD_MESSY, torso=TORSO_FUR, leg='normal', prop=torch,
             pal=dict(H=C(100, 64, 38), T=C(138, 100, 76), t=C(98, 70, 54), u=C(176, 134, 104),
                      N=C(110, 82, 58), n=C(78, 58, 40), O=C(90, 64, 42), o=C(64, 44, 30),
                      P=C(120, 84, 52), f=C(240, 148, 56), W=C(255, 232, 160))),
        dict(key="shaman", name="주술사", head=HEAD_MASK, torso=TORSO_FUR, leg='robe', prop=staff,
             pal=dict(H=C(52, 40, 38), T=C(122, 92, 88), t=C(86, 64, 62), u=C(160, 130, 124),
                      N=C(96, 74, 70), n=C(68, 52, 50), O=C(84, 62, 44), o=C(60, 44, 32),
                      W=C(238, 232, 214), P=C(122, 88, 58))),
        dict(key="kid", name="원시 꼬마", head=HEAD_KID, torso=TORSO_FUR, leg='kid', prop=stick,
             pal=dict(H=C(88, 58, 38), T=C(182, 148, 106), t=C(132, 106, 76), u=C(214, 186, 144),
                      N=C(140, 110, 76), n=C(100, 78, 54), O=C(102, 74, 48), o=C(72, 52, 34),
                      P=C(128, 92, 58))),
        dict(key="fisher", name="작살꾼", head=HEAD_MESSY, torso=TORSO_FUR, leg='normal', prop=harpoon,
             pal=dict(H=C(62, 46, 36), T=C(142, 124, 92), t=C(102, 88, 64), u=C(180, 162, 126),
                      N=C(114, 98, 68), n=C(82, 70, 48), O=C(88, 68, 44), o=C(62, 46, 30),
                      P=C(126, 90, 56), M=C(206, 202, 188), q=C(178, 194, 200), Q=C(122, 142, 154))),
        dict(key="elder", name="원시 노인", head=HEAD_ELDER, torso=TORSO_FUR, leg='robe', prop=staff,
             pal=dict(W=C(234, 230, 218), T=C(128, 106, 88), t=C(92, 76, 62), u=C(166, 142, 120),
                      N=C(100, 84, 70), n=C(72, 60, 50), O=C(84, 62, 44), o=C(60, 44, 32),
                      P=C(120, 86, 54))),
        dict(key="carrier", name="짐꾼", head=HEAD_MESSY, torso=TORSO_FUR, leg='normal', prop=bigpack,
             pal=dict(H=C(86, 56, 36), T=C(164, 130, 92), t=C(120, 94, 66), u=C(200, 172, 132),
                      N=C(128, 100, 68), n=C(92, 72, 48), O=C(96, 70, 46), o=C(68, 50, 32),
                      B=C(180, 152, 100), b=C(132, 108, 66))),
    ]),
    ("고대", [
        dict(key="potter", name="토기장이", head=HEAD_TOPKNOT, torso=TORSO_APRON, leg='normal', prop=pot,
             pal=dict(H=C(44, 36, 34), T=C(206, 198, 178), t=C(160, 152, 134), u=C(232, 226, 210),
                      N=C(128, 102, 74), n=C(92, 72, 52), O=C(98, 76, 52), o=C(70, 54, 36),
                      c=C(178, 122, 82), C=C(132, 88, 58))),
        dict(key="farmer", name="농부", head=HEAD_SATGAT, torso=TORSO_PLAIN, leg='normal', prop=hoe,
             pal=dict(H=C(46, 38, 34), T=C(198, 190, 168), t=C(152, 144, 126), u=C(226, 220, 202),
                      N=C(118, 106, 80), n=C(84, 76, 56), O=C(96, 74, 50), o=C(68, 52, 34),
                      a=C(206, 180, 118), P=C(126, 90, 56), M=C(170, 168, 160))),
        dict(key="warrior", name="무사", head=HEAD_HELM, torso=TORSO_ARMOR, leg='normal', prop=spear,
             pal=dict(H=C(48, 40, 38), T=C(128, 90, 66), t=C(92, 62, 44), u=C(166, 124, 92),
                      N=C(84, 72, 62), n=C(60, 52, 44), O=C(74, 60, 48), o=C(52, 42, 34),
                      M=C(188, 186, 178), m=C(132, 130, 124), P=C(122, 86, 54))),
        dict(key="priest", name="제사장", head=HEAD_TOPKNOT, torso=TORSO_HANBOK, leg='robe', prop=bell,
             pal=dict(H=C(40, 34, 32), T=C(234, 230, 218), t=C(188, 184, 172), u=C(250, 248, 240),
                      W=C(196, 84, 72), N=C(186, 182, 170), n=C(150, 146, 136),
                      O=C(96, 80, 60), o=C(68, 56, 42), Y=C(222, 186, 92), y=C(160, 130, 58))),
        dict(key="kid", name="고대 꼬마", head=HEAD_KID, torso=TORSO_PLAIN, leg='kid', prop=None,
             pal=dict(H=C(46, 38, 34), T=C(214, 206, 186), t=C(166, 158, 140), u=C(238, 232, 218),
                      N=C(156, 116, 84), n=C(112, 84, 60), O=C(100, 78, 52), o=C(72, 54, 36))),
    ]),
    ("조선", [
        dict(key="seonbi", name="선비", head=HEAD_GAT, torso=TORSO_HANBOK, leg='robe', prop=fan,
             pal=dict(H=C(46, 42, 48), T=C(238, 236, 228), t=C(192, 190, 182), u=C(250, 250, 246),
                      W=C(112, 140, 172), N=C(192, 190, 182), n=C(154, 152, 146),
                      O=C(88, 78, 66), o=C(62, 54, 46), P=C(118, 84, 52),
                      q=C(246, 244, 236), Q=C(198, 194, 182))),
        dict(key="merchant", name="등짐장수", head=HEAD_DUGEON, torso=TORSO_PLAIN, leg='normal', prop=jige,
             pal=dict(H=C(44, 36, 34), T=C(196, 188, 166), t=C(150, 142, 124), u=C(224, 218, 200),
                      N=C(120, 100, 74), n=C(86, 70, 52), O=C(94, 72, 48), o=C(66, 50, 34),
                      P=C(132, 96, 60), B=C(178, 152, 98), b=C(130, 110, 68))),
        dict(key="woman", name="아낙", head=HEAD_WATERJAR, torso=TORSO_HANBOK, leg='robe', prop=None,
             pal=dict(H=C(36, 30, 32), T=C(150, 178, 176), t=C(108, 136, 136), u=C(196, 216, 214),
                      W=C(240, 240, 234), N=C(108, 136, 136), n=C(80, 104, 104),
                      O=C(92, 78, 62), o=C(64, 54, 42), c=C(206, 150, 108), C=C(156, 106, 72))),
        dict(key="pojol", name="포졸", head=HEAD_JEONRIP, torso=TORSO_PLAIN, leg='normal', prop=baton,
             pal=dict(H=C(48, 42, 44), T=C(172, 100, 80), t=C(126, 70, 56), u=C(208, 140, 118),
                      c=C(206, 78, 66), N=C(78, 82, 100), n=C(56, 58, 74),
                      O=C(70, 60, 50), o=C(50, 42, 34), P=C(126, 90, 56), p=C(90, 64, 40))),
        dict(key="kid", name="댕기 꼬마", head=HEAD_KID_DAENGI, torso=TORSO_HANBOK, leg='kid', prop=None,
             pal=dict(H=C(36, 30, 32), T=C(208, 136, 130), t=C(160, 98, 96), u=C(236, 178, 172),
                      W=C(244, 240, 232), c=C(196, 70, 62), N=C(120, 148, 148), n=C(86, 110, 110),
                      O=C(92, 76, 58), o=C(64, 52, 40))),
    ]),
    ("근대", [
        dict(key="ragpicker", name="넝마주이", head=HEAD_DUGEON, torso=TORSO_PLAIN, leg='normal',
             prop=both(ragbag, tongs),
             pal=dict(H=C(44, 38, 36), T=C(118, 122, 96), t=C(84, 88, 68), u=C(152, 156, 126),
                      N=C(92, 86, 74), n=C(66, 62, 52), O=C(70, 58, 46), o=C(48, 40, 32),
                      B=C(152, 134, 96), b=C(108, 94, 64), M=C(198, 202, 208), m=C(132, 138, 146))),
        dict(key="worker", name="공장 노동자", head=HEAD_HARDHAT, torso=TORSO_APRON, leg='normal', prop=toolbelt,
             pal=dict(H=C(42, 36, 34), T=C(84, 110, 138), t=C(58, 78, 100), u=C(122, 150, 178),
                      N=C(74, 96, 120), n=C(52, 68, 88), O=C(60, 56, 52), o=C(42, 38, 36),
                      Y=C(230, 192, 76), G=C(84, 70, 58), m=C(176, 180, 188))),
        dict(key="gentleman", name="양복 신사", head=HEAD_FEDORA, torso=TORSO_SUIT, leg='normal', prop=briefcase,
             pal=dict(H=C(46, 44, 52), T=C(74, 74, 92), t=C(50, 50, 66), u=C(108, 108, 128),
                      W=C(240, 240, 236), N=C(62, 62, 80), n=C(44, 44, 58),
                      O=C(96, 66, 44), o=C(66, 44, 28))),
        dict(key="student", name="교복 학생", head=HEAD_CAP, torso=TORSO_PLAIN, leg='normal', prop=schoolbag,
             pal=dict(H=C(42, 40, 48), T=C(62, 72, 96), t=C(42, 50, 70), u=C(96, 108, 136),
                      N=C(52, 60, 82), n=C(36, 42, 58), O=C(112, 78, 48), o=C(80, 54, 32),
                      Y=C(228, 194, 100))),
        dict(key="ajumma", name="몸빼 아주머니", head=HEAD_BUNDLE, torso=TORSO_PLAIN, leg='normal', prop=None,
             pal=dict(H=C(76, 56, 50), T=C(192, 126, 114), t=C(146, 90, 82), u=C(224, 172, 160),
                      N=C(132, 150, 110), n=C(96, 112, 78), O=C(80, 68, 58), o=C(56, 46, 40),
                      q=C(228, 216, 176), Q=C(182, 168, 128))),
    ]),
    ("미래", [
        dict(key="researcher", name="복원 연구원", head=HEAD_VISOR, torso=TORSO_APRON, leg='normal', prop=tablet,
             pal=dict(H=C(52, 52, 66), T=C(234, 240, 246), t=C(182, 192, 204), u=C(250, 253, 255),
                      N=C(190, 200, 212), n=C(144, 154, 168), O=C(70, 82, 96), o=C(48, 58, 70),
                      f=C(124, 234, 240), W=C(238, 252, 254), M=C(96, 190, 204))),
        dict(key="dronebot", name="배송 드론", head=HEAD_DRONE, torso=None, leg='drone', prop=parcel,
             pal=dict(M=C(212, 220, 230), m=C(148, 160, 176), f=C(124, 234, 240),
                      B=C(200, 174, 124), b=C(152, 130, 88))),
        dict(key="workbot", name="작업 로봇", head=HEAD_ROBOT, torso=TORSO_ROBOT, leg='normal', prop=clawarm,
             pal=dict(M=C(198, 204, 214), m=C(136, 144, 158), f=C(124, 234, 240),
                      N=C(110, 120, 136), n=C(78, 86, 100), O=C(62, 70, 84), o=C(44, 50, 62))),
        dict(key="citizen", name="네온 시민", head=HEAD_VISOR, torso=TORSO_PLAIN, leg='normal', prop=neon,
             pal=dict(H=C(56, 48, 78), T=C(66, 62, 102), t=C(44, 42, 74), u=C(104, 98, 148),
                      N=C(48, 46, 76), n=C(34, 32, 56), O=C(72, 66, 104), o=C(50, 46, 76),
                      f=C(140, 242, 246), M=C(96, 190, 204))),
        dict(key="kid", name="호버 꼬마", head=HEAD_KID, torso=TORSO_PLAIN, leg='kid', prop=hoverboard,
             pal=dict(H=C(58, 50, 80), T=C(124, 212, 216), t=C(82, 158, 168), u=C(180, 240, 242),
                      N=C(80, 76, 118), n=C(56, 54, 86), O=C(68, 64, 98), o=C(46, 42, 68),
                      f=C(140, 242, 246), M=C(190, 202, 216))),
        dict(key="pilot", name="조종사", head=HEAD_PILOT, torso=TORSO_SUIT, leg='normal', prop=None,
             pal=dict(M=C(226, 232, 242), f=C(124, 234, 240), T=C(66, 94, 124), t=C(44, 66, 92),
                      u=C(104, 140, 172), W=C(242, 248, 252), N=C(56, 78, 102), n=C(38, 54, 74),
                      O=C(70, 82, 96), o=C(48, 58, 70))),
        dict(key="vendor", name="노점상", head=HEAD_VISOR, torso=TORSO_APRON, leg='normal', prop=cart,
             pal=dict(H=C(56, 52, 76), T=C(62, 104, 124), t=C(42, 74, 92), u=C(150, 222, 232),
                      N=C(74, 86, 106), n=C(52, 62, 78), O=C(62, 72, 88), o=C(42, 50, 64),
                      f=C(140, 242, 246), M=C(96, 190, 204), m=C(150, 162, 178),
                      B=C(202, 178, 130), b=C(152, 132, 92))),
        dict(key="mechanic", name="정비공", head=HEAD_HARDHAT, torso=TORSO_APRON, leg='normal', prop=wrench,
             pal=dict(H=C(48, 48, 62), Y=C(124, 234, 240), T=C(98, 108, 128), t=C(68, 76, 94),
                      u=C(144, 156, 178), N=C(82, 92, 110), n=C(58, 66, 80),
                      O=C(60, 68, 82), o=C(42, 48, 60), M=C(216, 222, 232), m=C(148, 160, 176))),
    ]),
]


# ============================================================== 조립
def build_frame(spec, phase):
    c = Canvas(W, W)
    bob = -1 if phase in (1, 3) else 0
    kid = spec['leg'] == 'kid'
    drop = 2 if kid else 0

    c.blit_rows(spec['head'], 0, bob + drop)
    if spec['torso'] is not None:
        c.blit_rows(spec['torso'], 0, 8 + bob + drop)

    LEGS[spec['leg']](c, phase)
    if spec['prop'] is not None:
        spec['prop'](c, phase)
    return c


def build():
    out = {}
    for era_index, (era_name, people) in enumerate(ERAS):
        cells = []
        for spec in people:
            for phase in range(4):
                cells.append((build_frame(spec, phase), spec['pal']))
        img = Image.new('RGBA', (W * len(cells), W), (0, 0, 0, 0))
        for i, (canvas, pal) in enumerate(cells):
            img.paste(canvas.to_image(overrides=pal), (i * W, 0))
        out[f'era_villagers_{era_index}'] = img
    return out


if __name__ == '__main__':
    import os
    os.makedirs('out/NPC', exist_ok=True)
    for name, img in build().items():
        img.save(f'out/NPC/{name}.png')
        print(name, img.size)
    print('ok')
