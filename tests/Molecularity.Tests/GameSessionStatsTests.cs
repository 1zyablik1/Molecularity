using System;
using System.Collections.Generic;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;
using Molecularity.Core.Items;
using Molecularity.Core.Items.Implementations;
using Molecularity.Core.Player;

namespace Molecularity.Tests;

public class GameSessionStatsTests {
    private static GameSession Session(PlayerInventory? inventory = null) => TestData.Session(
        new List<MoleculeConfig> {
            TestData.Simple(1, 5),
            TestData.Simple(2, 5),
            TestData.Simple(3, 5),
        },
        new List<ConnectionConfig> { new(1, 2), new(2, 3) },
        inventory ?? new PlayerInventory());

    [Fact]
    public void NewSession_StatsAreZero() {
        GameSession session = Session();
        Assert.Equal(0, session.TurnsTaken);
        Assert.Equal(0, session.ItemsUsed);
        Assert.Equal(new SessionStats(0, 0), session.Stats);
    }

    [Fact]
    public void EachTurn_IncrementsTurnsTaken() {
        GameSession session = Session();

        session.TakeTurn(1);
        Assert.Equal(1, session.TurnsTaken);

        session.TakeTurn(2);
        Assert.Equal(2, session.TurnsTaken);
    }

    [Fact]
    public void UsingItems_IncrementsItemsUsed_PerCategory() {
        var inventory = TestData.InventoryWith(new RevealAllItem(), new FreezeItem(), new ChainBreakItem());
        GameSession session = Session(inventory);

        session.UseInstantItem(LevelItemType.RevealAll);
        session.UseSingleTargetItem(LevelItemType.Freeze, 1);
        session.UseDoubleTargetItem(LevelItemType.ChainBreak, 1, 2);

        Assert.Equal(3, session.ItemsUsed);
        Assert.Equal(0, session.TurnsTaken);
    }

    [Fact]
    public void Undo_DecrementsTurnsTaken_AndCountsAsItemUsed() {
        var inventory = TestData.InventoryWith(new UndoItem());
        GameSession session = Session(inventory);

        session.TakeTurn(1);
        Assert.Equal(1, session.TurnsTaken);

        session.UseInstantItem(LevelItemType.Undo);

        Assert.Equal(0, session.TurnsTaken); // the turn was reverted
        Assert.Equal(1, session.ItemsUsed);  // undo item consumed
    }

    [Fact]
    public void FailedItemUse_DoesNotIncrementItemsUsed() {
        GameSession session = Session(new PlayerInventory()); // empty inventory

        Assert.Throws<InvalidOperationException>(() => session.UseInstantItem(LevelItemType.RevealAll));
        Assert.Equal(0, session.ItemsUsed);
    }

    [Fact]
    public void FailedTurn_DoesNotIncrementTurnsTaken() {
        GameSession session = Session();
        session.TakeTurn(1); // removes molecule 1; TurnsTaken = 1

        Assert.Throws<Molecularity.Core.Domain.Exceptions.MoleculeAlreadyRemovedException>(() => session.TakeTurn(1));
        Assert.Equal(1, session.TurnsTaken); // unchanged by the failed turn
    }
}
