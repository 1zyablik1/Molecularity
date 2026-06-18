namespace Molecularity.Core.Domain.Exceptions {
    public class MoleculeAlreadyRemovedException : MolecularityException {
        public int MoleculeId { get; }

        public MoleculeAlreadyRemovedException(int id) : base($"Molecule with id {id} is already removed.") {
            MoleculeId = id;
        }
    }
}
