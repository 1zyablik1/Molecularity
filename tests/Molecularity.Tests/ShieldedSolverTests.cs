using System.Collections.Generic;
using Molecularity.Core.Data;
using Molecularity.Solver;

namespace Molecularity.Tests;

public class ShieldedSolverTests {
    [Fact]
    public void LevelWithShield_OnlySolvableByClickingOtherMoleculesFirst() {
        // Shield (id 1, value 10, 2 turn protection) connected to two simples.
        // The shield cannot be first move; only after 2 turns it is removable.
        // Simples have high enough values that clicking them first is safe.
        var level = TestData.Level(
            new List<MoleculeConfig> {
                TestData.Shield(1, 10),
                TestData.Simple(2, 10),
                TestData.Simple(3, 10),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        SolveReport report = new LevelSolver().Analyze(level);

        Assert.True(report.Solvable);
        // Shield (id=1) is not removable as a first move
        Assert.Equal(2, report.SafeFirstMoves); // only mol 2 and 3 are valid first moves
        Assert.Equal(2, report.FirstMoveCount);
    }
}
