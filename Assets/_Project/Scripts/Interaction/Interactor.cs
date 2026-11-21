using UnityEngine;
using UnityEngine.InputSystem;

namespace Blackstone.Interaction
{
    /// <summary>
    /// Fica no jogador. Lança um raio a partir da câmera, descobre para qual
    /// objeto interativo o jogador está olhando e aciona a interação com E.
    /// Também guarda a mensagem atual que o HUD exibe.
    /// </summary>
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [Tooltip("Distância máxima (m) para interagir.")]
        [SerializeField] private float range = 2f;
        [SerializeField] private LayerMask interactionMask = ~0;
        [Tooltip("Tempo (s) que uma mensagem fica na tela.")]
        [SerializeField] private float messageDuration = 4f;

        public IInteractable Current { get; private set; }
        public string Message { get; private set; }
        public float MessageTimeLeft { get; private set; }

        private void Awake()
        {
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            Current = FindTarget();

            var keyboard = Keyboard.current;
            if (Current != null && keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                Current.Interact(this);
            }

            if (MessageTimeLeft > 0f)
            {
                MessageTimeLeft -= Time.deltaTime;
            }
        }

        private IInteractable FindTarget()
        {
            if (cameraTransform == null) return null;

            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit,
                    range, interactionMask, QueryTriggerInteraction.Ignore))
            {
                var interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null && interactable.CanInteract(this))
                {
                    return interactable;
                }
            }
            return null;
        }

        public void ShowMessage(string text)
        {
            Message = text;
            MessageTimeLeft = messageDuration;
        }
    }
}
