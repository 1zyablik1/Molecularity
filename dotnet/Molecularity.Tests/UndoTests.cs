using System.Collections.Generic;
using System.Linq;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;
using Molecularity.Core.Items;
using Molecularity.Core.Items.Implementations;
using Molecularity.Core.Player;

namespace Molecularity.Tests;

public class UndoTests {
    private static GameSession SessionWithUndo(int undoItems = 1) {
        var inventory = new PlayerInventory();
        for (int i = 0; i < undoItems; i++) {
            inventory.Add(new UndoItem());
        }

        return TestData.Session(
            new List<MoleculeConfig> {
                TestData.Simple(1, 5),
                TestData.Simple(2, 5),
                TestData.Simple(3, 5),
            },
            new List<ConnectionConfig> { new(1, 2), new(2, 3) },
            inventory);
    }

    [Fact]
    public void Undo_RestoresStateOfLastTurn_AndConsumesItem() {
        GameSession session = SessionWithUndo();

        session.TakeTurn(1);
        Assert.True(session.CanUndo);

        session.UseInstantItem(LevelItemType.Undo);

        // Molecule 1 is alive again with original value; others restored.
        Assert.True(session.Graph.GetMolecule(1).IsAlive);
        Assert.Equal(5, session.Graph.GetMolecule(2).Value);
        Assert.Equal(5, session.Graph.GetMolecule(3).Value);
        Assert.False(session.CanUndo); // item consumed + no snapshot
    }

    [Fact]
    public void Undo_CannotBeUsedTwiceInARow() {
        GameSession session = SessionWithUndo(undoItems: 2);

        session.TakeTurn(1);
        session.UseInstantItem(LevelItemType.Undo);

        Assert.False(session.CanUndo);
        Assert.Throws<System.InvalidOperationException>(
            () => session.UseInstantItem(LevelItemType.Undo));
    }

    [Fact]
    public void Undo_AvailableAgainAfterAnotherTurn_WithSpareItem() {
        GameSession session = SessionWithUndo(undoItems: 2);

        session.TakeTurn(1);
        session.UseInstantItem(LevelItemType.Undo);
        session.TakeTurn(2);

        Assert.True(session.CanUndo); // second undo item still in inventory
    }

    [Fact]
    public void CanUndo_FalseBeforeAnyTurn() {
        GameSession session = SessionWithUndo();
        Assert.False(session.CanUndo);
    }

    [Fact]
    public void Undo_RestoresPassiveCountdown_NotJustValues() {
        // Shield (id 1, value 1) protects for 2 turns. We spend both protected turns,
        // then undo the second turn. If undo restored only values but NOT the shield's
        // internal countdown, the redone turn would break through the shield and lose.
        var inventory = new PlayerInventory();
        inventory.Add(new UndoItem());

        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Shield(1, 1),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
                TestData.Simple(4, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3), new(1, 4) },
            inventory);

        session.TakeTurn(2); // shield turn 1 (protected)
        session.TakeTurn(3); // shield turn 2 (protected) — shield countdown now exhausted

        session.UseInstantItem(LevelItemType.Undo); // revert turn on molecule 3

        TurnResult redo = session.TakeTurn(3); // molecule 3 alive again, re-click it

        Assert.Equal(GameStatus.InProgress, session.Status); // still protected → not a loss
        Assert.Equal(1, session.Graph.GetMolecule(1).Value);
        Assert.DoesNotContain(redo.Events.OfType<ValueChangedEvent>(), c => c.MoleculeId == 1 && c.Delta != 0);
    }
}
