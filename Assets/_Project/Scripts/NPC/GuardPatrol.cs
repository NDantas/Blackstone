using System;
using System.Collections;
using Blackstone.Interaction;
using Blackstone.Knowledge;
using Blackstone.World;
using UnityEngine;

namespace Blackstone.NPC
{
    /// <summary>
    /// Guarda da ronda (graybox). Fica fora de vista até receber a ordem de patrulhar.
    /// Percorre os pontos do caminho, para e olha para dentro das celas,
    /// abre e fecha a porta do corredor, e volta a se esconder.
    ///
    /// Se o jogador vê a ronda acontecendo, o protagonista anota o que observou.
    /// (Detecção do jogador pelo guarda vem numa build futura.)
    /// </summary>
    public class GuardPatrol : MonoBehaviour
    {
        public enum WaypointAction { None, OpenDoor, CloseDoor }

        [Serializable]
        public class Waypoint
        {
            public Vector3 position;
            [Tooltip("Tempo (s) parado neste ponto.")]
            public float wait;
            [Tooltip("Direção para onde olhar enquanto espera (zero = mantém).")]
            public Vector3 lookDirection;
            public WaypointAction action;
        }

        [SerializeField] private Waypoint[] route = Array.Empty<Waypoint>();
        [SerializeField] private Door door;
        [SerializeField] private float walkSpeed = 1.3f;
        [SerializeField] private float turnSpeed = 220f;
        [Tooltip("Ponto da cabeça, para checar se o jogador consegue ver o guarda.")]
        [SerializeField] private Vector3 headOffset = new Vector3(0f, 1.65f, 0f);

        public bool IsPatrolling { get; private set; }

        private Camera playerCamera;
        private Transform playerRoot;
        private bool seenThisPatrol;
        private float firstSightingMinutes = -1f;
        private int witnessedPatrols;
        private float nextVisibilityCheck;

        private void Start()
        {
            playerCamera = Camera.main;
            var log = FindFirstObjectByType<KnowledgeLog>();
            if (log != null) playerRoot = log.transform;
        }

        public void BeginPatrol()
        {
            if (!IsPatrolling && route.Length > 0) StartCoroutine(PatrolRoutine());
        }

        private IEnumerator PatrolRoutine()
        {
            IsPatrolling = true;
            seenThisPatrol = false;
            float sightingTime = -1f;

            foreach (var point in route)
            {
                // Caminha até o ponto
                while (true)
                {
                    Vector3 to = point.position - transform.position;
                    to.y = 0f;
                    if (to.magnitude <= 0.03f) break;

                    float step = Mathf.Min(walkSpeed * Time.deltaTime, to.magnitude);
                    transform.position += to.normalized * step;
                    Face(to);
                    sightingTime = CheckSeen(sightingTime);
                    yield return null;
                }

                // Ação no ponto
                if (door != null)
                {
                    if (point.action == WaypointAction.OpenDoor) door.SetOpen(true);
                    else if (point.action == WaypointAction.CloseDoor) door.SetOpen(false);
                }

                // Espera, olhando para a direção indicada
                float t = 0f;
                while (t < point.wait)
                {
                    if (point.lookDirection.sqrMagnitude > 0.001f) Face(point.lookDirection);
                    sightingTime = CheckSeen(sightingTime);
                    t += Time.deltaTime;
                    yield return null;
                }
            }

            IsPatrolling = false;
            if (seenThisPatrol) OnPatrolWitnessed(sightingTime);
        }

        private void Face(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
        }

        // ---------- O jogador viu a ronda? ----------

        private float CheckSeen(float sightingTime)
        {
            if (seenThisPatrol || Time.time < nextVisibilityCheck) return sightingTime;
            nextVisibilityCheck = Time.time + 0.2f;

            if (PlayerCanSeeMe())
            {
                seenThisPatrol = true;
                if (GameClock.Instance != null) sightingTime = GameClock.Instance.TotalMinutes;
            }
            return sightingTime;
        }

        private bool PlayerCanSeeMe()
        {
            if (playerCamera == null) return false;

            Vector3 head = transform.TransformPoint(headOffset);
            Vector3 viewport = playerCamera.WorldToViewportPoint(head);
            if (viewport.z <= 0f || viewport.x < 0.05f || viewport.x > 0.95f || viewport.y < 0.05f || viewport.y > 0.95f)
                return false;

            Vector3 origin = playerCamera.transform.position;
            Vector3 dir = head - origin;
            var hits = Physics.RaycastAll(origin, dir.normalized, dir.magnitude + 0.5f, ~0, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                if (playerRoot != null && hit.collider.transform.IsChildOf(playerRoot)) continue;
                if (hit.collider.GetComponent<SeeThrough>() != null) continue;
                return hit.collider.transform.IsChildOf(transform);
            }
            return false;
        }

        private void OnPatrolWitnessed(float sightingMinutes)
        {
            var log = FindFirstObjectByType<KnowledgeLog>();
            if (log == null) return;

            witnessedPatrols++;
            string time = sightingMinutes >= 0f ? GameClock.Format(sightingMinutes) : "??:??";

            if (witnessedPatrols == 1)
            {
                firstSightingMinutes = sightingMinutes;
                log.Add("ronda_1", "O guarda da ronda",
                    $"Um guarda passou pelo corredor por volta das {time}. Andou devagar e parou para olhar dentro das celas. " +
                    "Olhou para os lados, para dentro, para o chão. Para cima, não.");
            }
            else if (witnessedPatrols == 2)
            {
                string interval = "";
                if (firstSightingMinutes >= 0f && sightingMinutes >= 0f)
                {
                    int minutes = Mathf.RoundToInt((sightingMinutes - firstSightingMinutes) / 5f) * 5;
                    interval = $" Cerca de {minutes} minutos depois da anterior.";
                }

                log.Add("ronda_padrao", "Padrão da ronda",
                    $"Outra ronda, por volta das {time}.{interval} E as luzes piscaram pouco antes de ele aparecer, de novo. " +
                    "O bilhete estava certo: dá para prever quando ele vem.");
            }
        }

        /// <summary>Usado pelas ferramentas de Editor.</summary>
        public void Configure(Waypoint[] newRoute, Door newDoor)
        {
            route = newRoute;
            door = newDoor;
        }
    }
}
