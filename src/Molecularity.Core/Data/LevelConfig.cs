using System.Collections.Generic;
using System.Linq;

namespace Molecularity.Core.Data {
    public record LevelConfig(int LevelId, List<MoleculeConfig> Molecules, List<ConnectionConfig> Connections, int? LayoutSeed = null, BalanceConfig? Balance = null) {
        public LevelValidationResult Validate() {
            var errors = new List<string>();

            if (Molecules == null || Molecules.Count < 1) {
                errors.Add("Level must contain at least one molecule.");
                return new LevelValidationResult(errors);
            }

            var idGroups = Molecules.GroupBy(m => m.Id).Where(g => g.Count() > 1);
            foreach (var group in idGroups) {
                errors.Add($"Duplicate molecule id: {group.Key}.");
            }

            var moleculeIds = new HashSet<int>(Molecules.Select(m => m.Id));

            if (Connections != null) {
                foreach (ConnectionConfig conn in Connections) {
                    if (!moleculeIds.Contains(conn.FromId)) {
                        errors.Add($"Connection references unknown molecule id: {conn.FromId}.");
                    }
                    if (!moleculeIds.Contains(conn.ToId)) {
                        errors.Add($"Connection references unknown molecule id: {conn.ToId}.");
                    }
                    if (conn.FromId == conn.ToId) {
                        errors.Add($"Self-connection on molecule id: {conn.FromId}.");
                    }
                }

                var seen = new HashSet<(int, int)>();
                foreach (ConnectionConfig conn in Connections) {
                    int a = conn.FromId < conn.ToId ? conn.FromId : conn.ToId;
                    int b = conn.FromId < conn.ToId ? conn.ToId : conn.FromId;
                    if (!seen.Add((a, b))) {
                        errors.Add($"Duplicate connection between {a} and {b}.");
                    }
                }
            }

            foreach (MoleculeConfig mol in Molecules) {
                if (mol.InitialValue < 1) {
                    errors.Add($"Molecule id {mol.Id} has InitialValue < 1.");
                }
            }

            if (Balance != null) {
                if (Balance.FreezeTurns < 1) {
                    errors.Add("Balance.FreezeTurns must be >= 1.");
                }
                if (Balance.LazyStep < 1) {
                    errors.Add("Balance.LazyStep must be >= 1.");
                }
                if (Balance.ShieldTurns < 0) {
                    errors.Add("Balance.ShieldTurns must be >= 0.");
                }
                if (Balance.LockTurns < 0) {
                    errors.Add("Balance.LockTurns must be >= 0.");
                }
                if (Balance.VirusBite < 0) {
                    errors.Add("Balance.VirusBite must be >= 0.");
                }
            }

            return new LevelValidationResult(errors);
        }
    }
}
