using System.Linq;
using Molecularity.Core.Data;
using Molecularity.Core.Domain.Abilities;

namespace Molecularity.Core.Domain.Passives {
    /// <summary>
    /// Virus passive: each turn, infects the highest-value alive non-Virus neighbour
    /// (tie-break: lowest Id). The infected molecule is converted to a Virus and dealt
    /// −VirusBite damage. A dormant VirusPassive skips infection on the first tick
    /// (used for freshly-infected molecules so they wait one turn before spreading).
    /// </summary>
    public class VirusPassive : IPassiveProperty {
        private readonly int _bite;
        private bool _dormant;

        public VirusPassive(int bite, bool dormant = false) {
            _bite = bite;
            _dormant = dormant;
        }

        public bool IsExpired => false;
        public bool PreventsRemoval => false;
        public bool PausesOwner => false;

        public int ModifyDelta(int delta, Molecule owner, MoleculeGraph graph) {
            return delta; // virus decrements at the base rate
        }

        public void OnPassiveApply(Molecule owner, MoleculeGraph graph) {
            if (_dormant) {
                _dormant = false;
                return;
            }

            // Find the alive, non-Virus neighbour with the highest Value (tie-break: lowest Id).
            Molecule? target = graph.GetAliveNeighbors(owner.Id)
                .Where(m => m.Type != MoleculeType.Virus)
                .OrderByDescending(m => m.Value)
                .ThenBy(m => m.Id)
                .FirstOrDefault();

            if (target == null) return;

            target.MutateTo(MoleculeType.Virus, new NoAbility(), new[] { new VirusPassive(_bite, dormant: true) });
            target.ApplyDelta(-_bite);
        }

        public IPassiveProperty Clone() {
            return new VirusPassive(_bite, _dormant);
        }
    }
}
