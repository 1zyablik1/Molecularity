namespace Molecularity.Core.Domain.Abilities {
    public interface IAbility {
        void Execute(Molecule source, MoleculeGraph graph);
    }
}
