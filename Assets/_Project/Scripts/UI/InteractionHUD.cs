using Blackstone.Interaction;
using UnityEngine;

namespace Blackstone.UI
{
    /// <summary>
    /// HUD provisório (graybox) feito com OnGUI:
    /// - ponto de mira no centro, que cresce ao mirar algo interativo;
    /// - nome do objeto e a ação ("[E] Examinar");
    /// - mensagem na parte de baixo da tela, que some sozinha.
    /// Será trocado por uma UI definitiva na fase de arte.
    /// </summary>
    [RequireComponent(typeof(Interactor))]
    public class InteractionHUD : MonoBehaviour
    {
        private Interactor interactor;
        private GUIStyle nameStyle;
        private GUIStyle promptStyle;
        private GUIStyle messageStyle;

        private void Awake()
        {
            interactor = GetComponent<Interactor>();
        }

        private void OnGUI()
        {
            if (interactor == null) return;
            EnsureStyles();

            float scale = Screen.height / 1080f;
            float cx = Screen.width / 2f;
            float cy = Screen.height / 2f;
            var target = interactor.Current;
            bool hasTarget = target != null;

            // Ponto de mira
            float size = (hasTarget ? 8f : 4f) * scale;
            GUI.color = hasTarget ? new Color(1f, 1f, 1f, 0.95f) : new Color(1f, 1f, 1f, 0.45f);
            GUI.DrawTexture(new Rect(cx - size / 2f, cy - size / 2f, size, size), Texture2D.whiteTexture);

            // Nome + ação
            if (hasTarget)
            {
                nameStyle.fontSize = Mathf.RoundToInt(24 * scale);
                promptStyle.fontSize = Mathf.RoundToInt(18 * scale);
                float w = 600f * scale;
                ShadowLabel(new Rect(cx - w / 2f, cy + 24f * scale, w, 34f * scale), target.DisplayName, nameStyle);
                ShadowLabel(new Rect(cx - w / 2f, cy + 56f * scale, w, 28f * scale), $"[E] {target.Prompt}", promptStyle);
            }

            // Mensagem
            if (interactor.MessageTimeLeft > 0f && !string.IsNullOrEmpty(interactor.Message))
            {
                float alpha = Mathf.Clamp01(interactor.MessageTimeLeft / 0.5f);
                messageStyle.fontSize = Mathf.RoundToInt(22 * scale);
                float w = Mathf.Min(900f * scale, Screen.width * 0.9f);
                float h = 90f * scale;
                var box = new Rect(cx - w / 2f, Screen.height - h - 60f * scale, w, h);

                GUI.color = new Color(0f, 0f, 0f, 0.6f * alpha);
                GUI.DrawTexture(box, Texture2D.whiteTexture);
                GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.Label(new Rect(box.x + 20f * scale, box.y, box.width - 40f * scale, box.height), interactor.Message, messageStyle);
            }

            GUI.color = Color.white;
        }

        private void EnsureStyles()
        {
            if (nameStyle != null) return;

            nameStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            promptStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            messageStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            nameStyle.normal.textColor = Color.white;
            promptStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
            messageStyle.normal.textColor = Color.white;
        }

        private static void ShadowLabel(Rect rect, string text, GUIStyle style)
        {
            Color original = style.normal.textColor;
            style.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
            GUI.Label(new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height), text, style);
            style.normal.textColor = original;
            GUI.Label(rect, text, style);
        }
    }
}
