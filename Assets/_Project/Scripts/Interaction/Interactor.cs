using System.Collections.Generic;
using Blackstone.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Blackstone.Interaction
{
    /// <summary>
    /// Fica no jogador. Descobre para qual objeto interativo o jogador está olhando
    /// e aciona a interação com E. Também guarda a mensagem atual que o HUD exibe.
    ///
    /// Mira:
    /// 1) Raio fino a partir do centro da tela (precisão).
    /// 2) Se o raio não achar nada interativo, usa um "raio grosso" (SphereCast)
    ///    como ajuda de mira, sem atravessar paredes.
    /// O objeto em foco fica levemente iluminado.
    /// </summary>
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [Tooltip("Distância máxima (m) para interagir.")]
        [SerializeField] private float range = 2.5f;
        [Tooltip("Raio (m) da ajuda de mira.")]
        [SerializeField] private float aimAssistRadius = 0.12f;
        [SerializeField] private LayerMask interactionMask = ~0;
        [Tooltip("Tempo (s) que uma mensagem fica na tela.")]
        [SerializeField] private float messageDuration = 4f;

        [Header("Destaque do objeto em foco")]
        [SerializeField] private bool highlightTarget = true;
        [Range(0f, 1f)]
        [SerializeField] private float highlightAmount = 0.35f;

        public IInteractable Current { get; private set; }
        public string Message { get; private set; }
        public float MessageTimeLeft { get; private set; }

        private Component highlighted;
        private readonly List<Renderer> highlightedRenderers = new List<Renderer>();
        private MaterialPropertyBlock block;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
            block = new MaterialPropertyBlock();
        }

        private void OnDisable() => SetHighlight(null);

        private void Update()
        {
            if (MessageTimeLeft > 0f)
            {
                MessageTimeLeft -= Time.deltaTime;
            }

            if (InputLock.Blocked)
            {
                Current = null;
                SetHighlight(null);
                return;
            }

            Current = FindTarget();
            SetHighlight(highlightTarget ? Current as Component : null);

            var keyboard = Keyboard.current;
            if (Current != null && keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                Current.Interact(this);
            }
        }

        private IInteractable FindTarget()
        {
            if (cameraTransform == null) return null;

            Vector3 origin = cameraTransform.position;
            Vector3 direction = cameraTransform.forward;
            float maxDistance = range;

            // 1) Raio fino
            if (Physics.Raycast(origin, direction, out RaycastHit hit, range, interactionMask, QueryTriggerInteraction.Ignore))
            {
                var interactable = Usable(hit.collider);
                if (interactable != null) return interactable;

                // Algo não interativo na frente: a ajuda de mira não passa muito além dele.
                maxDistance = Mathf.Min(range, hit.distance + 0.25f);
            }

            // 2) Ajuda de mira
            if (aimAssistRadius > 0f &&
                Physics.SphereCast(origin, aimAssistRadius, direction, out RaycastHit assistHit, maxDistance,
                    interactionMask, QueryTriggerInteraction.Ignore))
            {
                return Usable(assistHit.collider);
            }

            return null;
        }

        private IInteractable Usable(Collider collider)
        {
            var interactable = collider.GetComponentInParent<IInteractable>();
            return interactable != null && interactable.CanInteract(this) ? interactable : null;
        }

        private void SetHighlight(Component target)
        {
            if (target == highlighted) return;

            foreach (var r in highlightedRenderers)
            {
                if (r != null) r.SetPropertyBlock(null);
            }
            highlightedRenderers.Clear();
            highlighted = target;

            if (target == null) return;

            target.GetComponentsInChildren(highlightedRenderers);
            foreach (var r in highlightedRenderers)
            {
                var mat = r.sharedMaterial;
                if (mat == null || !mat.HasProperty(BaseColorId)) continue;

                Color baseColor = mat.GetColor(BaseColorId);
                block.Clear();
                block.SetColor(BaseColorId, Color.Lerp(baseColor, new Color(1f, 0.95f, 0.8f), highlightAmount));
                r.SetPropertyBlock(block);
            }
        }

        public void ShowMessage(string text, float duration = -1f)
        {
            Message = text;
            MessageTimeLeft = duration > 0f ? duration : messageDuration;
        }
    }
}
