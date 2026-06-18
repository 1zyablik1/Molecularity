using Molecularity.Core.Domain;
using Molecularity.Core.Items;
using Molecularity.Core.Player;

namespace Molecularity.Core.Interfaces {
    public enum PlayerAction {
        Click,
        UseItem,
        Quit
    }

    public interface IInputProvider {
        PlayerAction RequestAction(GameSession session);
        int RequestMoleculeId(MoleculeGraph graph);
        LevelItemType RequestItem(PlayerInventory inventory);
    }
}
