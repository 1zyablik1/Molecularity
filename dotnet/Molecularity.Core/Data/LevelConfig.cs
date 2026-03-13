using System.Collections.Generic;

namespace Molecularity.Core.Data {
    public record LevelConfig(int LevelId, List<MoleculeConfig> Molecules, List<ConnectionConfig> Connections);
}
