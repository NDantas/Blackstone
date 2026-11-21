using Blackstone.Knowledge;
using UnityEngine;

namespace Blackstone.Interaction
{
    /// <summary>
    /// Objeto que pode ser examinado. Cada vez que o jogador examina de novo,
    /// mostra a próxima descrição da lista; a última se repete.
    /// Opcionalmente, ao chegar na última descrição, o protagonista anota algo no caderno.
    /// </summary>
    public class InspectableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Objeto";
        [SerializeField] private string prompt = "Examinar";
        [TextArea(2, 4)]
        [SerializeField] private string[] descriptions = { "Nada de especial." };

        [Header("Anotação (opcional)")]
        [Tooltip("Se preenchido, vira anotação no caderno ao ver a última descrição.")]
        [SerializeField] private string noteId;
        [SerializeField] private string noteTitle;
        [TextArea(2, 5)]
        [SerializeField] private string noteText;

        private int index;

        public string DisplayName => displayName;
        public string Prompt => prompt;

        public bool CanInteract(Interactor interactor) => descriptions != null && descriptions.Length > 0;

        public void Interact(Interactor interactor)
        {
            interactor.ShowMessage(descriptions[index]);

            bool isLast = index >= descriptions.Length - 1;
            if (isLast && !string.IsNullOrEmpty(noteId))
            {
                var log = interactor.GetComponent<KnowledgeLog>();
                if (log != null) log.Add(noteId, noteTitle, noteText);
            }

            if (!isLast)
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

        /// <summary>Usado pelas ferramentas de Editor para definir a anotação.</summary>
        public void SetNote(string id, string title, string text)
        {
            noteId = id;
            noteTitle = title;
            noteText = text;
        }
    }
}
