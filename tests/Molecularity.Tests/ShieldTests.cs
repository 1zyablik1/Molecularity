using System.Collections.Generic;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;

namespace Molecularity.Tests;

public class ShieldTests {
    [Fact]
    public void Shield_BlocksDecrement_ForTwoTurns_ThenTakesDamage() {
        // Star: shield (id 1, value 1) connected to three simples we click one by one.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Shield(1, 1),
                TestData.Simple(2, 5),
                TestData.Simple(3, 5),
                TestData.Simple(4, 5),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3), new(1, 4) });

        session.TakeTurn(2);
        Assert.Equal(1, session.Graph.GetMolecule(1).Value); // turn 1: shielded

        session.TakeTurn(3);
        Assert.Equal(1, session.Graph.GetMolecule(1).Value); // turn 2: shielded

        TurnResult result = session.TakeTurn(4);
        Assert.Equal(GameStatus.Lose, session.Status); // turn 3: shield gone, 1 -> 0
        Assert.Equal(1, result.CulpritId);
    }
}
