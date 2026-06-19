using System.Collections.Generic;
using System.Linq;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;

namespace Molecularity.Tests;

public class BombTests {
    // ── Basic explosion: neighbours removed ──────────────────────────────────

    [Fact]
    public void Bomb_OnClick_RemovesAllAliveNeighbours() {
        // Bomb (id 1) connected to three simples (2, 3, 4).
        // Clicking the bomb should remove all three neighbours.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Bomb(1, 5),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
                TestData.Simple(4, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3), new(1, 4) });

        TurnResult result = session.TakeTurn(1);

        Assert.False(session.Graph.GetMolecule(2).IsAlive);
        Assert.False(session.Graph.GetMolecule(3).IsAlive);
        Assert.False(session.Graph.GetMolecule(4).IsAlive);
    }

    [Fact]
    public void Bomb_OnClick_EmitsRemovedEventForEachNeighbour() {
        // The TurnResult.Events must contain a MoleculeRemovedEvent for each neighbour
        // AND for the bomb itself.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Bomb(1, 5),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        TurnResult result = session.TakeTurn(1);

        var removedIds = result.Events.OfType<MoleculeRemovedEvent>().Select(e => e.MoleculeId).ToList();
        Assert.Contains(1, removedIds); // the bomb itself
        Assert.Contains(2, removedIds); // neighbour 2
        Assert.Contains(3, removedIds); // neighbour 3
    }

    // ── Win condition via explosion ──────────────────────────────────────────

    [Fact]
    public void Bomb_ClearsWholeGraph_CausesWin() {
        // Bomb (id 1) is the only non-neighbour after explosion removes 2 and 3;
        // TurnExecutor then removes the bomb itself → graph empty → Win.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Bomb(1, 5),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        session.TakeTurn(1);

        Assert.Equal(GameStatus.Win, session.Status);
    }

    // ── Protection is ignored by the explosion ───────────────────────────────

    [Fact]
    public void Bomb_ExplodesShieldNeighbour_IgnoresProtection() {
        // A Shield molecule normally cannot be removed by a player click while active.
        // However the Bomb's explosion calls graph.RemoveMolecule directly, bypassing
        // IsRemovable. After clicking the Bomb the Shield should no longer be alive.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Bomb(1, 5),
                TestData.Shield(2, 9),   // protected, ShieldTurns = 2 by default
                TestData.Simple(3, 9),   // extra molecule so the bomb itself is not isolated
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        // Verify shield IS protected before the click
        Assert.False(session.Graph.GetMolecule(2).IsRemovable);

        session.TakeTurn(1);

        Assert.False(session.Graph.GetMolecule(2).IsAlive, "Bomb explosion should remove a shielded neighbour.");
    }

    // ── Non-neighbours are NOT removed ──────────────────────────────────────

    [Fact]
    public void Bomb_DoesNotRemoveNonNeighbour() {
        // Graph: Bomb(1) — Simple(2) — Simple(3).
        // Molecule 3 is NOT a direct neighbour of the bomb.
        // After clicking the bomb, molecule 3 must still be alive.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Bomb(1, 5),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(2, 3) });

        session.TakeTurn(1);

        Assert.True(session.Graph.GetMolecule(3).IsAlive, "Non-neighbour should NOT be removed by the bomb explosion.");
    }

    // ── Bomb without neighbours ──────────────────────────────────────────────

    [Fact]
    public void Bomb_WithNoNeighbours_JustRemovesItself() {
        // Isolated bomb: no neighbours to explode.
        // The normal TurnExecutor flow removes the bomb after its ability (which does nothing).
        GameSession session = TestData.Session(
            new List<MoleculeConfig> { TestData.Bomb(1, 5) },
            new List<ConnectionConfig>());

        session.TakeTurn(1);

        Assert.Equal(GameStatus.Win, session.Status);
        Assert.False(session.Graph.GetMolecule(1).IsAlive);
    }

    // ── Solver: a level containing a Bomb is solvable ───────────────────────

    [Fact]
    public void BombLevel_IsSolvable() {
        // Simple solvable setup: Bomb(1, value=5) adjacent to Simple(2, value=5).
        // Clicking the bomb removes both the neighbour and the bomb → Win in one turn.
        var level = TestData.Level(
            new List<MoleculeConfig> {
                TestData.Bomb(1, 5),
                TestData.Simple(2, 5),
            },
            new List<ConnectionConfig> { new(1, 2) });

        var report = new Molecularity.Solver.LevelSolver().Analyze(level);

        Assert.True(report.Solvable);
    }
}
