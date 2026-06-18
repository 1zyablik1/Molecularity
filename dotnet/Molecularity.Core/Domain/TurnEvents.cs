namespace Molecularity.Core.Domain {
    public enum ValueChangeReason { Ability, Decrement }

    public abstract record TurnEvent;
    public record ValueChangedEvent(int MoleculeId, int Delta, int NewValue, bool IsRevealed, ValueChangeReason Reason) : TurnEvent;
    public record MoleculeRemovedEvent(int MoleculeId) : TurnEvent;
    public record MoleculeRevealedEvent(int MoleculeId) : TurnEvent;
}
