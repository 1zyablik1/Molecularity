using System.Collections.Generic;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;
using Molecularity.Core.Domain.Exceptions;

namespace Molecularity.Tests;

public class ShieldLockTests {
    // ── Shield: clicking it while protected throws MoleculeShieldedException ──

    [Fact]
    public void Shield_ClickWhileProtected_ThrowsMoleculeShieldedException() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Shield(1, 5),
                TestData.Simple(2, 5), // keeps the session playable (not Stuck)
            },
            new List<ConnectionConfig> { new(1, 2) });

        Assert.Throws<MoleculeShieldedException>(() => session.TakeTurn(1));
    }

    [Fact]
    public void Shield_ClickWhileProtected_DoesNotConsumeTurn() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Shield(1, 5),
                TestData.Simple(2, 5),
            },
            new List<ConnectionConfig> { new(1, 2) });

        int turnsBefore = session.TurnsTaken;
        try { session.TakeTurn(1); } catch (MoleculeShieldedException) { }
        Assert.Equal(turnsBefore, session.TurnsTaken);
    }

    [Fact]
    public void Shield_ValueDoesNotDrop_WhileProtected() {
        // Star: shield (id 1, value 5) connected to three simples we click
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Shield(1, 5),
                TestData.Simple(2, 10),
                TestData.Simple(3, 10),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        session.TakeTurn(2); // turn 1 — shield active, value stays 5
        Assert.Equal(5, session.Graph.GetMolecule(1).Value);

        session.TakeTurn(3); // turn 2 — shield expires after ticking to 0
        // After 2 turns the shield is done; value should still be 5 (blocked both turns)
        Assert.Equal(5, session.Graph.GetMolecule(1).Value);
    }

    [Fact]
    public void Shield_AfterNTurns_BecomesRemovable() {
        // Shield with 2 turns: after clicking 2 other molecules it should be removable
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Shield(1, 10),
                TestData.Simple(2, 10),
                TestData.Simple(3, 10),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        Assert.False(session.Graph.GetMolecule(1).IsRemovable);

        session.TakeTurn(2); // turn 1: shield still active (1 left)
        // The passive ticks on OnPassiveApply called during TurnExecutor
        // After 2 ticks (2 turns), shield expires
        session.TakeTurn(3); // turn 2: shield expires

        Assert.True(session.Graph.GetMolecule(1).IsRemovable);
    }

    [Fact]
    public void Shield_AfterBecomingRemovable_BehavesLikeSimple() {
        // After protection expires, Shield behaves like Simple (value ticks)
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Shield(1, 3),
                TestData.Simple(2, 10),
                TestData.Simple(3, 10),
                TestData.Simple(4, 10),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3), new(1, 4) });

        session.TakeTurn(2); // turn 1: shield active
        session.TakeTurn(3); // turn 2: shield expires
        // Now mol 1 is removable and should take damage
        session.TakeTurn(4); // turn 3: mol 1 takes -1 -> value 2
        Assert.Equal(2, session.Graph.GetMolecule(1).Value);
    }

    // ── Lock: clicking while protected throws, but value still decrements ──

    [Fact]
    public void Lock_ClickWhileProtected_ThrowsMoleculeShieldedException() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Lock(1, 5),
                TestData.Simple(2, 5), // keeps the session playable (not Stuck)
            },
            new List<ConnectionConfig> { new(1, 2) });

        Assert.Throws<MoleculeShieldedException>(() => session.TakeTurn(1));
    }

    [Fact]
    public void Lock_ValueDecrementsEachTurn_WhileProtected() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Lock(1, 5),
                TestData.Simple(2, 10),
                TestData.Simple(3, 10),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        session.TakeTurn(2); // turn 1: lock active, value 5 -> 4
        Assert.Equal(4, session.Graph.GetMolecule(1).Value);

        session.TakeTurn(3); // turn 2: lock expires (ticked to 0), value 4 -> 3
        Assert.Equal(3, session.Graph.GetMolecule(1).Value);
    }

    [Fact]
    public void Lock_AfterNTurns_BecomesRemovable() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Lock(1, 10),
                TestData.Simple(2, 10),
                TestData.Simple(3, 10),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        Assert.False(session.Graph.GetMolecule(1).IsRemovable);

        session.TakeTurn(2);
        session.TakeTurn(3);

        Assert.True(session.Graph.GetMolecule(1).IsRemovable);
    }

    [Fact]
    public void Lock_CanReachZero_WhileLocked_CausesLoss() {
        // Lock with 2 turns but value 2: after 2 decrements it hits 0 -> Lose
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Lock(1, 2),
                TestData.Simple(2, 10),
                TestData.Simple(3, 10),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        session.TakeTurn(2); // turn 1: lock active, value 2 -> 1
        Assert.Equal(GameStatus.InProgress, session.Status);

        session.TakeTurn(3); // turn 2: value 1 -> 0 -> Lose
        Assert.Equal(GameStatus.Lose, session.Status);
    }

    // ── IsRemovable contract ──

    [Fact]
    public void Simple_IsAlwaysRemovable() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> { TestData.Simple(1, 5) },
            new List<ConnectionConfig>());

        Assert.True(session.Graph.GetMolecule(1).IsRemovable);
    }

    [Fact]
    public void Lazy_IsAlwaysRemovable() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> { TestData.Lazy(1, 5) },
            new List<ConnectionConfig>());

        Assert.True(session.Graph.GetMolecule(1).IsRemovable);
    }

    [Fact]
    public void Shield_IsNotRemovable_WhileActive() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> { TestData.Shield(1, 5) },
            new List<ConnectionConfig>());

        Assert.False(session.Graph.GetMolecule(1).IsRemovable);
    }

    [Fact]
    public void Lock_IsNotRemovable_WhileActive() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> { TestData.Lock(1, 5) },
            new List<ConnectionConfig>());

        Assert.False(session.Graph.GetMolecule(1).IsRemovable);
    }

    // ── Stuck: alive molecules remain but none can be clicked ────────────────

    [Fact]
    public void Stuck_WhenOnlyProtectedMoleculeRemains() {
        // Shield (2 turns) + Simple. Clicking the Simple leaves only the still-active Shield,
        // which can't be clicked and can't tick further (nothing left to click) -> Stuck.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> { TestData.Shield(1, 5), TestData.Simple(2, 5) },
            new List<ConnectionConfig> { new(1, 2) });

        session.TakeTurn(2);

        Assert.Equal(GameStatus.Stuck, session.Status);
    }

    [Fact]
    public void Stuck_DetectedAtConstruction_WhenAllProtected() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> { TestData.Shield(1, 5) },
            new List<ConnectionConfig>());

        Assert.Equal(GameStatus.Stuck, session.Status);
    }

    [Fact]
    public void TakeTurn_WhenStuck_Throws() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> { TestData.Shield(1, 5) },
            new List<ConnectionConfig>());

        Assert.Throws<System.InvalidOperationException>(() => session.TakeTurn(1));
    }
}
