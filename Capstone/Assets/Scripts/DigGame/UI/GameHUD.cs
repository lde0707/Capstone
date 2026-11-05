using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DigGame
{
    /// <summary>쓰집소 픽셀 스킨을 사용하는 IMGUI HUD.</summary>
    public class GameHUD : MonoBehaviour
    {
        enum MenuPage { None, Pause, Settings, Controls, Title }

        const float MarginX = 28f;
        const float MarginY = 26f;
        /// <summary>우측 상단 패널 높이. 아래에 붙는 패널들이 이 값을 기준으로 내려간다.</summary>
        const float TopRightHeight = 232f;
        const float PanelW = 330f;
        /// <summary>이 거리 안에 들어온 복원 유물은 머리 위에 이름이 뜬다.</summary>
        const float RelicLabelRange = 4f;          // 칸
        /// <summary>세로 거리는 덜 친다 — 머리 위를 떠다니는 공중형도 잡히게.</summary>
        const float RelicLabelVerticalWeight = 0.6f;
        /// <summary>한 번에 띄울 이름표 수. 넘치면 가까운 것부터.</summary>
        const int RelicLabelMax = 4;
        const int HotbarSlotCount = GameConfig.InventoryColumns;
        const float HotbarSlotSize = 56f;
        public const float TitleReferenceWidth = 1920f;
        public const float TitleReferenceHeight = 1080f;

        GameManager _game;
        Font _font;
        public Font UIFont => _font;
        PixelUISkin _skin;
        public PixelUISkin UISkin => _skin;
        GUIStyle _label, _small, _micro, _title, _big, _center, _logo, _logoShadow, _button, _buttonSmall, _tab, _tabOn;
        GUIStyle _titleSubtitle, _titleTagline, _titleButton, _titleFooter;
        GUIStyle _nameplate;
        GUIStyle _questBubbleText, _questBang, _fullBubbleText;
        Texture2D _white;
        Texture2D _titleScene;
        bool _showHelp;
        bool _stylesReady;
        int _nightTab;   // 0 능력치 / 1 장비 / 2 펫 / 3 시설
        bool _showCodex;
        bool _showInventory;
        int _codexTab;   // 유물 도감 분류 탭
        MenuPage _menu = MenuPage.Title;
        MenuPage _returnMenu = MenuPage.Title;
        bool _hasEnteredGame;
        bool _soundEnabled = true;

        public bool BlocksGameplayInput => _menu != MenuPage.None || _showInventory;
        public bool TitleScreenOpen => _menu == MenuPage.Title;
        public bool InventoryOpen => _showInventory;

        /// <summary>주변에 E 상호작용 대상이 없을 때 GameManager가 호출한다.</summary>
        public void OpenInventory() => SetInventory(true);

        public void Init(GameManager game)
        {
            _game = game;
            if (game != null) game.Hud = this;
            SetMenu(MenuPage.Title);
        }

        /// <summary>HUD용 얇은 픽셀 프레임. 스킨 누락 시 단색 배경으로 대체한다.</summary>
        void Panel(Rect r, Color c)
        {
            if (_skin != null)
            {
                _skin.DrawHudPanel(r, c.a);
                return;
            }

            Color body = new Color(
                Mathf.Clamp01(0.04f + c.r * 0.18f),
                Mathf.Clamp01(0.035f + c.g * 0.18f),
                Mathf.Clamp01(0.07f + c.b * 0.18f),
                Mathf.Max(0.82f, c.a));
            Fill(r, body);
            Outline(r, new Color(0.72f, 0.56f, 0.24f, 0.82f), 2f);
        }

        /// <summary>타이틀과 ESC 메뉴에만 쓰는 얇은 금색 모서리 프레임.</summary>
        void MenuPanel(Rect r, Color c)
        {
            if (_skin != null)
            {
                _skin.DrawPanel(r, c.a);
                return;
            }
            Panel(r, c);
        }

        void Awake()
        {
            _white = new Texture2D(1, 1) { hideFlags = HideFlags.DontSave };
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
            _skin = PixelUISkin.Load();
            _font = _skin != null && _skin.Font != null ? _skin.Font : LoadKoreanFont();
            _titleScene = Resources.Load<Texture2D>("PixelUI/title_scene_background");
            if (_titleScene != null)
            {
                _titleScene.filterMode = FilterMode.Point;
                _titleScene.wrapMode = TextureWrapMode.Clamp;
                _titleScene.anisoLevel = 0;
            }
            _soundEnabled = AudioListener.volume > 0.01f;
        }

        void OnDestroy()
        {
            DestroyTexture(_white);
            if (Application.isPlaying && GameManager.Instance == _game)
                Time.timeScale = 1f;
        }

        static void DestroyTexture(Texture2D texture)
        {
            if (texture == null) return;
            if (Application.isPlaying) Destroy(texture);
            else DestroyImmediate(texture);
        }

        static Font LoadKoreanFont()
        {
            string[] candidates =
            {
                "Malgun Gothic", "맑은 고딕", "NanumGothic", "Nanum Gothic",
                "Apple SD Gothic Neo", "Noto Sans CJK KR", "Noto Sans KR",
                "Gulim", "굴림", "Dotum", "Arial Unicode MS"
            };

            string[] installed = Font.GetOSInstalledFontNames();
            if (installed == null || installed.Length == 0) return null;

            foreach (var want in candidates)
                foreach (var have in installed)
                    if (string.Equals(have, want, System.StringComparison.OrdinalIgnoreCase))
                    {
                        var f = Font.CreateDynamicFontFromOSFont(have, 16);
                        if (f != null) return f;
                    }

            string[] hints = { "Gothic", "고딕", "Nanum", "나눔", "Noto Sans", "Malgun", "Batang", "바탕" };
            foreach (var hint in hints)
                foreach (var have in installed)
                    if (have.IndexOf(hint, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var f = Font.CreateDynamicFontFromOSFont(have, 16);
                        if (f != null) return f;
                    }

            Debug.LogWarning("[DigGame] 한글 폰트를 찾지 못했습니다.");
            return null;
        }

        void Update()
        {
            if (_game != null && _game.Recycling != null && _game.Recycling.InputCaptured)
            { _showCodex = false; return; }
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.escapeKey.wasPressedThisFrame)
            {
                HandleEscape();
                return;
            }

            if (_menu != MenuPage.None) return;
            if (kb.iKey.wasPressedThisFrame)
            {
                SetInventory(!_showInventory);
                return;
            }
            if (_showInventory) return;
            if (kb.tabKey.wasPressedThisFrame) _showHelp = !_showHelp;
            if (kb.cKey.wasPressedThisFrame) _showCodex = !_showCodex;
        }

        void HandleEscape()
        {
            if (_showInventory)
            {
                SetInventory(false);
                return;
            }
            if (_menu == MenuPage.Settings || _menu == MenuPage.Controls)
            {
                SetMenu(_returnMenu);
                return;
            }
            if (_menu == MenuPage.Pause)
            {
                SetMenu(MenuPage.None);
                return;
            }
            if (_menu == MenuPage.Title) return;

            if (_showCodex)
            {
                _showCodex = false;
                return;
            }

            _game?.CloseShop();
            SetMenu(MenuPage.Pause);
        }

        void SetMenu(MenuPage page)
        {
            _menu = page;
            bool pause = page != MenuPage.None;
            if (pause)
            {
                _showInventory = false;
                _showCodex = false;
                _showHelp = false;
                _game?.CloseShop();
            }
            if (Application.isPlaying) Time.timeScale = pause ? 0f : 1f;
        }

        void SetInventory(bool open)
        {
            if (_menu != MenuPage.None && open) return;
            _showInventory = open;
            if (open)
            {
                _showCodex = false;
                _showHelp = false;
                _game?.CloseShop();
            }
            if (Application.isPlaying)
                Time.timeScale = open ? 0f : 1f;
        }

        void OpenSubMenu(MenuPage page)
        {
            _returnMenu = _menu == MenuPage.Title ? MenuPage.Title : MenuPage.Pause;
            SetMenu(page);
        }

        void EnsureStyles()
        {
            if (_stylesReady) return;
            _stylesReady = true;

            _label = new GUIStyle(GUI.skin.label) { fontSize = 15, richText = true };
            _small = new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true };
            _micro = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                richText = true,
                clipping = TextClipping.Clip
            };
            _title = new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold, richText = true };
            _big = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, richText = true };
            _center = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, richText = true
            };
            _logo = new GUIStyle(_center)
            {
                fontSize = 72,
                wordWrap = false,
                clipping = TextClipping.Clip
            };
            _logoShadow = new GUIStyle(_logo);
            GUIStyle buttonBase = _skin != null ? _skin.ButtonStyle : GUI.skin.button;
            GUIStyle selectedButtonBase = _skin != null ? _skin.HighlightButtonStyle : GUI.skin.button;
            _button = new GUIStyle(buttonBase) { fontSize = 15, richText = true };
            _buttonSmall = new GUIStyle(buttonBase) { fontSize = 13, richText = true };
            _tab = new GUIStyle(buttonBase) { fontSize = 14, richText = true };
            _tabOn = new GUIStyle(selectedButtonBase) { fontSize = 14, fontStyle = FontStyle.Bold, richText = true };
            _titleSubtitle = new GUIStyle(_center)
            {
                fontSize = 30,
                wordWrap = false,
                clipping = TextClipping.Clip
            };
            _titleTagline = new GUIStyle(_center)
            {
                fontSize = 24,
                wordWrap = false,
                clipping = TextClipping.Clip
            };
            _titleButton = new GUIStyle(buttonBase)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
                clipping = TextClipping.Clip
            };
            _titleFooter = new GUIStyle(_small)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
                clipping = TextClipping.Clip
            };
            _nameplate = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = false,
            };
            _questBubbleText = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                richText = true,
                wordWrap = true,
                clipping = TextClipping.Clip
            };
            _questBang = new GUIStyle(GUI.skin.label)
            {
                fontSize = 29,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = false
            };
            _fullBubbleText = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = false
            };

            if (_font != null)
            {
                _label.font = _font; _small.font = _font; _micro.font = _font; _title.font = _font;
                _big.font = _font; _center.font = _font; _logo.font = _font; _logoShadow.font = _font;
                _button.font = _font; _buttonSmall.font = _font;
                _tab.font = _font; _tabOn.font = _font;
                _titleSubtitle.font = _font; _titleTagline.font = _font;
                _titleButton.font = _font; _titleFooter.font = _font;
                _nameplate.font = _font;
                _questBubbleText.font = _font;
                _questBang.font = _font;
                _fullBubbleText.font = _font;
            }

            var c = new Color(0.94f, 0.94f, 0.96f);
            _label.normal.textColor = c;
            _small.normal.textColor = c;
            _micro.normal.textColor = c;
            _title.normal.textColor = c;
            _big.normal.textColor = c;
            _center.normal.textColor = c;
            _logo.normal.textColor = new Color32(248, 215, 118, 255);
            _logoShadow.normal.textColor = new Color(0.025f, 0.018f, 0.045f, 0.96f);
            _titleSubtitle.normal.textColor = new Color32(203, 188, 240, 255);
            _titleTagline.normal.textColor = new Color32(242, 233, 214, 255);
            _titleFooter.normal.textColor = new Color32(205, 199, 218, 255);
            _nameplate.normal.textColor = new Color(1f, 0.97f, 0.9f);
            _questBubbleText.normal.textColor = new Color32(48, 38, 57, 255);
            _questBang.normal.textColor = new Color32(117, 55, 168, 255);
            _fullBubbleText.normal.textColor = new Color32(74, 42, 37, 255);
        }

        void OnGUI()
        {
            if (_game == null || _game.Grid == null) return;
            if (_game.Recycling != null && _game.Recycling.InputCaptured) return;
            EnsureStyles();

            if (_menu == MenuPage.Title)
            {
                DrawTitleScreen();
                return;
            }

            bool menuOpen = _menu != MenuPage.None;
            bool modalOpen = menuOpen || _showInventory;
            bool enabled = GUI.enabled;
            if (modalOpen) GUI.enabled = false;

            switch (_game.State)
            {
                case GameState.Day:
                    DrawRelicNames();
                    DrawQuestGiverLabel();
                    DrawRecycleFullBubble();
                    DrawStatsPanel();
                    DrawTopRight();
                    DrawContext();
                    DrawLogPanel();
                    if (_showHelp && !modalOpen) DrawHelpPanel();
                    DrawRelicStatus();
                    if (_game.ShopOpen && !modalOpen) DrawDayShop();
                    DrawInventoryHotbar();
                    if (_showCodex && !modalOpen) DrawCodex();
                    break;

                case GameState.Night:
                    DrawNightScreen();
                    if (_showCodex && !modalOpen) DrawCodex();
                    break;

                case GameState.Won:
                case GameState.Lost:
                    DrawResultOverlay();
                    break;
            }

            GUI.enabled = enabled;
            if (menuOpen) DrawMenuOverlay();
            else if (_showInventory) DrawInventoryOverlay();
        }

        Rect PanelContent(Rect panel, float side = 28f, float top = 24f, float bottom = 22f) =>
            new Rect(panel.x + side, panel.y + top,
                Mathf.Max(1f, panel.width - side * 2f),
                Mathf.Max(1f, panel.height - top - bottom));

        Rect CenteredPanel(float width, float height) => new Rect(
            (Screen.width - width) * 0.5f,
            (Screen.height - height) * 0.5f,
            width,
            height);

        void DrawMenuBackdrop()
        {
            Fill(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.015f, 0.012f, 0.035f, 0.82f));
        }

        void DrawTitleScreen()
        {
            DrawTitleSceneBackground();
            Matrix4x4 oldMatrix = GUI.matrix;
            float scale = Mathf.Min(
                Screen.width / TitleReferenceWidth,
                Screen.height / TitleReferenceHeight);
            scale = Mathf.Max(0.01f, scale);
            float offsetX = (Screen.width - TitleReferenceWidth * scale) * 0.5f;
            float offsetY = (Screen.height - TitleReferenceHeight * scale) * 0.5f;
            GUI.matrix = Matrix4x4.TRS(
                new Vector3(offsetX, offsetY, 0f),
                Quaternion.identity,
                new Vector3(scale, scale, 1f));

            // PC 16:9 기준의 중앙 안전 영역. 울트라와이드와 16:10에서도 전부 화면 안에 남는다.
            const float menuW = 720f;
            const float menuX = (TitleReferenceWidth - menuW) * 0.5f;
            Rect shade = new Rect(menuX - 46f, 38f, menuW + 92f, TitleReferenceHeight - 76f);
            Fill(shade, new Color(0.012f, 0.009f, 0.027f, 0.32f));
            Fill(new Rect(shade.x, shade.y, 2f, shade.height), new Color(0.45f, 0.35f, 0.62f, 0.28f));
            Fill(new Rect(shade.xMax - 2f, shade.y, 2f, shade.height), new Color(0.45f, 0.35f, 0.62f, 0.28f));

            const string titleText = "쓰레기 집게 소년";
            Rect logoRect = new Rect(menuX, 92f, menuW, 112f);
            GUIStyle fittedLogo = FittedTitleStyle(_logo, titleText, logoRect, 72, 48, 24f, 12f);
            GUIStyle fittedShadow = new GUIStyle(_logoShadow) { fontSize = fittedLogo.fontSize };
            GUI.Label(new Rect(logoRect.x + 4f, logoRect.y + 5f, logoRect.width, logoRect.height),
                titleText, fittedShadow);
            GUI.Label(logoRect, titleText, fittedLogo);
            DrawFittedTitleLabel(new Rect(menuX, 207f, menuW, 48f), "쓰집소",
                _titleSubtitle, 30, 22);
            DrawFittedTitleLabel(new Rect(menuX, 266f, menuW, 44f),
                "땅을 파고 · 쓰레기를 집고 · 마을을 되살려라",
                _titleTagline, 24, 17);

            const float buttonW = 420f;
            const float buttonH = 62f;
            const float buttonGap = 16f;
            const float buttonX = (TitleReferenceWidth - buttonW) * 0.5f;
            float y = 390f;
            if (TitleButton(new Rect(buttonX, y, buttonW, buttonH),
                    _hasEnteredGame ? "이어하기" : "게임 시작"))
            {
                _hasEnteredGame = true;
                SetMenu(MenuPage.None);
            }
            y += buttonH + buttonGap;

            if (_hasEnteredGame)
            {
                if (TitleButton(new Rect(buttonX, y, buttonW, buttonH), "새 게임"))
                {
                    if (Application.isPlaying) Time.timeScale = 1f;
                    Bootstrap.Restart();
                    GUI.matrix = oldMatrix;
                    return;
                }
                y += buttonH + buttonGap;
            }

            if (TitleButton(new Rect(buttonX, y, buttonW, buttonH), "설정"))
                OpenSubMenu(MenuPage.Settings);
            y += buttonH + buttonGap;
            if (TitleButton(new Rect(buttonX, y, buttonW, buttonH), "조작법"))
                OpenSubMenu(MenuPage.Controls);

            DrawFittedTitleLabel(new Rect(menuX - 40f, 990f, menuW + 80f, 36f),
                "E / I 인벤토리 · ESC 메뉴 · TAB 간단 조작 안내",
                _titleFooter, 18, 13);
            GUI.matrix = oldMatrix;
        }

        GUIStyle FittedTitleStyle(GUIStyle source, string text, Rect rect,
            int maxFontSize, int minFontSize, float horizontalPadding = 20f, float verticalPadding = 8f)
        {
            var style = new GUIStyle(source)
            {
                fontSize = maxFontSize,
                wordWrap = false,
                clipping = TextClipping.Clip
            };
            var content = new GUIContent(text);
            float availableW = Mathf.Max(1f, rect.width - horizontalPadding);
            float availableH = Mathf.Max(1f, rect.height - verticalPadding);
            while (style.fontSize > minFontSize)
            {
                Vector2 measured = style.CalcSize(content);
                if (measured.x <= availableW && measured.y <= availableH) break;
                style.fontSize--;
            }
            return style;
        }

        void DrawFittedTitleLabel(Rect rect, string text, GUIStyle source,
            int maxFontSize, int minFontSize)
        {
            GUI.Label(rect, text,
                FittedTitleStyle(source, text, rect, maxFontSize, minFontSize));
        }

        bool TitleButton(Rect rect, string text)
        {
            GUIStyle style = FittedTitleStyle(
                _titleButton, text, rect, 22, 15, 42f, 18f);
            return GUI.Button(rect, text, style);
        }

        void DrawTitleSceneBackground()
        {
            var screen = new Rect(0f, 0f, Screen.width, Screen.height);
            if (_titleScene == null)
            {
                DrawMenuBackdrop();
                return;
            }

            float screenAspect = Mathf.Max(0.01f, screen.width / screen.height);
            float textureAspect = _titleScene.width / (float)Mathf.Max(1, _titleScene.height);
            Rect uv = new Rect(0f, 0f, 1f, 1f);
            if (textureAspect > screenAspect)
            {
                float visible = screenAspect / textureAspect;
                uv.x = (1f - visible) * 0.5f;
                uv.width = visible;
            }
            else
            {
                float visible = textureAspect / screenAspect;
                uv.y = (1f - visible) * 0.5f;
                uv.height = visible;
            }
            GUI.DrawTextureWithTexCoords(screen, _titleScene, uv, true);
            Fill(screen, new Color(0.025f, 0.015f, 0.05f, 0.18f));
        }

        void DrawMenuOverlay()
        {
            DrawMenuBackdrop();
            switch (_menu)
            {
                case MenuPage.Pause: DrawPauseMenu(); break;
                case MenuPage.Settings: DrawSettingsMenu(); break;
                case MenuPage.Controls: DrawControlsMenu(); break;
            }
        }

        void DrawPauseMenu()
        {
            Rect panel = CenteredPanel(460f, 440f);
            MenuPanel(panel, new Color(0.04f, 0.03f, 0.09f, 1f));
            Rect content = PanelContent(panel, 42f, 34f, 32f);
            GUI.Label(new Rect(content.x, content.y, content.width, 40f), "일시정지", _center);

            float y = content.y + 58f;
            const float bh = 44f, gap = 10f;
            if (GUI.Button(new Rect(content.x, y, content.width, bh), "계속하기", _button))
                SetMenu(MenuPage.None);
            y += bh + gap;
            if (GUI.Button(new Rect(content.x, y, content.width, bh), "설정", _button))
                OpenSubMenu(MenuPage.Settings);
            y += bh + gap;
            if (GUI.Button(new Rect(content.x, y, content.width, bh), "조작법", _button))
                OpenSubMenu(MenuPage.Controls);
            y += bh + gap;
            if (GUI.Button(new Rect(content.x, y, content.width, bh), "타이틀 화면", _button))
                SetMenu(MenuPage.Title);
            y += bh + gap;
            if (GUI.Button(new Rect(content.x, y, content.width, bh), "게임 종료", _buttonSmall))
                Application.Quit();
        }

        void DrawSettingsMenu()
        {
            Rect panel = CenteredPanel(560f, 370f);
            MenuPanel(panel, new Color(0.04f, 0.03f, 0.09f, 1f));
            Rect content = PanelContent(panel, 44f, 34f, 32f);
            GUI.Label(new Rect(content.x, content.y, content.width, 40f), "설정", _center);

            float y = content.y + 66f;
            GUI.Label(new Rect(content.x, y, 180f, 36f), "전체 화면", _title);
            if (GUI.Button(new Rect(content.xMax - 190f, y - 4f, 190f, 42f),
                    Screen.fullScreen ? "켜짐" : "꺼짐", _button))
                Screen.fullScreen = !Screen.fullScreen;
            y += 64f;

            GUI.Label(new Rect(content.x, y, 180f, 36f), "전체 소리", _title);
            if (GUI.Button(new Rect(content.xMax - 190f, y - 4f, 190f, 42f),
                    _soundEnabled ? "켜짐" : "꺼짐", _button))
            {
                _soundEnabled = !_soundEnabled;
                AudioListener.volume = _soundEnabled ? 1f : 0f;
            }
            y += 64f;

            GUI.Label(new Rect(content.x, y, content.width, 24f),
                "픽셀 폰트와 UI 배율은 화면 해상도에 맞춰 자동 적용됩니다.", _small);
            if (GUI.Button(new Rect(content.center.x - 120f, content.yMax - 48f, 240f, 44f),
                    "뒤로  (ESC)", _button))
                SetMenu(_returnMenu);
        }

        void DrawControlsMenu()
        {
            float w = Mathf.Min(760f, Screen.width - 48f);
            float h = Mathf.Min(560f, Screen.height - 40f);
            Rect panel = CenteredPanel(w, h);
            MenuPanel(panel, new Color(0.04f, 0.03f, 0.09f, 1f));
            Rect content = PanelContent(panel, 44f, 34f, 32f);
            GUI.Label(new Rect(content.x, content.y, content.width, 40f), "조작법", _center);

            string[] lines =
            {
                "<color=#f8d776>이동·굴착</color>   WASD 이동 / 방향키 조준 / Space 길게 눌러 굴착",
                "<color=#f8d776>상호작용</color>   E 시설 이용·보관·회수 / F 분리수거 시작",
                "<color=#f8d776>장비</color>   Q 쓰레기 탐지기 / B 귀환 벨 / F 에너지 드링크",
                "<color=#f8d776>분리수거</color>   숫자 1~5로 종류 선택 / ESC로 작업대 나가기",
                "<color=#f8d776>도시광산</color>   H 자동 재활용 일꾼 / J 대량 운반 일꾼 고용",
                "<color=#f8d776>유물</color>   C 도감 / 발굴한 유물은 지상 복원소에서 E",
                "<color=#f8d776>화면</color>   E(상호작용 없을 때) / I 인벤토리 · TAB 도움말 · ESC 메뉴",
                "<color=#9be89b>목표</color>   해가 지기 전에 지상으로 돌아와 일일 할당량 달성",
            };

            float y = content.y + 58f;
            for (int i = 0; i < lines.Length; i++)
            {
                GUI.Label(new Rect(content.x + 8f, y, content.width - 16f, 30f), lines[i], _label);
                y += 36f;
            }

            if (GUI.Button(new Rect(content.center.x - 120f, content.yMax - 48f, 240f, 44f),
                    "뒤로  (ESC)", _button))
                SetMenu(_returnMenu);
        }

        void DrawInventoryOverlay()
        {
            var player = _game.Player;
            if (player == null) return;

            DrawMenuBackdrop();
            float panelW = Mathf.Min(940f, Screen.width - 36f);
            float panelH = Mathf.Min(620f, Screen.height - 36f);
            Rect panel = CenteredPanel(panelW, panelH);
            MenuPanel(panel, new Color(0.04f, 0.03f, 0.09f, 1f));
            Rect content = PanelContent(panel, 34f, 28f, 26f);

            GUI.Label(new Rect(content.x, content.y, content.width, 38f), "인벤토리", _center);
            GUI.Label(new Rect(content.x, content.y + 39f, content.width, 22f),
                $"슬롯 {player.InventoryUsedSlots} / {GameConfig.InventorySlotCapacity} · " +
                $"쓰레기 부피 {player.InventoryVolume} / {player.BagCapacity} · 유물 {_game.Relics?.CarriedCount ?? 0}개",
                _small);

            float previewW = Mathf.Min(250f, content.width * 0.30f);
            float gap = 24f;
            float gridAreaW = content.width - previewW - gap;
            const float cellGap = 4f;
            const float hotbarSectionGap = 24f;
            float cell = Mathf.Floor(Mathf.Min(58f,
                (gridAreaW - cellGap * (GameConfig.InventoryColumns - 1)) / GameConfig.InventoryColumns));
            float gridW = cell * GameConfig.InventoryColumns + cellGap * (GameConfig.InventoryColumns - 1);
            float gridH = cell * GameConfig.InventoryRows + cellGap * (GameConfig.InventoryRows - 1)
                + hotbarSectionGap;
            float bodyY = content.y + 72f;

            Rect preview = new Rect(content.x, bodyY, previewW, gridH);
            DrawPlayerInventoryPreview(preview, player);

            float gridX = content.xMax - gridW;
            int total = player.InventoryUsedSlots;
            int hotbarStart = Mathf.Max(0, total - HotbarSlotCount);
            LootItem? hoveredLoot = null;
            RelicDef hoveredRelic = null;

            GUI.Label(new Rect(gridX, bodyY - 22f, gridW, 20f), "보관 공간 3줄", _small);
            for (int row = 0; row < GameConfig.InventoryRows; row++)
            {
                for (int col = 0; col < GameConfig.InventoryColumns; col++)
                {
                    var rect = new Rect(gridX + col * (cell + cellGap),
                        bodyY + row * (cell + cellGap) +
                            (row == GameConfig.InventoryRows - 1 ? hotbarSectionGap : 0f),
                        cell, cell);
                    bool hotbarRow = row == GameConfig.InventoryRows - 1;
                    Fill(rect, hotbarRow
                        ? new Color(0.14f, 0.10f, 0.04f, 0.96f)
                        : new Color(0.055f, 0.045f, 0.09f, 0.96f));
                    Outline(rect, hotbarRow
                        ? new Color(0.72f, 0.50f, 0.16f, 0.95f)
                        : new Color(0.36f, 0.30f, 0.46f, 0.95f), 2f);

                    int itemIndex;
                    if (hotbarRow)
                    {
                        itemIndex = hotbarStart + col;
                        if (itemIndex >= total) itemIndex = -1;
                    }
                    else
                    {
                        itemIndex = row * GameConfig.InventoryColumns + col;
                        if (itemIndex >= hotbarStart) itemIndex = -1;
                    }

                    if (itemIndex < 0) continue;
                    if (itemIndex < player.Inventory.Count)
                    {
                        LootItem item = player.Inventory[itemIndex];
                        DrawLootSlotContents(rect, item);
                        if (rect.Contains(Event.current.mousePosition)) hoveredLoot = item;
                    }
                    else
                    {
                        int relicIndex = itemIndex - player.Inventory.Count;
                        var carried = _game.Relics?.Carried;
                        if (carried == null || relicIndex < 0 || relicIndex >= carried.Count) continue;
                        RelicDef def = RelicCatalog.Get(carried[relicIndex]);
                        DrawRelicSlotContents(rect, def);
                        if (rect.Contains(Event.current.mousePosition)) hoveredRelic = def;
                    }
                }
            }

            float hotbarLabelY = bodyY + (GameConfig.InventoryRows - 1) * (cell + cellGap) + 3f;
            GUI.Label(new Rect(gridX, hotbarLabelY, gridW, 18f), "하단 인벤토리 바", _small);

            float detailY = bodyY + gridH + 14f;
            float detailH = Mathf.Clamp(content.yMax - detailY - 4f, 58f, 78f);
            Rect detail = new Rect(gridX, detailY, gridW, detailH);
            DrawInventoryDetail(detail, hoveredLoot, hoveredRelic);
            GUI.Label(new Rect(content.x, content.yMax - 20f, previewW, 18f), "I 또는 ESC로 닫기", _small);
        }

        void DrawPlayerInventoryPreview(Rect rect, PlayerController player)
        {
            Panel(rect, new Color(0.06f, 0.05f, 0.10f, 0.98f));
            GUI.Label(new Rect(rect.x + 14f, rect.y + 12f, rect.width - 28f, 24f), "현재 모습", _title);

            Sprite sprite = player.CurrentSprite;
            if (sprite == null)
            {
                Sprite[] frames = PlayerSpriteCatalog.Frames(PlayerVisualState.Walk, Facing.Down);
                if (frames.Length > 0) sprite = frames[0];
            }
            float avatarSize = Mathf.Min(126f, rect.width - 54f);
            var avatar = new Rect(rect.center.x - avatarSize * 0.5f, rect.y + 44f, avatarSize, avatarSize);
            Fill(new Rect(avatar.x - 5f, avatar.y - 5f, avatar.width + 10f, avatar.height + 10f),
                new Color(0.02f, 0.02f, 0.04f, 0.72f));
            DrawSprite(avatar, sprite, player.CurrentSpriteFlipX);

            int tier = _game.PickaxeQuests?.PickaxeTier ?? 1;
            string pickaxe = _game.PickaxeQuests?.PickaxeName ?? "낡은 집게";
            var toolRect = new Rect(rect.center.x - 42f, avatar.yMax + 8f, 84f, 56f);
            DrawPickaxePreview(toolRect, tier);
            GUI.Label(new Rect(rect.x + 12f, toolRect.yMax + 2f, rect.width - 24f, 22f),
                $"{tier}단계 · {pickaxe}", _label);
        }

        void DrawPickaxePreview(Rect rect, int tier)
        {
            // 실제로 손에 드는 그림을 그대로 띄운다 — 미리보기와 인게임이 어긋나지 않게.
            Sprite sprite = PickaxeSpriteCatalog.Preview(tier);
            if (sprite == null || sprite.texture == null) return;

            Rect cell = sprite.rect;
            var uv = new Rect(cell.x / sprite.texture.width, cell.y / sprite.texture.height,
                              cell.width / sprite.texture.width, cell.height / sprite.texture.height);

            float size = Mathf.Min(rect.width, rect.height) * 0.94f;
            var draw = new Rect(rect.x + (rect.width - size) * 0.5f,
                                rect.y + (rect.height - size) * 0.5f, size, size);
            GUI.DrawTextureWithTexCoords(draw, sprite.texture, uv, true);
        }

        void DrawInventoryDetail(Rect rect, LootItem? loot, RelicDef relic)
        {
            Panel(rect, new Color(0.04f, 0.035f, 0.07f, 0.96f));
            Rect inner = new Rect(rect.x + 16f, rect.y + 10f, rect.width - 32f, rect.height - 18f);
            if (loot.HasValue)
            {
                LootItem item = loot.Value;
                GUI.Label(new Rect(inner.x, inner.y, inner.width, 22f), $"<b>{item.Name}</b>", _label);
                GUI.Label(new Rect(inner.x, inner.y + 24f, inner.width, 20f),
                    $"{MaterialInfo.DisplayName(item.Material)} · {item.Value:N0}원 · 부피 {item.Volume}", _small);
            }
            else if (relic != null)
            {
                GUI.Label(new Rect(inner.x, inner.y, inner.width, 22f), $"<b>유물 · {relic.Name}</b>", _label);
                GUI.Label(new Rect(inner.x, inner.y + 24f, inner.width, 20f),
                    $"{relic.Depth}층 · {relic.GroupName} · 복원 비용 {RelicSystem.RestoreCost(relic):N0}원", _small);
            }
            else
            {
                GUI.Label(new Rect(inner.x, inner.y + 12f, inner.width, 20f),
                    "아이템이나 유물에 마우스를 올리면 상세 정보가 표시됩니다.", _small);
            }
        }

        // ==================================================================
        // 좌측 상단 능력치
        // ==================================================================
        void DrawStatsPanel()
        {
            var p = _game.Player;
            if (p == null || p.Stats == null) return;
            var s = p.Stats;
            var g = p.Gear;

            float h = 220f;
            var r = new Rect(MarginX, MarginY, PanelW, h);
            Panel(r, new Color(0f, 0f, 0f, 0.58f));

            Rect content = PanelContent(r);
            float x = content.x, w = content.width, y = content.y;

            GUI.Label(new Rect(x, y, w, 20), $"이동속도  <b>{s.MoveSpeed:0.0}</b>", _label);
            y += 24;

            var quests = _game.PickaxeQuests;
            string pickaxe = quests == null ? "1단계 집게" : $"{quests.PickaxeTier}단계 {quests.PickaxeName}";
            GUI.Label(new Rect(x, y, w, 20),
                $"집게  <b>{pickaxe}</b> · 속도 +{s.DigSpeedBonusPercent}%", _label);
            y += 26;

            Bar(new Rect(x, y, w, 16), p.InventoryVolume / (float)p.BagCapacity,
                new Color32(200, 170, 90, 255),
                $"가방  {p.InventoryVolume} / {p.BagCapacity}  ({p.InventoryValue:N0}원)");
            y += 23;

            GUI.Label(new Rect(x, y, w, 20),
                $"깊이  {Mathf.Max(0, p.Depth)}m · {ZoneInfo.DisplayName(p.CurrentZone)}", _small);
            y += 19;

            // 보유 장비 요약
            var gear = new List<string>();
            if (g.HeadlampLevel > 0) gear.Add($"랜턴{g.HeadlampLevel}");
            if (g.HasDetector) gear.Add(g.DetectorCooldown > 0f ? $"탐지Q({g.DetectorCooldown:0}s)" : "탐지Q");
            if (g.HasHazmat) gear.Add("방호복");
            if (g.HasCompressor) gear.Add("압축기");
            if (g.HasReturnBell) gear.Add(g.BellUsedToday ? "벨(사용함)" : "벨B");
            if (g.EnergyDrinks > 0) gear.Add($"드링크F x{g.EnergyDrinks}");
            if (g.Pet != PetKind.None) gear.Add(PlayerGear.PetName(g.Pet));
            if (gear.Count > 0)
                GUI.Label(new Rect(x, y, w, 20), string.Join(" · ", gear), _small);

            y += 20;
            if (quests != null && !quests.AllCompleted)
                GUI.Label(new Rect(x, y, w, 20),
                    quests.Accepted ? $"집게 퀘스트 · {quests.ObjectiveText}" : "입구 옆 ! 주민에게 집게 퀘스트", _small);
        }

        void PollutionBar(Rect r, int value)
        {
            const int slots = PollutionMeter.MaxValue;
            const float gap = 2f;
            Rect track = r;
            if (_skin != null)
            {
                _skin.DrawBarFrame(r);
                track = new Rect(r.x + 3f, r.y + 3f, r.width - 6f, r.height - 6f);
            }
            float slotW = (track.width - gap * (slots - 1)) / slots;
            for (int i = 0; i < slots; i++)
            {
                var slot = new Rect(track.x + i * (slotW + gap), track.y, slotW, track.height);
                Fill(slot, i < value
                    ? new Color32(151, 67, 211, 255)
                    : new Color(1f, 1f, 1f, 0.13f));
            }
            GUI.Label(new Rect(r.x + 7, r.y - 1, r.width, r.height + 3),
                $"오염  {value} / {slots}" + (value > 0
                    ? $" · 굴착 {Mathf.RoundToInt(value * GameConfig.PollutionDigSlowdownPerLevel * 100f)}% 느림"
                    : ""), _small);
        }

        void Bar(Rect r, float t, Color color, string text)
        {
            Rect track = r;
            if (_skin != null)
            {
                _skin.DrawBarFrame(r);
                track = new Rect(r.x + 3f, r.y + 3f, r.width - 6f, r.height - 6f);
            }
            else Fill(r, new Color(1f, 1f, 1f, 0.13f));
            Fill(new Rect(track.x, track.y, track.width * Mathf.Clamp01(t), track.height), color);
            GUI.Label(new Rect(r.x + 7, r.y - 1, r.width, r.height + 3), text, _small);
        }

        // ==================================================================
        // 우측 상단: 일차 / 할당량 / 돈
        // ==================================================================
        void DrawTopRight()
        {
            float w = 380f;
            var r = new Rect(Screen.width - w - MarginX, MarginY, w, TopRightHeight);
            Panel(r, new Color(0f, 0f, 0f, 0.58f));

            bool warn = _game.TimeLeft < GameConfig.DayEndWarnSeconds;

            Rect content = PanelContent(r);
            float x = content.x, y = content.y, iw = content.width;

            GUI.Label(new Rect(x, y, iw, 24),
                $"<b>{_game.Day}일차</b> / {GameConfig.DaysToWin}일", _title);
            int secondsLeft = Mathf.CeilToInt(_game.TimeLeft);
            string timeColor = warn ? "#ff8888" : "#f3dda0";
            GUI.Label(new Rect(x + iw - 112, y + 2, 112, 22),
                $"<color={timeColor}>남은 시간 {secondsLeft}초</color>", _small);
            y += 30;

            DrawQuotaGauge(x, ref y, iw);

            GUI.Label(new Rect(x, y, iw, 24), $"보유금  <b>{_game.Money:N0}</b>원", _title);
            y += 26;

            if (warn)
                GUI.Label(new Rect(x, y, iw, 20),
                    "<color=#ff8888>마감 임박! 지하에 있으면 가방을 몰수당한다</color>", _small);
            else if (_game.AllQuotaStagesCleared)
                GUI.Label(new Rect(x, y, iw, 20),
                    $"<color=#9be89b>오늘 {GameConfig.MaxQuotaStages}단계 전부 달성! 보너스 +{_game.QuotaCompletionBonus:N0}원</color>", _small);
            else if (_game.QuotaCleared > 0)
                GUI.Label(new Rect(x, y, iw, 20),
                    $"<color=#9be89b>{_game.QuotaCleared}단계 달성 · 다음 단계까지 {_game.QuotaRemaining:N0}원</color>", _small);
            else
                GUI.Label(new Rect(x, y, iw, 20),
                    $"현재 부족 {_game.QuotaShortfall:N0}원 · 긴급 정산 {_game.QuotaShortfallPenalty:N0}원", _small);

            // 지상 마을 — 오늘의 노동이 무엇으로 남는지 항상 보이게 한다
            var village = _game.Village;
            if (village != null)
            {
                y += 22;
                GUI.Label(new Rect(x, y, iw, 20),
                    $"<color=#bcd9f2>지상  <b>{village.EraName} 시대</b> · {village.StageName}" +
                    $" · 집 {village.HouseCount}채 · 주민 {village.Population}명" +
                    (village.RelicCount > 0 ? $" · 유물 {village.RelicCount}" : "") + "</color>",
                    _small);
            }
        }

        /// <summary>
        /// 오늘의 할당량 게이지. 칸 하나가 단계 하나이고,
        /// 지금 도전 중인 칸만 실시간으로 차오른다. 넘긴 칸은 초록으로 굳는다.
        /// </summary>
        void DrawQuotaGauge(float x, ref float y, float iw)
        {
            const float BarH = 24f;
            const float Gap = 3f;
            int stages = Mathf.Max(1, GameConfig.MaxQuotaStages);
            float segW = (iw - Gap * (stages - 1)) / stages;

            var trackColor = new Color(1f, 1f, 1f, 0.10f);
            var clearedColor = new Color32(88, 200, 130, 255);
            var activeColor = new Color32(240, 172, 62, 255);

            for (int i = 0; i < stages; i++)
            {
                var seg = new Rect(x + i * (segW + Gap), y, segW, BarH);
                Fill(seg, trackColor);

                bool isCleared = i < _game.QuotaCleared;
                bool isActive = i == _game.QuotaCleared;

                if (isCleared)
                {
                    Fill(seg, clearedColor);
                }
                else if (isActive)
                {
                    float p = Mathf.Clamp01(_game.QuotaProgress01);
                    if (p > 0.001f)
                        Fill(new Rect(seg.x, seg.y, seg.width * p, seg.height), activeColor);
                    Outline(seg, new Color(1f, 1f, 1f, 0.65f));
                }

                string numColor = isCleared ? "#123322" : (isActive ? "#fff4d8" : "#8f8c86");
                GUI.Label(new Rect(seg.x + segW * 0.5f - 7f, seg.y + 3f, 16f, 18f),
                    $"<color={numColor}><b>{i + 1}</b></color>", _small);
            }
            y += BarH + 6f;

            string amount = _game.AllQuotaStagesCleared
                ? $"오늘 반입 <b>{_game.DeliveredToday:N0}</b>원 — 모든 단계 달성"
                : $"할당량 <b>{_game.QuotaStage}단계</b>   {_game.DeliveredToday:N0} / <b>{_game.Quota:N0}</b>원";
            GUI.Label(new Rect(x, y, iw, 22), amount, _label);
            y += 24f;
        }

        /// <summary>픽셀 사각형 테두리.</summary>
        void Outline(Rect r, Color c, float thickness = 1f)
        {
            Fill(new Rect(r.x, r.y, r.width, thickness), c);
            Fill(new Rect(r.x, r.y + r.height - thickness, r.width, thickness), c);
            Fill(new Rect(r.x, r.y, thickness, r.height), c);
            Fill(new Rect(r.x + r.width - thickness, r.y, thickness, r.height), c);
        }

        // ==================================================================
        // 상황 안내
        // ==================================================================
        void DrawContext()
        {
            var p = _game.Player;
            if (p == null) return;

            if (p.IsDigging)
            {
                var pr = new Rect(Screen.width * 0.5f - 110,
                    Screen.height - HotbarSlotSize - 105f, 220, 18);
                Bar(pr, p.DigProgress01, new Color32(232, 192, 92, 255), "");
                GUI.Label(new Rect(pr.x, pr.y - 22, pr.width, 20), "굴착 중...", _small);
            }

            if (!string.IsNullOrEmpty(p.BlockedReason))
            {
                var br = new Rect(Screen.width * 0.5f - 230,
                    Screen.height - HotbarSlotSize - 105f, 460, 26);
                Panel(br, new Color(0.42f, 0.10f, 0.10f, 0.88f));
                Fill(new Rect(br.x + 7f, br.y + 6f, br.width - 14f, br.height - 12f),
                    new Color(0.42f, 0.10f, 0.10f, 0.36f));
                GUI.Label(new Rect(br.x + 12, br.y + 3, br.width - 24, 22), p.BlockedReason, _label);
            }

            DigFeature feature = p.TargetDigFeature;
            if (!_game.ShopOpen && feature != DigFeature.None)
            {
                Color featureColor = DigFeatureInfo.ColorOf(feature);
                var fr = new Rect(Screen.width * 0.5f - 210, 64, 420, 34);
                Panel(fr, new Color(featureColor.r, featureColor.g, featureColor.b, 0.92f));
                GUI.Label(new Rect(fr.x + 12, fr.y + 6, fr.width - 24, 23),
                    $"<b>{DigFeatureInfo.DisplayName(feature)}</b> — {DigFeatureInfo.Description(feature)}", _label);
            }

            int digSpeedBonus = p.TargetDigSpeedBonusPercent;
            if (!_game.ShopOpen && digSpeedBonus > 0)
            {
                float y = feature != DigFeature.None ? 102f : 64f;
                var sr = new Rect(Screen.width * 0.5f - 145, y, 290, 30);
                Panel(sr, new Color(0.18f, 0.12f, 0.05f, 0.90f));
                GUI.Label(new Rect(sr.x + 12, sr.y + 4, sr.width - 24, 22),
                    $"<b>힘 강화</b> — 굴착 속도 +{digSpeedBonus}%", _label);
            }

            // 시설이 붙어 있어 사정거리가 겹친다 — 안내는 HandleInteract 의 우선순위와
            // 똑같은 순서로 골라야 "E 눌렀더니 딴 게 열리는" 일이 없다.
            var relics = _game.Relics;
            if (!_game.ShopOpen)
            {
                bool nearRestorer = _game.PlayerNearRestorer();
                bool nearRecycle = _game.PlayerNearRecycle();
                bool nearShop = _game.PlayerNearBuilding();
                bool nearQuest = _game.PlayerNearPickaxeQuest();

                string text = null;
                Color tint = default;

                if (nearQuest)
                {
                    text = _game.PickaxeQuests.InteractionText;
                    tint = new Color(0.28f, 0.18f, 0.03f, 0.92f);
                }
                else if (nearRestorer && relics != null && relics.CanDepositNow)
                {
                    text = $"[E] 유물 {relics.CarriedCount}개 복원 맡기기 · 비용 {relics.CarriedRestoreCost:N0}원";
                    tint = new Color(0.16f, 0.08f, 0.26f, 0.90f);
                }
                else if (nearRecycle && p.Inventory.Count > 0)
                {
                    text = _game.Recycling.Session.Queue.IsFull
                        ? $"작업대 가득 참 ({RecyclingQueue.MaxCapacity}/{RecyclingQueue.MaxCapacity}) · [F] 분류 시작"
                        : $"[E] {p.Inventory.Count}개 보관 · [F] 분류 시작 (대기 {_game.Recycling.Session.Queue.Count}/{RecyclingQueue.MaxCapacity}개)";
                    tint = new Color(0.06f, 0.22f, 0.10f, 0.88f);
                }
                else if (nearShop)
                {
                    text = nearRecycle ? $"[E] 강화 · [F] 분류 (대기 {_game.Recycling.Session.Queue.Count}/{RecyclingQueue.MaxCapacity}개)" : "[E] 강화 (간이 상점)";
                    tint = new Color(0.10f, 0.14f, 0.08f, 0.82f);
                }
                else if (nearRestorer)
                {
                    text = relics != null && relics.CarriedCount > 0
                        ? $"복원소 · 돈이 모자라다 (가장 싼 것 {relics.CheapestRestoreCost:N0}원)"
                        : relics != null && relics.QueueCount > 0
                            ? $"복원소 · 지금 {relics.QueueCount}개 복원 중"
                            : "복원소 · 맡길 유물이 없다";
                    tint = new Color(0.16f, 0.08f, 0.26f, 0.90f);
                }
                else if (nearRecycle)
                {
                    text = _game.Recycling.Session.Queue.IsFull
                        ? $"작업대 가득 참 ({RecyclingQueue.MaxCapacity}/{RecyclingQueue.MaxCapacity}) · [F] 분류 시작"
                        : $"[E] 가방 보관 · [F] 분류 시작 · 대기 {_game.Recycling.Session.Queue.Count}/{RecyclingQueue.MaxCapacity}개";
                    tint = new Color(0.06f, 0.22f, 0.10f, 0.88f);
                }

                if (text != null)
                {
                    var hr = new Rect(Screen.width * 0.5f - 210, 26, 420, 30);
                    Panel(hr, tint);
                    GUI.Label(new Rect(hr.x + 14, hr.y + 5, hr.width - 20, 22), text, _label);
                }
            }

            var mine = p.MineHere();
            if (mine != null)
            {
                float w = 380f;
                // 정보 표시일 뿐 버튼이 없으므로 클릭 차단 영역으로 등록하지 않는다.
                var mr = new Rect(Screen.width - w - MarginX,
                    MarginY + TopRightHeight + 8f + RelicPanelHeight, w, 250);
                Panel(mr, new Color(0.13f, 0.11f, 0.03f, 0.90f));

                Rect content = PanelContent(mr);
                float x = content.x, y = content.y, iw = content.width;
                GUI.Label(new Rect(x, y, iw, 22), mine.Label, _label); y += 24;
                GUI.Label(new Rect(x, y, iw, 20), $"남은 매장량  {mine.Reserve}", _label); y += 20;
                GUI.Label(new Rect(x, y, iw, 20),
                    $"안전 보관  {mine.Stockpile.Count} / {GameConfig.MineStockpileCap}" +
                    $" · 예상 { _game.EstimateRecycledValue(mine.Stockpile):N0}원", _label); y += 22;
                string interact = p.Inventory.Count > 0
                    ? $"[E] 가방 {p.Inventory.Count}개 안전 보관"
                    : mine.Stockpile.Count > 0 ? "[E] 보관품 가방으로 회수" : "[E] 보관함 비어 있음";
                GUI.Label(new Rect(x, y, iw, 20), interact, _small); y += 21;
                GUI.Label(new Rect(x, y, iw, 20),
                    $"일꾼  자동 재활용 {mine.Miners}명 · 대량 운반 {mine.Haulers}명", _label); y += 24;
                GUI.Label(new Rect(x, y, iw, 20),
                    $"오늘 자동 수익  {mine.AutoIncomeToday:N0}원 / {mine.AutoItemsToday}개", _label); y += 22;
                GUI.Label(new Rect(x, y, iw, 20),
                    $"[H] 자동 재활용  {GameConfig.MinerHireCost:N0}원 · 20초마다 수익", _small); y += 19;
                GUI.Label(new Rect(x, y, iw, 20),
                    $"[J] 대량 운반  {GameConfig.HaulerHireCost:N0}원 · 보관함 5개씩", _small);
            }
        }

        // ==================================================================
        // 유물 — 등에 진 유물과 복원 진행
        // ==================================================================
        /// <summary>우측 패널이 겹치지 않도록 유물 패널이 차지하는 높이.</summary>
        float RelicPanelHeight
        {
            get
            {
                var r = _game.Relics;
                if (r == null) return 0f;
                if (r.CarriedCount == 0 && r.QueueCount == 0 && r.FoundCount == 0) return 0f;
                return (r.QueueCount > 0 ? 120f : 94f) + 8f;
            }
        }

        void DrawRelicStatus()
        {
            var r = _game.Relics;
            if (r == null) return;
            if (RelicPanelHeight <= 0f) return;

            float w = 380f;
            float h = RelicPanelHeight - 8f;
            var panel = new Rect(Screen.width - w - MarginX,
                MarginY + TopRightHeight + 8f, w, h);
            Panel(panel, new Color(0.12f, 0.06f, 0.20f, 0.90f));

            Rect content = PanelContent(panel);
            float x = content.x, y = content.y, iw = content.width;
            GUI.Label(new Rect(x, y, iw, 22),
                $"<color=#dcaaff><b>유물 도감</b></color>  복원 {r.RestoredCount}/{r.TotalCount}" +
                $" · 발굴 {r.FoundCount}", _label);
            y += 22;
            GUI.Label(new Rect(x, y, iw, 20),
                $"유물 {r.CarriedCount}개 · 슬롯 {_game.Player.InventoryUsedSlots}/{GameConfig.InventorySlotCapacity} · [C] 도감", _small);
            y += 22;

            if (r.QueueCount > 0)
            {
                var def = RelicCatalog.Get(r.Queue[0].RelicId);
                var bar = new Rect(x, y, iw, 16);
                Bar(bar, r.FrontProgress01, new Color32(186, 122, 240, 255), "");
                GUI.Label(new Rect(bar.x + 6, bar.y - 2, bar.width - 12, 20),
                    $"<b>{(def != null ? def.Name : "")}</b> 복원 중" +
                    (r.QueueCount > 1 ? $"  (대기 {r.QueueCount - 1})" : ""), _small);
            }
        }

        // ==================================================================
        // 유물 도감
        // ==================================================================
        static readonly string[] CodexTabs =
        { "공룡", "요괴", "괴수", "역사", "근현대", "SF" };

        void DrawCodex()
        {
            var sys = _game.Relics;
            if (sys == null) return;

            const int Cols = 8;
            const float CellW = 104f, CellH = 118f, Gap = 6f;

            var group = (RelicGroup)Mathf.Clamp(_codexTab, 0, RelicCatalog.GroupCount - 1);
            var defs = RelicCatalog.OfGroup(group);
            int rows = Mathf.Max(1, Mathf.CeilToInt(defs.Count / (float)Cols));

            float w = Cols * CellW + (Cols - 1) * Gap + 80f;
            float h = 180f + rows * (CellH + Gap) + 46f;
            var panel = new Rect(Screen.width * 0.5f - w * 0.5f,
                Mathf.Max(20f, Screen.height * 0.5f - h * 0.5f), w, h);
            Panel(panel, new Color(0.07f, 0.06f, 0.11f, 0.97f));
            Outline(panel, new Color(0.68f, 0.52f, 0.92f, 0.65f));

            Rect content = PanelContent(panel, 32f, 26f, 26f);
            float x = content.x, y = content.y, iw = content.width;

            GUI.Label(new Rect(x, y, iw, 30),
                $"<b>유물 도감</b>   복원 <color=#c79bff>{sys.RestoredCount}</color> / {sys.TotalCount}" +
                $"   ·   발굴 {sys.FoundCount} / {sys.TotalCount}", _title);
            if (GUI.Button(new Rect(content.xMax - 88f, y - 2f, 88f, 28f), "닫기 (C)", _buttonSmall))
                _showCodex = false;
            y += 32f;

            string bonus = sys.CompletedSets > 0
                ? $"<color=#9be89b>세트 {sys.CompletedSets}개 완성 · 판매액 +{sys.SetSellBonus * 100f:0}%</color>"
                : "한 분류를 전부 복원하면 마을에 랜드마크가 서고 세트 보너스가 붙는다";
            GUI.Label(new Rect(x, y, iw, 20), bonus, _small);
            y += 22f;

            // 지금 보고 있는 분류의 보상
            int gi = (int)group;
            bool hasSet = sys.HasSet(group);
            int doneHere = sys.RestoredInGroup(group);
            int totalHere = defs.Count;
            string mark = hasSet ? "<color=#9be89b>✦ 완성</color>" : $"<color=#b9b4c2>{doneHere}/{totalHere}</color>";
            GUI.Label(new Rect(x, y, iw, 20),
                $"{mark}  <b>{RelicCatalog.SetBonusName(group)}</b> — " +
                $"{VillageSpriteCatalog.SetLandmarkNames[gi]} · {RelicSystem.SetPerkText(group)}", _small);
            y += 24f;

            // 분류 탭
            float tw = (iw - (CodexTabs.Length - 1) * 6f) / CodexTabs.Length;
            for (int i = 0; i < CodexTabs.Length; i++)
            {
                var g = (RelicGroup)i;
                int done = sys.RestoredInGroup(g);
                int total = RelicCatalog.OfGroup(g).Count;
                string label = $"{CodexTabs[i]} {done}/{total}";
                if (GUI.Button(new Rect(x + i * (tw + 6f), y, tw, 30f), label,
                        i == _codexTab ? _tabOn : _tab))
                    _codexTab = i;
            }
            y += 38f;

            // 칸
            for (int i = 0; i < defs.Count; i++)
            {
                int col = i % Cols, row = i / Cols;
                var cell = new Rect(x + col * (CellW + Gap), y + row * (CellH + Gap), CellW, CellH);
                DrawCodexCell(cell, defs[i], sys);
            }

            y += rows * (CellH + Gap) + 4f;
            GUI.Label(new Rect(x, y, iw, 20),
                "땅속에 비치는 <color=#dcaaff>발굴 전 이미지</color>가 유물이다 · 캐서 복원소 앞에서 E",
                _small);
        }

        void DrawCodexCell(Rect cell, RelicDef def, RelicSystem sys)
        {
            bool restored = sys.IsRestored(def.Id);
            bool found = sys.IsFound(def.Id);
            bool pending = sys.IsPending(def.Id);

            if (_skin != null) _skin.DrawSlot(cell, restored, found ? 1f : 0.78f);
            else Fill(cell, restored ? new Color(0.17f, 0.13f, 0.24f, 1f)
                : found ? new Color(0.12f, 0.11f, 0.14f, 1f)
                : new Color(0.08f, 0.08f, 0.09f, 1f));
            Outline(cell, restored ? new Color(0.72f, 0.56f, 0.96f, 0.9f)
                : new Color(1f, 1f, 1f, 0.12f));

            var art = new Rect(cell.x + 6f, cell.y + 6f, cell.width - 12f, 74f);
            Sprite sprite = restored ? RelicSpriteCatalog.AliveIcon(def)
                : RelicSpriteCatalog.Buried(def);

            if (sprite != null && sprite.texture != null)
            {
                Color keep = GUI.color;
                GUI.color = restored ? Color.white
                    : found ? new Color(0.72f, 0.70f, 0.78f, 1f)
                    : new Color(0f, 0f, 0f, 0.62f);          // 미발견은 검은 실루엣
                DrawSprite(art, sprite);
                GUI.color = keep;
            }

            string name = found ? def.Name : "???";
            string color = restored ? "#e8d6ff" : found ? "#bdb8c6" : "#6f6c78";
            GUI.Label(new Rect(cell.x + 4f, cell.y + 80f, cell.width - 8f, 18f),
                $"<color={color}>{name}</color>", _micro);

            string state = restored ? "<color=#9be89b>복원 완료</color>"
                : pending ? "<color=#ffd98a>복원 대기</color>"
                : found ? "<color=#ffd98a>미복원</color>"
                : $"{def.Depth}층";
            string move = def.Behavior == RelicBehavior.Wander ? "배회" : "전시";
            GUI.Label(new Rect(cell.x + 4f, cell.y + 97f, cell.width - 8f, 16f),
                restored ? $"<color=#8fa3b8>{move}</color>" : state, _micro);
        }

        /// <summary>스프라이트를 시트에서 잘라 비율을 지켜 그린다.</summary>
        void DrawSprite(Rect area, Sprite sprite)
        {
            var tex = sprite.texture;
            if (tex == null || tex.width <= 0 || tex.height <= 0) return;

            Rect src = sprite.rect;
            var coords = new Rect(src.x / tex.width, src.y / tex.height,
                src.width / tex.width, src.height / tex.height);

            float aspect = src.width / Mathf.Max(1f, src.height);
            float dw = area.width, dh = area.width / Mathf.Max(0.01f, aspect);
            if (dh > area.height)
            {
                dh = area.height;
                dw = area.height * aspect;
            }
            var dst = new Rect(area.x + (area.width - dw) * 0.5f,
                area.y + (area.height - dh) * 0.5f, dw, dh);
            GUI.DrawTextureWithTexCoords(dst, tex, coords, true);
        }

        // ==================================================================
        // 낮 간이 상점 (능력치만)
        // ==================================================================
        void DrawDayShop()
        {
            var s = _game.Player.Stats;

            float w = 500f, h = 320f;
            var r = new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.5f - h * 0.5f, w, h);
            Fill(new Rect(r.x - 3, r.y - 3, r.width + 6, r.height + 6), new Color(1f, 0.9f, 0.7f, 0.28f));
            Panel(r, new Color(0.08f, 0.09f, 0.12f, 0.96f));

            Rect content = PanelContent(r, 32f, 28f, 26f);
            float x = content.x, y = content.y, iw = content.width;

            GUI.Label(new Rect(x, y, iw, 26), "간이 강화 (낮)", _title);
            GUI.Label(new Rect(x + iw - 150, y + 4, 150, 22), $"보유금 {_game.Money:N0}원", _label);
            y += 30;
            GUI.Label(new Rect(x, y, iw, 20), "장비·펫·시설은 밤 상점에서 판다", _small);
            y += 26;

            DrawUpgradeRow(x, ref y, iw, StatKind.MoveSpeed, s);
            DrawUpgradeRow(x, ref y, iw, StatKind.Stamina, s);
            DrawUpgradeRow(x, ref y, iw, StatKind.Power, s);

            y += 4;
            if (GUI.Button(new Rect(x + iw - 110, y, 110, 30), "닫기 (E)", _button))
                _game.CloseShop();
        }

        void DrawUpgradeRow(float x, ref float y, float w, StatKind kind, PlayerStats s)
        {
            var row = new Rect(x, y, w, 50);
            Fill(row, new Color(1f, 1f, 1f, 0.05f));

            bool maxed = s.IsMaxed(kind);
            int cost = s.CostOf(kind);
            bool afford = _game.Money >= cost;

            GUI.Label(new Rect(row.x + 12, row.y + 4, 210, 22),
                $"<b>{PlayerStats.DisplayName(kind)}</b>  Lv.{s.LevelOf(kind)}", _label);
            GUI.Label(new Rect(row.x + 12, row.y + 25, 250, 20),
                maxed ? $"현재 {s.ValueText(kind)}"
                      : $"{s.ValueText(kind)}  →  <color=#9be89b>{s.NextValueText(kind)}</color>", _small);

            var btn = new Rect(row.x + row.width - 146, row.y + 9, 134, 32);
            if (maxed)
                GUI.Label(new Rect(btn.x, btn.y + 6, btn.width, 22), "  최대 단계", _small);
            else
            {
                GUI.enabled = afford;
                if (GUI.Button(btn, $"{cost:N0}원", _buttonSmall)) _game.TryUpgrade(kind);
                GUI.enabled = true;
            }
            y += 55;
        }

        // ==================================================================
        // 밤 화면 (정산 + 탭 상점)
        // ==================================================================
        void DrawNightScreen()
        {
            Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(0.03f, 0.03f, 0.08f, 0.94f));

            float w = Mathf.Min(680f, Screen.width - 80f);
            float h = Mathf.Min(560f, Screen.height - 60f);
            var r = new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.5f - h * 0.5f, w, h);
            Panel(r, new Color(0.04f, 0.04f, 0.10f, 0.96f));

            Rect content = PanelContent(r, 34f, 28f, 28f);
            float x = content.x, y = content.y, iw = content.width;

            GUI.Label(new Rect(x, y, iw, 30),
                $"{_game.Day}일차 밤 — 정비 시간", _big);
            GUI.Label(new Rect(x + iw - 190, y + 6, 190, 24), $"보유금  <b>{_game.Money:N0}</b>원", _label);
            y += 36;

            // 정산 리포트
            if (!string.IsNullOrEmpty(_game.SettlementReport))
            {
                var lines = _game.SettlementReport.Split('\n');
                float rh = lines.Length * 19f + 14f;
                Fill(new Rect(x, y, iw, rh), new Color(1f, 1f, 1f, 0.06f));
                float ly = y + 7;
                foreach (var l in lines)
                {
                    GUI.Label(new Rect(x + 12, ly, iw - 24, 18), l, _small);
                    ly += 19;
                }
                y += rh + 8;
            }

            // 지상 마을 성장 요약
            if (_game.Village != null)
            {
                GUI.Label(new Rect(x, y, iw, 22),
                    $"지상 마을  <color=#9be89b><b>{_game.Village.EraName} 시대</b></color>" +
                    $" · {_game.Village.StageName}" +
                    $" · 내 집 {Village.HouseName(_game.Village.Era)}" +
                    $" · 집 {_game.Village.HouseCount}채 · 주민 {_game.Village.Population}명", _label);
                y += 28;
            }

            // 내일 예고
            int nextQuota = GameConfig.QuotaForDay(_game.Day + 1);
            GUI.Label(new Rect(x, y, iw, 22),
                $"내일({_game.Day + 1}일차) 할당량  <color=#ffd166><b>{nextQuota:N0}원</b></color>", _label);
            y += 26;

            // 내일 시대가 넘어가는지 미리 알려 준다
            if (_game.Village != null)
            {
                int nextEra = GameConfig.EraForDay(_game.Day + 1);
                GUI.Label(new Rect(x, y, iw, 22),
                    nextEra > _game.Village.Era
                        ? $"내일 첫 할당량을 채우면  <color=#8ce0ff><b>{VillageSpriteCatalog.EraName(nextEra)} 시대</b></color>로 도약한다"
                        : $"내일도 <color=#bcd9f2>{_game.Village.EraName} 시대</color> — 마을이 옆으로 더 넓어진다",
                    _small);
            }
            y += 26;

            // 탭
            string[] tabs = { "능력치", "장비", "펫", "시설" };
            float tw = 110f;
            for (int i = 0; i < tabs.Length; i++)
            {
                if (GUI.Button(new Rect(x + i * (tw + 6), y, tw, 30), tabs[i], i == _nightTab ? _tabOn : _tab))
                    _nightTab = i;
            }
            y += 38;

            float bodyH = content.yMax - y - 52f;
            Rect body = new Rect(x, y, iw, bodyH);
            Panel(body, new Color(1f, 1f, 1f, 0.42f));
            Rect bodyContent = PanelContent(body, 26f, 20f, 20f);

            switch (_nightTab)
            {
                case 0: DrawNightStats(bodyContent.x, bodyContent.y, bodyContent.width); break;
                case 1: DrawNightGear(bodyContent.x, bodyContent.y, bodyContent.width); break;
                case 2: DrawNightPets(bodyContent.x, bodyContent.y, bodyContent.width); break;
                case 3: DrawNightFacility(bodyContent.x, bodyContent.y, bodyContent.width); break;
            }

            // 다음 날
            if (GUI.Button(new Rect(x + iw - 210, content.yMax - 42, 210, 36),
                    $"다음 날 시작  (Enter)", _button))
                _game.StartNextDay();
        }

        void DrawNightStats(float x, float y, float w)
        {
            var s = _game.Player.Stats;
            DrawUpgradeRow(x, ref y, w, StatKind.MoveSpeed, s);
            DrawUpgradeRow(x, ref y, w, StatKind.Stamina, s);
            DrawUpgradeRow(x, ref y, w, StatKind.Power, s);
        }

        void ShopRow(float x, ref float y, float w, string name, string desc,
                     string buttonText, bool enabled, System.Action onBuy)
        {
            var row = new Rect(x, y, w, 48);
            Fill(row, new Color(1f, 1f, 1f, 0.05f));

            GUI.Label(new Rect(row.x + 12, row.y + 4, w - 170, 22), $"<b>{name}</b>", _label);
            GUI.Label(new Rect(row.x + 12, row.y + 25, w - 170, 20), desc, _small);

            var btn = new Rect(row.x + w - 150, row.y + 8, 138, 32);
            if (buttonText == null)
            {
                GUI.Label(new Rect(btn.x, btn.y + 5, btn.width, 22), "  보유 중", _small);
            }
            else
            {
                GUI.enabled = enabled;
                if (GUI.Button(btn, buttonText, _buttonSmall)) onBuy?.Invoke();
                GUI.enabled = true;
            }
            y += 53;
        }

        void DrawNightGear(float x, float y, float w)
        {
            var g = _game.Player.Gear;
            int money = _game.Money;

            int lampCost = g.HeadlampLevel == 0 ? GameConfig.HeadlampCost1 : GameConfig.HeadlampCost2;
            ShopRow(x, ref y, w, $"헤드랜턴 Lv.{g.HeadlampLevel}", "시야 +1 (최대 2단계)",
                g.HeadlampLevel >= 2 ? null : $"{lampCost:N0}원",
                money >= lampCost, () => _game.TryBuyHeadlamp());

            ShopRow(x, ref y, w, "쓰레기 탐지기", "지하에서 Q — 주변 쓰레기가 벽 너머로 잠시 보인다",
                g.HasDetector ? null : $"{GameConfig.DetectorCost:N0}원",
                money >= GameConfig.DetectorCost, () => _game.TryBuyDetector());

            ShopRow(x, ref y, w, "방호복", "유해 폐기물을 다치지 않고 줍는다 (3층 필수)",
                g.HasHazmat ? null : $"{GameConfig.HazmatCost:N0}원",
                money >= GameConfig.HazmatCost, () => _game.TryBuyHazmat());

            ShopRow(x, ref y, w, "휴대용 압축기", "줍는 쓰레기의 부피 30% 감소",
                g.HasCompressor ? null : $"{GameConfig.CompressorCost:N0}원",
                money >= GameConfig.CompressorCost, () => _game.TryBuyCompressor());

            ShopRow(x, ref y, w, "귀환 벨", "지하에서 B — 하루 1회 즉시 지상 귀환",
                g.HasReturnBell ? null : $"{GameConfig.ReturnBellCost:N0}원",
                money >= GameConfig.ReturnBellCost, () => _game.TryBuyReturnBell());

            ShopRow(x, ref y, w, $"에너지 드링크 ({g.EnergyDrinks}/{GameConfig.EnergyDrinkMax})",
                $"지하에서 F — 기력 +{GameConfig.EnergyDrinkStamina:0}",
                g.EnergyDrinks >= GameConfig.EnergyDrinkMax ? null : $"{GameConfig.EnergyDrinkCost:N0}원",
                money >= GameConfig.EnergyDrinkCost, () => _game.TryBuyEnergyDrink());
        }

        void DrawNightPets(float x, float y, float w)
        {
            GUI.Label(new Rect(x + 4, y, w, 20),
                $"펫은 한 마리만 동행한다 · 먹이값 하루 {GameConfig.PetFeedCost}원", _small);
            y += 26;

            DrawPetRow(x, ref y, w, PetKind.Mole);
            DrawPetRow(x, ref y, w, PetKind.Firefly);
            DrawPetRow(x, ref y, w, PetKind.Turtle);
        }

        void DrawPetRow(float x, ref float y, float w, PetKind kind)
        {
            var g = _game.Player.Gear;
            int cost = PlayerGear.PetCost(kind);
            bool owned = g.Pet == kind;

            ShopRow(x, ref y, w, PlayerGear.PetName(kind), PlayerGear.PetDesc(kind),
                owned ? null : $"{cost:N0}원",
                _game.Money >= cost, () => _game.TryBuyPet(kind));
        }

        void DrawNightFacility(float x, float y, float w)
        {
            int lv = _game.RecycleLevel;

            string desc = lv == 0
                ? $"기본 재질 보너스 x1.25~x2.10 · 설비 가동 시 추가 x{GameConfig.RecycleBonus1:0.0}"
                : lv == 1
                    ? $"재질 보너스에 설비 x{GameConfig.RecycleBonus1:0.0} · 확장 시 x{GameConfig.RecycleBonus2:0.0}"
                    : $"재질 보너스에 최고 설비 x{GameConfig.RecycleBonus2:0.0} 적용 중";

            int cost = lv == 0 ? GameConfig.RecycleStationCost : GameConfig.RecycleUpgradeCost;

            ShopRow(x, ref y, w, $"분리수거장 Lv.{lv}", desc,
                lv >= 2 ? null : $"{cost:N0}원",
                _game.Money >= cost, () => _game.TryBuyRecycleStation());

            // 땅굴 입구 확장
            int ewl = _game.EntranceWidenLevel;
            bool ewMax = ewl >= GameConfig.MaxEntranceWidenLevel;
            int ewCost = GameConfig.EntranceWidenCost(ewl);
            string ewDesc = ewMax
                ? $"입구 폭 {_game.EntranceWidth}칸 — 더 넓힐 수 없다"
                : $"현재 폭 {_game.EntranceWidth}칸 → {_game.EntranceWidth + 2}칸. "
                  + "좌우로 한 칸씩 뚫어 오르내리기가 빨라진다";

            ShopRow(x, ref y, w, $"땅굴 입구 확장 Lv.{ewl}", ewDesc,
                ewMax ? null : $"{ewCost:N0}원",
                _game.Money >= ewCost, () => _game.TryBuyEntranceWiden());

            var relics = _game.Relics;
            if (relics != null)
            {
                y += 6;
                GUI.Label(new Rect(x + 4, y, w, 20),
                    $"<color=#dcaaff>유물</color>  복원 {relics.RestoredCount}/{relics.TotalCount}" +
                    $" · 발굴 {relics.FoundCount}" +
                    (relics.CompletedSets > 0
                        ? $" · 세트 {relics.CompletedSets}개 (판매 +{relics.SetSellBonus * 100f:0}%)"
                        : "") + "   [C] 도감", _small);
                y += 20;
            }

            y += 6;
            GUI.Label(new Rect(x + 4, y, w, 20),
                $"고용된 일꾼  {_game.Workers.Count}명 — 일당은 매일 밤 자동 지급된다", _small);
            y += 20;
            GUI.Label(new Rect(x + 4, y, w, 20),
                "일꾼 고용은 낮에 발견한 지하 분리수거장 위에서 H / J", _small);
        }

        // ==================================================================
        void DrawLogPanel()
        {
            var lines = _game.RecentLog(6);
            if (lines.Count == 0) return;

            float h = lines.Count * 19 + 44;
            var r = new Rect(MarginX, Screen.height - h - MarginY, 480, h);
            Panel(r, new Color(0f, 0f, 0f, 0.5f));

            Rect content = PanelContent(r, 26f, 20f, 18f);
            for (int i = 0; i < lines.Count; i++)
                GUI.Label(new Rect(content.x, content.y + i * 19, content.width, 18), lines[i], _small);
        }

        void DrawHelpPanel()
        {
            // 플레이 중에는 핵심 키만 작은 카드로 표시한다. 전체 설명은 ESC 메뉴에 둔다.
            var r = new Rect(MarginX, MarginY + 280f, 460f, 188f);
            Panel(r, new Color(0f, 0f, 0f, 0.72f));
            Rect content = PanelContent(r, 26f, 22f, 18f);
            float y = content.y;
            GUI.Label(new Rect(content.x, y, content.width, 22), "간단 조작  (Tab: 닫기 · ESC: 메뉴)", _label);
            y += 28f;
            string[] lines =
            {
                "WASD 이동 · Space 굴착 · E 상호작용",
                "Q 탐지기 · B 귀환 · F 드링크/분류",
                "1~5 분리수거 · C 유물 도감 · E(상호작용 없을 때)/I 인벤토리",
                "H 자동 재활용 · J 대량 운반",
                "해 지기 전 지상 귀환 · 할당량 달성"
            };
            foreach (var l in lines)
            {
                GUI.Label(new Rect(content.x, y, content.width, 19), l, _small);
                y += 21f;
            }
        }

        // ==================================================================
        // 화면 하단: 하나의 긴 프레임을 9칸으로 나눈 인벤토리 핫바
        // ==================================================================
        void DrawInventoryHotbar()
        {
            var player = _game.Player;
            if (player == null) return;

            float slotsW = HotbarSlotCount * HotbarSlotSize;
            float startX = Mathf.Round((Screen.width - slotsW) * 0.5f);
            float slotY = Screen.height - HotbarSlotSize - 18f;
            var frame = new Rect(startX - 7f, slotY - 27f, slotsW + 14f, HotbarSlotSize + 34f);
            const float meterGap = 8f;
            const float meterHeight = 20f;
            float meterY = frame.y - meterHeight - 8f;
            float meterWidth = (frame.width - meterGap) * 0.5f;

            var stats = player.Stats;
            if (stats != null)
            {
                Bar(new Rect(frame.x, meterY, meterWidth, meterHeight),
                    stats.Stamina / stats.MaxStamina, new Color32(86, 166, 240, 255),
                    $"기력  {stats.Stamina:0} / {stats.MaxStamina:0}");
                PollutionBar(new Rect(frame.x + meterWidth + meterGap, meterY, meterWidth, meterHeight),
                    _game.PollutionLevel);
            }

            Fill(frame, new Color(0.055f, 0.045f, 0.09f, 0.96f));
            Outline(frame, new Color(0.26f, 0.21f, 0.36f, 1f), 3f);
            Outline(new Rect(frame.x + 3f, frame.y + 3f, frame.width - 6f, frame.height - 6f),
                new Color(0.57f, 0.48f, 0.72f, 0.72f), 1f);
            Fill(new Rect(frame.x + 5f, slotY - 3f, frame.width - 10f, 2f),
                new Color(0.38f, 0.31f, 0.52f, 0.88f));

            int itemCount = player.InventoryUsedSlots;
            int firstItem = Mathf.Max(0, itemCount - HotbarSlotCount);
            int hidden = firstItem;
            string header = $"인벤토리  {itemCount} / {GameConfig.InventorySlotCapacity}칸" +
                $"   ·   부피 {player.InventoryVolume} / {player.BagCapacity}";
            if (hidden > 0) header += $"   ·   앞쪽 {hidden}개 더 있음";
            GUI.Label(new Rect(frame.x + 8f, frame.y + 3f, frame.width - 16f, 20f), header, _small);

            LootItem? hovered = null;
            RelicDef hoveredRelic = null;
            int newestSlot = Mathf.Min(itemCount, HotbarSlotCount) - 1;
            for (int slot = 0; slot < HotbarSlotCount; slot++)
            {
                float x = startX + slot * HotbarSlotSize;
                var r = new Rect(x, slotY, HotbarSlotSize, HotbarSlotSize);
                Fill(r, slot == newestSlot
                    ? new Color(0.34f, 0.24f, 0.08f, 0.32f)
                    : new Color(0.07f, 0.06f, 0.11f, 0.58f));
                if (slot > 0)
                    Fill(new Rect(r.x, r.y + 4f, 2f, r.height - 8f),
                        new Color(0.35f, 0.29f, 0.46f, 0.92f));
                if (slot == newestSlot)
                    Fill(new Rect(r.x + 4f, r.yMax - 3f, r.width - 8f, 2f),
                        new Color(0.95f, 0.72f, 0.20f, 0.95f));

                int itemIndex = firstItem + slot;
                if (itemIndex >= itemCount)
                {
                    GUI.Label(new Rect(r.x + 3f, r.y + 2f, 14f, 13f), (slot + 1).ToString(), _micro);
                    continue;
                }

                GUI.Label(new Rect(r.x + 3f, r.y + 2f, 14f, 13f), (slot + 1).ToString(), _micro);
                if (itemIndex < player.Inventory.Count)
                {
                    LootItem item = player.Inventory[itemIndex];
                    DrawLootSlotContents(r, item);
                    if (r.Contains(Event.current.mousePosition)) hovered = item;
                }
                else
                {
                    int relicIndex = itemIndex - player.Inventory.Count;
                    var carried = _game.Relics?.Carried;
                    if (carried != null && relicIndex >= 0 && relicIndex < carried.Count)
                    {
                        RelicDef def = RelicCatalog.Get(carried[relicIndex]);
                        DrawRelicSlotContents(r, def);
                        if (r.Contains(Event.current.mousePosition)) hoveredRelic = def;
                    }
                }
            }

            if (hovered.HasValue)
                DrawInventoryTooltip(hovered.Value, meterY);
            else if (hoveredRelic != null)
                DrawRelicInventoryTooltip(hoveredRelic, meterY);
        }

        void DrawLootSlotContents(Rect r, LootItem item)
        {
            var icon = new Rect(r.x + 17f, r.y + 5f, 22f, 20f);
            Fill(new Rect(icon.x - 2f, icon.y - 2f, icon.width + 4f, icon.height + 4f),
                new Color(0f, 0f, 0f, 0.62f));
            Sprite pixelArt = TrashSpriteCatalog.For(item);
            if (pixelArt != null)
            {
                Fill(icon, new Color(item.Color.r, item.Color.g, item.Color.b, 0.28f));
                DrawSprite(icon, pixelArt, false);
            }
            else
            {
                Fill(icon, item.Color);
                GUI.Label(icon, MaterialGlyph(item.Material), _micro);
            }

            GUI.Label(new Rect(r.x + r.width - 19f, r.y + 2f, 17f, 13f), $"V{item.Volume}", _micro);
            GUI.Label(new Rect(r.x + 2f, r.y + 27f, r.width - 4f, 13f), ShortItemName(item.Name), _micro);
            GUI.Label(new Rect(r.x + 2f, r.y + 40f, r.width - 4f, 13f), CompactMoney(item.Value), _micro);
        }

        void DrawRelicSlotContents(Rect r, RelicDef def)
        {
            if (def == null) return;
            GUI.Label(new Rect(r.x + 3f, r.y + 2f, 16f, 13f), "유", _micro);
            Sprite sprite = RelicSpriteCatalog.Buried(def);
            if (sprite != null)
                DrawSprite(new Rect(r.x + 16f, r.y + 5f, 24f, 24f), sprite, false);
            GUI.Label(new Rect(r.x + 2f, r.y + 30f, r.width - 4f, 13f), ShortItemName(def.Name), _micro);
            GUI.Label(new Rect(r.x + 2f, r.y + 42f, r.width - 4f, 12f), $"유물 · {def.Depth}층", _micro);
        }

        static void DrawSprite(Rect rect, Sprite sprite, bool flipX)
        {
            if (sprite == null || sprite.texture == null) return;
            Rect sr = sprite.rect;
            Texture2D texture = sprite.texture;
            var uv = new Rect(sr.x / texture.width, sr.y / texture.height,
                sr.width / texture.width, sr.height / texture.height);
            if (flipX)
            {
                uv.x += uv.width;
                uv.width = -uv.width;
            }
            GUI.DrawTextureWithTexCoords(rect, texture, uv, true);
        }

        void DrawInventoryTooltip(LootItem item, float anchorY)
        {
            float w = 350f, h = 70f;
            var r = new Rect(
                Mathf.Clamp(Event.current.mousePosition.x - w * 0.5f, 8f, Screen.width - w - 8f),
                anchorY - h - 10f,
                w,
                h);
            Panel(r, new Color(0.025f, 0.025f, 0.04f, 0.96f));
            Fill(new Rect(r.x + 5f, r.y + 5f, 3f, r.height - 10f), item.Color);
            GUI.Label(new Rect(r.x + 26f, r.y + 14f, r.width - 52f, 20f), $"<b>{item.Name}</b>", _label);
            GUI.Label(new Rect(r.x + 26f, r.y + 36f, r.width - 52f, 18f),
                $"{MaterialInfo.DisplayName(item.Material)} · {item.Value:N0}원 · 부피 {item.Volume}", _small);
        }

        void DrawRelicInventoryTooltip(RelicDef def, float anchorY)
        {
            float w = 350f, h = 70f;
            var r = new Rect(
                Mathf.Clamp(Event.current.mousePosition.x - w * 0.5f, 8f, Screen.width - w - 8f),
                anchorY - h - 10f, w, h);
            Panel(r, new Color(0.10f, 0.05f, 0.16f, 0.96f));
            GUI.Label(new Rect(r.x + 22f, r.y + 13f, r.width - 44f, 20f), $"<b>유물 · {def.Name}</b>", _label);
            GUI.Label(new Rect(r.x + 22f, r.y + 37f, r.width - 44f, 18f),
                $"{def.Depth}층 · {def.GroupName} · 복원 비용 {RelicSystem.RestoreCost(def):N0}원", _small);
        }

        static string ShortItemName(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length <= 4) return name;
            return name.Substring(0, 4);
        }

        static string CompactMoney(int value)
        {
            if (value >= 10000) return $"{value / 1000f:0.#}k원";
            return $"{value:N0}원";
        }

        static string MaterialGlyph(MaterialKind material)
        {
            switch (material)
            {
                case MaterialKind.General: return "일";
                case MaterialKind.Metal: return "철";
                case MaterialKind.Glass: return "유";
                case MaterialKind.Plastic: return "P";
                case MaterialKind.Electronics: return "전";
                case MaterialKind.Hazard: return "독";
                default: return "★";
            }
        }

        void DrawResultOverlay()
        {
            Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.8f));

            bool won = _game.State == GameState.Won;
            string title = won ? "<color=#8ce89a>완주!</color>" : "<color=#ff7b7b>해고당했다</color>";

            float y = Screen.height * 0.34f;
            GUI.Label(new Rect(0, y, Screen.width, 40), title, _center);
            GUI.Label(new Rect(0, y + 46, Screen.width, 30), _game.ResultReason, _center);
            GUI.Label(new Rect(0, y + 86, Screen.width, 30),
                $"{_game.Day}일차 · 최종 보유금 {_game.Money:N0}원", _center);
            GUI.Label(new Rect(0, y + 132, Screen.width, 30), "[R] 다시 시작", _center);
        }

        // ==================================================================
        // 월드 시설/NPC 말풍선
        // ==================================================================
        void DrawRecycleFullBubble()
        {
            var queue = _game.Recycling?.Session?.Queue;
            if (queue == null || !queue.IsFull || _showCodex || _game.ShopOpen) return;
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return;

            RectInt rect = _game.RecycleRect;
            float worldX = (rect.xMin + rect.width * 0.5f) * GameConfig.CellSize;
            float worldY = -GameConfig.SurfaceRow * GameConfig.CellSize
                + 5.8f * GameConfig.BuildingScale * GameConfig.CellSize;
            Vector3 screen = _cam.WorldToScreenPoint(new Vector3(worldX, worldY, 0f));
            if (screen.z < 0f) return;

            const float w = 126f, h = 42f;
            float x = Mathf.Clamp(screen.x - w * 0.5f, 8f, Screen.width - w - 8f);
            float y = Mathf.Clamp(Screen.height - screen.y - h - 8f, 8f, Screen.height - h - 18f);
            var bubble = new Rect(x, y, w, h);
            Color outline = new Color(0.18f, 0.09f, 0.08f, 0.97f);
            Color body = new Color(1f, 0.88f, 0.62f, 0.98f);

            if (_skin != null) _skin.DrawSpeechBubble(new Rect(bubble.x - 4f, bubble.y - 4f, bubble.width + 8f, bubble.height + 17f));
            else
            {
                Fill(new Rect(bubble.x - 3f, bubble.y - 3f, bubble.width + 6f, bubble.height + 6f), outline);
                Fill(bubble, body);
                float tailX = Mathf.Clamp(screen.x, bubble.x + 18f, bubble.xMax - 18f);
                Fill(new Rect(tailX - 7f, bubble.yMax, 14f, 8f), outline);
                Fill(new Rect(tailX - 4f, bubble.yMax, 8f, 8f), body);
                Fill(new Rect(tailX - 3f, bubble.yMax + 8f, 6f, 5f), outline);
            }
            GUI.Label(new Rect(bubble.x + 18f, bubble.y + 7f, bubble.width - 36f, bubble.height - 18f),
                "가득 참!", _fullBubbleText);
        }

        void DrawQuestGiverLabel()
        {
            var quests = _game.PickaxeQuests;
            var giver = quests?.Giver;
            if (giver == null || quests.AllCompleted || _showCodex || _game.ShopOpen) return;
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return;

            Vector3 screen = _cam.WorldToScreenPoint(new Vector3(
                giver.transform.position.x, giver.QuestLabelWorldY, 0f));
            if (screen.z < 0f) return;

            string message = $"지하 {quests.TargetDepth}층에서 유물을\n" +
                $"캐서 복원시켜줘! ({quests.Progress}/{quests.RequiredCount})";

            const float w = 286f, h = 64f;
            float x = Mathf.Round(Mathf.Clamp(screen.x - w * 0.5f, 8f, Screen.width - w - 8f));
            float y = Mathf.Round(Mathf.Clamp(Screen.height - screen.y - h - 8f, 8f, Screen.height - h - 18f));
            var bubble = new Rect(x, y, w, h);

            DrawThinQuestBubble(bubble, screen.x);

            // 느낌표는 별도 박스를 쓰지 않고 말풍선 안에 기호만 배치한다.
            GUI.Label(new Rect(bubble.x + 14f, bubble.y + 10f, 34f, 44f), "!", _questBang);
            GUI.Label(new Rect(bubble.x + 52f, bubble.y + 13f, bubble.width - 72f, bubble.height - 26f),
                message, _questBubbleText);
        }

        /// <summary>
        /// 퀘스트 말풍선은 2px 전용 프레임을 쓴다. 큰 동굴 프레임이나 9-slice를
        /// 축소하지 않아 작은 높이에서도 본문 영역을 침범하지 않는다.
        /// </summary>
        void DrawThinQuestBubble(Rect bubble, float targetScreenX)
        {
            Color outline = new Color32(48, 34, 60, 255);
            Color body = new Color32(247, 229, 187, 255);
            Color highlight = new Color32(255, 244, 214, 255);
            Color shadow = new Color32(205, 170, 112, 255);
            float tailX = Mathf.Round(Mathf.Clamp(targetScreenX, bubble.x + 18f, bubble.xMax - 18f));

            // 두 칸씩 좁아지는 계단형 꼬리. 바깥선도 본체와 같은 2px 두께다.
            Fill(new Rect(tailX - 7f, bubble.yMax - 2f, 14f, 5f), outline);
            Fill(new Rect(tailX - 5f, bubble.yMax + 3f, 10f, 4f), outline);
            Fill(new Rect(tailX - 3f, bubble.yMax + 7f, 6f, 4f), outline);
            Fill(new Rect(tailX - 1f, bubble.yMax + 11f, 2f, 3f), outline);
            Fill(new Rect(tailX - 5f, bubble.yMax - 2f, 10f, 4f), body);
            Fill(new Rect(tailX - 3f, bubble.yMax + 2f, 6f, 4f), body);
            Fill(new Rect(tailX - 1f, bubble.yMax + 6f, 2f, 4f), body);

            Fill(bubble, outline);
            Fill(new Rect(bubble.x + 2f, bubble.y + 2f, bubble.width - 4f, bubble.height - 4f), body);
            Fill(new Rect(bubble.x + 3f, bubble.y + 3f, bubble.width - 6f, 1f), highlight);
            Fill(new Rect(bubble.x + 3f, bubble.yMax - 4f, bubble.width - 6f, 1f), shadow);
        }

        void DrawRelicNames()
        {
            if (_showCodex || _game.ShopOpen) return;

            var village = _game.Village;
            var player = _game.Player;
            if (village == null || player == null) return;
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return;

            var units = village.RelicUnits;
            if (units == null || units.Count == 0) return;

            Vector3 eye = player.transform.position;
            float range = RelicLabelRange * GameConfig.CellSize;

            // OnGUI 는 한 프레임에 여러 번 돈다. 정렬 델리게이트를 새로 만들지 않도록
            // 가까운 순서로 직접 끼워 넣고, 목록은 RelicLabelMax 개로 잘라 둔다.
            _nearbyRelics.Clear();
            for (int i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                if (unit == null || !unit.HasArt) continue;

                Vector3 at = unit.transform.position;
                float dx = at.x - eye.x;
                float dy = (at.y - eye.y) * RelicLabelVerticalWeight;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                if (distance > range) continue;

                if (_nearbyRelics.Count >= RelicLabelMax &&
                    distance >= _nearbyRelics[_nearbyRelics.Count - 1].Distance) continue;

                var plate = new Nameplate
                {
                    Text = unit.DisplayName,
                    World = new Vector3(at.x, unit.LabelWorldY, 0f),
                    Distance = distance,
                    // 경계에서 툭 나타나지 않도록 가장자리 35% 구간에서 서서히 뜬다
                    Alpha = Mathf.Clamp01((range - distance) / Mathf.Max(0.01f, range * 0.35f)),
                };

                int slot = _nearbyRelics.Count;
                while (slot > 0 && _nearbyRelics[slot - 1].Distance > distance) slot--;
                _nearbyRelics.Insert(slot, plate);
                if (_nearbyRelics.Count > RelicLabelMax) _nearbyRelics.RemoveAt(RelicLabelMax);
            }

            // 가까운 것이 위에 오도록 먼 것부터 그린다
            for (int i = _nearbyRelics.Count - 1; i >= 0; i--) DrawNameplate(_nearbyRelics[i]);
            _nearbyRelics.Clear();
        }

        struct Nameplate
        {
            public string Text;
            public Vector3 World;
            public float Distance;
            public float Alpha;
        }

        readonly List<Nameplate> _nearbyRelics = new List<Nameplate>();
        Camera _cam;

        void DrawNameplate(Nameplate plate)
        {
            if (string.IsNullOrEmpty(plate.Text)) return;

            Vector3 screen = _cam.WorldToScreenPoint(plate.World);
            if (screen.z < 0f) return;                       // 카메라 뒤

            // OnGUI 는 Layout·Repaint·입력 이벤트마다 돈다. 이름은 변하지 않으니 한 번만 잰다.
            _plateContent.text = plate.Text;
            if (!_plateSizes.TryGetValue(plate.Text, out Vector2 size))
            {
                size = _nameplate.CalcSize(_plateContent);
                _plateSizes[plate.Text] = size;
            }
            float w = size.x + 16f, h = size.y + 7f;

            float x = screen.x - w * 0.5f;
            float y = Screen.height - screen.y - h;          // GUI 는 y 가 아래로 간다
            if (x + w < 0f || x > Screen.width || y + h < 0f || y > Screen.height) return;

            x = Mathf.Clamp(x, 2f, Mathf.Max(2f, Screen.width - w - 2f));
            y = Mathf.Clamp(y, 2f, Mathf.Max(2f, Screen.height - h - 2f));

            var box = new Rect(x, y, w, h);
            Panel(box, new Color(0.04f, 0.03f, 0.06f, 0.72f * plate.Alpha));
            Fill(new Rect(box.x, box.yMax - 2f, box.width, 2f),
                new Color(0.98f, 0.84f, 0.49f, 0.85f * plate.Alpha));

            var old = GUI.color;
            GUI.color = new Color(1f, 0.97f, 0.9f, plate.Alpha);
            GUI.Label(box, _plateContent, _nameplate);
            GUI.color = old;
        }

        readonly GUIContent _plateContent = new GUIContent();
        readonly Dictionary<string, Vector2> _plateSizes = new Dictionary<string, Vector2>();

        void Fill(Rect r, Color c)
        {
            var old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, _white);
            GUI.color = old;
        }
    }
}
