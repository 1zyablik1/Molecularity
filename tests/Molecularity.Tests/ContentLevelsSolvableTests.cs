using System;
using System.Collections.Generic;
using System.IO;
using Molecularity.Core.Data;
using Molecularity.Solver;

namespace Molecularity.Tests;

public class ContentLevelsSolvableTests {
    private static string FindLevelsDirectory() {
        string? dir = AppContext.BaseDirectory;
        while (dir != null) {
            string candidate = Path.Combine(dir, "content", "levels");
            if (Directory.Exists(candidate)) {
                return candidate;
            }
            dir = Directory.GetParent(dir)?.FullName;
        }

        throw new DirectoryNotFoundException(
            "Could not locate content/levels directory by walking up from " +
            $"'{AppContext.BaseDirectory}'. Ensure the tests are run from within the repository.");
    }

    public static IEnumerable<object[]> AllLevelIds() {
        string levelsDir = FindLevelsDirectory();
        var repo = new JsonLevelRepository(levelsDir);
        foreach (int id in repo.GetAll()) {
            yield return new object[] { id };
        }
    }

    [Theory]
    [MemberData(nameof(AllLevelIds))]
    public void ContentLevel_IsSolvable(int levelId) {
        string levelsDir = FindLevelsDirectory();
        var repo = new JsonLevelRepository(levelsDir);
        LevelConfig level = repo.Get(levelId);

        SolveReport report = new LevelSolver().Analyze(level);

        Assert.True(report.Solvable, $"Level {levelId} is not solvable — it must be solvable to ship.");
    }
}
