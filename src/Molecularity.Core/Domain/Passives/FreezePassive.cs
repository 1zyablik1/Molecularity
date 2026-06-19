namespace Molecularity.Core.Domain.Passives {
    public class FreezePassive : IPassiveProperty {
        public bool IsExpired { get; private set; }
        public bool PreventsRemoval => false;
        public int TurnsLeft { get; private set; }

        public FreezePassive(int turns) {
            TurnsLeft = turns;
        }

        public int ModifyDelta(int delta, Molecule owner, MoleculeGraph graph) {
            return 0;
        }

        public void OnPassiveApply(Molecule owner, MoleculeGraph graph) {
            TurnsLeft--;

            if (TurnsLeft <= 0) {
                IsExpired = true;
            }
        }

        public IPassiveProperty Clone() {
            return new FreezePassive(TurnsLeft);
        }
    }
}
