using System.Collections.Generic;
using Molecularity.Core.Data;
using Molecularity.Solver;

namespace Molecularity.Tests;

public class SolverTests {
    private static SolveReport Analyze(List<MoleculeConfig> molecules, List<ConnectionConfig> connections) {
        LevelConfig level = TestData.Level(molecules, connections);
        return new LevelSolver().Analyze(level);
    }

    // ── Single molecule ────────────────────────────────────────────────────

    [Fact]
    public void SingleSimpleMolecule_IsSolvable() {
        SolveReport report = Analyze(
            new List<MoleculeConfig> { TestData.Simple(1, 5) },
            new List<ConnectionConfig>());

        Assert.True(report.Solvable);
    }

    [Fact]
    public void SingleSimpleMolecule_WinningLinesIsOne() {
        SolveReport report = Analyze(
            new List<MoleculeConfig> { TestData.Simple(1, 5) },
            new List<ConnectionConfig>());

        Assert.Equal(1, report.WinningLines);
    }

    [Fact]
    public void SingleSimpleMolecule_SafeFirstMovesIsOne() {
        SolveReport report = Analyze(
            new List<MoleculeConfig> { TestData.Simple(1, 5) },
            new List<ConnectionConfig>());

        Assert.Equal(1, report.SafeFirstMoves);
    }

    [Fact]
    public void SingleSimpleMolecule_MoleculeCountIsOne() {
        SolveReport report = Analyze(
            new List<MoleculeConfig> { TestData.Simple(1, 5) },
            new List<ConnectionConfig>());

        Assert.Equal(1, report.MoleculeCount);
    }

    // ── Two connected molecules both value 1 — unsolvable ─────────────────
    // Clicking either removes it, then the other decrements from 1 to 0 → LOSE.

    [Fact]
    public void TwoConnectedValue1_IsUnsolvable() {
        SolveReport report = Analyze(
            new List<MoleculeConfig> { TestData.Simple(1, 1), TestData.Simple(2, 1) },
            new List<ConnectionConfig> { new(1, 2) });

        Assert.False(report.Solvable);
    }

    [Fact]
    public void TwoConnectedValue1_WinningLinesIsZero() {
        SolveReport report = Analyze(
            new List<MoleculeConfig> { TestData.Simple(1, 1), TestData.Simple(2, 1) },
            new List<ConnectionConfig> { new(1, 2) });

        Assert.Equal(0, report.WinningLines);
    }

    [Fact]
    public void TwoConnectedValue1_SafeFirstMovesIsZero() {
        SolveReport report = Analyze(
            new List<MoleculeConfig> { TestData.Simple(1, 1), TestData.Simple(2, 1) },
            new List<ConnectionConfig> { new(1, 2) });

        Assert.Equal(0, report.SafeFirstMoves);
    }

    // ── Level_1 chain: Simple 1=3, 2=2, 3=1; connections 1-2, 2-3 ─────────
    // Only clicking molecule 3 first avoids an immediate cascade loss.
    // Engine-verified: SafeFirstMoves == 1, WinningLines == 1.

    [Fact]
    public void Level1Chain_IsSolvable() {
        SolveReport report = Analyze(
            new List<MoleculeConfig> {
                TestData.Simple(1, 3),
                TestData.Simple(2, 2, revealed: false),
                TestData.Simple(3, 1, revealed: false),
            },
            new List<ConnectionConfig> { new(1, 2), new(2, 3) });

        Assert.True(report.Solvable);
    }

    [Fact]
    public void Level1Chain_SafeFirstMovesIsOne() {
        // The engine confirms only clicking molecule 3 first leads to a win.
        // Clicking 1 or 2 first immediately drops molecule 3 to value ≤ 0 → LOSE.
        SolveReport report = Analyze(
            new List<MoleculeConfig> {
                TestData.Simple(1, 3),
                TestData.Simple(2, 2, revealed: false),
                TestData.Simple(3, 1, revealed: false),
            },
            new List<ConnectionConfig> { new(1, 2), new(2, 3) });

        Assert.Equal(1, report.SafeFirstMoves);
    }

    // ── Forgiving triangle: 3 Simple molecules each value 9 ──────────────
    // All opening moves are safe; every ordering wins.

    [Fact]
    public void ForgivingTriangle_SafeFirstMovesEqualsFirstMoveCount() {
        SolveReport report = Analyze(
            new List<MoleculeConfig> {
                TestData.Simple(1, 9),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(2, 3), new(1, 3) });

        Assert.Equal(report.FirstMoveCount, report.SafeFirstMoves);
    }

    [Fact]
    public void ForgivingTriangle_IsSolvable() {
        SolveReport report = Analyze(
            new List<MoleculeConfig> {
                TestData.Simple(1, 9),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(2, 3), new(1, 3) });

        Assert.True(report.Solvable);
    }

    // ── Solution density (size-normalized difficulty) ────────────────────

    [Fact]
    public void SingleMolecule_DensityIsOne() {
        SolveReport report = Analyze(
            new List<MoleculeConfig> { TestData.Simple(1, 5) },
            new List<ConnectionConfig>());

        Assert.Equal(1.0, report.SolutionDensity, precision: 9);
    }

    [Fact]
    public void ForgivingTriangle_DensityIsOne() {
        // Every one of the 3! = 6 orderings wins → density 1.0.
        SolveReport report = Analyze(
            new List<MoleculeConfig> {
                TestData.Simple(1, 9),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(2, 3), new(1, 3) });

        Assert.Equal(1.0, report.SolutionDensity, precision: 9);
    }

    [Fact]
    public void Level1Chain_DensityIsOneSixth() {
        // 1 winning ordering out of 3! = 6 → density ≈ 0.1667.
        SolveReport report = Analyze(
            new List<MoleculeConfig> {
                TestData.Simple(1, 3),
                TestData.Simple(2, 2, revealed: false),
                TestData.Simple(3, 1, revealed: false),
            },
            new List<ConnectionConfig> { new(1, 2), new(2, 3) });

        Assert.Equal(1.0 / 6.0, report.SolutionDensity, precision: 9);
    }

    [Fact]
    public void Unsolvable_DensityIsZero() {
        SolveReport report = Analyze(
            new List<MoleculeConfig> { TestData.Simple(1, 1), TestData.Simple(2, 1) },
            new List<ConnectionConfig> { new(1, 2) });

        Assert.Equal(0.0, report.SolutionDensity, precision: 9);
    }

    // ── Fog-of-war fairness (VisibleOnlySolvable) ────────────────────────

    [Fact]
    public void HiddenDanger_IsSolvableButNotVisibleOnlySolvable() {
        // Diamond: 1 (revealed) — 2,3 (hidden) — 4 (hidden, the urgent value-2 node).
        // Winnable with full info (click 4 early), but clicking only visible molecules loses,
        // because 4 is hidden until a neighbour is removed and dies by then.
        SolveReport report = Analyze(
            new List<MoleculeConfig> {
                TestData.Simple(1, 4, revealed: true),
                TestData.Simple(2, 4, revealed: false),
                TestData.Simple(3, 4, revealed: false),
                TestData.Simple(4, 2, revealed: false),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3), new(2, 4), new(3, 4) });

        Assert.True(report.Solvable);
        Assert.False(report.VisibleOnlySolvable);
    }

    [Fact]
    public void RevealingTheDanger_MakesItVisibleOnlySolvable() {
        // Same level but molecule 4 is revealed → the player can see and click it first.
        SolveReport report = Analyze(
            new List<MoleculeConfig> {
                TestData.Simple(1, 4, revealed: true),
                TestData.Simple(2, 4, revealed: false),
                TestData.Simple(3, 4, revealed: false),
                TestData.Simple(4, 2, revealed: true),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3), new(2, 4), new(3, 4) });

        Assert.True(report.Solvable);
        Assert.True(report.VisibleOnlySolvable);
    }

    [Fact]
    public void Unsolvable_IsNotVisibleOnlySolvable() {
        SolveReport report = Analyze(
            new List<MoleculeConfig> { TestData.Simple(1, 1), TestData.Simple(2, 1) },
            new List<ConnectionConfig> { new(1, 2) });

        Assert.False(report.VisibleOnlySolvable);
    }
}
