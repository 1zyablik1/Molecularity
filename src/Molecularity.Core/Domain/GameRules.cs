namespace Molecularity.Core.Domain {
    public static class GameRules {
        public static bool IsWin(MoleculeGraph graph) {
            return graph.IsEmpty();
        }

        public static (bool IsLoss, int? CulpritId) IsLose(MoleculeGraph graph) {
            foreach (Molecule molecule in graph.GetAliveAll()) {
                if (molecule.Value <= 0) {
                    return (true, molecule.Id);
                }
            }

            return (false, null);
        }

        /// <summary>
        /// True when molecules remain but none can be clicked (all are protected/unremovable),
        /// so the field can never be cleared. Should be checked only after win/lose.
        /// </summary>
        public static bool IsStuck(MoleculeGraph graph) {
            bool anyAlive = false;
            foreach (Molecule molecule in graph.GetAliveAll()) {
                anyAlive = true;
                if (molecule.IsRemovable) {
                    return false;
                }
            }

            return anyAlive;
        }
    }
}
