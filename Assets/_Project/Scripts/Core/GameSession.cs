using UnityEngine;
using UnityEngine.InputSystem;

namespace Blackstone.Core
{
    /// <summary>
    /// Controle geral da sessão de jogo. ESC sai do jogo
    /// (a menos que alguma tela, como o caderno, esteja aberta: aí o ESC fecha a tela).
    /// No Editor, ESC encerra o modo Play; na build, fecha o executável.
    /// </summary>
    public class GameSession : MonoBehaviour
    {
        private void Update()
        {
            if (InputLock.Blocked) return;

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Quit();
            }
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
