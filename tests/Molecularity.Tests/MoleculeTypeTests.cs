using System.Collections.Generic;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;

namespace Molecularity.Tests;

public class MoleculeTypeTests {
    // ---------- Parasite ----------

    [Fact]
    public void Parasite_DecrementsByAliveNeighborCount() {
        // Parasite (id 1) has 2 neighbors (2, 3). We click an unrelated molecule (4),
        // so the parasite keeps both neighbors and must lose 2.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Parasite(1, 9),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
                TestData.Simple(4, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        session.TakeTurn(4); // 4 is isolated; parasite still has neighbors 2 and 3

        Assert.Equal(7, session.Graph.GetMolecule(1).Value); // 9 - 2
    }

    [Fact]
    public void Parasite_RemovingNeighbor_ReducesItsDecrement() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Parasite(1, 9),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        session.TakeTurn(2); // remove one neighbor; parasite now has a single neighbor

        Assert.Equal(8, session.Graph.GetMolecule(1).Value); // 9 - 1
    }

    [Fact]
    public void Parasite_WithNoNeighbors_DoesNotDecrement() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Parasite(1, 5),
                TestData.Simple(2, 5),
            },
            new List<ConnectionConfig>()); // no connections at all

        session.TakeTurn(2);

        Assert.Equal(5, session.Graph.GetMolecule(1).Value); // 0 neighbors -> 0 decrement
    }

    // ---------- Anchor ----------

    [Fact]
    public void Anchor_OnClick_HealsNeighbors_NetZeroAfterDecrement() {
        // Clicking the anchor heals neighbors +1, then the global -1 applies: net 0.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Anchor(1, 5),
                TestData.Simple(2, 5),
                TestData.Simple(3, 5),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        session.TakeTurn(1);

        Assert.Equal(5, session.Graph.GetMolecule(2).Value);
        Assert.Equal(5, session.Graph.GetMolecule(3).Value);
    }

    [Fact]
    public void Anchor_WhenNotClicked_DecrementsByTwo() {
        // Click molecule 3; the anchor stays on the field and loses 2 this turn.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Anchor(1, 9),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(2, 3) });

        session.TakeTurn(3);

        Assert.Equal(7, session.Graph.GetMolecule(1).Value); // 9 - 2
        Assert.Equal(8, session.Graph.GetMolecule(2).Value); // 9 - 1 (simple)
    }
}
