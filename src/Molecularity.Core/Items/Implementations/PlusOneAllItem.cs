using Molecularity.Core.Domain;

namespace Molecularity.Core.Items.Implementations {
    public class PlusOneAllItem : IInstantItem {
        public LevelItemType Type => LevelItemType.PlusOneAll;

        public void Use(MoleculeGraph graph) {
            foreach (Molecule molecule in graph.GetAliveAll()) {
                molecule.ApplyDelta(+1);
            }
        }
    }
}
