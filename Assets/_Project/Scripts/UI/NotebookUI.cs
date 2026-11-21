using Blackstone.Knowledge;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Blackstone.UI
{
    /// <summary>
    /// Tela provisória (graybox, OnGUI) do caderno de anotações
    /// e o aviso de "nova anotação". Será trocada na fase de arte.
    ///
    /// Rolagem: roda do mouse, W/S, setas ou Page Up/Page Down.
    /// As anotações mais recentes aparecem primeiro.
    /// </summary>
    [RequireComponent(typeof(KnowledgeLog))]
    public class NotebookUI : MonoBehaviour
    {
        private const float MinScale = 0.85f;     // tamanho mínimo legível
        private const float KeyScrollSpeed = 700f; // px/s (em escala 1080p)
        private const float WheelStep = 80f;       // px por "clique" da roda

        private KnowledgeLog log;
        private float scrollY;
        private float maxScroll;
        private float viewHeight;
        private int lastEntryCount;
        private GUIStyle titleStyle, entryTitleStyle, entryTextStyle, hintStyle, noticeStyle;

        private void Awake()
        {
            log = GetComponent<KnowledgeLog>();
        }

        private void Update()
        {
            if (log == null || !log.IsOpen) return;

            // Nova anotação desde a última vez: volta para o topo (onde ela aparece).
            if (log.Entries.Count != lastEntryCount)
            {
                lastEntryCount = log.Entries.Count;
                scrollY = 0f;
            }

            float s = Scale;
            float delta = 0f;

            var mouse = Mouse.current;
            if (mouse != null)
            {
                float wheel = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(wheel) > 0.01f)
                {
                    delta -= Mathf.Sign(wheel) * WheelStep * s;
                }
            }

            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) delta += KeyScrollSpeed * s * Time.unscaledDeltaTime;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) delta -= KeyScrollSpeed * s * Time.unscaledDeltaTime;
                if (kb.pageDownKey.wasPressedThisFrame) delta += viewHeight * 0.9f;
                if (kb.pageUpKey.wasPressedThisFrame) delta -= viewHeight * 0.9f;
                if (kb.homeKey.wasPressedThisFrame) scrollY = 0f;
                if (kb.endKey.wasPressedThisFrame) scrollY = maxScroll;
            }

            scrollY = Mathf.Clamp(scrollY + delta, 0f, maxScroll);
        }

        private static float Scale => Mathf.Max(Screen.height / 1080f, MinScale);

        private void OnGUI()
        {
            if (log == null) return;
            EnsureStyles();
            float s = Scale;

            if (log.IsOpen)
            {
                DrawNotebook(s);
            }
            else if (log.NewEntryTimeLeft > 0f)
            {
                DrawNotice(s);
            }

            GUI.color = Color.white;
        }

        private void DrawNotebook(float s)
        {
            // Escurece o fundo
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

            float w = Mathf.Min(950f * s, Screen.width * 0.9f);
            float h = Screen.height * 0.82f;
            var panel = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

            GUI.color = new Color(0.93f, 0.90f, 0.82f, 0.97f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = Color.white;

            titleStyle.fontSize = Mathf.RoundToInt(34 * s);
            entryTitleStyle.fontSize = Mathf.RoundToInt(23 * s);
            entryTextStyle.fontSize = Mathf.RoundToInt(20 * s);
            hintStyle.fontSize = Mathf.RoundToInt(16 * s);

            float pad = 30f * s;
            float titleH = 50f * s;
            float hintH = 30f * s;

            GUI.Label(new Rect(panel.x + pad, panel.y + pad * 0.6f, panel.width - pad * 2f, titleH), "Caderno", titleStyle);

            // Área rolável
            var view = new Rect(panel.x + pad, panel.y + pad * 0.6f + titleH + 8f * s,
                panel.width - pad * 2f, panel.height - titleH - hintH - pad * 1.6f - 8f * s);
            viewHeight = view.height;

            float scrollbarW = 14f * s;
            float textW = view.width - scrollbarW - 12f * s;
            float gap = 18f * s;

            // Mede a altura total do conteúdo
            float contentH = 0f;
            if (log.Entries.Count == 0)
            {
                contentH = entryTextStyle.CalcHeight(new GUIContent("Nada anotado ainda."), textW);
            }
            else
            {
                for (int i = log.Entries.Count - 1; i >= 0; i--)
                {
                    var e = log.Entries[i];
                    contentH += entryTitleStyle.CalcHeight(new GUIContent(e.title), textW);
                    contentH += entryTextStyle.CalcHeight(new GUIContent(e.text), textW) + gap;
                }
            }
            maxScroll = Mathf.Max(0f, contentH - view.height);
            scrollY = Mathf.Clamp(scrollY, 0f, maxScroll);

            // Desenha com recorte
            GUI.BeginGroup(view);
            float y = -scrollY;
            if (log.Entries.Count == 0)
            {
                GUI.Label(new Rect(0, y, textW, contentH), "Nada anotado ainda.", entryTextStyle);
            }
            else
            {
                for (int i = log.Entries.Count - 1; i >= 0; i--)
                {
                    var e = log.Entries[i];
                    float th = entryTitleStyle.CalcHeight(new GUIContent(e.title), textW);
                    float bh = entryTextStyle.CalcHeight(new GUIContent(e.text), textW);
                    if (y + th + bh > 0f && y < view.height)
                    {
                        GUI.Label(new Rect(0, y, textW, th), e.title, entryTitleStyle);
                        GUI.Label(new Rect(0, y + th, textW, bh), e.text, entryTextStyle);
                    }
                    y += th + bh + gap;
                }
            }
            GUI.EndGroup();

            // Barra de rolagem (indicador)
            if (maxScroll > 0f)
            {
                var track = new Rect(view.xMax - scrollbarW, view.y, scrollbarW, view.height);
                GUI.color = new Color(0f, 0f, 0f, 0.12f);
                GUI.DrawTexture(track, Texture2D.whiteTexture);

                float thumbH = Mathf.Max(30f * s, track.height * (view.height / contentH));
                float thumbY = track.y + (track.height - thumbH) * (scrollY / maxScroll);
                GUI.color = new Color(0.25f, 0.22f, 0.2f, 0.6f);
                GUI.DrawTexture(new Rect(track.x, thumbY, track.width, thumbH), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            string hint = maxScroll > 0f ? "Roda do mouse / W S para rolar     [Tab] fechar" : "[Tab] fechar";
            GUI.Label(new Rect(panel.x + pad, panel.yMax - hintH - pad * 0.5f, panel.width - pad * 2f, hintH), hint, hintStyle);
        }

        private void DrawNotice(float s)
        {
            float alpha = Mathf.Clamp01(log.NewEntryTimeLeft / 0.5f);
            noticeStyle.fontSize = Mathf.RoundToInt(20 * s);
            var rect = new Rect(30f * s, 30f * s, 520f * s, 44f * s);

            GUI.color = new Color(0f, 0f, 0f, 0.55f * alpha);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.Label(new Rect(rect.x + 14f * s, rect.y, rect.width - 28f * s, rect.height),
                "Nova anotação no caderno  [Tab]", noticeStyle);
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;

            var ink = new Color(0.15f, 0.13f, 0.12f);
            titleStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            titleStyle.normal.textColor = ink;
            entryTitleStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, wordWrap = true };
            entryTitleStyle.normal.textColor = ink;
            entryTextStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
            entryTextStyle.normal.textColor = ink;
            hintStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
            hintStyle.normal.textColor = new Color(0.35f, 0.33f, 0.3f);
            noticeStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            noticeStyle.normal.textColor = Color.white;
        }
    }
}
