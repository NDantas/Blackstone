namespace Blackstone.Interaction
{
    /// <summary>
    /// Interativos que podem ser alcançados de mais longe que o normal
    /// (ex.: conversar com alguém do outro lado do corredor).
    /// </summary>
    public interface IRangedInteractable
    {
        float MaxDistance { get; }
    }
}
