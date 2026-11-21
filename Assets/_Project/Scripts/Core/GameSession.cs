using UnityEngine;
using UnityEngine.InputSystem;

namespace Blackstone.Core
{
    /// <summary>
    /// Controle geral da sessão de jogo. Na Build 001: ESC sai do jogo.
    /// No Editor, ESC encerra o modo Play; na build, fecha o executável.
    /// </summary>
    public class GameSession : MonoBehaviour
    {
        private void Update()
        {
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
