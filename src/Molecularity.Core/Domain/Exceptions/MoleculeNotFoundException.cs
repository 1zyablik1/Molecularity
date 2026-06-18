namespace Molecularity.Core.Domain.Exceptions {
    public class MoleculeNotFoundException : MolecularityException {
        public int MoleculeId { get; }

        public MoleculeNotFoundException(int id) : base($"Molecule with id {id} not found.") {
            MoleculeId = id;
        }
    }
}
