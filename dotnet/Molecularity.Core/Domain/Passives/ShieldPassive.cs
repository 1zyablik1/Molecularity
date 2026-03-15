namespace Molecularity.Core.Domain.Passives {
    public class ShieldPassive : IPassiveProperty {
        private int ShieldLeft { get; set; }
        public bool IsExpired { get; private set; }

        public ShieldPassive(int shieldLeft) {
            ShieldLeft = shieldLeft;
        }

        public int ModifyDelta(int delta, Molecule owner, MoleculeGraph graph) {
            return ShieldLeft > 0 ? 0 : delta;
        }

        public void OnPassiveApply(Molecule owner, MoleculeGraph graph) {
            if (ShieldLeft > 0) {
                ShieldLeft--;
                return;
            }

            IsExpired = true;
        }

        public IPassiveProperty Clone() {
            return new ShieldPassive(ShieldLeft);
        }
    }
}
