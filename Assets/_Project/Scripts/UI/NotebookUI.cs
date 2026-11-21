using Blackstone.Knowledge;
using UnityEngine;

namespace Blackstone.UI
{
    /// <summary>
    /// Tela provisória (graybox, OnGUI) do caderno de anotações
    /// e o aviso de "nova anotação". Será trocada na fase de arte.
    /// </summary>
    [RequireComponent(typeof(KnowledgeLog))]
    public class NotebookUI : MonoBehaviour
    {
        private KnowledgeLog log;
        private Vector2 scroll;
        private GUIStyle titleStyle, entryTitleStyle, entryTextStyle, hintStyle, noticeStyle;

        private void Awake()
        {
            log = GetComponent<KnowledgeLog>();
        }

        private void OnGUI()
        {
            if (log == null) return;
            EnsureStyles();
            float s = Mathf.Max(Screen.height / 1080f, 0.85f); // tamanho mínimo legível

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

            float w = Mathf.Min(900f * s, Screen.width * 0.85f);
            float h = Screen.height * 0.75f;
            var panel = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

            // "Papel"
            GUI.color = new Color(0.93f, 0.90f, 0.82f, 0.97f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = Color.white;

            titleStyle.fontSize = Mathf.RoundToInt(34 * s);
            entryTitleStyle.fontSize = Mathf.RoundToInt(22 * s);
            entryTextStyle.fontSize = Mathf.RoundToInt(19 * s);
            hintStyle.fontSize = Mathf.RoundToInt(16 * s);

            float pad = 32f * s;
            GUILayout.BeginArea(new Rect(panel.x + pad, panel.y + pad, panel.width - pad * 2f, panel.height - pad * 2f));
            GUILayout.Label("Caderno", titleStyle);
            GUILayout.Space(12f * s);

            scroll = GUILayout.BeginScrollView(scroll);
            if (log.Entries.Count == 0)
            {
                GUILayout.Label("Nada anotado ainda.", entryTextStyle);
            }
            else
            {
                foreach (var entry in log.Entries)
                {
                    GUILayout.Label(entry.title, entryTitleStyle);
                    GUILayout.Label(entry.text, entryTextStyle);
                    GUILayout.Space(16f * s);
                }
            }
            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            GUILayout.Label("[Tab] fechar", hintStyle);
            GUILayout.EndArea();
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
            hintStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.LowerRight };
            hintStyle.normal.textColor = new Color(0.35f, 0.33f, 0.3f);
            noticeStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            noticeStyle.normal.textColor = Color.white;
        }
    }
}
