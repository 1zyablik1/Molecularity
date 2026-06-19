using System.Linq;

namespace Molecularity.Core.Domain.Passives {
    /// <summary>
    /// Parasite behaviour: the per-turn decrement equals the number of alive neighbors.
    /// Overrides the incoming base delta. With 0 neighbors the decrement is 0.
    /// </summary>
    public class NeighborCountDecrementPassive : IPassiveProperty {
        public bool IsExpired => false;
        public bool PreventsRemoval => false;

        public int ModifyDelta(int delta, Molecule owner, MoleculeGraph graph) {
            return -graph.GetAliveNeighbors(owner.Id).Count();
        }

        public void OnPassiveApply(Molecule owner, MoleculeGraph graph) {
        }

        public IPassiveProperty Clone() {
            return new NeighborCountDecrementPassive();
        }
    }
}
