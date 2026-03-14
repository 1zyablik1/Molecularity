using Molecularity.Core.Domain;

namespace Molecularity.Core.Items.Implementations {
    public class RevealAllItem : IInstantItem{
        public LevelItemType Type => LevelItemType.RevealAll;

        public void Use(MoleculeGraph graph) {
            foreach (Molecule molecule in graph.GetAliveAll()) {
                molecule.Reveal();
            }
        }
    }
}
