namespace Molecularity.Core.Domain.Passives {
    public class LockPassive : IPassiveProperty {
        private int _turnsLeft;
        public bool IsExpired { get; private set; }
        public bool PreventsRemoval => _turnsLeft > 0;

        public LockPassive(int turnsLeft) {
            _turnsLeft = turnsLeft;
        }

        public int ModifyDelta(int delta, Molecule owner, MoleculeGraph graph) {
            return delta;
        }

        public void OnPassiveApply(Molecule owner, MoleculeGraph graph) {
            if (_turnsLeft > 0) {
                _turnsLeft--;
                return;
            }

            IsExpired = true;
        }

        public IPassiveProperty Clone() {
            return new LockPassive(_turnsLeft);
        }
    }
}
