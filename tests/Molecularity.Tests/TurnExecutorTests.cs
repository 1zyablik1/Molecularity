using System.Collections.Generic;
using System.Linq;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;
using Molecularity.Core.Domain.Exceptions;

namespace Molecularity.Tests;

public class TurnExecutorTests {
    private static (MoleculeGraph graph, TurnExecutor executor) Build(
        List<MoleculeConfig> molecules, List<ConnectionConfig> connections) {
        MoleculeGraph graph = TestData.Graph(molecules, connections);
        return (graph, new TurnExecutor(graph));
    }

    [Fact]
    public void Execute_OnRemovedMolecule_Throws() {
        (_, TurnExecutor executor) = Build(
            new List<MoleculeConfig> { TestData.Simple(1, 5), TestData.Simple(2, 5) },
            new List<ConnectionConfig> { new(1, 2) });

        executor.Execute(1);
        Assert.Throws<MoleculeAlreadyRemovedException>(() => executor.Execute(1));
    }

    [Fact]
    public void Execute_OnUnknownMolecule_Throws() {
        (_, TurnExecutor executor) = Build(
            new List<MoleculeConfig> { TestData.Simple(1, 5) },
            new List<ConnectionConfig>());

        Assert.Throws<MoleculeNotFoundException>(() => executor.Execute(999));
    }

    [Fact]
    public void Execute_RemovedMolecule_NotInChanges() {
        (_, TurnExecutor executor) = Build(
            new List<MoleculeConfig> { TestData.Simple(1, 5), TestData.Simple(2, 5) },
            new List<ConnectionConfig> { new(1, 2) });

        TurnResult result = executor.Execute(1);

        Assert.DoesNotContain(result.Events.OfType<ValueChangedEvent>(), c => c.MoleculeId == 1);
        Assert.Contains(result.Events.OfType<MoleculeRemovedEvent>(), e => e.MoleculeId == 1);
        Assert.Equal(1, result.RemovedMoleculeId);
    }

    [Fact]
    public void Execute_RemovingNeighbor_MarksItRevealedInChanges() {
        (_, TurnExecutor executor) = Build(
            new List<MoleculeConfig> {
                TestData.Simple(1, 5, revealed: true),
                TestData.Simple(2, 5, revealed: false),
            },
            new List<ConnectionConfig> { new(1, 2) });

        TurnResult result = executor.Execute(1);

        Assert.Contains(result.Events.OfType<MoleculeRevealedEvent>(), e => e.MoleculeId == 2);
    }
}
