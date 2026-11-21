using System;
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
    /// 1) Raio fino a partir do centro da tela.
    /// 2) Se não achar nada, um "raio grosso" (ajuda de mira), sem atravessar paredes.
    /// - Colliders marcados com SeeThrough (barras) deixam a mira passar.
    /// - Interativos com IRangedInteractable (ex.: NPCs) podem ser alcançados de mais longe.
    /// O objeto em foco fica levemente iluminado.
    /// </summary>
    public class Interactor : MonoBehaviour
    {
        private const float MaxQueryDistance = 8f;

        [SerializeField] private Transform cameraTransform;
        [Tooltip("Distância máxima (m) para interagir com objetos comuns.")]
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
        private static readonly Comparison<RaycastHit> ByDistance = (a, b) => a.distance.CompareTo(b.distance);

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

            // 1) Raio fino
            var hits = Physics.RaycastAll(origin, direction, MaxQueryDistance, interactionMask, QueryTriggerInteraction.Ignore);
            if (Pick(hits, out IInteractable result, out float solidDistance)) return result;

            // 2) Ajuda de mira (não passa muito além do primeiro obstáculo sólido)
            if (aimAssistRadius > 0f)
            {
                float maxDistance = Mathf.Min(MaxQueryDistance, solidDistance + 0.25f);
                var assistHits = Physics.SphereCastAll(origin, aimAssistRadius, direction, maxDistance,
                    interactionMask, QueryTriggerInteraction.Ignore);
                if (Pick(assistHits, out result, out _)) return result;
            }

            return null;
        }

        /// <summary>
        /// Percorre os acertos do mais perto ao mais longe.
        /// Barras (SeeThrough) são atravessadas, mas lembradas como candidatas.
        /// O primeiro objeto sólido decide: se for interativo e estiver no alcance, é ele.
        /// </summary>
        private bool Pick(RaycastHit[] hits, out IInteractable result, out float solidDistance)
        {
            Array.Sort(hits, ByDistance);
            result = null;
            solidDistance = MaxQueryDistance;
            IInteractable seeThroughCandidate = null;

            foreach (var hit in hits)
            {
                // Ignora os colliders do próprio jogador.
                if (hit.collider.transform.IsChildOf(transform)) continue;

                var interactable = Usable(hit.collider);
                bool inRange = interactable != null && hit.distance <= AllowedDistance(interactable);

                if (hit.collider.GetComponent<SeeThrough>() != null)
                {
                    if (inRange && seeThroughCandidate == null) seeThroughCandidate = interactable;
                    continue;
                }

                solidDistance = hit.distance;
                if (inRange)
                {
                    result = interactable;
                    return true;
                }
                break;
            }

            result = seeThroughCandidate;
            return result != null;
        }

        private float AllowedDistance(IInteractable interactable)
        {
            return interactable is IRangedInteractable ranged ? Mathf.Max(range, ranged.MaxDistance) : range;
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
