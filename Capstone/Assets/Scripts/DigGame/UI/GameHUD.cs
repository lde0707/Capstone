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
        /// <summary>우측 상단 패널 높이. 아래에 붙는 패널들이 이 값을 기준으로 내려간다.</summary>
        const float TopRightHeight = 178f;
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
        bool _showCodex;
        int _codexTab;   // 유물 도감 분류 탭

        public void Init(GameManager game)
        {
            _game = game;
            if (game != null) game.Hud = this;
        }

        /// <summary>패널 배경. 반투명 사각형 한 장.</summary>
        void Panel(Rect r, Color c) => Fill(r, c);

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
            if (kb == null) return;
            if (kb.tabKey.wasPressedThisFrame) _showHelp = !_showHelp;
            if (kb.cKey.wasPressedThisFrame) _showCodex = !_showCodex;
            if (kb.escapeKey.wasPressedThisFrame) _showCodex = false;
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

            switch (_game.State)
            {
                case GameState.Day:
                    DrawStatsPanel();
                    DrawTopRight();
                    DrawContext();
                    DrawLogPanel();
                    if (_showHelp) DrawHelpPanel();
                    DrawRelicStatus();
                    if (_game.ShopOpen) DrawDayShop();
                    DrawInventoryHotbar();
                    if (_showCodex) DrawCodex();
                    break;

                case GameState.Night:
                    DrawNightScreen();
                    if (_showCodex) DrawCodex();
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
            var r = new Rect(Screen.width - w - MarginX, MarginY, w, TopRightHeight);
            Panel(r, new Color(0f, 0f, 0f, 0.58f));

            bool warn = _game.TimeLeft < GameConfig.DayEndWarnSeconds;

            float x = r.x + 12, y = r.y + 10, iw = r.width - 24;

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

        /// <summary>사각형 테두리 1px.</summary>
        void Outline(Rect r, Color c)
        {
            Fill(new Rect(r.x, r.y, r.width, 1f), c);
            Fill(new Rect(r.x, r.y + r.height - 1f, r.width, 1f), c);
            Fill(new Rect(r.x, r.y, 1f, r.height), c);
            Fill(new Rect(r.x + r.width - 1f, r.y, 1f, r.height), c);
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

            DigFeature feature = p.TargetDigFeature;
            if (!_game.ShopOpen && feature != DigFeature.None)
            {
                Color featureColor = DigFeatureInfo.ColorOf(feature);
                var fr = new Rect(Screen.width * 0.5f - 210, 64, 420, 34);
                Fill(fr, new Color(featureColor.r * 0.25f, featureColor.g * 0.25f,
                    featureColor.b * 0.25f, 0.92f));
                GUI.Label(new Rect(fr.x + 12, fr.y + 6, fr.width - 24, 23),
                    $"<b>{DigFeatureInfo.DisplayName(feature)}</b> — {DigFeatureInfo.Description(feature)}", _label);
            }

            int digSpeedBonus = p.TargetDigSpeedBonusPercent;
            if (!_game.ShopOpen && digSpeedBonus > 0)
            {
                float y = feature != DigFeature.None ? 102f : 64f;
                var sr = new Rect(Screen.width * 0.5f - 145, y, 290, 30);
                Fill(sr, new Color(0.18f, 0.12f, 0.05f, 0.90f));
                GUI.Label(new Rect(sr.x + 12, sr.y + 4, sr.width - 24, 22),
                    $"<b>힘 우위</b> — 굴착 속도 +{digSpeedBonus}%", _label);
            }

            // 시설이 붙어 있어 사정거리가 겹친다 — 안내는 HandleInteract 의 우선순위와
            // 똑같은 순서로 골라야 "E 눌렀더니 딴 게 열리는" 일이 없다.
            var relics = _game.Relics;
            if (!_game.ShopOpen)
            {
                bool nearRestorer = _game.PlayerNearRestorer();
                bool nearRecycle = _game.PlayerNearRecycle();
                bool nearShop = _game.PlayerNearBuilding();

                string text = null;
                Color tint = default;

                if (nearRestorer && relics != null && relics.CanDepositNow)
                {
                    text = $"[E] 유물 {relics.CarriedCount}개 복원 맡기기 · 비용 {relics.CarriedRestoreCost:N0}원";
                    tint = new Color(0.16f, 0.08f, 0.26f, 0.90f);
                }
                else if (nearRecycle && p.Inventory.Count > 0)
                {
                    text = $"[E] {p.Inventory.Count}개 재활용 · 예상 {_game.EstimateRecycledValue(p.Inventory):N0}원";
                    tint = new Color(0.06f, 0.22f, 0.10f, 0.88f);
                }
                else if (nearShop)
                {
                    text = "[E] 강화 (간이 상점)";
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
                    text = "투입소 · 가방이 비어 있음";
                    tint = new Color(0.06f, 0.22f, 0.10f, 0.88f);
                }

                if (text != null)
                {
                    var hr = new Rect(Screen.width * 0.5f - 210, 26, 420, 30);
                    Fill(hr, tint);
                    GUI.Label(new Rect(hr.x + 14, hr.y + 5, hr.width - 20, 22), text, _label);
                }
            }

            var mine = p.MineHere();
            if (mine != null)
            {
                float w = 330f;
                // 정보 표시일 뿐 버튼이 없으므로 클릭 차단 영역으로 등록하지 않는다.
                var mr = new Rect(Screen.width - w - MarginX,
                    MarginY + TopRightHeight + 8f + RelicPanelHeight, w, 224);
                Fill(mr, new Color(0.13f, 0.11f, 0.03f, 0.86f));

                float x = mr.x + 12, y = mr.y + 10, iw = mr.width - 24;
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
                return (r.QueueCount > 0 ? 82f : 56f) + 8f;
            }
        }

        void DrawRelicStatus()
        {
            var r = _game.Relics;
            if (r == null) return;
            if (RelicPanelHeight <= 0f) return;

            float w = 330f;
            float h = RelicPanelHeight - 8f;
            var panel = new Rect(Screen.width - w - MarginX,
                MarginY + TopRightHeight + 8f, w, h);
            Fill(panel, new Color(0.12f, 0.06f, 0.20f, 0.86f));

            float x = panel.x + 12, y = panel.y + 8, iw = panel.width - 24;
            GUI.Label(new Rect(x, y, iw, 22),
                $"<color=#dcaaff><b>유물 도감</b></color>  복원 {r.RestoredCount}/{r.TotalCount}" +
                $" · 발굴 {r.FoundCount}", _label);
            y += 22;
            GUI.Label(new Rect(x, y, iw, 20),
                $"등에 진 유물 {r.CarriedCount}/{GameConfig.RelicCarryLimit} · [C] 도감", _small);
            y += 22;

            if (r.QueueCount > 0)
            {
                var def = RelicCatalog.Get(r.Queue[0].RelicId);
                var bar = new Rect(x, y, iw, 16);
                Fill(bar, new Color(1f, 1f, 1f, 0.12f));
                Fill(new Rect(bar.x, bar.y, bar.width * r.FrontProgress01, bar.height),
                    new Color32(186, 122, 240, 255));
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

            float w = Cols * CellW + (Cols - 1) * Gap + 40f;
            float h = 152f + rows * (CellH + Gap) + 46f;
            var panel = new Rect(Screen.width * 0.5f - w * 0.5f,
                Mathf.Max(20f, Screen.height * 0.5f - h * 0.5f), w, h);
            Panel(panel, new Color(0.07f, 0.06f, 0.11f, 0.97f));
            Outline(panel, new Color(0.68f, 0.52f, 0.92f, 0.65f));

            float x = panel.x + 20f, y = panel.y + 14f, iw = panel.width - 40f;

            GUI.Label(new Rect(x, y, iw, 30),
                $"<b>유물 도감</b>   복원 <color=#c79bff>{sys.RestoredCount}</color> / {sys.TotalCount}" +
                $"   ·   발굴 {sys.FoundCount} / {sys.TotalCount}", _title);
            if (GUI.Button(new Rect(panel.xMax - 108f, y - 2f, 88f, 28f), "닫기 (C)", _buttonSmall))
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
                "땅속 <color=#dcaaff>보랏빛 광점</color>이 유물이다 · 캐서 복원소(스폰 왼쪽 끝 시설) 앞에서 E",
                _small);
        }

        void DrawCodexCell(Rect cell, RelicDef def, RelicSystem sys)
        {
            bool restored = sys.IsRestored(def.Id);
            bool found = sys.IsFound(def.Id);
            bool pending = sys.IsPending(def.Id);

            Fill(cell, restored ? new Color(0.17f, 0.13f, 0.24f, 1f)
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

            float h = lines.Count * 19 + 10;
            var r = new Rect(MarginX, Screen.height - h - MarginY, 440, h);
            Panel(r, new Color(0f, 0f, 0f, 0.5f));

            for (int i = 0; i < lines.Count; i++)
                GUI.Label(new Rect(r.x + 8, r.y + 5 + i * 19, r.width - 16, 18), lines[i], _small);
        }

        void DrawHelpPanel()
        {
            // 하단 인벤토리 핫바와 겹치지 않게 그 위에 배치한다.
            var r = new Rect(Screen.width * 0.5f - 255, Screen.height - 429, 510, 305);
            Panel(r, new Color(0f, 0f, 0f, 0.58f));

            float y = r.y + 8;
            GUI.Label(new Rect(r.x + 14, y, r.width - 28, 22), "조작법  (Tab: 숨기기)", _label);
            y += 24;

            string[] lines =
            {
                "WASD — 이동 + 마지막 입력 방향의 동·서·남·북 한 칸 조준",
                "Space 길게 — 표시된 인접 한 칸을 판다",
                "E — 스폰 왼쪽에 투입소·강화소·복원소가 나란히 있다",
                "Q 탐지기 · B 귀환 벨 · F 드링크 (밤 상점에서 구입)",
                "쓰레기는 지상 분리수거장 앞에서 E를 눌러야 판매된다",
                "파랑: 푸석한 흙 · 금색: 고가품이 든 압축 폐기물층",
                "연두: 매립 가스층 — 방호복이 있으면 피해를 막는다",
                "힘이 땅의 요구치보다 높으면 단계마다 굴착 속도 +35%",
                "초록 잔디 지반은 파괴 불가 · 스폰 바로 오른쪽 통로로 지하에 들어간다",
                "도시광산 E: 보관/회수 · H 20초 자동 재활용 · J 대량 운반",
                "보랏빛 광점 — 유물. 캐서 복원소(세 시설 중 가장 왼쪽) 앞에서 E",
                "C — 유물 도감 (한 분류를 다 모으면 판매액 세트 보너스)",
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
