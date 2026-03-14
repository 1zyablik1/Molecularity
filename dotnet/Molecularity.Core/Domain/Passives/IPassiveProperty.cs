namespace Molecularity.Core.Domain.Passives {
    public interface IPassiveProperty {
        int ModifyDelta(int delta, Molecule owner, MoleculeGraph graph);
        void OnPassiveApply(Molecule owner, MoleculeGraph graph);
    }
}
