using Blackstone.Player;
using UnityEngine;

namespace Blackstone.NPC
{
    /// <summary>
    /// Comportamento simples do vizinho de cela (graybox):
    /// - Alterna entre ficar na grade e descansar perto da cama.
    /// - Olha para o jogador quando ele está por perto.
    /// - Depois de uma conversa, fica na grade por um tempo.
    /// </summary>
    [RequireComponent(typeof(DialogueNPC))]
    public class NeighborBehaviour : MonoBehaviour
    {
        [Tooltip("Ponto (mundo) junto à grade.")]
        [SerializeField] private Vector3 barsPoint;
        [Tooltip("Ponto (mundo) perto da cama.")]
        [SerializeField] private Vector3 restPoint;
        [SerializeField] private float walkSpeed = 0.9f;
        [SerializeField] private float turnSpeed = 200f;
        [Tooltip("Distância (m) em que ele repara no jogador.")]
        [SerializeField] private float noticeDistance = 7f;
        [Tooltip("Tempo (s) que ele fica na grade depois de conversar.")]
        [SerializeField] private float stayAfterTalk = 20f;
        [SerializeField] private Vector2 wanderInterval = new Vector2(12f, 25f);

        private Transform player;
        private DialogueNPC dialogue;
        private Vector3 target;
        private float nextSwitch;

        private void Start()
        {
            dialogue = GetComponent<DialogueNPC>();
            var controller = FindFirstObjectByType<FirstPersonController>();
            if (controller != null) player = controller.transform;

            target = barsPoint;
            ScheduleNextSwitch();
        }

        private void Update()
        {
            bool playerNear = player != null && Vector3.Distance(player.position, transform.position) < noticeDistance;
            bool engaged = dialogue.InConversation || Time.time - dialogue.LastTalkTime < stayAfterTalk;

            if (engaged)
            {
                target = barsPoint;
            }
            else if (Time.time >= nextSwitch)
            {
                target = (target == barsPoint) ? restPoint : barsPoint;
                ScheduleNextSwitch();
            }

            Vector3 toTarget = target - transform.position;
            toTarget.y = 0f;

            if (toTarget.magnitude > 0.05f)
            {
                float step = Mathf.Min(walkSpeed * Time.deltaTime, toTarget.magnitude);
                transform.position += toTarget.normalized * step;
                Face(toTarget);
            }
            else if (playerNear)
            {
                Face(player.position - transform.position);
            }
        }

        private void Face(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            var goal = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, turnSpeed * Time.deltaTime);
        }

        private void ScheduleNextSwitch()
        {
            nextSwitch = Time.time + Random.Range(wanderInterval.x, wanderInterval.y);
        }

        /// <summary>Usado pelas ferramentas de Editor.</summary>
        public void Configure(Vector3 bars, Vector3 rest)
        {
            barsPoint = bars;
            restPoint = rest;
        }
    }
}
