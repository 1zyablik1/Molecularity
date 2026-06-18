using System.Collections.Generic;
using System.Linq;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;

namespace Molecularity.Tests;

public class MoleculeGraphTests {
    [Fact]
    public void RemoveMolecule_RevealsAliveNeighbors() {
        MoleculeGraph graph = TestData.Graph(
            new List<MoleculeConfig> {
                TestData.Simple(1, 3, revealed: true),
                TestData.Simple(2, 3, revealed: false),
            },
            new List<ConnectionConfig> { new(1, 2) });

        graph.RemoveMolecule(1);

        Assert.True(graph.GetMolecule(2).IsRevealed);
    }

    [Fact]
    public void GetAliveNeighbors_ExcludesRemoved() {
        MoleculeGraph graph = TestData.Graph(
            new List<MoleculeConfig> {
                TestData.Simple(1, 3),
                TestData.Simple(2, 3),
                TestData.Simple(3, 3),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        graph.RemoveMolecule(2);

        List<int> neighborIds = graph.GetAliveNeighbors(1).Select(m => m.Id).ToList();
        Assert.Equal(new[] { 3 }, neighborIds);
    }

    [Fact]
    public void IsEmpty_TrueOnlyWhenAllRemoved() {
        MoleculeGraph graph = TestData.Graph(
            new List<MoleculeConfig> { TestData.Simple(1, 3), TestData.Simple(2, 3) },
            new List<ConnectionConfig> { new(1, 2) });

        Assert.False(graph.IsEmpty());
        graph.RemoveMolecule(1);
        Assert.False(graph.IsEmpty());
        graph.RemoveMolecule(2);
        Assert.True(graph.IsEmpty());
    }
}
