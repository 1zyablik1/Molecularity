using System.Collections.Generic;
using System.Linq;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;

namespace Molecularity.Tests;

public class GameSessionTests {
    [Fact]
    public void ClickSimple_DecrementsRemainingByOne() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Simple(1, 3),
                TestData.Simple(2, 5),
                TestData.Simple(3, 5),
            },
            new List<ConnectionConfig> { new(1, 2), new(2, 3) });

        TurnResult result = session.TakeTurn(1);

        Assert.Equal(GameStatus.InProgress, session.Status);
        Assert.Equal(4, session.Graph.GetMolecule(2).Value);
        Assert.Equal(4, session.Graph.GetMolecule(3).Value);
        Assert.All(result.Events.OfType<ValueChangedEvent>().Where(c => c.Reason == ValueChangeReason.Decrement), c => Assert.Equal(-1, c.Delta));
    }

    [Fact]
    public void ClearingGraph_ResultsInWin() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> { TestData.Simple(1, 5), TestData.Simple(2, 5) },
            new List<ConnectionConfig> { new(1, 2) });

        session.TakeTurn(1);
        session.TakeTurn(2);

        Assert.Equal(GameStatus.Win, session.Status);
    }

    [Fact]
    public void MoleculeReachingZero_ResultsInLossWithCulprit() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Simple(1, 5),
                TestData.Simple(2, 1),
                TestData.Simple(3, 5),
            },
            new List<ConnectionConfig> { new(1, 2), new(2, 3) });

        TurnResult result = session.TakeTurn(1);

        Assert.Equal(GameStatus.Lose, session.Status);
        Assert.Equal(2, result.CulpritId);
    }

    [Fact]
    public void ClickingDangerousMolecule_SavesFromLoss() {
        // Molecule 2 sits at 1; clicking it removes it before the loss check.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Simple(1, 5),
                TestData.Simple(2, 1),
            },
            new List<ConnectionConfig> { new(1, 2) });

        session.TakeTurn(2);

        Assert.Equal(GameStatus.InProgress, session.Status);
        Assert.Equal(4, session.Graph.GetMolecule(1).Value);
    }

    [Fact]
    public void TakeTurn_AfterGameOver_Throws() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> { TestData.Simple(1, 5) },
            new List<ConnectionConfig>());

        session.TakeTurn(1); // wins

        Assert.Throws<System.InvalidOperationException>(() => session.TakeTurn(1));
    }
}
