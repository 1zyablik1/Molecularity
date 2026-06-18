using System.Collections.Generic;
using System.Linq;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;
using Molecularity.Core.Domain.Exceptions;

namespace Molecularity.Tests;

public class GraphCornerTests {
    [Fact]
    public void AddMolecule_DuplicateId_Throws() {
        var graph = new MoleculeGraph();
        graph.AddMolecule(MoleculeFactory.Create(TestData.Simple(1, 3)));

        Assert.Throws<DuplicateMoleculeException>(() => graph.AddMolecule(MoleculeFactory.Create(TestData.Simple(1, 5))));
    }

    [Fact]
    public void AddConnection_UnknownMolecule_Throws() {
        var graph = new MoleculeGraph();
        graph.AddMolecule(MoleculeFactory.Create(TestData.Simple(1, 3)));

        Assert.Throws<MoleculeNotFoundException>(() => graph.AddConnection(1, 99));
    }

    [Fact]
    public void GetMolecule_Unknown_Throws() {
        var graph = new MoleculeGraph();
        Assert.Throws<MoleculeNotFoundException>(() => graph.GetMolecule(42));
    }

    [Fact]
    public void TryGetMolecule_Unknown_ReturnsFalse() {
        var graph = new MoleculeGraph();
        Assert.False(graph.TryGetMolecule(42, out _));
    }

    [Fact]
    public void RemoveMolecule_Unknown_Throws() {
        var graph = new MoleculeGraph();
        Assert.Throws<MoleculeNotFoundException>(() => graph.RemoveMolecule(42));
    }

    [Fact]
    public void RemoveMolecule_AlreadyRemoved_Throws() {
        MoleculeGraph graph = TestData.Graph(
            new List<MoleculeConfig> { TestData.Simple(1, 3), TestData.Simple(2, 3) },
            new List<ConnectionConfig> { new(1, 2) });

        graph.RemoveMolecule(1);
        Assert.Throws<MoleculeAlreadyRemovedException>(() => graph.RemoveMolecule(1));
    }

    [Fact]
    public void GetAliveNeighbors_IsolatedMolecule_ReturnsEmpty() {
        MoleculeGraph graph = TestData.Graph(
            new List<MoleculeConfig> { TestData.Simple(1, 3) },
            new List<ConnectionConfig>());

        Assert.Empty(graph.GetAliveNeighbors(1));
    }

    [Fact]
    public void RemoveConnection_Nonexistent_DoesNotThrow() {
        MoleculeGraph graph = TestData.Graph(
            new List<MoleculeConfig> { TestData.Simple(1, 3), TestData.Simple(2, 3) },
            new List<ConnectionConfig>());

        // No connection between 1 and 2 — must be a safe no-op.
        graph.RemoveConnection(1, 2);
    }

    [Fact]
    public void Snapshot_RestoresRemovedMoleculeAndConnections() {
        MoleculeGraph graph = TestData.Graph(
            new List<MoleculeConfig> { TestData.Simple(1, 3), TestData.Simple(2, 3) },
            new List<ConnectionConfig> { new(1, 2) });

        GraphSnapshot snapshot = graph.TakeSnapshot();
        graph.RemoveMolecule(1);
        Assert.False(graph.GetMolecule(1).IsAlive);

        graph.RestoreSnapshot(snapshot);

        Assert.True(graph.GetMolecule(1).IsAlive);
        Assert.Equal(new[] { 1 }, graph.GetAliveNeighbors(2).Select(m => m.Id).ToArray());
    }
}
