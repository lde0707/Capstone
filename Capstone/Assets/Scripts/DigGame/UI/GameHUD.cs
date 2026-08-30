using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DigGame
{
    /// <summary>프로토타입용 IMGUI HUD. 캔버스/폰트 에셋 없이 바로 동작한다.</summary>
    public class GameHUD : MonoBehaviour
    {
        const float MarginX = 28f;
        const float MarginY = 26f;
        const float PanelW = 268f;
        const int HotbarSlotCount = 9;
        const float HotbarSlotSize = 56f;
        const float HotbarSlotGap = 4f;

        GameManager _game;
        Font _font;
        GUIStyle _label, _small, _micro, _title, _big, _center, _button, _buttonSmall, _tab, _tabOn;
        Texture2D _white;
        bool _showHelp = true;
        bool _stylesReady;
        int _nightTab;   // 0 능력치 / 1 장비 / 2 펫 / 3 시설

        readonly List<Rect> _blockers = new List<Rect>();

        public void Init(GameManager game)
        {
            _game = game;
            if (game != null) game.Hud = this;
        }

        public bool BlocksPointer(Vector2 screenPos)
        {
            var p = new Vector2(screenPos.x, Screen.height - screenPos.y);
            for (int i = 0; i < _blockers.Count; i++)
                if (_blockers[i].Contains(p)) return true;
            return false;
        }

        void Panel(Rect r, Color c)
        {
            Fill(r, c);
            _blockers.Add(r);
        }

        void Awake()
        {
            _white = new Texture2D(1, 1) { hideFlags = HideFlags.DontSave };
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
            _font = LoadKoreanFont();
        }

        void OnDestroy()
        {
            DestroyTexture(_white);
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
            var kb = Keyboard.current;
            if (kb != null && kb.tabKey.wasPressedThisFrame) _showHelp = !_showHelp;
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
            _button = new GUIStyle(GUI.skin.button) { fontSize = 15, richText = true };
            _buttonSmall = new GUIStyle(GUI.skin.button) { fontSize = 13, richText = true };
            _tab = new GUIStyle(GUI.skin.button) { fontSize = 14, richText = true };
            _tabOn = new GUIStyle(GUI.skin.button) { fontSize = 14, fontStyle = FontStyle.Bold, richText = true };

            if (_font != null)
            {
                _label.font = _font; _small.font = _font; _micro.font = _font; _title.font = _font;
                _big.font = _font; _center.font = _font;
                _button.font = _font; _buttonSmall.font = _font;
                _tab.font = _font; _tabOn.font = _font;
            }

            var c = new Color(0.94f, 0.94f, 0.96f);
            _label.normal.textColor = c;
            _small.normal.textColor = c;
            _micro.normal.textColor = c;
            _title.normal.textColor = c;
            _big.normal.textColor = c;
            _center.normal.textColor = c;
        }

        void OnGUI()
        {
            if (_game == null || _game.Grid == null) return;
            EnsureStyles();

            if (Event.current.type == EventType.Layout) _blockers.Clear();

            switch (_game.State)
            {
                case GameState.Day:
                    DrawStatsPanel();
                    DrawTopRight();
                    DrawContext();
                    DrawLogPanel();
                    if (_showHelp) DrawHelpPanel();
                    if (_game.ShopOpen) DrawDayShop();
                    DrawInventoryHotbar();
                    break;

                case GameState.Night:
                    DrawNightScreen();
                    break;

                case GameState.Won:
                case GameState.Lost:
                    DrawResultOverlay();
                    break;
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

            float h = 196f;
            var r = new Rect(MarginX, MarginY, PanelW, h);
            Panel(r, new Color(0f, 0f, 0f, 0.58f));

            float x = r.x + 12f, w = r.width - 24f, y = r.y + 12f;

            Bar(new Rect(x, y, w, 18), s.Health / s.MaxHealth,
                new Color32(224, 78, 78, 255), $"체력  {s.Health:0} / {s.MaxHealth:0}");
            y += 26;

            GUI.Label(new Rect(x, y, w, 20), $"이동속도  <b>{s.MoveSpeed:0.0}</b>", _label);
            y += 24;

            Bar(new Rect(x, y, w, 18), s.Stamina / s.MaxStamina,
                new Color32(86, 166, 240, 255), $"기력  {s.Stamina:0} / {s.MaxStamina:0}");
            y += 26;

            GUI.Label(new Rect(x, y, w, 20), $"힘  <b>{s.Power}</b>", _label);
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
        }

        void Bar(Rect r, float t, Color color, string text)
        {
            Fill(r, new Color(1f, 1f, 1f, 0.13f));
            Fill(new Rect(r.x, r.y, r.width * Mathf.Clamp01(t), r.height), color);
            GUI.Label(new Rect(r.x + 7, r.y - 1, r.width, r.height + 3), text, _small);
        }

        // ==================================================================
        // 우측 상단: 일차 / 할당량 / 돈
        // ==================================================================
        void DrawTopRight()
        {
            float w = 330f;
            var r = new Rect(Screen.width - w - MarginX, MarginY, w, 122);
            Panel(r, new Color(0f, 0f, 0f, 0.58f));

            bool warn = _game.TimeLeft < GameConfig.DayEndWarnSeconds;

            float x = r.x + 12, y = r.y + 10, iw = r.width - 24;

            GUI.Label(new Rect(x, y, iw, 24),
                $"<b>{_game.Day}일차</b> / {GameConfig.DaysToWin}일", _title);
            y += 28;

            Fill(new Rect(x, y, iw, 18), new Color(1f, 1f, 1f, 0.13f));
            Fill(new Rect(x, y, iw * _game.QuotaProgress01, 18),
                _game.QuotaProgress01 >= 1f ? new Color32(88, 200, 130, 255) : new Color32(230, 160, 60, 255));
            GUI.Label(new Rect(x + 6, y - 1, iw, 22),
                $"오늘 할당량  {_game.DeliveredToday:N0} / {_game.Quota:N0}원", _small);
            y += 26;

            GUI.Label(new Rect(x, y, iw, 24), $"보유금  <b>{_game.Money:N0}</b>원", _title);
            y += 26;

            if (warn)
                GUI.Label(new Rect(x, y, iw, 20),
                    "<color=#ff8888>마감 임박! 지하에 있으면 가방을 몰수당한다</color>", _small);
            else if (_game.SellMultiplier > 1f)
                GUI.Label(new Rect(x, y, iw, 20),
                    $"분리수거장 가동 중 — 판매가 x{_game.SellMultiplier:0.0}", _small);
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
                var pr = new Rect(Screen.width * 0.5f - 110, Screen.height - 92, 220, 18);
                Fill(new Rect(pr.x - 2, pr.y - 2, pr.width + 4, pr.height + 4), new Color(0, 0, 0, 0.6f));
                Fill(pr, new Color(1f, 1f, 1f, 0.15f));
                Fill(new Rect(pr.x, pr.y, pr.width * p.DigProgress01, pr.height), new Color32(232, 192, 92, 255));
                GUI.Label(new Rect(pr.x, pr.y - 22, pr.width, 20), "굴착 중...", _small);
            }

            if (!string.IsNullOrEmpty(p.BlockedReason))
            {
                var br = new Rect(Screen.width * 0.5f - 230, Screen.height - 58, 460, 26);
                Fill(br, new Color(0.42f, 0.10f, 0.10f, 0.78f));
                GUI.Label(new Rect(br.x + 12, br.y + 3, br.width - 24, 22), p.BlockedReason, _label);
            }

            if (!_game.ShopOpen && _game.PlayerNearRecycle())
            {
                var hr = new Rect(Screen.width * 0.5f - 190, 26, 380, 30);
                Fill(hr, new Color(0.06f, 0.22f, 0.10f, 0.88f));
                string text = p.Inventory.Count > 0
                    ? $"[E] 쓰레기 {p.Inventory.Count}개 버리기 · {p.InventoryValue:N0}원"
                    : "[E] 분리수거장 · 가방이 비어 있음";
                GUI.Label(new Rect(hr.x + 14, hr.y + 5, hr.width - 20, 22), text, _label);
            }
            else if (!_game.ShopOpen && _game.PlayerNearBuilding())
            {
                var hr = new Rect(Screen.width * 0.5f - 130, 26, 260, 30);
                Fill(hr, new Color(0.10f, 0.14f, 0.08f, 0.82f));
                GUI.Label(new Rect(hr.x + 14, hr.y + 5, hr.width - 20, 22), "[E] 강화 (간이 상점)", _label);
            }

            var mine = p.MineHere();
            if (mine != null)
            {
                float w = 330f;
                // 정보 표시일 뿐 버튼이 없으므로 클릭 차단 영역으로 등록하지 않는다.
                var mr = new Rect(Screen.width - w - MarginX, MarginY + 134, w, 164);
                Fill(mr, new Color(0.13f, 0.11f, 0.03f, 0.86f));

                float x = mr.x + 12, y = mr.y + 10, iw = mr.width - 24;
                GUI.Label(new Rect(x, y, iw, 22), mine.Label, _label); y += 24;
                GUI.Label(new Rect(x, y, iw, 20), $"남은 매장량  {mine.Reserve}", _label); y += 20;
                GUI.Label(new Rect(x, y, iw, 20),
                    $"분류 대기  {mine.Stockpile.Count} / {GameConfig.MineStockpileCap}", _label); y += 20;
                GUI.Label(new Rect(x, y, iw, 20),
                    $"일꾼  수거 {mine.Miners}명 · 운반 {mine.Haulers}명", _label); y += 24;
                GUI.Label(new Rect(x, y, iw, 20),
                    $"[H] 수거 일꾼  {GameConfig.MinerHireCost:N0}원 (일당 {GameConfig.MinerWage})", _small); y += 19;
                GUI.Label(new Rect(x, y, iw, 20),
                    $"[J] 운반 일꾼  {GameConfig.HaulerHireCost:N0}원 (일당 {GameConfig.HaulerWage})", _small);
            }
        }

        // ==================================================================
        // 낮 간이 상점 (능력치만)
        // ==================================================================
        void DrawDayShop()
        {
            var s = _game.Player.Stats;

            float w = 460f, h = 330f;
            var r = new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.5f - h * 0.5f, w, h);
            Fill(new Rect(r.x - 3, r.y - 3, r.width + 6, r.height + 6), new Color(1f, 0.9f, 0.7f, 0.28f));
            Panel(r, new Color(0.08f, 0.09f, 0.12f, 0.96f));

            float x = r.x + 20, y = r.y + 16, iw = r.width - 40;

            GUI.Label(new Rect(x, y, iw, 26), "간이 강화 (낮)", _title);
            GUI.Label(new Rect(x + iw - 150, y + 4, 150, 22), $"보유금 {_game.Money:N0}원", _label);
            y += 30;
            GUI.Label(new Rect(x, y, iw, 20), "장비·펫·시설은 밤 상점에서 판다", _small);
            y += 26;

            DrawUpgradeRow(x, ref y, iw, StatKind.Health, s);
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
            _blockers.Add(new Rect(0, 0, Screen.width, Screen.height));

            float w = Mathf.Min(680f, Screen.width - 80f);
            float h = Mathf.Min(560f, Screen.height - 60f);
            var r = new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.5f - h * 0.5f, w, h);

            float x = r.x, y = r.y, iw = r.width;

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

            // 내일 예고
            int nextQuota = GameConfig.QuotaForDay(_game.Day + 1);
            GUI.Label(new Rect(x, y, iw, 22),
                $"내일({_game.Day + 1}일차) 할당량  <color=#ffd166><b>{nextQuota:N0}원</b></color>", _label);
            y += 30;

            // 탭
            string[] tabs = { "능력치", "장비", "펫", "시설" };
            float tw = 110f;
            for (int i = 0; i < tabs.Length; i++)
            {
                if (GUI.Button(new Rect(x + i * (tw + 6), y, tw, 30), tabs[i], i == _nightTab ? _tabOn : _tab))
                    _nightTab = i;
            }
            y += 38;

            float bodyH = r.yMax - y - 52f;
            Fill(new Rect(x, y, iw, bodyH), new Color(1f, 1f, 1f, 0.04f));

            switch (_nightTab)
            {
                case 0: DrawNightStats(x + 10, y + 8, iw - 20); break;
                case 1: DrawNightGear(x + 10, y + 8, iw - 20); break;
                case 2: DrawNightPets(x + 10, y + 8, iw - 20); break;
                case 3: DrawNightFacility(x + 10, y + 8, iw - 20); break;
            }

            // 다음 날
            if (GUI.Button(new Rect(x + iw - 210, r.yMax - 42, 210, 36),
                    $"다음 날 시작  (Enter)", _button))
                _game.StartNextDay();
        }

        void DrawNightStats(float x, float y, float w)
        {
            var s = _game.Player.Stats;
            DrawUpgradeRow(x, ref y, w, StatKind.Health, s);
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
                ? $"분류 설비를 가동하면 모든 판매가 x{GameConfig.RecycleBonus1:0.0}"
                : lv == 1
                    ? $"확장하면 판매가 x{GameConfig.RecycleBonus2:0.0} (현재 x{GameConfig.RecycleBonus1:0.0})"
                    : $"판매가 x{GameConfig.RecycleBonus2:0.0} 적용 중";

            int cost = lv == 0 ? GameConfig.RecycleStationCost : GameConfig.RecycleUpgradeCost;

            ShopRow(x, ref y, w, $"분리수거장 Lv.{lv}", desc,
                lv >= 2 ? null : $"{cost:N0}원",
                _game.Money >= cost, () => _game.TryBuyRecycleStation());

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

            float h = lines.Count * 19 + 10;
            var r = new Rect(MarginX, Screen.height - h - MarginY, 440, h);
            Panel(r, new Color(0f, 0f, 0f, 0.5f));

            for (int i = 0; i < lines.Count; i++)
                GUI.Label(new Rect(r.x + 8, r.y + 5 + i * 19, r.width - 16, 18), lines[i], _small);
        }

        void DrawHelpPanel()
        {
            // 하단 인벤토리 핫바와 겹치지 않게 그 위에 배치한다.
            var r = new Rect(Screen.width * 0.5f - 255, Screen.height - 306, 510, 184);
            Panel(r, new Color(0f, 0f, 0f, 0.58f));

            float y = r.y + 8;
            GUI.Label(new Rect(r.x + 14, y, r.width - 28, 22), "조작법  (Tab: 숨기기)", _label);
            y += 24;

            string[] lines =
            {
                "WASD — 이동 + 마지막 입력 방향의 동·서·남·북 한 칸 조준",
                "Space 길게 — 표시된 인접 한 칸을 판다",
                "E — 왼쪽 분리수거장에 투입 / 오른쪽 처리장에서 강화",
                "Q 탐지기 · B 귀환 벨 · F 드링크 (밤 상점에서 구입)",
                "쓰레기는 지상 분리수거장 앞에서 E를 눌러야 판매된다",
                "해가 지기 전에 꼭 지상으로! 지하에 있으면 가방을 몰수당한다",
                "Shift+R — 새 게임"
            };
            foreach (var l in lines)
            {
                GUI.Label(new Rect(r.x + 14, y, r.width - 28, 19), l, _small);
                y += 19;
            }
        }

        // ==================================================================
        // 화면 하단: 마인크래프트식 9칸 인벤토리 핫바
        // ==================================================================
        void DrawInventoryHotbar()
        {
            var player = _game.Player;
            if (player == null) return;

            float slotsW = HotbarSlotCount * HotbarSlotSize + (HotbarSlotCount - 1) * HotbarSlotGap;
            float startX = Mathf.Round((Screen.width - slotsW) * 0.5f);
            float slotY = Screen.height - HotbarSlotSize - 18f;
            var frame = new Rect(startX - 9f, slotY - 27f, slotsW + 18f, HotbarSlotSize + 36f);

            Fill(frame, new Color(0.035f, 0.035f, 0.045f, 0.90f));
            Fill(new Rect(frame.x, frame.y, frame.width, 3f), new Color(0.72f, 0.72f, 0.76f, 0.95f));
            Fill(new Rect(frame.x, frame.yMax - 3f, frame.width, 3f), new Color(0.08f, 0.08f, 0.10f, 1f));
            _blockers.Add(frame);

            int itemCount = player.Inventory.Count;
            int firstItem = Mathf.Max(0, itemCount - HotbarSlotCount);
            int hidden = firstItem;
            string header = $"가방  {player.InventoryVolume} / {player.BagCapacity}";
            if (hidden > 0) header += $"   ·   앞쪽 {hidden}개 더 있음";
            GUI.Label(new Rect(frame.x + 8f, frame.y + 3f, frame.width - 16f, 20f), header, _small);

            LootItem? hovered = null;
            for (int slot = 0; slot < HotbarSlotCount; slot++)
            {
                float x = startX + slot * (HotbarSlotSize + HotbarSlotGap);
                var r = new Rect(x, slotY, HotbarSlotSize, HotbarSlotSize);
                DrawHotbarSlotFrame(r, slot == Mathf.Min(itemCount, HotbarSlotCount) - 1);

                int itemIndex = firstItem + slot;
                if (itemIndex >= itemCount)
                {
                    GUI.Label(new Rect(r.x + 3f, r.y + 2f, 14f, 13f), (slot + 1).ToString(), _micro);
                    continue;
                }

                LootItem item = player.Inventory[itemIndex];
                GUI.Label(new Rect(r.x + 3f, r.y + 2f, 14f, 13f), (slot + 1).ToString(), _micro);

                var icon = new Rect(r.x + 17f, r.y + 5f, 22f, 20f);
                Fill(new Rect(icon.x - 2f, icon.y - 2f, icon.width + 4f, icon.height + 4f),
                    new Color(0f, 0f, 0f, 0.62f));
                Sprite pixelArt = TrashSpriteCatalog.For(item);
                if (pixelArt != null)
                {
                    Fill(icon, new Color(item.Color.r, item.Color.g, item.Color.b, 0.28f));
                    Rect spriteRect = pixelArt.rect;
                    Texture2D texture = pixelArt.texture;
                    var uv = new Rect(
                        spriteRect.x / texture.width,
                        spriteRect.y / texture.height,
                        spriteRect.width / texture.width,
                        spriteRect.height / texture.height);
                    GUI.DrawTextureWithTexCoords(icon, texture, uv, true);
                }
                else
                {
                    Fill(icon, item.Color);
                    GUI.Label(icon, MaterialGlyph(item.Material), _micro);
                }

                GUI.Label(new Rect(r.x + r.width - 19f, r.y + 2f, 17f, 13f), $"V{item.Volume}", _micro);
                GUI.Label(new Rect(r.x + 2f, r.y + 27f, r.width - 4f, 13f), ShortItemName(item.Name), _micro);
                GUI.Label(new Rect(r.x + 2f, r.y + 40f, r.width - 4f, 13f), CompactMoney(item.Value), _micro);

                if (r.Contains(Event.current.mousePosition)) hovered = item;
            }

            if (hovered.HasValue)
                DrawInventoryTooltip(hovered.Value, slotY);
        }

        void DrawHotbarSlotFrame(Rect r, bool newest)
        {
            Color border = newest
                ? new Color(1f, 0.90f, 0.38f, 1f)
                : new Color(0.70f, 0.70f, 0.74f, 1f);
            Fill(r, border);
            Fill(new Rect(r.x + 3f, r.y + 3f, r.width - 6f, r.height - 6f),
                new Color(0.12f, 0.12f, 0.15f, 0.96f));
            Fill(new Rect(r.x + 3f, r.y + 3f, r.width - 6f, 2f),
                new Color(0.30f, 0.30f, 0.34f, 1f));
        }

        void DrawInventoryTooltip(LootItem item, float slotY)
        {
            float w = 310f, h = 48f;
            var r = new Rect(
                Mathf.Clamp(Event.current.mousePosition.x - w * 0.5f, 8f, Screen.width - w - 8f),
                slotY - h - 10f,
                w,
                h);
            Fill(r, new Color(0.025f, 0.025f, 0.04f, 0.96f));
            Fill(new Rect(r.x, r.y, 3f, r.height), item.Color);
            GUI.Label(new Rect(r.x + 11f, r.y + 4f, r.width - 20f, 20f), $"<b>{item.Name}</b>", _label);
            GUI.Label(new Rect(r.x + 11f, r.y + 24f, r.width - 20f, 18f),
                $"{MaterialInfo.DisplayName(item.Material)} · {item.Value:N0}원 · 부피 {item.Volume}", _small);
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
            _blockers.Add(new Rect(0, 0, Screen.width, Screen.height));

            bool won = _game.State == GameState.Won;
            string title = won ? "<color=#8ce89a>완주!</color>" : "<color=#ff7b7b>해고당했다</color>";

            float y = Screen.height * 0.34f;
            GUI.Label(new Rect(0, y, Screen.width, 40), title, _center);
            GUI.Label(new Rect(0, y + 46, Screen.width, 30), _game.ResultReason, _center);
            GUI.Label(new Rect(0, y + 86, Screen.width, 30),
                $"{_game.Day}일차 · 최종 보유금 {_game.Money:N0}원", _center);
            GUI.Label(new Rect(0, y + 132, Screen.width, 30), "[R] 다시 시작", _center);
        }

        void Fill(Rect r, Color c)
        {
            var old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, _white);
            GUI.color = old;
        }
    }
}
