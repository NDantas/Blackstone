using Blackstone.Interaction;
using Blackstone.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blackstone.EditorTools
{
    /// <summary>
    /// Ferramenta de Editor da Build 002.
    /// Menu: Blackstone > Build 002 > Adicionar interações
    /// Adiciona o Interactor e o HUD ao jogador e torna examináveis
    /// os objetos da cela criada na Build 001. Pode ser rodada de novo.
    /// </summary>
    public static class Build002InteractionSetup
    {
        [MenuItem("Blackstone/Build 002/Adicionar interações")]
        public static void Setup()
        {
            var player = GameObject.Find("Player");
            if (player == null || GameObject.Find("Cela_Graybox") == null)
            {
                EditorUtility.DisplayDialog("Blackstone",
                    "Não encontrei o Player ou a Cela_Graybox na cena. Rode antes: Blackstone > Build 001 > Criar cela graybox.",
                    "OK");
                return;
            }

            // Jogador: Interactor + HUD
            var interactor = player.GetComponent<Interactor>();
            if (interactor == null) interactor = Undo.AddComponent<Interactor>(player);
            if (player.GetComponent<InteractionHUD>() == null) Undo.AddComponent<InteractionHUD>(player);

            if (Camera.main != null)
            {
                var so = new SerializedObject(interactor);
                so.FindProperty("cameraTransform").objectReferenceValue = Camera.main.transform;
                so.ApplyModifiedProperties();
            }

            // Objetos examináveis (textos provisórios, revisar no Game Design)
            int count = 0;
            count += Inspectable("Cela_Graybox/Mobilia/Cama", "Cama",
                "Estrutura de metal parafusada no chão.",
                "Os parafusos estão firmes. Quase todos.");
            count += Inspectable("Cela_Graybox/Mobilia/Colchao", "Colchão",
                "Colchão fino. Cheira a mofo.",
                "Num dos cantos, a costura foi refeita à mão.");
            count += Inspectable("Cela_Graybox/Mobilia/Pia", "Pia",
                "Pia de aço inox. A torneira pinga sem parar.");
            count += Inspectable("Cela_Graybox/Mobilia/Vaso", "Vaso sanitário",
                "Aço inox, sem tampa. Nada de útil aqui.");
            count += Inspectable("Cela_Graybox/Mobilia/Luminaria", "Luminária",
                "Lâmpada protegida por uma tela de metal. Ela zumbe baixinho.");
            count += Inspectable("Cela_Graybox/Grade", "Grade",
                "Barras de aço grossas. Nenhuma se mexe.",
                "A tranca fica do lado de fora, fora do alcance da mão.");
            count += Inspectable("Cela_Graybox/Estrutura/Parede_Fundo", "Parede",
                "Concreto frio e úmido.",
                "Alguém riscou marcas aqui. Grupos de cinco. Muitos grupos.");

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[Blackstone] Build 002: interações configuradas ({count} objetos). Salve a cena (Ctrl+S) e aperte Play.");
        }

        private static int Inspectable(string path, string displayName, params string[] lines)
        {
            var go = GameObject.Find(path);
            if (go == null)
            {
                Debug.LogWarning($"[Blackstone] Objeto não encontrado: {path}");
                return 0;
            }

            var inspectable = go.GetComponent<InspectableObject>();
            if (inspectable == null) inspectable = Undo.AddComponent<InspectableObject>(go);
            else Undo.RecordObject(inspectable, "Configurar objeto examinável");

            inspectable.Configure(displayName, lines);
            EditorUtility.SetDirty(inspectable);
            return 1;
        }
    }
}
