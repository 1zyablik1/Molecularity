namespace Molecularity.Core.Domain.Exceptions {
    public class MoleculeShieldedException : MolecularityException {
        public int MoleculeId { get; }

        public MoleculeShieldedException(int id) : base($"Molecule {id} is shielded and cannot be removed yet.") {
            MoleculeId = id;
        }
    }
}
