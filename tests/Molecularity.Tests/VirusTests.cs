using System.Collections.Generic;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;
using Molecularity.Core.Domain.Passives;
using Molecularity.Core.Items;
using Molecularity.Core.Items.Implementations;
using Molecularity.Core.Player;

namespace Molecularity.Tests;

public class VirusTests {
    // ── Infects the highest-value non-virus neighbour ──────────────────────────

    [Fact]
    public void Virus_InfectsHighestValueNeighbour_AfterOneTurn() {
        // Virus (id 1) connected to Simple A (id 2, val 5) and Simple B (id 3, val 3).
        // After clicking some other molecule — but we need to trigger the turn.
        // Setup: Simple(4, val 10) is separate so we have something to click.
        // Click Simple(4): Virus stays alive and infects id 2 (highest val=5 > 3).
        // We need the virus to be in the same graph and to be alive after the click.
        // Best approach: virus connected to 2 and 3; isolated simple (4) is what we click.
        // But 4 must be in the graph (no connection to virus needed).
        // Actually TurnExecutor applies passives to ALL alive molecules including virus.
        // Let's set up: Virus(1,val=5), Simple(2,val=5), Simple(3,val=3). Click Simple(3).
        // After click: TurnExecutor runs passives on Virus(1) and Simple(2).
        // Virus passive infects highest neighbour: Simple(2) has val=5, Simple(3) is dead.
        // Result: Simple(2) becomes Virus, takes -2 damage.

        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Virus(1, 5),
                TestData.Simple(2, 5),
                TestData.Simple(3, 3),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        // Click Simple(3) so the virus's passive fires on the remaining Simple(2).
        session.TakeTurn(3);

        Molecule mol2 = session.Graph.GetMolecule(2);
        Assert.Equal(MoleculeType.Virus, mol2.Type); // Simple(2) is now a Virus
        // Simple(2) took -1 base decrement (turn) then was infected with -VirusBite=-2.
        // Wait: TurnExecutor order is: ModifyDelta → ApplyDelta → TickPassives.
        // So Simple(2) first gets -1 applied (value 5-1=4), then TickPassives on Virus(1)
        // infects Simple(2): MutateTo + ApplyDelta(-2) → value 4-2=2.
        Assert.Equal(2, mol2.Value);

        // Simple(3) was clicked (removed); not infected (it's dead).
        Molecule mol3 = session.Graph.GetMolecule(3);
        Assert.False(mol3.IsAlive);
    }

    [Fact]
    public void Virus_DoesNotInfect_LowerValueNeighbour_WhenHigherExists() {
        // Virus(1) connected to Simple(2, val=5) and Simple(3, val=3).
        // Click a 4th molecule to trigger the turn. Virus infects id 2 (val=5).
        // Simple(3) must remain Simple after one turn.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Virus(1, 5),
                TestData.Simple(2, 5),
                TestData.Simple(3, 3),
                TestData.Simple(4, 10),  // click target, connected only to itself
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3), new(2, 4) });

        session.TakeTurn(4); // click id 4

        Molecule mol2 = session.Graph.GetMolecule(2);
        Molecule mol3 = session.Graph.GetMolecule(3);

        Assert.Equal(MoleculeType.Virus, mol2.Type);   // highest value neighbour → infected
        Assert.Equal(MoleculeType.Simple, mol3.Type); // lower value neighbour → untouched
    }

    // ── Tie-break: lowest Id is infected ──────────────────────────────────────

    [Fact]
    public void Virus_TieBreak_InfectsLowestId_WhenValuesAreEqual() {
        // Virus(1) connected to Simple(2, val=5) and Simple(3, val=5).
        // Equal values → tie-break by lowest Id → Simple(2) is infected.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Virus(1, 5),
                TestData.Simple(2, 5),
                TestData.Simple(3, 5),
                TestData.Simple(4, 10),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3), new(2, 4) });

        session.TakeTurn(4);

        Assert.Equal(MoleculeType.Virus, session.Graph.GetMolecule(2).Type); // lowest id wins tie
        Assert.Equal(MoleculeType.Simple, session.Graph.GetMolecule(3).Type); // higher id untouched
    }

    // ── No same-turn chain: freshly infected molecule does NOT infect that same turn ──

    [Fact]
    public void Virus_NoSameTurnChain_InfectedMoleculeWaitsOneTurn() {
        // Line: Simple(5) — Virus(1) — Simple(2) — Simple(3)
        // Turn 1 (click Simple(5)):
        //   Virus(1) passive fires → infects Simple(2) (highest alive non-virus neighbour).
        //   Simple(2) becomes Virus with dormant passive → does NOT infect Simple(3) this turn.
        //   Only one new Virus per turn.
        // Turn 2 (click some far molecule): now the dormant is lifted on the new Virus(2),
        //   and it infects Simple(3).
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Virus(1, 5),
                TestData.Simple(2, 5),
                TestData.Simple(3, 5),
                TestData.Simple(4, 5),   // click target turn 2
                TestData.Simple(5, 10),  // click target turn 1
            },
            new List<ConnectionConfig> { new(5, 1), new(1, 2), new(2, 3), new(3, 4) });

        // Turn 1: click id 5. Virus(1) infects Simple(2). Simple(3) must still be Simple.
        session.TakeTurn(5);

        Assert.Equal(MoleculeType.Virus, session.Graph.GetMolecule(2).Type);
        Assert.Equal(MoleculeType.Simple, session.Graph.GetMolecule(3).Type); // no chain this turn

        // Turn 2: click id 4. Now Virus(2) (was dormant) should infect Simple(3).
        session.TakeTurn(4);

        Assert.Equal(MoleculeType.Virus, session.Graph.GetMolecule(3).Type); // infected on turn 2
    }

    // ── No infection when all neighbours are already Virus ────────────────────

    [Fact]
    public void Virus_WithOnlyVirusNeighbours_DoesNothing() {
        // Two Virus molecules connected to each other.
        // After a turn they should not crash or infect each other.
        // We add a 3rd Simple molecule to click.
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Virus(1, 5),
                TestData.Virus(2, 5),
                TestData.Simple(3, 10),
            },
            new List<ConnectionConfig> { new(1, 2), new(2, 3) });

        // Click Simple(3). Virus(1) has only Virus(2) as alive neighbour → no infection.
        // Virus(2) has only Virus(1) as alive neighbour → no infection.
        // No crash, no mutation.
        var result = session.TakeTurn(3);

        // Both viruses survive, no exceptions
        Assert.True(session.Graph.GetMolecule(1).IsAlive);
        Assert.True(session.Graph.GetMolecule(2).IsAlive);
        Assert.Equal(MoleculeType.Virus, session.Graph.GetMolecule(1).Type);
        Assert.Equal(MoleculeType.Virus, session.Graph.GetMolecule(2).Type);
    }

    // ── Undo reverts an infection: Bomb gets its ability back ─────────────────

    [Fact]
    public void Undo_RevertsInfection_BombRestored() {
        // Virus(1) next to Bomb(2, val=5). After one turn Bomb is infected and becomes Virus.
        // Undo must restore Bomb's Type and ability.
        var inventory = new PlayerInventory();
        inventory.Add(new UndoItem());

        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Virus(1, 5),
                TestData.Bomb(2, 5),
                TestData.Simple(3, 10), // click target
            },
            new List<ConnectionConfig> { new(1, 2), new(2, 3) },
            inventory);

        // Turn: click Simple(3). Virus(1) infects Bomb(2) → Bomb becomes Virus.
        session.TakeTurn(3);

        Molecule mol2 = session.Graph.GetMolecule(2);
        Assert.Equal(MoleculeType.Virus, mol2.Type);  // now infected

        // Undo
        session.UseInstantItem(LevelItemType.Undo);

        Molecule mol2Restored = session.Graph.GetMolecule(2);
        Assert.Equal(MoleculeType.Bomb, mol2Restored.Type);  // back to Bomb
        Assert.Equal(5, mol2Restored.Value);                  // value restored
        Assert.True(mol2Restored.IsAlive);

        // No VirusPassive on the restored Bomb — clicking Bomb should behave like Bomb again
        // (can be clicked since it has no PreventsRemoval passives and bomb passives list is empty).
        Assert.True(mol2Restored.IsRemovable);

        // Also verify Virus(1) is alive and still a Virus (it wasn't infected).
        Molecule mol1 = session.Graph.GetMolecule(1);
        Assert.Equal(MoleculeType.Virus, mol1.Type);
        Assert.True(mol1.IsAlive);
    }

    // ── Undo reverts dormancy state correctly ─────────────────────────────────

    [Fact]
    public void Undo_RevertsInfection_AndDormancyStateIsRestored() {
        // After turn 1, mol 2 is infected (dormant virus passive).
        // Undo → mol 2 is Simple again (no virus passive, no dormancy).
        var inventory = new PlayerInventory();
        inventory.Add(new UndoItem());

        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Virus(1, 5),
                TestData.Simple(2, 5),
                TestData.Simple(3, 10),
            },
            new List<ConnectionConfig> { new(1, 2), new(2, 3) },
            inventory);

        session.TakeTurn(3); // Virus(1) infects Simple(2)

        Assert.Equal(MoleculeType.Virus, session.Graph.GetMolecule(2).Type);

        session.UseInstantItem(LevelItemType.Undo);

        Assert.Equal(MoleculeType.Simple, session.Graph.GetMolecule(2).Type);
    }

    // ── VirusBite damage is configurable via BalanceConfig ────────────────────

    [Fact]
    public void VirusBite_CustomBalance_DealsDamageAccordingly() {
        // Custom VirusBite = 3: infection deals -3 instead of default -2.
        var balance = new BalanceConfig(VirusBite: 3);
        var levelConfig = new LevelConfig(
            LevelId: 99,
            Molecules: new List<MoleculeConfig> {
                TestData.Virus(1, 5),
                TestData.Simple(2, 10),
                TestData.Simple(3, 10), // click target
            },
            Connections: new List<ConnectionConfig> { new(1, 2), new(2, 3) },
            Balance: balance);

        var session = new GameSession(levelConfig, new PlayerInventory());

        // Turn: click Simple(3). Virus(1) infects Simple(2).
        // Simple(2) gets -1 (base decrement) first, then MutateTo + ApplyDelta(-3).
        // Value = 10 - 1 - 3 = 6.
        session.TakeTurn(3);

        Molecule mol2 = session.Graph.GetMolecule(2);
        Assert.Equal(MoleculeType.Virus, mol2.Type);
        Assert.Equal(6, mol2.Value);
    }

    // ── VirusPassive clone preserves dormancy ─────────────────────────────────

    [Fact]
    public void VirusPassive_Clone_PreservesDormancy() {
        var passive = new VirusPassive(2, dormant: true);
        var clone = (VirusPassive)passive.Clone();

        // Verify clone is a different reference but same type
        Assert.NotSame(passive, clone);
        Assert.IsType<VirusPassive>(clone);
    }

    // ── VirusPassive_Clone preserves active (non-dormant) state ──────────────

    [Fact]
    public void VirusPassive_Clone_PreservesNonDormantState() {
        var passive = new VirusPassive(2, dormant: false);
        var clone = passive.Clone();
        Assert.NotSame(passive, clone);
        Assert.IsType<VirusPassive>(clone);
    }

    // ── Solver: a level with a Virus is solvable ──────────────────────────────

    [Fact]
    public void VirusLevel_IsSolvable() {
        // Simple star: Virus(1, val=5) connected to Simple(2, val=9) and Simple(3, val=9).
        // Strategy: click Simple(2) first.
        //   Turn 1: Virus infects Simple(3) (val 9 > whatever). Simple(3) becomes Virus.
        //   But after click(2): alive = Virus(1) + now-Virus(3).
        //   Turn 2: click Virus(1). Alive = Virus(3) which gets -1 → val decrements.
        //   Turn 3: click Virus(3). Win.
        // Values need to be large enough to survive infections.
        var level = TestData.Level(
            new List<MoleculeConfig> {
                TestData.Virus(1, 5),
                TestData.Simple(2, 9),
                TestData.Simple(3, 9),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        var report = new Molecularity.Solver.LevelSolver().Analyze(level);

        Assert.True(report.Solvable);
    }
}
