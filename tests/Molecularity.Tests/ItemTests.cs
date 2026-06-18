using System.Collections.Generic;
using System.Linq;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;
using Molecularity.Core.Items;
using Molecularity.Core.Items.Implementations;
using Molecularity.Core.Player;

namespace Molecularity.Tests;

public class ItemTests {
    [Fact]
    public void RevealAll_RevealsEveryAliveMolecule() {
        PlayerInventory inventory = TestData.InventoryWith(new RevealAllItem());
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Simple(1, 3, revealed: false),
                TestData.Simple(2, 3, revealed: false),
            },
            new List<ConnectionConfig> { new(1, 2) },
            inventory);

        session.UseInstantItem(LevelItemType.RevealAll);

        Assert.True(session.Graph.GetMolecule(1).IsRevealed);
        Assert.True(session.Graph.GetMolecule(2).IsRevealed);
        Assert.Equal(0, inventory.Count(LevelItemType.RevealAll));
    }

    [Fact]
    public void PlusOneAll_IncreasesEveryAliveValue() {
        PlayerInventory inventory = TestData.InventoryWith(new PlusOneAllItem());
        GameSession session = TestData.Session(
            new List<MoleculeConfig> { TestData.Simple(1, 3), TestData.Simple(2, 2) },
            new List<ConnectionConfig> { new(1, 2) },
            inventory);

        session.UseInstantItem(LevelItemType.PlusOneAll);

        Assert.Equal(4, session.Graph.GetMolecule(1).Value);
        Assert.Equal(3, session.Graph.GetMolecule(2).Value);
    }

    [Fact]
    public void ChainBreak_RemovesConnection() {
        PlayerInventory inventory = TestData.InventoryWith(new ChainBreakItem());
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Simple(1, 3),
                TestData.Simple(2, 3),
                TestData.Simple(3, 3),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) },
            inventory);

        session.UseDoubleTargetItem(LevelItemType.ChainBreak, 1, 2);

        List<int> neighbors = session.Graph.GetAliveNeighbors(1).Select(m => m.Id).ToList();
        Assert.Equal(new[] { 3 }, neighbors);
    }

    [Fact]
    public void Freeze_PreventsDecrementOnNextTurn() {
        PlayerInventory inventory = TestData.InventoryWith(new FreezeItem());
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Simple(1, 3),
                TestData.Simple(2, 3),
                TestData.Simple(3, 3),
            },
            new List<ConnectionConfig> { new(1, 2), new(2, 3) },
            inventory);

        session.UseSingleTargetItem(LevelItemType.Freeze, 2);
        session.TakeTurn(1); // click 1; molecule 2 is frozen and must not lose value

        Assert.Equal(3, session.Graph.GetMolecule(2).Value);
        Assert.Equal(2, session.Graph.GetMolecule(3).Value); // molecule 3 decremented normally
    }

    [Fact]
    public void UseInstantItem_WhenNotInInventory_Throws() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> { TestData.Simple(1, 3) },
            new List<ConnectionConfig>());

        Assert.Throws<System.InvalidOperationException>(
            () => session.UseInstantItem(LevelItemType.RevealAll));
    }
}
