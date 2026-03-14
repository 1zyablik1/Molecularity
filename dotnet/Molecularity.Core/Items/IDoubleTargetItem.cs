using Molecularity.Core.Domain;

namespace Molecularity.Core.Items {
    public interface IDoubleTargetItem : ILevelItem {
        void Use(int fromId, int toId, MoleculeGraph graph);
    }
}
