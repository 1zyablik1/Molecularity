using System.Collections.Generic;
using System.Linq;

namespace Molecularity.Core.Data {
    public class HardcodedLevelRepository : ILevelRepository {
        private readonly LevelConfig[] _levels = new LevelConfig[1] {
            new(
                LevelId: 1,
                Molecules: new List<MoleculeConfig> {
                    new(1, MoleculeType.Simple, InitialValue: 3, true),
                    new(2, MoleculeType.Simple, InitialValue: 2, false),
                    new(3, MoleculeType.Simple, InitialValue: 1, false),
                },
                Connections: new List<ConnectionConfig> {
                    new(1, 2),
                    new(2, 3),
                }
            )
        };

        public LevelConfig Get(int levelId) {
            return _levels.FirstOrDefault(l => l.LevelId == levelId) ??
                   throw new KeyNotFoundException($"Level with id {levelId} not found.");
        }

        public IEnumerable<int> GetAll() {
            return _levels.Select(l => l.LevelId);
        }
    }
}
