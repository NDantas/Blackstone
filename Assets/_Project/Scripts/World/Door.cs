using Blackstone.Interaction;
using UnityEngine;

namespace Blackstone.World
{
    /// <summary>
    /// Porta interativa. Pode deslizar (porta de cela) ou girar na dobradiça (porta comum).
    /// O objeto com este componente é o pivô: para portas que giram, ele fica na dobradiça.
    /// Se estiver trancada, mostra a mensagem de tranca em vez de abrir.
    /// NPCs (ex.: guardas) podem abrir e fechar com SetOpen, com ou sem chave.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Door : MonoBehaviour, IInteractable
    {
        public enum MotionType { Slide, Swing }

        [Header("Identificação")]
        [SerializeField] private string displayName = "Porta";

        [Header("Movimento")]
        [SerializeField] private MotionType motion = MotionType.Swing;
        [Tooltip("Deslocamento local quando totalmente aberta (porta deslizante).")]
        [SerializeField] private Vector3 slideOffset = new Vector3(1f, 0f, 0f);
        [Tooltip("Ângulo em Y quando totalmente aberta (porta com dobradiça).")]
        [SerializeField] private float swingAngle = -90f;
        [Tooltip("Tempo (s) para abrir ou fechar.")]
        [SerializeField] private float duration = 1f;
        [SerializeField] private bool startsOpen;

        [Header("Tranca")]
        [SerializeField] private bool locked;
        [TextArea(2, 3)]
        [SerializeField] private string lockedMessage = "Trancada.";

        private Vector3 closedPosition;
        private Quaternion closedRotation;
        private float progress;      // 0 = fechada, 1 = aberta
        private bool targetOpen;

        public string DisplayName => displayName;
        public string Prompt => targetOpen ? "Fechar" : "Abrir";
        public bool IsOpen => targetOpen;
        public bool IsFullyClosed => !targetOpen && progress <= 0.001f;
        public bool IsLocked => locked;

        private void Awake()
        {
            var body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            closedPosition = transform.localPosition;
            closedRotation = transform.localRotation;
            targetOpen = startsOpen;
            progress = startsOpen ? 1f : 0f;
            Apply();
        }

        private void Update()
        {
            float goal = targetOpen ? 1f : 0f;
            if (!Mathf.Approximately(progress, goal))
            {
                progress = Mathf.MoveTowards(progress, goal, Time.deltaTime / Mathf.Max(0.01f, duration));
                Apply();
            }
        }

        private void Apply()
        {
            float eased = Mathf.SmoothStep(0f, 1f, progress);
            if (motion == MotionType.Slide)
            {
                transform.localPosition = closedPosition + slideOffset * eased;
            }
            else
            {
                transform.localRotation = closedRotation * Quaternion.Euler(0f, swingAngle * eased, 0f);
            }
        }

        public bool CanInteract(Interactor interactor) => true;

        public void Interact(Interactor interactor)
        {
            if (locked && !targetOpen)
            {
                interactor.ShowMessage(lockedMessage);
                return;
            }
            targetOpen = !targetOpen;
        }

        /// <summary>Abre ou fecha a porta por script (ignora a tranca: quem chama tem a chave).</summary>
        public void SetOpen(bool open) => targetOpen = open;

        public void Lock() => locked = true;
        public void Unlock() => locked = false;

        /// <summary>Usado pelas ferramentas de Editor.</summary>
        public void Configure(string newName, MotionType newMotion, Vector3 newSlideOffset, float newSwingAngle,
            bool isLocked, string newLockedMessage)
        {
            displayName = newName;
            motion = newMotion;
            slideOffset = newSlideOffset;
            swingAngle = newSwingAngle;
            locked = isLocked;
            lockedMessage = newLockedMessage;
        }
    }
}
