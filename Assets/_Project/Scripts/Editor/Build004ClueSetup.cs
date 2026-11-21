using Blackstone.Interaction;
using Blackstone.Knowledge;
using Blackstone.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blackstone.EditorTools
{
    /// <summary>
    /// Ferramenta de Editor da Build 004.
    /// Menu: Blackstone > Build 004 > Adicionar primeira pista
    /// - Adiciona o caderno (KnowledgeLog + NotebookUI) ao jogador.
    /// - Transforma o colchão num objeto com pista escondida (o bilhete).
    /// - Cria a grade de ventilação no teto.
    /// - Faz cama, parede, porta e duto gerarem anotações no caderno.
    /// Pode ser rodada de novo.
    /// </summary>
    public static class Build004ClueSetup
    {
        private const string MaterialsFolder = "Assets/_Project/Materials/Graybox";

        [MenuItem("Blackstone/Build 004/Adicionar primeira pista")]
        public static void Setup()
        {
            var player = GameObject.Find("Player");
            var cell = GameObject.Find("Cela_Graybox");
            if (player == null || cell == null)
            {
                EditorUtility.DisplayDialog("Blackstone",
                    "Não encontrei o Player ou a Cela_Graybox. Rode antes as ferramentas das Builds 001 a 003.", "OK");
                return;
            }

            // ---------- Caderno no jogador ----------
            if (player.GetComponent<KnowledgeLog>() == null) Undo.AddComponent<KnowledgeLog>(player);
            if (player.GetComponent<NotebookUI>() == null) Undo.AddComponent<NotebookUI>(player);

            // ---------- Colchão: a pista ----------
            var mattress = GameObject.Find("Cela_Graybox/Mobilia/Colchao");
            if (mattress != null)
            {
                var oldInspect = mattress.GetComponent<InspectableObject>();
                if (oldInspect != null) Undo.DestroyObjectImmediate(oldInspect);

                var clue = mattress.GetComponent<ClueObject>();
                if (clue == null) clue = Undo.AddComponent<ClueObject>(mattress);
                else Undo.RecordObject(clue, "Configurar pista");

                clue.Configure(
                    "Colchão",
                    new[]
                    {
                        "Colchão fino. Cheira a mofo.",
                        "Num dos cantos, a costura foi refeita à mão. Os pontos são mais novos que o resto."
                    },
                    "Abrir a costura",
                    "Dentro da costura, um papel dobrado várias vezes. Letra apressada:\n\"Ele passa quando a luz pisca. Nunca olha pra cima.\"",
                    "bilhete_colchao",
                    "Bilhete no colchão",
                    "Escondido na costura do colchão, deixado por quem esteve aqui antes:\n\"Ele passa quando a luz pisca. Nunca olha pra cima.\"\n\n" +
                    "\"Ele\" deve ser o guarda da ronda. Se a luz pisca antes de ele passar, dá para prever a ronda. " +
                    "E se ele nunca olha para cima, o que está no alto é um ponto cego.",
                    "A costura está aberta. Não há mais nada dentro.");
                EditorUtility.SetDirty(clue);
            }
            else
            {
                Debug.LogWarning("[Blackstone] Colchão não encontrado.");
            }

            // ---------- Grade de ventilação no teto ----------
            var structure = GameObject.Find("Cela_Graybox/Estrutura");
            GameObject vent = GameObject.Find("Cela_Graybox/Estrutura/Duto_Ventilacao");
            if (vent == null && structure != null)
            {
                vent = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Undo.RegisterCreatedObjectUndo(vent, "Criar duto");
                vent.name = "Duto_Ventilacao";
                vent.transform.SetParent(structure.transform, false);
                // Acima do vaso, perto do fundo da cela (teto em 2,8 m).
                vent.transform.localPosition = new Vector3(0.6f, 2.78f, -1.2f);
                vent.transform.localScale = new Vector3(0.5f, 0.04f, 0.4f);
                var metal = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsFolder}/GB_Metal.mat");
                if (metal != null) vent.GetComponent<MeshRenderer>().sharedMaterial = metal;
            }

            // ---------- Objetos que geram anotações ----------
            Inspectable("Cela_Graybox/Estrutura/Duto_Ventilacao", "Grade de ventilação",
                new[]
                {
                    "Uma grade de ventilação no teto. Sai um ar frio dela.",
                    "Presa por quatro parafusos de cabeça comum. Nada de parafusos especiais."
                },
                "duto_teto", "Duto no teto",
                "Grade de ventilação no teto, sobre o vaso. Quatro parafusos comuns. Com alguma ferramenta, talvez dê para soltar.");

            Inspectable("Cela_Graybox/Mobilia/Cama", "Cama",
                new[]
                {
                    "Estrutura de metal parafusada no chão.",
                    "Os parafusos estão firmes. Quase todos: um deles gira um pouco com a ponta dos dedos."
                },
                "parafuso_cama", "Parafuso frouxo",
                "Um dos parafusos da cama está frouxo. Com paciência, talvez saia.");

            Inspectable("Cela_Graybox/Estrutura/Parede_Fundo", "Parede",
                new[]
                {
                    "Concreto frio e úmido.",
                    "Alguém riscou marcas aqui. Grupos de cinco. Muitos grupos."
                },
                "marcas_parede", "Marcas na parede",
                "Alguém contou muitos dias nesta cela. Tempo de sobra para observar a rotina.");

            // A Grade agora tem a porta ao lado: a anotação fica na própria grade.
            Inspectable("Cela_Graybox/Grade", "Grade",
                new[]
                {
                    "Barras de aço grossas. Nenhuma se mexe.",
                    "A tranca da porta fica do lado de fora, fora do alcance da mão."
                },
                "tranca_fora", "Tranca do lado de fora",
                "A porta da cela só abre por fora. Sair por ela depende de alguém (ou algo) abrir.");

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Blackstone] Build 004: pista e caderno configurados. Salve a cena (Ctrl+S) e aperte Play.");
        }

        private static void Inspectable(string path, string displayName, string[] lines, string noteId, string noteTitle, string noteText)
        {
            var go = GameObject.Find(path);
            if (go == null)
            {
                Debug.LogWarning($"[Blackstone] Objeto não encontrado: {path}");
                return;
            }

            var inspectable = go.GetComponent<InspectableObject>();
            if (inspectable == null) inspectable = Undo.AddComponent<InspectableObject>(go);
            else Undo.RecordObject(inspectable, "Configurar objeto examinável");

            inspectable.Configure(displayName, lines);
            inspectable.SetNote(noteId, noteTitle, noteText);
            EditorUtility.SetDirty(inspectable);
        }
    }
}
