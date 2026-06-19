namespace Molecularity.Core.Domain.Passives {
    public class NoPassive : IPassiveProperty {
        public bool IsExpired => false;
        public bool PreventsRemoval => false;
        public bool PausesOwner => false;

        public int ModifyDelta(int delta, Molecule owner, MoleculeGraph graph) {
            return delta;
        }

        public void OnPassiveApply(Molecule owner, MoleculeGraph graph) {
        }

        public IPassiveProperty Clone() {
            return new NoPassive();
        }
    }
}
