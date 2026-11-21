using Blackstone.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blackstone.EditorTools
{
    /// <summary>
    /// Correção da Build 004.
    /// Menu: Blackstone > Build 004 > Ajustar foco e colchão
    /// - Aplica as novas configurações de mira no Interactor do jogador.
    /// - Deixa o colchão mais grosso, para ser fácil de mirar.
    /// </summary>
    public static class Build004FocusFix
    {
        [MenuItem("Blackstone/Build 004/Ajustar foco e colchão")]
        public static void Fix()
        {
            var player = GameObject.Find("Player");
            var interactor = player != null ? player.GetComponent<Interactor>() : null;
            if (interactor == null)
            {
                EditorUtility.DisplayDialog("Blackstone", "Não encontrei o Interactor no Player.", "OK");
                return;
            }

            var so = new SerializedObject(interactor);
            so.FindProperty("range").floatValue = 2.5f;
            so.FindProperty("aimAssistRadius").floatValue = 0.12f;
            so.FindProperty("highlightTarget").boolValue = true;
            so.FindProperty("highlightAmount").floatValue = 0.35f;
            so.ApplyModifiedProperties();

            var mattress = GameObject.Find("Cela_Graybox/Mobilia/Colchao");
            if (mattress != null)
            {
                Undo.RecordObject(mattress.transform, "Ajustar colchão");
                // Cama vai até 0,45 m de altura; colchão de 18 cm por cima.
                mattress.transform.localPosition = new Vector3(mattress.transform.localPosition.x, 0.54f, mattress.transform.localPosition.z);
                mattress.transform.localScale = new Vector3(0.78f, 0.18f, 1.85f);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Blackstone] Build 004: foco e colchão ajustados. Salve a cena (Ctrl+S) e aperte Play.");
        }
    }
}
