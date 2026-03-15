using Molecularity.Core.Domain;
using Molecularity.Core.Domain.Passives;

namespace Molecularity.Core.Items.Implementations {
    public class FreezeItem : ISingleTargetItem {
        public LevelItemType Type => LevelItemType.Freeze;

        public void Use(int targetId, MoleculeGraph graph) {
            Molecule target = graph.GetMolecule(targetId);

            //TODO: hardcoded turns
            target.AddPassive(new FreezePassive(3));
        }
    }
}
