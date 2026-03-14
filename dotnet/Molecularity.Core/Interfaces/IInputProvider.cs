using Molecularity.Core.Domain;

namespace Molecularity.Core.Interfaces {
    public interface IInputProvider {
        int RequestMoleculeId(MoleculeGraph graph);
    }
}
