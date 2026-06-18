using Molecularity.Core.Domain;

namespace Molecularity.Core.Items.Implementations {
    /// <summary>
    /// Marker item. The actual revert is handled by <see cref="Domain.GameSession.UseInstantItem"/>,
    /// which restores the snapshot taken before the last turn. <see cref="Use"/> is intentionally a no-op.
    /// </summary>
    public class UndoItem : IInstantItem {
        public LevelItemType Type => LevelItemType.Undo;

        public void Use(MoleculeGraph graph) {
        }
    }
}
