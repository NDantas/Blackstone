using Blackstone.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blackstone.EditorTools
{
    /// <summary>
    /// Ferramenta de Editor da Build 003.
    /// Menu: Blackstone > Build 003 > Criar portas
    /// - Transforma parte da grade da cela numa porta deslizante (trancada).
    /// - Fecha as pontas do corredor; numa delas cria uma porta comum (dobradiça)
    ///   que leva a uma sala vazia.
    /// Pode ser rodada de novo (substitui o que criou antes).
    /// </summary>
    public static class Build003DoorsSetup
    {
        private const string DoorsGroupName = "Portas";
        private const string ExtrasGroupName = "Corredor_Extensao";
        private const string MaterialsFolder = "Assets/_Project/Materials/Graybox";

        // Mesmas medidas da Build 001
        private const float Width = 2.5f;
        private const float Depth = 3.5f;
        private const float Height = 2.8f;
        private const float Wall = 0.2f;

        // Porta da cela: ocupa x de DoorStart até DoorStart + DoorWidth
        private const float DoorStart = 0.25f;
        private const float DoorWidth = 1.0f;

        [MenuItem("Blackstone/Build 003/Criar portas")]
        public static void Setup()
        {
            var root = GameObject.Find("Cela_Graybox");
            if (root == null)
            {
                EditorUtility.DisplayDialog("Blackstone",
                    "Não encontrei a Cela_Graybox. Rode antes as ferramentas das Builds 001 e 002.", "OK");
                return;
            }

            Transform r = root.transform;
            var oldDoors = r.Find(DoorsGroupName);
            var oldExtras = r.Find(ExtrasGroupName);
            if (oldDoors != null || oldExtras != null)
            {
                if (!EditorUtility.DisplayDialog("Blackstone", "As portas já existem. Substituir?", "Substituir", "Cancelar"))
                    return;
                if (oldDoors != null) Undo.DestroyObjectImmediate(oldDoors.gameObject);
                if (oldExtras != null) Undo.DestroyObjectImmediate(oldExtras.gameObject);
            }

            Material concrete = LoadMat("GB_Concreto");
            Material floorMat = LoadMat("GB_Piso");
            Material metal = LoadMat("GB_Metal");
            Material wood = LoadMat("GB_Mobilia");

            float halfW = Width / 2f;
            float halfD = Depth / 2f;
            float barZ = halfD + Wall / 2f;

            // ---------- 1) Abrir espaço na grade fixa ----------
            var grade = r.Find("Grade");
            if (grade != null)
            {
                for (int i = grade.childCount - 1; i >= 0; i--)
                {
                    var child = grade.GetChild(i);
                    if (child.name.StartsWith("Barra_") && child.localPosition.x > DoorStart - 0.05f)
                    {
                        Undo.DestroyObjectImmediate(child.gameObject);
                    }
                    else if (child.name.StartsWith("Travessa_"))
                    {
                        Undo.RecordObject(child, "Encurtar travessa");
                        float left = -halfW;
                        float right = DoorStart;
                        var p = child.localPosition;
                        var s = child.localScale;
                        child.localPosition = new Vector3((left + right) / 2f, p.y, p.z);
                        child.localScale = new Vector3(right - left, s.y, s.z);
                    }
                }

                if (grade.Find("Batente") == null)
                {
                    var post = MakeBox("Batente", grade, new Vector3(DoorStart, Height / 2f, barZ), new Vector3(0.08f, Height, 0.08f), metal);
                    GameObjectUtility.SetStaticEditorFlags(post, StaticEditorFlags.BatchingStatic);
                }
            }

            var doorsGroup = new GameObject(DoorsGroupName);
            Undo.RegisterCreatedObjectUndo(doorsGroup, "Criar portas");
            doorsGroup.transform.SetParent(r, false);

            // ---------- 2) Porta da cela (deslizante, trancada) ----------
            var cellDoor = new GameObject("Porta_Cela");
            cellDoor.transform.SetParent(doorsGroup.transform, false);
            // Corre por fora da cela, na frente da parede do corredor.
            cellDoor.transform.localPosition = new Vector3(DoorStart, 0f, barZ + 0.16f);

            MakeBox("Montante_Esq", cellDoor.transform, new Vector3(0.03f, Height / 2f, 0f), new Vector3(0.06f, Height, 0.08f), metal);
            MakeBox("Montante_Dir", cellDoor.transform, new Vector3(DoorWidth - 0.03f, Height / 2f, 0f), new Vector3(0.06f, Height, 0.08f), metal);
            MakeBox("Travessa_Inferior", cellDoor.transform, new Vector3(DoorWidth / 2f, 0.05f, 0f), new Vector3(DoorWidth, 0.1f, 0.08f), metal);
            MakeBox("Travessa_Central", cellDoor.transform, new Vector3(DoorWidth / 2f, 1.1f, 0f), new Vector3(DoorWidth, 0.06f, 0.06f), metal);
            MakeBox("Travessa_Superior", cellDoor.transform, new Vector3(DoorWidth / 2f, Height - 0.05f, 0f), new Vector3(DoorWidth, 0.1f, 0.08f), metal);
            int doorBars = 6;
            for (int i = 1; i <= doorBars; i++)
            {
                float x = DoorWidth * i / (doorBars + 1);
                MakeBox($"Barra_{i:00}", cellDoor.transform, new Vector3(x, Height / 2f, 0f), new Vector3(0.04f, Height, 0.04f), metal);
            }
            MakeBox("Fechadura", cellDoor.transform, new Vector3(DoorWidth - 0.1f, 1.1f, 0.06f), new Vector3(0.14f, 0.22f, 0.08f), metal);

            var cellDoorComp = cellDoor.AddComponent<Door>();
            cellDoorComp.Configure("Porta da cela", Door.MotionType.Slide, new Vector3(DoorWidth, 0f, 0f), 0f,
                true, "Trancada. A fechadura fica do lado de fora.");

            // ---------- 3) Pontas do corredor ----------
            var extras = new GameObject(ExtrasGroupName);
            Undo.RegisterCreatedObjectUndo(extras, "Criar extensão do corredor");
            extras.transform.SetParent(r, false);
            Transform e = extras.transform;

            float corridorDepth = 3f;
            float zMin = halfD + Wall;                   // começo do corredor
            float zMax = zMin + corridorDepth;           // fim do corredor
            float zMid = (zMin + zMax) / 2f;
            float endX = 4f + Wall / 2f;

            // Ponta direita: parede inteira
            Static(MakeBox("Parede_Fim_Dir", e, new Vector3(endX, Height / 2f, zMid), new Vector3(Wall, Height, corridorDepth), concrete));

            // Ponta esquerda: parede com vão de porta (1,0 x 2,1 m)
            float doorW = 1.0f, doorH = 2.1f;
            float gapMin = zMid - doorW / 2f, gapMax = zMid + doorW / 2f;
            Static(MakeBox("Parede_Fim_Esq_A", e, new Vector3(-endX, Height / 2f, (zMin + gapMin) / 2f), new Vector3(Wall, Height, gapMin - zMin), concrete));
            Static(MakeBox("Parede_Fim_Esq_B", e, new Vector3(-endX, Height / 2f, (gapMax + zMax) / 2f), new Vector3(Wall, Height, zMax - gapMax), concrete));
            Static(MakeBox("Parede_Fim_Esq_Verga", e, new Vector3(-endX, (doorH + Height) / 2f, zMid), new Vector3(Wall, Height - doorH, doorW), concrete));

            // Sala vazia atrás da porta
            float roomW = 2.5f;
            float roomX = -endX - Wall / 2f - roomW / 2f;
            Static(MakeBox("Sala_Piso", e, new Vector3(roomX, -Wall / 2f, zMid), new Vector3(roomW, Wall, corridorDepth + Wall * 2f), floorMat));
            Static(MakeBox("Sala_Parede_Fundo", e, new Vector3(roomX - roomW / 2f - Wall / 2f, Height / 2f, zMid), new Vector3(Wall, Height, corridorDepth + Wall * 2f), concrete));
            Static(MakeBox("Sala_Parede_A", e, new Vector3(roomX, Height / 2f, zMin - Wall / 2f), new Vector3(roomW, Height, Wall), concrete));
            Static(MakeBox("Sala_Parede_B", e, new Vector3(roomX, Height / 2f, zMax + Wall / 2f), new Vector3(roomW, Height, Wall), concrete));

            // ---------- 4) Porta comum (dobradiça, destrancada) ----------
            var hinge = new GameObject("Porta_Corredor");
            hinge.transform.SetParent(doorsGroup.transform, false);
            hinge.transform.localPosition = new Vector3(-endX, 0f, gapMin);   // dobradiça
            MakeBox("Folha", hinge.transform, new Vector3(0f, doorH / 2f, doorW / 2f), new Vector3(0.05f, doorH - 0.02f, doorW - 0.04f), wood);
            MakeBox("Macaneta", hinge.transform, new Vector3(0.06f, 1.0f, doorW - 0.12f), new Vector3(0.06f, 0.04f, 0.12f), metal);

            var hingeDoor = hinge.AddComponent<Door>();
            // -90 faz a porta abrir para dentro da sala (para longe do corredor).
            hingeDoor.Configure("Porta", Door.MotionType.Swing, Vector3.zero, -90f, false, "Trancada.");

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Blackstone] Build 003: portas criadas. Salve a cena (Ctrl+S) e aperte Play.");
        }

        private static GameObject MakeBox(string name, Transform parent, Vector3 center, Vector3 size, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            go.transform.localScale = size;
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            // Sem flags estáticas: peças de porta se movem.
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
    }
}
