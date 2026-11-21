using UnityEngine;

namespace Blackstone.Interaction
{
    /// <summary>
    /// Objeto que pode ser examinado. Cada vez que o jogador examina de novo,
    /// mostra a próxima descrição da lista; a última se repete.
    /// Assim, insistir em observar pode revelar detalhes novos.
    /// </summary>
    public class InspectableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Objeto";
        [SerializeField] private string prompt = "Examinar";
        [TextArea(2, 4)]
        [SerializeField] private string[] descriptions = { "Nada de especial." };

        private int index;

        public string DisplayName => displayName;
        public string Prompt => prompt;

        public bool CanInteract(Interactor interactor) => descriptions != null && descriptions.Length > 0;

        public void Interact(Interactor interactor)
        {
            interactor.ShowMessage(descriptions[index]);
            if (index < descriptions.Length - 1)
            {
                index++;
            }
        }

        /// <summary>Usado pelas ferramentas de Editor para preencher o objeto.</summary>
        public void Configure(string newName, params string[] newDescriptions)
        {
            displayName = newName;
            descriptions = newDescriptions;
            index = 0;
        }
    }
}
