using Molecularity.Core.Data;

namespace Molecularity.Core.Domain.Exceptions {
    public class UnknownMoleculeTypeException : MolecularityException {
        public MoleculeType Type { get; }

        public UnknownMoleculeTypeException(MoleculeType type) : base($"Unknown molecule type: {type}.") {
            Type = type;
        }
    }
}
