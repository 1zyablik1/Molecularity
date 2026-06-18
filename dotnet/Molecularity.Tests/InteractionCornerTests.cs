using System;
using System.Collections.Generic;
using System.Linq;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;
using Molecularity.Core.Items;
using Molecularity.Core.Items.Implementations;
using Molecularity.Core.Player;

namespace Molecularity.Tests;

/// <summary>
/// Cross-feature corner cases: item × molecule-type interactions, balance threading,
/// reveal severing, and undo availability after game over.
/// </summary>
public class InteractionCornerTests {
    // ── Freeze beats Parasite (passive ordering contract: blocking wins) ─────
    [Fact]
    public void Freeze_OnParasite_BlocksItsDecrementThisTurn() {
        var inventory = TestData.InventoryWith(new FreezeItem());
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Parasite(1, 9),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
                TestData.Simple(4, 9), // isolated, clicked to advance the turn
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) },
            inventory);

        session.UseSingleTargetItem(LevelItemType.Freeze, 1);
        session.TakeTurn(4);

        // Parasite would lose 2 (two neighbors), but freeze overrides the delta to 0.
        Assert.Equal(9, session.Graph.GetMolecule(1).Value);
    }

    // ── ChainBreak reduces a Parasite's decrement ───────────────────────────
    [Fact]
    public void ChainBreak_OnParasite_ReducesItsDecrement() {
        var inventory = TestData.InventoryWith(new ChainBreakItem());
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Parasite(1, 9),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
                TestData.Simple(4, 9), // isolated, clicked to advance the turn
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) },
            inventory);

        session.UseDoubleTargetItem(LevelItemType.ChainBreak, 1, 2);
        session.TakeTurn(4);

        // One neighbor severed -> parasite now loses only 1 instead of 2.
        Assert.Equal(8, session.Graph.GetMolecule(1).Value);
    }

    // ── ChainBreak severs the fog-of-war reveal relationship ────────────────
    [Fact]
    public void ChainBreak_SeversRevealRelationship() {
        var inventory = TestData.InventoryWith(new ChainBreakItem());
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Simple(1, 5, revealed: true),
                TestData.Simple(2, 5, revealed: false),
            },
            new List<ConnectionConfig> { new(1, 2) },
            inventory);

        session.UseDoubleTargetItem(LevelItemType.ChainBreak, 1, 2);
        session.TakeTurn(1); // removing 1 no longer reveals 2 (no longer neighbors)

        Assert.False(session.Graph.GetMolecule(2).IsRevealed);
    }

    // ── Base decrement override (balance threaded into TurnExecutor) ─────────
    [Fact]
    public void BaseDecrementOverride_AppliesToAllMolecules() {
        var levelConfig = new LevelConfig(
            LevelId: 1,
            Molecules: new List<MoleculeConfig> {
                TestData.Simple(1, 9),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
            },
            Connections: new List<ConnectionConfig> { new(1, 2), new(2, 3) },
            Balance: new BalanceConfig(BaseDecrement: -2));

        var session = new GameSession(levelConfig, new PlayerInventory());
        session.TakeTurn(1);

        Assert.Equal(7, session.Graph.GetMolecule(2).Value); // 9 - 2
        Assert.Equal(7, session.Graph.GetMolecule(3).Value);
    }

    // ── PlusOneAll twice == revive (+2) ─────────────────────────────────────
    [Fact]
    public void PlusOneAll_UsedTwice_AddsTwoToEveryMolecule() {
        var inventory = TestData.InventoryWith(new PlusOneAllItem(), new PlusOneAllItem());
        GameSession session = TestData.Session(
            new List<MoleculeConfig> { TestData.Simple(1, 3), TestData.Simple(2, 3) },
            new List<ConnectionConfig> { new(1, 2) },
            inventory);

        session.UseInstantItem(LevelItemType.PlusOneAll);
        session.UseInstantItem(LevelItemType.PlusOneAll);

        Assert.Equal(5, session.Graph.GetMolecule(1).Value);
        Assert.Equal(5, session.Graph.GetMolecule(2).Value);
        Assert.Equal(2, session.ItemsUsed);
    }

    // ── Anchor with no alive neighbors emits no ability events ──────────────
    [Fact]
    public void Anchor_WithNoNeighbors_EmitsNoAbilityEvents() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Anchor(1, 5),
                TestData.Simple(2, 5), // isolated; no connection to the anchor
            },
            new List<ConnectionConfig>());

        TurnResult result = session.TakeTurn(1);

        Assert.Empty(result.Events.OfType<ValueChangedEvent>().Where(e => e.Reason == ValueChangeReason.Ability));
        Assert.Contains(result.Events.OfType<MoleculeRemovedEvent>(), e => e.MoleculeId == 1);
    }

    // ── Undo is unavailable after the game is over ──────────────────────────
    [Fact]
    public void Undo_AfterWin_IsUnavailableAndThrows() {
        var inventory = TestData.InventoryWith(new UndoItem());
        GameSession session = TestData.Session(
            new List<MoleculeConfig> { TestData.Simple(1, 5) },
            new List<ConnectionConfig>(),
            inventory);

        session.TakeTurn(1); // clears the graph -> Win

        Assert.Equal(GameStatus.Win, session.Status);
        Assert.False(session.CanUndo);
        Assert.Throws<InvalidOperationException>(() => session.UseInstantItem(LevelItemType.Undo));
    }
}
