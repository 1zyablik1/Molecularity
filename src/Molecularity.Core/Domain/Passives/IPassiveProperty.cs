namespace Molecularity.Core.Domain.Passives {
    public interface IPassiveProperty {
        bool IsExpired { get; }
        bool PreventsRemoval { get; }
        int ModifyDelta(int delta, Molecule owner, MoleculeGraph graph);
        void OnPassiveApply(Molecule owner, MoleculeGraph graph);
        IPassiveProperty Clone();
    }
}
