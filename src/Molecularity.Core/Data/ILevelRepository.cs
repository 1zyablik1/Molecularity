using System.Collections.Generic;

namespace Molecularity.Core.Data {
    public interface ILevelRepository {
        LevelConfig Get(int levelId);
        IEnumerable<int> GetAll();
    }
}
