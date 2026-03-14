using Molecularity.Core.Domain;

namespace Molecularity.Core.Items {
    public interface IInstantItem : ILevelItem {
        void Use(MoleculeGraph graph);
    }
}
