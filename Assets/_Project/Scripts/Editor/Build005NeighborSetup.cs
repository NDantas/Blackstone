using System.Collections.Generic;
using Blackstone.Interaction;
using Blackstone.NPC;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blackstone.EditorTools
{
    /// <summary>
    /// Ferramenta de Editor da Build 005.
    /// Menu: Blackstone > Build 005 > Criar vizinho
    /// - Cria a cela do vizinho do outro lado do corredor, de frente para a do jogador.
    /// - Cria o vizinho (Bento) com conversa e comportamento simples.
    /// - Marca as barras como "vazadas" (a mira enxerga através delas).
    /// Pode ser rodada de novo (substitui o que criou antes).
    /// </summary>
    public static class Build005NeighborSetup
    {
        private const string GroupName = "Cela_Vizinho";
        private const string NpcName = "Bento";
        private const string MaterialsFolder = "Assets/_Project/Materials/Graybox";

        private const float Width = 2.5f;
        private const float Depth = 3.5f;
        private const float Height = 2.8f;
        private const float Wall = 0.2f;

        [MenuItem("Blackstone/Build 005/Criar vizinho")]
        public static void Setup()
        {
            var root = GameObject.Find("Cela_Graybox");
            if (root == null)
            {
                EditorUtility.DisplayDialog("Blackstone", "Não encontrei a Cela_Graybox. Rode as ferramentas das Builds 001 a 004.", "OK");
                return;
            }
            Transform r = root.transform;

            var oldGroup = r.Find(GroupName);
            var oldNpc = GameObject.Find(NpcName);
            if (oldGroup != null || oldNpc != null)
            {
                if (!EditorUtility.DisplayDialog("Blackstone", "O vizinho já existe. Substituir?", "Substituir", "Cancelar")) return;
                if (oldGroup != null) Undo.DestroyObjectImmediate(oldGroup.gameObject);
                if (oldNpc != null) Undo.DestroyObjectImmediate(oldNpc);
            }

            Material concrete = LoadMat("GB_Concreto");
            Material floorMat = LoadMat("GB_Piso");
            Material metal = LoadMat("GB_Metal");
            Material furniture = LoadMat("GB_Mobilia");
            Material porcelain = LoadMat("GB_Loucas");
            Material jumpsuit = GetOrCreateMat("GB_Preso", new Color(0.85f, 0.45f, 0.15f));
            Material dark = GetOrCreateMat("GB_Escuro", new Color(0.12f, 0.12f, 0.12f));

            // ---------- Barras do jogador: "vazadas" ----------
            MarkSeeThrough(r.Find("Grade"));
            MarkSeeThrough(r.Find("Portas/Porta_Cela"));

            // ---------- Medidas ----------
            float halfW = Width / 2f;
            float halfD = Depth / 2f;
            float outerW = Width + Wall * 2f;
            float outerD = Depth + Wall * 2f;
            float corridorDepth = 3f;
            float oppositeZ = halfD + Wall + corridorDepth + Wall / 2f;  // plano da parede oposta (5,05)
            float cellCenterZ = oppositeZ + Wall / 2f + halfD;           // centro da cela do vizinho

            var group = new GameObject(GroupName);
            Undo.RegisterCreatedObjectUndo(group, "Criar cela do vizinho");
            group.transform.SetParent(r, false);
            Transform g = group.transform;

            // ---------- Troca a parede oposta inteira por segmentos + a cela ----------
            var oldWall = r.Find("Corredor/Parede_Oposta");
            if (oldWall != null) Undo.DestroyObjectImmediate(oldWall.gameObject);

            float segLen = 4f - outerW / 2f;
            Static(Box("Parede_Oposta_Esq", g, new Vector3(-outerW / 2f - segLen / 2f, 1.5f, oppositeZ), new Vector3(segLen, 3f, Wall), concrete));
            Static(Box("Parede_Oposta_Dir", g, new Vector3(outerW / 2f + segLen / 2f, 1.5f, oppositeZ), new Vector3(segLen, 3f, Wall), concrete));
            Static(Box("Parede_Oposta_Topo", g, new Vector3(0f, Height + 0.1f, oppositeZ), new Vector3(outerW, 3f - Height, Wall), concrete));

            // ---------- Cela do vizinho ----------
            var structure = Group("Estrutura", g);
            Static(Box("Piso", structure, new Vector3(0f, -Wall / 2f, cellCenterZ), new Vector3(outerW, Wall, outerD), floorMat));
            Static(Box("Teto", structure, new Vector3(0f, Height + Wall / 2f, cellCenterZ), new Vector3(outerW, Wall, outerD), concrete));
            Static(Box("Parede_Esq", structure, new Vector3(-halfW - Wall / 2f, Height / 2f, cellCenterZ), new Vector3(Wall, Height, outerD), concrete));
            Static(Box("Parede_Dir", structure, new Vector3(halfW + Wall / 2f, Height / 2f, cellCenterZ), new Vector3(Wall, Height, outerD), concrete));
            Static(Box("Parede_Fundo", structure, new Vector3(0f, Height / 2f, cellCenterZ + halfD + Wall / 2f), new Vector3(outerW, Height, Wall), concrete));

            var bars = Group("Grade", g);
            int barCount = 17;
            float step = Width / (barCount - 1);
            for (int i = 0; i < barCount; i++)
            {
                var bar = Static(Box($"Barra_{i:00}", bars, new Vector3(-halfW + i * step, Height / 2f, oppositeZ), new Vector3(0.04f, Height, 0.04f), metal));
                bar.AddComponent<SeeThrough>();
            }
            Static(Box("Travessa_Inferior", bars, new Vector3(0f, 0.05f, oppositeZ), new Vector3(Width, 0.1f, 0.08f), metal));
            Static(Box("Travessa_Central", bars, new Vector3(0f, 1.1f, oppositeZ), new Vector3(Width, 0.06f, 0.06f), metal));
            Static(Box("Travessa_Superior", bars, new Vector3(0f, Height - 0.05f, oppositeZ), new Vector3(Width, 0.1f, 0.08f), metal));

            var props = Group("Mobilia", g);
            float backZ = cellCenterZ + halfD;
            Static(Box("Cama", props, new Vector3(halfW - 0.4f, 0.225f, backZ - 1.05f), new Vector3(0.8f, 0.45f, 1.9f), furniture));
            Static(Box("Colchao", props, new Vector3(halfW - 0.4f, 0.54f, backZ - 1.05f), new Vector3(0.78f, 0.18f, 1.85f), porcelain));
            Static(Box("Vaso", props, new Vector3(-halfW + 0.25f, 0.2f, backZ - 0.35f), new Vector3(0.4f, 0.4f, 0.5f), porcelain));
            Static(Box("Pia", props, new Vector3(-halfW + 0.2f, 0.85f, backZ - 1.4f), new Vector3(0.4f, 0.2f, 0.35f), porcelain));

            var lightGo = new GameObject("Luz_Cela_Vizinho");
            lightGo.transform.SetParent(g, false);
            lightGo.transform.localPosition = new Vector3(0f, Height - 0.2f, cellCenterZ);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 6f;
            light.intensity = 1.3f;
            light.color = new Color(1f, 0.9f, 0.75f);
            light.shadows = LightShadows.Soft;

            // ---------- O vizinho ----------
            Vector3 barsPoint = r.TransformPoint(new Vector3(-0.3f, 0f, oppositeZ + 0.45f));
            Vector3 restPoint = r.TransformPoint(new Vector3(0.3f, 0f, backZ - 1.2f));

            var npc = new GameObject(NpcName);
            Undo.RegisterCreatedObjectUndo(npc, "Criar vizinho");
            npc.transform.position = barsPoint;
            npc.transform.rotation = Quaternion.Euler(0f, 180f, 0f);   // olhando para o corredor

            var capsule = npc.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 0.9f, 0f);
            capsule.height = 1.8f;
            capsule.radius = 0.3f;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Corpo";
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.transform.SetParent(npc.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
            body.GetComponent<MeshRenderer>().sharedMaterial = jumpsuit;

            var face = GameObject.CreatePrimitive(PrimitiveType.Cube);
            face.name = "Rosto";
            Object.DestroyImmediate(face.GetComponent<Collider>());
            face.transform.SetParent(npc.transform, false);
            face.transform.localPosition = new Vector3(0f, 1.58f, 0.27f);
            face.transform.localScale = new Vector3(0.32f, 0.08f, 0.06f);
            face.GetComponent<MeshRenderer>().sharedMaterial = dark;

            var dialogue = npc.AddComponent<DialogueNPC>();
            dialogue.Configure("Prisioneiro", "Bento", "vizinho_bento", BuildTopics(), new[]
            {
                "— Já falei demais por hoje, novato.",
                "— Vai descansar. A contagem não espera ninguém.",
                "— Tô pensando. Me deixa."
            });

            var behaviour = npc.AddComponent<NeighborBehaviour>();
            behaviour.Configure(barsPoint, restPoint);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Blackstone] Build 005: vizinho criado. Salve a cena (Ctrl+S) e aperte Play.");
        }

        private static List<DialogueNPC.Topic> BuildTopics()
        {
            return new List<DialogueNPC.Topic>
            {
                new DialogueNPC.Topic
                {
                    id = "apresentacao",
                    requires = new string[0],
                    lines = new[]
                    {
                        "— Acordou, novato? Achei que ia dormir até a contagem.",
                        "— Me chamam de Bento. Tô aqui faz tempo demais pra lembrar quanto.",
                        "— Conselho de graça: não fica parado na grade quando o guarda passar. Ele não gosta de cara curiosa."
                    },
                    noteId = "vizinho_bento",
                    noteTitle = "Bento, o vizinho",
                    noteText = "Preso da cela em frente. Está aqui há muito tempo e conhece a rotina. " +
                               "Diz que o guarda não gosta de quem fica na grade quando ele passa."
                },
                new DialogueNPC.Topic
                {
                    id = "bilhete",
                    requires = new[] { "vizinho_bento", "bilhete_colchao" },
                    lines = new[]
                    {
                        "— Achou o papel do Silva, foi? Ele vivia rabiscando.",
                        "— Um dia ele não voltou da contagem. Uns dizem solitária. Outros dizem que ele saiu.",
                        "— Eu não sei de nada. E você também não."
                    },
                    noteId = "silva",
                    noteTitle = "O preso anterior",
                    noteText = "Bento chama o antigo ocupante da minha cela de Silva. Ele sumiu durante uma contagem. " +
                               "Solitária ou fuga? Bento não quer falar disso, o que já diz alguma coisa."
                },
                new DialogueNPC.Topic
                {
                    id = "contagem",
                    requires = new[] { "vizinho_bento" },
                    lines = new[]
                    {
                        "— A contagem? Duas vezes por dia. Abrem todas as portas e a gente vai pra frente da cela.",
                        "— O guarda conta de uma ponta à outra. Leva uns segundos. Ninguém nunca fez nada com esses segundos."
                    },
                    noteId = "contagem",
                    noteTitle = "A contagem",
                    noteText = "Duas vezes por dia todas as portas abrem e os presos ficam na frente da cela. " +
                               "O guarda conta de uma ponta à outra: alguns segundos com as portas abertas."
                },
                new DialogueNPC.Topic
                {
                    id = "ajuda",
                    requires = new[] { "contagem" },
                    lines = new[]
                    {
                        "— Tá me olhando com cara de quem quer pedir coisa.",
                        "— Não. Eu sei o que acontece com quem arruma confusão aqui dentro.",
                        "— Me prova que você sabe o que tá fazendo. Aí a gente conversa."
                    },
                    noteId = "bento_desconfia",
                    noteTitle = "Bento desconfia",
                    noteText = "Bento não quer se arriscar por um novato. Talvez ajude se eu provar que sei o que estou fazendo."
                }
            };
        }

        private static void MarkSeeThrough(Transform parent)
        {
            if (parent == null) return;
            foreach (Transform child in parent)
            {
                if (child.name.StartsWith("Barra_") && child.GetComponent<SeeThrough>() == null)
                {
                    Undo.AddComponent<SeeThrough>(child.gameObject);
                }
            }
        }

        private static Transform Group(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static GameObject Box(string name, Transform parent, Vector3 center, Vector3 size, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            go.transform.localScale = size;
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        private static GameObject Static(GameObject go)
        {
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI |
                                                       StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
            return go;
        }

        private static Material LoadMat(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsFolder}/{name}.mat");
        }

        private static Material GetOrCreateMat(string name, Color color)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            else mat.color = color;
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }
    }
}
