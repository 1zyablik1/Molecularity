namespace Molecularity.Core.Domain.Exceptions {
    public class DuplicateMoleculeException : MolecularityException {
        public int MoleculeId { get; }

        public DuplicateMoleculeException(int id) : base($"Molecule with id {id} already exists.") {
            MoleculeId = id;
        }
    }
}
