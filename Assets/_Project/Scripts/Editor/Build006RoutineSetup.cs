using Blackstone.NPC;
using Blackstone.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blackstone.EditorTools
{
    /// <summary>
    /// Ferramenta de Editor da Build 006.
    /// Menu: Blackstone > Build 006 > Criar rotina e ronda
    /// - Relógio do jogo e relógio de parede no corredor.
    /// - Guarda da ronda (sai da sala lateral, percorre o corredor, olha as celas e volta).
    /// - Luzes das celas piscam alguns minutos (de jogo) antes de cada ronda.
    /// Pode ser rodada de novo (substitui o que criou antes).
    /// </summary>
    public static class Build006RoutineSetup
    {
        private const string SystemsName = "Sistemas_Rotina";
        private const string GuardName = "Guarda_Ronda";
        private const string ClockName = "Relogio_Corredor";
        private const string MaterialsFolder = "Assets/_Project/Materials/Graybox";

        // Linha central do corredor (z) e pontos-chave (x), conforme as Builds 001-005.
        private const float CorridorZ = 3.45f;

        [MenuItem("Blackstone/Build 006/Criar rotina e ronda")]
        public static void Setup()
        {
            var cell = GameObject.Find("Cela_Graybox");
            var doorGo = GameObject.Find("Cela_Graybox/Portas/Porta_Corredor");
            if (cell == null || doorGo == null || GameObject.Find("Bento") == null)
            {
                EditorUtility.DisplayDialog("Blackstone",
                    "Não encontrei a cela, a porta do corredor ou o Bento. Rode as ferramentas das Builds 001 a 005.", "OK");
                return;
            }

            var oldSystems = GameObject.Find(SystemsName);
            var oldGuard = GameObject.Find(GuardName);
            var oldClock = GameObject.Find("Cela_Graybox/" + ClockName);
            if (oldSystems != null || oldGuard != null || oldClock != null)
            {
                if (!EditorUtility.DisplayDialog("Blackstone", "A rotina já existe. Substituir?", "Substituir", "Cancelar")) return;
                if (oldSystems != null) Undo.DestroyObjectImmediate(oldSystems);
                if (oldGuard != null) Undo.DestroyObjectImmediate(oldGuard);
                if (oldClock != null) Undo.DestroyObjectImmediate(oldClock);
            }

            Material porcelain = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsFolder}/GB_Loucas.mat");
            Material dark = GetOrCreateMat("GB_Escuro", new Color(0.12f, 0.12f, 0.12f));
            Material uniform = GetOrCreateMat("GB_Guarda", new Color(0.16f, 0.24f, 0.42f));

            // ---------- Sistemas: relógio, luzes, agenda ----------
            var systems = new GameObject(SystemsName);
            Undo.RegisterCreatedObjectUndo(systems, "Criar rotina");
            systems.AddComponent<GameClock>();

            var flicker = systems.AddComponent<LightFlicker>();
            var lights = new System.Collections.Generic.List<Light>();
            AddLight(lights, "Cela_Graybox/Luz_Cela");
            AddLight(lights, "Cela_Graybox/Cela_Vizinho/Luz_Cela_Vizinho");
            flicker.Configure(lights.ToArray());

            var scheduler = systems.AddComponent<PatrolScheduler>();

            // ---------- Relógio de parede ----------
            var clockRoot = new GameObject(ClockName);
            Undo.RegisterCreatedObjectUndo(clockRoot, "Criar relógio");
            clockRoot.transform.SetParent(cell.transform, false);
            // Na parede oposta, à esquerda da cela do Bento (visível de dentro da cela do jogador).
            clockRoot.transform.localPosition = new Vector3(-2.4f, 2.25f, 4.93f);

            var face = GameObject.CreatePrimitive(PrimitiveType.Cube);
            face.name = "Mostrador";
            face.transform.SetParent(clockRoot.transform, false);
            face.transform.localScale = new Vector3(0.6f, 0.32f, 0.04f);
            if (porcelain != null) face.GetComponent<MeshRenderer>().sharedMaterial = porcelain;

            var textGo = new GameObject("Hora");
            textGo.transform.SetParent(clockRoot.transform, false);
            textGo.transform.localPosition = new Vector3(0f, 0f, -0.03f);
            var text = textGo.AddComponent<TextMesh>();
            text.text = "05:50";
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 80;
            text.characterSize = 0.035f;
            text.color = new Color(0.1f, 0.1f, 0.1f);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                text.font = font;
                textGo.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            }

            var wallClock = clockRoot.AddComponent<WallClock>();
            wallClock.Configure(text);

            // ---------- Guarda ----------
            Vector3 hidden = new Vector3(-6.2f, 0f, CorridorZ);
            var guard = new GameObject(GuardName);
            Undo.RegisterCreatedObjectUndo(guard, "Criar guarda");
            guard.transform.position = cell.transform.TransformPoint(hidden);
            guard.transform.rotation = Quaternion.LookRotation(Vector3.right);

            var capsule = guard.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 0.9f, 0f);
            capsule.height = 1.85f;
            capsule.radius = 0.3f;

            Part(guard.transform, PrimitiveType.Capsule, "Corpo", new Vector3(0f, 0.92f, 0f), new Vector3(0.62f, 0.92f, 0.62f), uniform);
            Part(guard.transform, PrimitiveType.Cylinder, "Quepe", new Vector3(0f, 1.86f, 0f), new Vector3(0.4f, 0.05f, 0.4f), dark);
            Part(guard.transform, PrimitiveType.Cube, "Aba", new Vector3(0f, 1.82f, 0.2f), new Vector3(0.3f, 0.03f, 0.14f), dark);
            Part(guard.transform, PrimitiveType.Cube, "Rosto", new Vector3(0f, 1.6f, 0.28f), new Vector3(0.32f, 0.08f, 0.06f), dark);

            var patrol = guard.AddComponent<GuardPatrol>();
            var toPlayerCell = new Vector3(0f, 0f, -1f);
            var toNeighbor = new Vector3(0f, 0f, 1f);
            patrol.Configure(new[]
            {
                Wp(-5.4f, 1.0f, Vector3.right, GuardPatrol.WaypointAction.OpenDoor),
                Wp(-3.6f, 0f, Vector3.zero, GuardPatrol.WaypointAction.None),
                Wp(-0.4f, 3.0f, toPlayerCell, GuardPatrol.WaypointAction.None),   // olha a cela do jogador
                Wp(-0.4f, 1.5f, toNeighbor, GuardPatrol.WaypointAction.None),     // olha a cela do Bento
                Wp(3.5f, 1.0f, Vector3.left, GuardPatrol.WaypointAction.None),    // fim do corredor
                Wp(0.4f, 2.0f, toPlayerCell, GuardPatrol.WaypointAction.None),    // olha de novo na volta
                Wp(-3.6f, 0f, Vector3.zero, GuardPatrol.WaypointAction.None),
                Wp(-5.4f, 0.3f, Vector3.zero, GuardPatrol.WaypointAction.None),
                Wp(-6.2f, 0.5f, Vector3.right, GuardPatrol.WaypointAction.CloseDoor),
            }, doorGo.GetComponent<Door>());

            scheduler.Configure(patrol, flicker);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Blackstone] Build 006: rotina e ronda criadas. Salve a cena (Ctrl+S) e aperte Play.");

            GuardPatrol.Waypoint Wp(float x, float wait, Vector3 look, GuardPatrol.WaypointAction action) =>
                new GuardPatrol.Waypoint
                {
                    position = cell.transform.TransformPoint(new Vector3(x, 0f, CorridorZ)),
                    wait = wait,
                    lookDirection = look,
                    action = action
                };
        }

        private static void AddLight(System.Collections.Generic.List<Light> list, string path)
        {
            var go = GameObject.Find(path);
            var light = go != null ? go.GetComponent<Light>() : null;
            if (light != null) list.Add(light);
            else Debug.LogWarning($"[Blackstone] Luz não encontrada: {path}");
        }

        private static void Part(Transform parent, PrimitiveType type, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
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
