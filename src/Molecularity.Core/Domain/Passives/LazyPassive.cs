namespace Molecularity.Core.Domain.Passives {
    /// <summary>
    /// Lazy molecule: its per-turn decrement grows arithmetically — by default 1, 2, 3, …
    /// (<c>step</c> is the common difference and the first decrement). The longer it is left on
    /// the field, the faster it decays. Always removable; never blocks the decrement.
    /// </summary>
    public class LazyPassive : IPassiveProperty {
        private readonly int _step;
        private int _current;

        public LazyPassive(int step) : this(step, step) {
        }

        private LazyPassive(int step, int current) {
            _step = step;
            _current = current;
        }

        public bool IsExpired => false;
        public bool PreventsRemoval => false;

        public int ModifyDelta(int delta, Molecule owner, MoleculeGraph graph) {
            return -_current;
        }

        public void OnPassiveApply(Molecule owner, MoleculeGraph graph) {
            _current += _step;
        }

        public IPassiveProperty Clone() {
            return new LazyPassive(_step, _current);
        }
    }
}
