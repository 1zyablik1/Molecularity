using Molecularity.Core.Domain;

namespace Molecularity.Core.Items {
    public interface ISingleTargetItem : ILevelItem {
        void Use(int targetId, MoleculeGraph graph);
    }
}
