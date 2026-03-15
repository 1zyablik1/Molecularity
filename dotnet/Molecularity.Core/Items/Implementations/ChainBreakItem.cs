using Molecularity.Core.Domain;

namespace Molecularity.Core.Items.Implementations {
    public class ChainBreakItem : IDoubleTargetItem {
        public LevelItemType Type => LevelItemType.ChainBreak;

        public void Use(int fromId, int toId, MoleculeGraph graph) {
            graph.RemoveConnection(fromId, toId);
        }
    }
}
