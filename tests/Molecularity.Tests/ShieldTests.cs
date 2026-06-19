using System.Collections.Generic;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;

namespace Molecularity.Tests;

public class LazyTests {
    [Fact]
    public void Lazy_Decrement_Accelerates_AndCanKillIfLeftTooLong() {
        // Lazy (id 1) decays by an arithmetic progression: -1, -2, -3, …
        // Star: lazy connected to three simples we click one by one.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Lazy(1, 6),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
                TestData.Simple(4, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3), new(1, 4) });

        session.TakeTurn(2);
        Assert.Equal(5, session.Graph.GetMolecule(1).Value); // turn 1: -1 → 5

        session.TakeTurn(3);
        Assert.Equal(3, session.Graph.GetMolecule(1).Value); // turn 2: -2 → 3

        TurnResult result = session.TakeTurn(4);
        Assert.Equal(GameStatus.Lose, session.Status); // turn 3: -3 → 0
        Assert.Equal(1, result.CulpritId);
    }

    [Fact]
    public void Lazy_IsRemovable_AtAnyTime() {
        // A Lazy molecule can always be clicked; it is not unremovable.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Lazy(1, 5),
            },
            new List<ConnectionConfig>());

        Assert.True(session.Graph.GetMolecule(1).IsRemovable);
        session.TakeTurn(1); // should succeed
        Assert.Equal(GameStatus.Win, session.Status);
    }
}
