namespace Molecularity.Core.Domain.Passives {
    /// <summary>
    /// Overrides the per-turn decrement with a fixed amount (e.g. Anchor: -2),
    /// ignoring the incoming base delta.
    /// </summary>
    public class FlatDecrementPassive : IPassiveProperty {
        private readonly int _delta;

        public FlatDecrementPassive(int delta) {
            _delta = delta;
        }

        public bool IsExpired => false;

        public int ModifyDelta(int delta, Molecule owner, MoleculeGraph graph) {
            return _delta;
        }

        public void OnPassiveApply(Molecule owner, MoleculeGraph graph) {
        }

        public IPassiveProperty Clone() {
            return new FlatDecrementPassive(_delta);
        }
    }
}
