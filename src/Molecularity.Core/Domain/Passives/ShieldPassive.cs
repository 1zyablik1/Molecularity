namespace Molecularity.Core.Domain.Passives {
    public class ShieldPassive : IPassiveProperty {
        private int _turnsLeft;
        public bool IsExpired { get; private set; }
        public bool PreventsRemoval => _turnsLeft > 0;
        public bool PausesOwner => false;

        public ShieldPassive(int turnsLeft) {
            _turnsLeft = turnsLeft;
        }

        public int ModifyDelta(int delta, Molecule owner, MoleculeGraph graph) {
            return _turnsLeft > 0 ? 0 : delta;
        }

        public void OnPassiveApply(Molecule owner, MoleculeGraph graph) {
            if (_turnsLeft > 0) {
                _turnsLeft--;
                return;
            }

            IsExpired = true;
        }

        public IPassiveProperty Clone() {
            return new ShieldPassive(_turnsLeft);
        }
    }
}
