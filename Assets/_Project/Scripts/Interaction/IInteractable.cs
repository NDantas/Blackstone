namespace Blackstone.Interaction
{
    /// <summary>
    /// Qualquer coisa com que o jogador pode interagir.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Nome mostrado quando o jogador mira no objeto.</summary>
        string DisplayName { get; }

        /// <summary>Verbo da ação (ex.: "Examinar", "Pegar", "Abrir").</summary>
        string Prompt { get; }

        bool CanInteract(Interactor interactor);

        void Interact(Interactor interactor);
    }
}
