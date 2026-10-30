using UnityEngine;

namespace DigGame
{
    /// <summary>
    /// 쓰집소의 IMGUI 전용 픽셀 스킨. 모든 원본은 개별 Resources PNG이며
    /// 9-slice GUIStyle로 그려 화면 비율이 달라도 테두리가 최대한 유지된다.
    /// </summary>
    public sealed class PixelUISkin
    {
        public Font Font { get; }
        public GUIStyle PanelStyle { get; }
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

            Texture2D panel = LoadTexture("PixelUI/panel");
            Texture2D button = LoadTexture("PixelUI/button_normal");
            Texture2D buttonHighlight = LoadTexture("PixelUI/button_highlight");
            Texture2D slot = LoadTexture("PixelUI/slot_normal");
            Texture2D slotSelected = LoadTexture("PixelUI/slot_selected");
            Texture2D bar = LoadTexture("PixelUI/bar_frame");
            Texture2D speech = LoadTexture("PixelUI/speech_bubble");
            Texture2D card = LoadTexture("PixelUI/item_card");
            Texture2D badge = LoadTexture("PixelUI/quest_badge");

            PanelStyle = CreateStyle(panel, 24, 24, 20, 20);
            SlotStyle = CreateStyle(slot, 22, 22, 22, 22);
            SelectedSlotStyle = CreateStyle(slotSelected, 22, 22, 22, 22);
            BarStyle = CreateStyle(bar, 22, 22, 16, 16);
            SpeechBubbleStyle = CreateStyle(speech, 30, 30, 24, 34);
            ItemCardStyle = CreateStyle(card, 26, 26, 26, 26);
            QuestBadgeStyle = CreateStyle(badge, 20, 20, 20, 20);

            ButtonStyle = CreateButtonStyle(button, buttonHighlight, false);
            HighlightButtonStyle = CreateButtonStyle(buttonHighlight, buttonHighlight, true);
        }

        public static PixelUISkin Load()
        {
            var skin = new PixelUISkin();
            if (skin.PanelStyle.normal.background == null)
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
            var style = CreateStyle(selected ? highlight : normal, 30, 30, 24, 24);
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
        public void DrawSlot(Rect rect, bool selected = false, float alpha = 1f) =>
            Draw(rect, selected ? SelectedSlotStyle : SlotStyle, alpha);
        public void DrawBarFrame(Rect rect, float alpha = 1f) => Draw(rect, BarStyle, alpha);
        public void DrawSpeechBubble(Rect rect, float alpha = 1f) => Draw(rect, SpeechBubbleStyle, alpha);
        public void DrawItemCard(Rect rect, float alpha = 1f) => Draw(rect, ItemCardStyle, alpha);
        public void DrawQuestBadge(Rect rect, float alpha = 1f) => Draw(rect, QuestBadgeStyle, alpha);
    }
}
