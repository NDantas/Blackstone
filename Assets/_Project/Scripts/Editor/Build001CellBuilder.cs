using Blackstone.Core;
using Blackstone.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blackstone.EditorTools
{
    /// <summary>
    /// Ferramenta de Editor da Build 001.
    /// Menu: Blackstone > Build 001 > Criar cela graybox
    /// Monta na cena aberta: a cela em graybox, um trecho de corredor,
    /// a luz da cela, o jogador com a câmera e o GameSession.
    /// Tudo pode ser desfeito com Ctrl+Z.
    /// </summary>
    public static class Build001CellBuilder
    {
        private const string RootName = "Cela_Graybox";
        private const string PlayerName = "Player";
        private const string SessionName = "GameSession";
        private const string MaterialsFolder = "Assets/_Project/Materials/Graybox";

        // Medidas internas da cela (metros)
        private const float Width = 2.5f;   // eixo X
        private const float Depth = 3.5f;   // eixo Z
        private const float Height = 2.8f;  // eixo Y
        private const float Wall = 0.2f;    // espessura das paredes

        [MenuItem("Blackstone/Build 001/Criar cela graybox")]
        public static void CreateCell()
        {
            if (!PrepareScene()) return;

            Material concrete = GetMaterial("GB_Concreto", new Color(0.55f, 0.55f, 0.53f));
            Material floorMat = GetMaterial("GB_Piso", new Color(0.38f, 0.38f, 0.38f));
            Material metal = GetMaterial("GB_Metal", new Color(0.22f, 0.24f, 0.26f));
            Material furniture = GetMaterial("GB_Mobilia", new Color(0.62f, 0.52f, 0.40f));
            Material porcelain = GetMaterial("GB_Loucas", new Color(0.85f, 0.85f, 0.82f));

            var root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Criar cela graybox");
            Transform r = root.transform;

            float halfW = Width / 2f;
            float halfD = Depth / 2f;
            float outerW = Width + Wall * 2f;
            float outerD = Depth + Wall * 2f;

            // --- Estrutura da cela ---
            var structure = Group("Estrutura", r);
            Box("Piso", structure, new Vector3(0f, -Wall / 2f, 0f), new Vector3(outerW, Wall, outerD), floorMat);
            Box("Teto", structure, new Vector3(0f, Height + Wall / 2f, 0f), new Vector3(outerW, Wall, outerD), concrete);
            Box("Parede_Esquerda", structure, new Vector3(-halfW - Wall / 2f, Height / 2f, 0f), new Vector3(Wall, Height, outerD), concrete);
            Box("Parede_Direita", structure, new Vector3(halfW + Wall / 2f, Height / 2f, 0f), new Vector3(Wall, Height, outerD), concrete);
            Box("Parede_Fundo", structure, new Vector3(0f, Height / 2f, -halfD - Wall / 2f), new Vector3(outerW, Height, Wall), concrete);

            // --- Grade frontal (porta funcional fica para a Build 003) ---
            var bars = Group("Grade", r);
            float barZ = halfD + Wall / 2f;
            int barCount = 17;
            float step = Width / (barCount - 1);
            for (int i = 0; i < barCount; i++)
            {
                float x = -halfW + i * step;
                Box($"Barra_{i:00}", bars, new Vector3(x, Height / 2f, barZ), new Vector3(0.04f, Height, 0.04f), metal);
            }
            Box("Travessa_Inferior", bars, new Vector3(0f, 0.05f, barZ), new Vector3(Width, 0.1f, 0.08f), metal);
            Box("Travessa_Central", bars, new Vector3(0f, 1.1f, barZ), new Vector3(Width, 0.06f, 0.06f), metal);
            Box("Travessa_Superior", bars, new Vector3(0f, Height - 0.05f, barZ), new Vector3(Width, 0.1f, 0.08f), metal);

            // --- Mobília ---
            var props = Group("Mobilia", r);
            Box("Cama", props, new Vector3(-halfW + 0.4f, 0.225f, -0.7f), new Vector3(0.8f, 0.45f, 1.9f), furniture);
            Box("Colchao", props, new Vector3(-halfW + 0.4f, 0.5f, -0.7f), new Vector3(0.75f, 0.1f, 1.85f), porcelain);
            Box("Pia", props, new Vector3(halfW - 0.2f, 0.85f, -0.3f), new Vector3(0.4f, 0.2f, 0.35f), porcelain);
            Box("Vaso", props, new Vector3(halfW - 0.25f, 0.2f, -halfD + 0.35f), new Vector3(0.4f, 0.4f, 0.5f), porcelain);
            Box("Luminaria", props, new Vector3(0f, Height - 0.025f, 0f), new Vector3(0.4f, 0.05f, 0.2f), porcelain);

            // --- Corredor do lado de fora (só para não ver o vazio pela grade) ---
            var corridor = Group("Corredor", r);
            float corridorDepth = 3f;
            float corridorZ = halfD + Wall + corridorDepth / 2f;
            Box("Piso_Corredor", corridor, new Vector3(0f, -Wall / 2f, corridorZ), new Vector3(8f, Wall, corridorDepth), floorMat);
            Box("Parede_Oposta", corridor, new Vector3(0f, 1.5f, corridorZ + corridorDepth / 2f + Wall / 2f), new Vector3(8f, 3f, Wall), concrete);
            float sideLen = (8f - outerW) / 2f;
            Box("Parede_Celas_Esq", corridor, new Vector3(-outerW / 2f - sideLen / 2f, Height / 2f, barZ), new Vector3(sideLen, Height, Wall), concrete);
            Box("Parede_Celas_Dir", corridor, new Vector3(outerW / 2f + sideLen / 2f, Height / 2f, barZ), new Vector3(sideLen, Height, Wall), concrete);

            // --- Luz da cela ---
            var lightGo = new GameObject("Luz_Cela");
            lightGo.transform.SetParent(r, false);
            lightGo.transform.localPosition = new Vector3(0f, Height - 0.2f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 6f;
            light.intensity = 1.5f;
            light.color = new Color(1f, 0.92f, 0.78f);
            light.shadows = LightShadows.Soft;

            CreatePlayer(new Vector3(0.3f, 0.02f, 0.3f));
            CreateSession();

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Blackstone] Build 001: cela criada. Salve a cena (Ctrl+S) e aperte Play.");
        }

        // Remove uma montagem anterior (se o usuário confirmar) para não duplicar.
        private static bool PrepareScene()
        {
            var oldRoot = GameObject.Find(RootName);
            var oldPlayer = GameObject.Find(PlayerName);
            var oldSession = GameObject.Find(SessionName);

            if (oldRoot == null && oldPlayer == null && oldSession == null) return true;

            bool replace = EditorUtility.DisplayDialog(
                "Blackstone",
                "A cena já tem uma cela/jogador criados por esta ferramenta. Substituir?",
                "Substituir", "Cancelar");
            if (!replace) return false;

            // Tira a câmera de dentro do jogador antigo antes de apagá-lo.
            var cam = Camera.main;
            if (cam != null && oldPlayer != null && cam.transform.IsChildOf(oldPlayer.transform))
            {
                Undo.SetTransformParent(cam.transform, null, "Soltar câmera");
            }

            if (oldRoot != null) Undo.DestroyObjectImmediate(oldRoot);
            if (oldPlayer != null) Undo.DestroyObjectImmediate(oldPlayer);
            if (oldSession != null) Undo.DestroyObjectImmediate(oldSession);
            return true;
        }

        private static void CreatePlayer(Vector3 position)
        {
            var player = new GameObject(PlayerName);
            Undo.RegisterCreatedObjectUndo(player, "Criar jogador");
            player.transform.position = position;
            player.transform.rotation = Quaternion.identity; // olhando para a grade (+Z)

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.3f;
            cc.skinWidth = 0.02f;

            var cameraRoot = new GameObject("CameraRoot");
            cameraRoot.transform.SetParent(player.transform, false);
            cameraRoot.transform.localPosition = new Vector3(0f, 1.65f, 0f);

            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
                Undo.RegisterCreatedObjectUndo(camGo, "Criar câmera");
            }
            Undo.SetTransformParent(cam.transform, cameraRoot.transform, "Mover câmera");
            cam.transform.localPosition = Vector3.zero;
            cam.transform.localRotation = Quaternion.identity;
            cam.nearClipPlane = 0.05f;
            cam.fieldOfView = 70f;

            var fpc = player.AddComponent<FirstPersonController>();
            var so = new SerializedObject(fpc);
            so.FindProperty("cameraRoot").objectReferenceValue = cameraRoot.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateSession()
        {
            var session = new GameObject(SessionName);
            Undo.RegisterCreatedObjectUndo(session, "Criar GameSession");
            session.AddComponent<GameSession>();
        }

        private static Transform Group(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void Box(string name, Transform parent, Vector3 center, Vector3 size, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
        }

        private static Material GetMaterial(string name, Color color)
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project"))
                AssetDatabase.CreateFolder("Assets", "_Project");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Materials"))
                AssetDatabase.CreateFolder("Assets/_Project", "Materials");
            if (!AssetDatabase.IsValidFolder(MaterialsFolder))
                AssetDatabase.CreateFolder("Assets/_Project/Materials", "Graybox");

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
