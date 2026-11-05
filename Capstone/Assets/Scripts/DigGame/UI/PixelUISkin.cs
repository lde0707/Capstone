using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 쓰집소의 IMGUI 전용 픽셀 스킨. 일러스트형 UI는 Resources PNG를 사용하고,
    /// 패널과 버튼은 표시 크기에 맞는 얇은 픽셀 프레임을 생성해 사용한다.
    /// </summary>
    public sealed class PixelUISkin
    {
        public Font Font { get; }
        public GUIStyle PanelStyle { get; }
        public GUIStyle HudPanelStyle { get; }
        public GUIStyle ButtonStyle { get; }
        public GUIStyle HighlightButtonStyle { get; }
        public GUIStyle SlotStyle { get; }
        public GUIStyle SelectedSlotStyle { get; }
        public GUIStyle BarStyle { get; }
        public GUIStyle SpeechBubbleStyle { get; }
        public GUIStyle ItemCardStyle { get; }
        public GUIStyle QuestBadgeStyle { get; }

        PixelUISkin()
        {
            Font = Resources.Load<Font>("Fonts/Galmuri11");

            Texture2D panel = CreateFrameTexture(
                "DigGame_MenuPanel_Thin", 32,
                new Color32(25, 20, 43, 255), new Color32(11, 8, 19, 255),
                new Color32(88, 71, 110, 255), new Color32(198, 145, 47, 255), true);
            Texture2D hudPanel = CreateFrameTexture(
                "DigGame_HudPanel_Thin", 16,
                new Color32(23, 19, 38, 246), new Color32(10, 8, 17, 255),
                new Color32(77, 65, 96, 255), new Color32(0, 0, 0, 0), false);
            Texture2D button = CreateFrameTexture(
                "DigGame_Button_Thin", 16,
                new Color32(30, 25, 49, 255), new Color32(12, 9, 20, 255),
                new Color32(92, 75, 113, 255), new Color32(164, 112, 34, 255), false);
            Texture2D buttonHighlight = CreateFrameTexture(
                "DigGame_ButtonHighlight_Thin", 16,
                new Color32(190, 126, 25, 255), new Color32(77, 44, 11, 255),
                new Color32(247, 201, 79, 255), new Color32(255, 229, 135, 255), false);
            Texture2D bar = CreateFrameTexture(
                "DigGame_BarFrame_Thin", 16,
                new Color32(18, 15, 29, 255), new Color32(8, 7, 13, 255),
                new Color32(104, 84, 122, 255), new Color32(0, 0, 0, 0), false);
            Texture2D slot = LoadTexture("PixelUI/slot_normal");
            Texture2D slotSelected = LoadTexture("PixelUI/slot_selected");
            Texture2D speech = LoadTexture("PixelUI/speech_bubble");
            Texture2D card = LoadTexture("PixelUI/item_card");
            Texture2D badge = LoadTexture("PixelUI/quest_badge");

            PanelStyle = CreateStyle(panel, 6, 6, 6, 6);
            HudPanelStyle = CreateStyle(hudPanel, 4, 4, 4, 4);
            SlotStyle = CreateStyle(slot, 22, 22, 22, 22);
            SelectedSlotStyle = CreateStyle(slotSelected, 22, 22, 22, 22);
            BarStyle = CreateStyle(bar, 3, 3, 3, 3);
            SpeechBubbleStyle = CreateStyle(speech, 30, 30, 24, 34);
            ItemCardStyle = CreateStyle(card, 26, 26, 26, 26);
            QuestBadgeStyle = CreateStyle(badge, 20, 20, 20, 20);

            ButtonStyle = CreateButtonStyle(button, buttonHighlight, false);
            HighlightButtonStyle = CreateButtonStyle(buttonHighlight, buttonHighlight, true);
        }

        public static PixelUISkin Load()
        {
            var skin = new PixelUISkin();
            if (skin.PanelStyle.normal.background == null ||
                skin.HudPanelStyle.normal.background == null ||
                skin.ButtonStyle.normal.background == null ||
                skin.SlotStyle.normal.background == null ||
                skin.BarStyle.normal.background == null ||
                skin.SpeechBubbleStyle.normal.background == null ||
                skin.ItemCardStyle.normal.background == null)
            {
                Debug.LogWarning("[DigGame] PixelUI 스킨을 불러오지 못해 기본 IMGUI로 표시합니다.");
                return null;
            }
            return skin;
        }

        static Texture2D LoadTexture(string path)
        {
            Texture2D texture = Resources.Load<Texture2D>(path);
            if (texture == null) return null;
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.anisoLevel = 0;
            return texture;
        }

        static Texture2D CreateFrameTexture(string name, int size, Color32 body,
            Color32 outer, Color32 inner, Color32 accent, bool decoratedCorners)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0,
                hideFlags = HideFlags.HideAndDontSave,
            };

            Paint(texture, 0, 0, size, size, outer);
            Paint(texture, 2, 2, size - 4, size - 4, inner);
            Paint(texture, 3, 3, size - 6, size - 6, body);

            if (decoratedCorners)
            {
                const int arm = 5;
                Paint(texture, 2, 2, arm, 2, accent);
                Paint(texture, 2, 2, 2, arm, accent);
                Paint(texture, size - arm - 2, 2, arm, 2, accent);
                Paint(texture, size - 4, 2, 2, arm, accent);
                Paint(texture, 2, size - 4, arm, 2, accent);
                Paint(texture, 2, size - arm - 2, 2, arm, accent);
                Paint(texture, size - arm - 2, size - 4, arm, 2, accent);
                Paint(texture, size - 4, size - arm - 2, 2, arm, accent);
            }
            else if (accent.a > 0)
            {
                // 작은 버튼은 모서리 장식 대신 양 끝에 한 픽셀만 넣어 밀도를 낮춘다.
                texture.SetPixel(2, 2, accent);
                texture.SetPixel(size - 3, 2, accent);
                texture.SetPixel(2, size - 3, accent);
                texture.SetPixel(size - 3, size - 3, accent);
            }

            texture.Apply(false, true);
            return texture;
        }

        static void Paint(Texture2D texture, int x, int y, int width, int height, Color32 color)
        {
            for (int py = y; py < y + height; py++)
                for (int px = x; px < x + width; px++)
                    texture.SetPixel(px, py, color);
        }

        static GUIStyle CreateStyle(Texture2D texture, int left, int right, int top, int bottom)
        {
            var style = new GUIStyle
            {
                border = new RectOffset(left, right, top, bottom),
                overflow = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
            };
            style.normal.background = texture;
            return style;
        }

        GUIStyle CreateButtonStyle(Texture2D normal, Texture2D highlight, bool selected)
        {
            var style = CreateStyle(selected ? highlight : normal, 4, 4, 4, 4);
            style.hover.background = highlight;
            style.active.background = highlight;
            style.focused.background = highlight;
            style.onNormal.background = highlight;
            style.onHover.background = highlight;
            style.onActive.background = highlight;
            style.font = Font;
            style.alignment = TextAnchor.MiddleCenter;
            style.padding = new RectOffset(9, 9, 5, 5);

            Color cream = new Color32(246, 232, 196, 255);
            Color ink = new Color32(49, 35, 34, 255);
            style.normal.textColor = selected ? ink : cream;
            style.hover.textColor = ink;
            style.active.textColor = ink;
            style.focused.textColor = ink;
            style.onNormal.textColor = ink;
            style.onHover.textColor = ink;
            style.onActive.textColor = ink;
            return style;
        }

        static void Draw(Rect rect, GUIStyle style, float alpha = 1f)
        {
            if (style == null || style.normal.background == null) return;
            Color old = GUI.color;
            GUI.color = new Color(old.r, old.g, old.b, old.a * Mathf.Clamp01(alpha));
            GUI.Box(rect, GUIContent.none, style);
            GUI.color = old;
        }

        public void DrawPanel(Rect rect, float alpha = 1f) => Draw(rect, PanelStyle, alpha);
        public void DrawHudPanel(Rect rect, float alpha = 1f) => Draw(rect, HudPanelStyle, alpha);
        public void DrawSlot(Rect rect, bool selected = false, float alpha = 1f) =>
            Draw(rect, selected ? SelectedSlotStyle : SlotStyle, alpha);
        public void DrawBarFrame(Rect rect, float alpha = 1f) => Draw(rect, BarStyle, alpha);
        public void DrawSpeechBubble(Rect rect, float alpha = 1f) => Draw(rect, SpeechBubbleStyle, alpha);
        public void DrawItemCard(Rect rect, float alpha = 1f) => Draw(rect, ItemCardStyle, alpha);
        public void DrawQuestBadge(Rect rect, float alpha = 1f) => Draw(rect, QuestBadgeStyle, alpha);
    }
}
