using UnityEngine;

namespace Blackstone.Core
{
    /// <summary>
    /// Bloqueia o controle do jogador (andar, olhar, interagir) enquanto
    /// alguma tela estiver aberta, como o caderno. Cada tela chama Push ao abrir
    /// e Pop ao fechar.
    /// </summary>
    public static class InputLock
    {
        private static int count;

        public static bool Blocked => count > 0;

        public static void Push() => count++;

        public static void Pop() => count = Mathf.Max(0, count - 1);

        // Garante que o contador zere a cada Play, mesmo sem recarregar o domínio.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay() => count = 0;
    }
}
