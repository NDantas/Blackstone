using Blackstone.Knowledge;
using UnityEngine;

namespace Blackstone.Interaction
{
    /// <summary>
    /// Objeto que esconde uma pista. O jogador precisa examiná-lo com atenção
    /// (uma ou mais vezes) até perceber algo; então a ação muda (ex.: "Abrir a costura")
    /// e, ao executá-la, a pista é revelada e anotada no caderno.
    /// </summary>
    public class ClueObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Objeto";

        [Header("Antes da descoberta")]
        [TextArea(2, 4)]
        [SerializeField] private string[] examineLines = { "Nada de especial." };

        [Header("Descoberta")]
        [SerializeField] private string revealPrompt = "Procurar";
        [TextArea(2, 4)]
        [SerializeField] private string revealMessage = "Você encontrou algo.";

        [Header("Anotação no caderno")]
        [SerializeField] private string noteId = "pista";
        [SerializeField] private string noteTitle = "Pista";
        [TextArea(3, 6)]
        [SerializeField] private string noteText = "";

        [Header("Depois da descoberta")]
        [TextArea(2, 4)]
        [SerializeField] private string afterRevealMessage = "Não há mais nada aqui.";

        private int step;
        private bool revealed;

        private int LineCount => examineLines != null ? examineLines.Length : 0;
        private bool ReadyToReveal => !revealed && step >= LineCount;

        public string DisplayName => displayName;
        public string Prompt => ReadyToReveal ? revealPrompt : "Examinar";
        public bool IsRevealed => revealed;

        public bool CanInteract(Interactor interactor) => true;

        public void Interact(Interactor interactor)
        {
            if (revealed)
            {
                interactor.ShowMessage(afterRevealMessage);
                return;
            }

            if (step < LineCount)
            {
                interactor.ShowMessage(examineLines[step]);
                step++;
                return;
            }

            revealed = true;
            interactor.ShowMessage(revealMessage, 6f);

            var log = interactor.GetComponent<KnowledgeLog>();
            if (log != null) log.Add(noteId, noteTitle, noteText);
        }

        /// <summary>Usado pelas ferramentas de Editor.</summary>
        public void Configure(string newName, string[] lines, string newRevealPrompt, string newRevealMessage,
            string id, string title, string text, string afterMessage)
        {
            displayName = newName;
            examineLines = lines;
            revealPrompt = newRevealPrompt;
            revealMessage = newRevealMessage;
            noteId = id;
            noteTitle = title;
            noteText = text;
            afterRevealMessage = afterMessage;
        }
    }
}
