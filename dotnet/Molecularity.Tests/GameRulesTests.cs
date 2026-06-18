using System.Collections.Generic;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;

namespace Molecularity.Tests;

public class GameRulesTests {
    private static MoleculeGraph SingleMolecule(int value) =>
        TestData.Graph(new List<MoleculeConfig> { TestData.Simple(1, value) }, new List<ConnectionConfig>());

    [Fact]
    public void IsWin_TrueOnlyWhenEmpty() {
        MoleculeGraph graph = SingleMolecule(3);
        Assert.False(GameRules.IsWin(graph));

        graph.RemoveMolecule(1);
        Assert.True(GameRules.IsWin(graph));
    }

    [Fact]
    public void IsLose_ValueZero_IsLossWithCulprit() {
        MoleculeGraph graph = SingleMolecule(0);
        (bool isLoss, int? culpritId) = GameRules.IsLose(graph);

        Assert.True(isLoss);
        Assert.Equal(1, culpritId);
    }

    [Fact]
    public void IsLose_ValueOne_NotLoss() {
        (bool isLoss, int? culpritId) = GameRules.IsLose(SingleMolecule(1));

        Assert.False(isLoss);
        Assert.Null(culpritId);
    }

    [Fact]
    public void IsLose_NegativeValue_IsLoss() {
        MoleculeGraph graph = SingleMolecule(-3);
        (bool isLoss, _) = GameRules.IsLose(graph);

        Assert.True(isLoss);
    }

    [Fact]
    public void IsLose_RemovedMoleculeAtZero_DoesNotCountAsLoss() {
        // A dead molecule must never trigger a loss, even with value 0.
        MoleculeGraph graph = TestData.Graph(
            new List<MoleculeConfig> { TestData.Simple(1, 0), TestData.Simple(2, 5) },
            new List<ConnectionConfig> { new(1, 2) });

        graph.RemoveMolecule(1);
        (bool isLoss, _) = GameRules.IsLose(graph);

        Assert.False(isLoss);
    }
}
