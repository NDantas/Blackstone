using Blackstone.Interaction;
using UnityEngine;

namespace Blackstone.World
{
    /// <summary>
    /// Relógio de parede no corredor. Mostra a hora do GameClock
    /// e pode ser examinado de longe (de dentro da cela).
    /// </summary>
    public class WallClock : MonoBehaviour, IInteractable, IRangedInteractable
    {
        [SerializeField] private TextMesh display;
        [SerializeField] private float examineDistance = 6f;

        public string DisplayName => "Relógio";
        public string Prompt => "Examinar";
        public float MaxDistance => examineDistance;

        private void Update()
        {
            if (display != null && GameClock.Instance != null)
            {
                display.text = GameClock.Instance.TimeText;
            }
        }

        public bool CanInteract(Interactor interactor) => GameClock.Instance != null;

        public void Interact(Interactor interactor)
        {
            interactor.ShowMessage($"O relógio do corredor marca {GameClock.Instance.TimeText}.");
        }

        /// <summary>Usado pelas ferramentas de Editor.</summary>
        public void Configure(TextMesh text) => display = text;
    }
}
