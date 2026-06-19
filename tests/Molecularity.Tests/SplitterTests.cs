using System.Collections.Generic;
using System.Linq;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;
using Molecularity.Core.Domain.Exceptions;
using Molecularity.Core.Items;
using Molecularity.Core.Items.Implementations;
using Molecularity.Core.Player;

namespace Molecularity.Tests;

public class SplitterTests {
    // ── Basic split: 2 children spawned, each connected to splitter's neighbours ──

    [Fact]
    public void Splitter_OnClick_SpawnsTwoSimpleChildren_EachConnectedToNeighbours() {
        // Splitter (id 1) connected to Simple A (id 2, val 9) and Simple B (id 3, val 9).
        // After click: splitter gone, 2 children (ids 4 and 5) each connected to ids 2 and 3.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Splitter(1, 5),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        int aliveCountBefore = session.Graph.GetAliveAll().Count(); // 3

        session.TakeTurn(1);

        // Splitter is gone
        Assert.False(session.Graph.GetMolecule(1).IsAlive);

        // Net alive count: (before=3) − 1 (splitter) + 2 (children) = 4
        Assert.Equal(aliveCountBefore - 1 + 2, session.Graph.GetAliveAll().Count());

        // Two new Simple molecules at ids 4 and 5
        Molecule child1 = session.Graph.GetMolecule(4);
        Molecule child2 = session.Graph.GetMolecule(5);
        Assert.True(child1.IsAlive);
        Assert.True(child2.IsAlive);
        Assert.Equal(MoleculeType.Simple, child1.Type);
        Assert.Equal(MoleculeType.Simple, child2.Type);

        // Each child is connected to original neighbours 2 and 3
        var child1Neighbours = session.Graph.GetAliveNeighbors(4).Select(m => m.Id).ToHashSet();
        var child2Neighbours = session.Graph.GetAliveNeighbors(5).Select(m => m.Id).ToHashSet();
        Assert.Contains(2, child1Neighbours);
        Assert.Contains(3, child1Neighbours);
        Assert.Contains(2, child2Neighbours);
        Assert.Contains(3, child2Neighbours);

        // Children are NOT connected to each other
        Assert.DoesNotContain(5, child1Neighbours);
        Assert.DoesNotContain(4, child2Neighbours);
    }

    // ── Children value: spawned at 2, decremented by -1 in the same turn → 1 ──

    [Fact]
    public void Splitter_Children_HaveValueOneAfterSpawnTurnDecrement() {
        // Splitter (id 1) connected to Simple (id 2, val 9).
        // Children get ids 3 and 4 (NextId after existing 1 and 2).
        // They spawn at value 2 then the turn decrement applies → value 1.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Splitter(1, 5),
                TestData.Simple(2, 9),
            },
            new List<ConnectionConfig> { new(1, 2) });

        session.TakeTurn(1);

        // Children at ids 3 and 4
        Molecule child1 = session.Graph.GetMolecule(3);
        Molecule child2 = session.Graph.GetMolecule(4);
        Assert.True(child1.IsAlive);
        Assert.True(child2.IsAlive);
        // Spawn value = 2; turn decrement = -1; result = 1
        Assert.Equal(1, child1.Value);
        Assert.Equal(1, child2.Value);
    }

    // ── Events contain two MoleculeSpawnedEvents ─────────────────────────────

    [Fact]
    public void Splitter_OnClick_EmitsTwoMoleculeSpawnedEvents() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Splitter(1, 5),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        TurnResult result = session.TakeTurn(1);

        var spawnEvents = result.Events.OfType<MoleculeSpawnedEvent>().ToList();
        Assert.Equal(2, spawnEvents.Count);
        Assert.All(spawnEvents, e => {
            Assert.Equal(MoleculeType.Simple, e.Type);
            Assert.Equal(2, e.Value);
        });
    }

    // ── Undo after a split: children gone, splitter restored ─────────────────

    [Fact]
    public void Undo_AfterSplit_RemovesChildren_RestoresSplitter() {
        // Splitter (id 1, val 5) connected to Simple A (id 2, val 9) and Simple B (id 3, val 9).
        var inventory = new PlayerInventory();
        inventory.Add(new UndoItem());

        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Splitter(1, 5),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) },
            inventory);

        // Take the split turn
        session.TakeTurn(1);

        // Children exist at ids 4 and 5
        Assert.True(session.Graph.GetMolecule(4).IsAlive);
        Assert.True(session.Graph.GetMolecule(5).IsAlive);

        // Undo
        session.UseInstantItem(LevelItemType.Undo);

        // Splitter is alive again with its original value
        Molecule splitter = session.Graph.GetMolecule(1);
        Assert.True(splitter.IsAlive);
        Assert.Equal(5, splitter.Value);

        // Original neighbours are fully restored
        Assert.Equal(9, session.Graph.GetMolecule(2).Value);
        Assert.Equal(9, session.Graph.GetMolecule(3).Value);

        // Spawned children (ids 4 and 5) no longer exist in the graph
        Assert.Throws<MoleculeNotFoundException>(() => session.Graph.GetMolecule(4));
        Assert.Throws<MoleculeNotFoundException>(() => session.Graph.GetMolecule(5));

        // Alive count back to original 3
        Assert.Equal(3, session.Graph.GetAliveAll().Count());
    }

    // ── Splitter with no neighbours spawns 2 isolated children ───────────────

    [Fact]
    public void Splitter_WithNoNeighbours_SpawnsTwoIsolatedChildren() {
        // Isolated splitter: no neighbours → children inherit no connections.
        // Children get ids 2 and 3 (NextId after existing id 1).
        GameSession session = TestData.Session(
            new List<MoleculeConfig> { TestData.Splitter(1, 5) },
            new List<ConnectionConfig>());

        session.TakeTurn(1);

        Assert.False(session.Graph.GetMolecule(1).IsAlive);

        Molecule child1 = session.Graph.GetMolecule(2);
        Molecule child2 = session.Graph.GetMolecule(3);
        Assert.True(child1.IsAlive);
        Assert.True(child2.IsAlive);

        // Children are isolated (no alive neighbours)
        Assert.Empty(session.Graph.GetAliveNeighbors(2));
        Assert.Empty(session.Graph.GetAliveNeighbors(3));
    }

    // ── Solver: a level with a Splitter is solvable ──────────────────────────
    //
    // Strategy: Splitter(1) connected to Bomb(2) and Simple(3).
    // Turn 1: click Splitter → children(4,5) each connected to Bomb(2) and Simple(3).
    //         Children: 2→1. Bomb(2): decrements by -1. Simple(3): decrements by -1.
    // Turn 2: click Bomb(2) → explosion removes all alive Bomb neighbours = child4, child5, Simple(3).
    //         TurnExecutor removes Bomb itself. Graph empty → WIN.

    [Fact]
    public void SplitterLevel_IsSolvable() {
        var level = TestData.Level(
            new List<MoleculeConfig> {
                TestData.Splitter(1, 5),
                TestData.Bomb(2, 5),
                TestData.Simple(3, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        var report = new Molecularity.Solver.LevelSolver().Analyze(level);

        Assert.True(report.Solvable);
    }
}
