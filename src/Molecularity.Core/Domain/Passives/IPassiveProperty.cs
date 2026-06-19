namespace Molecularity.Core.Domain.Passives {
    public interface IPassiveProperty {
        bool IsExpired { get; }
        bool PreventsRemoval { get; }

        /// <summary>
        /// While true, this passive fully pauses the owner's per-turn logic: its decrement is
        /// skipped and its OTHER passives do not advance (only the pausing passive itself ticks).
        /// Used by Freeze.
        /// </summary>
        bool PausesOwner { get; }

        int ModifyDelta(int delta, Molecule owner, MoleculeGraph graph);
        void OnPassiveApply(Molecule owner, MoleculeGraph graph);
        IPassiveProperty Clone();
    }
}
