using Molecularity.Core.Domain;
using Molecularity.Core.Domain.Passives;

namespace Molecularity.Core.Items.Implementations {
    public class FreezeItem : ISingleTargetItem {
        public LevelItemType Type => LevelItemType.Freeze;

        public void Use(int targetId, MoleculeGraph graph) {
            Molecule target = graph.GetMolecule(targetId);

            target.AddPassive(new FreezePassive(GameBalance.FreezeTurns));
        }
    }
}
