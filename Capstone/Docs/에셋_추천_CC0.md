# 마을/도시 픽셀 에셋 — 교체용 CC0 팩 추천

지금 프로젝트에 들어간 마을 에셋(`Resources/PixelRefresh/Village`, `NPC`, `Player`)은
**직접 그린 것**이라 라이선스 걱정 없이 그냥 쓰면 된다.
나중에 물량을 확 늘리고 싶을 때 아래 팩들을 얹으면 된다.

## 고르는 기준

| 항목 | 이 프로젝트 기준 |
| --- | --- |
| 타일 크기 | **16x16**(=한 칸) 또는 **32x32**(=두 칸), **48x48**(=세 칸) |
| 시점 | **측면(사이드뷰)**. 탑다운 팩은 건물 정면만 잘라 쓰는 식으로 변환 필요 |
| 픽셀 밀도 | `VillageSpriteCatalog.PixelsPerUnit` = `16 / CellSize` 고정. 다른 크기 팩은 16의 배수만 |
| 라이선스 | CC0 우선. CC-BY는 크레딧 표기 필요, LPC 계열(CC-BY-SA/GPL)은 **소스 공개 의무**가 붙어서 캡스톤엔 비추 |

## 1순위 — CC0, 16x16, 바로 쓸 수 있음

| 팩 | 크기 | 라이선스 | 쓸 곳 |
| --- | --- | --- | --- |
| [Kenney — Tiny Town](https://kenney.nl/assets/tiny-town) | 16x16, 130장 | **CC0** | 건물 조각·지붕·문·창문·울타리·나무. 탑다운이지만 건물 파츠가 정면이라 사이드뷰로 조립 가능 |
| [Kenney — Tiny Farm](https://kenney.nl/assets/tiny-farm) | 16x16 | **CC0** | 밭·헛간·가축 — 마을 초반 단계 채우기 |
| [Kenney — Tiny Dungeon](https://kenney.nl/assets/tiny-dungeon) | 16x16 | **CC0** | 지하 소품 보강 |
| [Kenney — Roguelike/RPG pack](https://kenney.nl/assets/roguelike-rpg-pack) | 16x16, 1700장+ | **CC0** | 물량이 압도적. 도시 단계 소품 대량 확보 |
| [Pixel Frog — Pixel Adventure 1](https://pixelfrog-assets.itch.io/pixel-adventure-1) | 사이드뷰 | **CC0** | 캐릭터 애니메이션 참고 / NPC 교체 후보 |

> Kenney 팩은 전부 CC0라 크레딧 없이 상업적 사용까지 가능하지만,
> 그래도 `LICENSES.md`에 출처는 남겨두는 게 좋다.

## 2순위 — OpenGameArt CC0 모음 (개별 확인 필요)

- [CC0 Tiles & Tilesets 모음](https://opengameart.org/content/cc0-tiles-tilesets)
- [Good CC0-Art 모음](https://opengameart.org/content/good-cc0-art)
- [Generic Platformer Tileset (16x16) + Background](https://opengameart.org/content/generic-platformer-tileset-16x16-background)
- [16x16 Platformer Tileset](https://opengameart.org/content/16x16-platformer-tileset)

OpenGameArt는 **작품마다 라이선스가 다르다**. 받기 전에 페이지 하단 License 항목을
반드시 확인하고, 받은 날짜와 함께 `LICENSES.md`에 적어둘 것.

## 피해야 할 것

| 팩 | 이유 |
| --- | --- |
| [Pixel Frog — Tiny Swords](https://pixelfrog-assets.itch.io/tiny-swords) | 재배포 금지 조항이 있어 CC0 아님. 그리고 64x64라 격자도 안 맞는다 |
| LPC (Liberated Pixel Cup) 계열 | CC-BY-SA 3.0 / GPL 3.0 — 2차 저작물도 같은 라이선스로 공개해야 한다 |
| 스타일이 다른 팩 여러 개 섞기 | 외곽선 두께·팔레트 채도·명암 방향이 달라지면 한 화면에서 바로 티가 난다 |

## 섞어 쓸 때 스타일 맞추는 법

1. **외곽선**: 지금 에셋은 전부 `#1C161E` 계열 진한 외곽선 1px. 외곽선 없는 팩은 붕 뜬다
2. **명암 방향**: 광원은 좌상단. 오른쪽·아래에 어두운 색을 깐다
3. **채도**: 도언이 취향대로 살짝 빠진 채도로 통일. 너무 쨍한 팩은 채도 -15% 정도 낮춰서 쓰기
4. **바닥선**: 건물·소품 스프라이트는 전부 **bottom-center 피벗**. 잘라 쓸 때 캔버스 아래쪽에 딱 붙여야 지면에 정확히 선다

새 팩을 넣을 위치는 `Assets/Resources/PixelRefresh/Village/` 이고,
`VillageSpriteCatalog.cs` 의 시트 규격(집 48px 6장 / 소품 32px 11장 / 길·잔디 16px)만 맞추면
코드는 하나도 안 고쳐도 된다.
