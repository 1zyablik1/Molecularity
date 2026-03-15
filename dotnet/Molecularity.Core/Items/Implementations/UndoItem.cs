using Molecularity.Core.Domain;

namespace Molecularity.Core.Items.Implementations {
    public class UndoItem : IInstantItem {
        public LevelItemType Type => LevelItemType.Undo;
        public bool IsUnlocked { get; private set; }

        public void Use(MoleculeGraph graph) {
            IsUnlocked = true;
        }
    }
}
