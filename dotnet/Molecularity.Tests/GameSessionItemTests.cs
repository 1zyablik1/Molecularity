using System;
using System.Collections.Generic;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;
using Molecularity.Core.Items;
using Molecularity.Core.Items.Implementations;
using Molecularity.Core.Player;

namespace Molecularity.Tests;

public class GameSessionItemTests {
    private static GameSession SessionWith(PlayerInventory inventory) =>
        TestData.Session(
            new List<MoleculeConfig> { TestData.Simple(1, 5), TestData.Simple(2, 5) },
            new List<ConnectionConfig> { new(1, 2) },
            inventory);

    [Fact]
    public void UseInstantItem_OnSingleTargetItem_Throws() {
        GameSession session = SessionWith(TestData.InventoryWith(new FreezeItem()));
        Assert.Throws<InvalidOperationException>(() => session.UseInstantItem(LevelItemType.Freeze));
    }

    [Fact]
    public void UseSingleTargetItem_OnInstantItem_Throws() {
        GameSession session = SessionWith(TestData.InventoryWith(new RevealAllItem()));
        Assert.Throws<InvalidOperationException>(() => session.UseSingleTargetItem(LevelItemType.RevealAll, 1));
    }

    [Fact]
    public void UseDoubleTargetItem_OnInstantItem_Throws() {
        GameSession session = SessionWith(TestData.InventoryWith(new PlusOneAllItem()));
        Assert.Throws<InvalidOperationException>(() => session.UseDoubleTargetItem(LevelItemType.PlusOneAll, 1, 2));
    }

    [Fact]
    public void UseSingleTargetItem_NotInInventory_Throws() {
        GameSession session = SessionWith(new PlayerInventory());
        Assert.Throws<InvalidOperationException>(() => session.UseSingleTargetItem(LevelItemType.Freeze, 1));
    }

    [Fact]
    public void UseDoubleTargetItem_NotInInventory_Throws() {
        GameSession session = SessionWith(new PlayerInventory());
        Assert.Throws<InvalidOperationException>(() => session.UseDoubleTargetItem(LevelItemType.ChainBreak, 1, 2));
    }
}
