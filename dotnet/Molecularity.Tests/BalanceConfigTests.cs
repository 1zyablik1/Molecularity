using System.Collections.Generic;
using System.IO;
using System;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;

namespace Molecularity.Tests;

public class BalanceConfigTests : IDisposable {
    private readonly List<string> _tempDirs = new List<string>();

    private string CreateTempDir() {
        string dir = Path.Combine(Path.GetTempPath(), "MolecularityBalanceTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        _tempDirs.Add(dir);
        return dir;
    }

    public void Dispose() {
        foreach (string dir in _tempDirs) {
            if (Directory.Exists(dir)) {
                Directory.Delete(dir, recursive: true);
            }
        }
    }

    private static void WriteJson(string dir, string fileName, string json) {
        File.WriteAllText(Path.Combine(dir, fileName), json);
    }

    // ── Unit: BalanceConfig.Default ──────────────────────────────────────────

    [Fact]
    public void Default_HasExpectedValues() {
        Assert.Equal(-1, BalanceConfig.Default.BaseDecrement);
        Assert.Equal(2, BalanceConfig.Default.ShieldTurns);
        Assert.Equal(3, BalanceConfig.Default.FreezeTurns);
        Assert.Equal(-2, BalanceConfig.Default.AnchorDecrement);
        Assert.Equal(1, BalanceConfig.Default.AnchorHeal);
    }

    // ── JSON repository: partial balance override ────────────────────────────

    private static readonly string LevelWithPartialBalance = @"{
  ""levelId"": 20,
  ""balance"": { ""shieldTurns"": 3 },
  ""molecules"": [
    { ""id"": 1, ""type"": ""Simple"", ""initialValue"": 5, ""isInitiallyRevealed"": true }
  ],
  ""connections"": []
}";

    private static readonly string LevelWithoutBalance = @"{
  ""levelId"": 21,
  ""molecules"": [
    { ""id"": 1, ""type"": ""Simple"", ""initialValue"": 5, ""isInitiallyRevealed"": true }
  ],
  ""connections"": []
}";

    [Fact]
    public void JsonRepo_PartialBalance_ShieldTurnsOverridden_OtherFieldsAreDefaults() {
        string dir = CreateTempDir();
        WriteJson(dir, "level_20.json", LevelWithPartialBalance);

        var repo = new JsonLevelRepository(dir);
        LevelConfig level = repo.Get(20);

        Assert.NotNull(level.Balance);
        Assert.Equal(3, level.Balance!.ShieldTurns);
        // All other fields must equal GameBalance defaults (not 0!)
        Assert.Equal(GameBalance.BaseDecrement, level.Balance.BaseDecrement);
        Assert.Equal(GameBalance.FreezeTurns, level.Balance.FreezeTurns);
        Assert.Equal(GameBalance.AnchorDecrement, level.Balance.AnchorDecrement);
        Assert.Equal(GameBalance.AnchorHeal, level.Balance.AnchorHeal);
    }

    [Fact]
    public void JsonRepo_NoBalance_LevelConfigBalanceIsNull() {
        string dir = CreateTempDir();
        WriteJson(dir, "level_21.json", LevelWithoutBalance);

        var repo = new JsonLevelRepository(dir);
        LevelConfig level = repo.Get(21);

        Assert.Null(level.Balance);
    }

    // ── Behavioral: custom ShieldTurns = 3 ──────────────────────────────────

    [Fact]
    public void Shield_WithShieldTurns3_SurvivesThreeDecrementsAndTakesDamageOnFourth() {
        // Star: shield (id 1, value 1) with 4 simple neighbours to click one per turn.
        var balance = new BalanceConfig(ShieldTurns: 3);
        var levelConfig = new LevelConfig(
            LevelId: 99,
            Molecules: new List<MoleculeConfig> {
                TestData.Shield(1, 1),
                TestData.Simple(2, 10),
                TestData.Simple(3, 10),
                TestData.Simple(4, 10),
                TestData.Simple(5, 10),
            },
            Connections: new List<ConnectionConfig> { new(1, 2), new(1, 3), new(1, 4), new(1, 5) },
            Balance: balance);

        var session = new GameSession(levelConfig, new Molecularity.Core.Player.PlayerInventory());

        session.TakeTurn(2);
        Assert.Equal(1, session.Graph.GetMolecule(1).Value); // turn 1: shielded

        session.TakeTurn(3);
        Assert.Equal(1, session.Graph.GetMolecule(1).Value); // turn 2: shielded

        session.TakeTurn(4);
        Assert.Equal(1, session.Graph.GetMolecule(1).Value); // turn 3: shielded (extra turn vs default)

        TurnResult result = session.TakeTurn(5);
        Assert.Equal(GameStatus.Lose, session.Status); // turn 4: shield gone, 1 -> 0
        Assert.Equal(1, result.CulpritId);
    }

    // ── Behavioral: custom AnchorDecrement = -3 ─────────────────────────────

    [Fact]
    public void Anchor_WithAnchorDecrement3_LosesThreePerTurn() {
        var balance = new BalanceConfig(AnchorDecrement: -3);
        var levelConfig = new LevelConfig(
            LevelId: 100,
            Molecules: new List<MoleculeConfig> {
                TestData.Anchor(1, 10),
                TestData.Simple(2, 10),
            },
            Connections: new List<ConnectionConfig> { new(1, 2) },
            Balance: balance);

        var session = new GameSession(levelConfig, new Molecularity.Core.Player.PlayerInventory());

        // Click molecule 2 so the anchor takes its decrement each turn.
        // Turn 1: click 2. Anchor ability = no (Simple clicked). Anchor decrement = -3 -> 7.
        // Wait — clicking the Simple, not the Anchor. Anchor ability fires on click of Anchor.
        // Clicking Simple(2): no ability. After removal, Anchor(1) decrements by -3.
        session.TakeTurn(2);

        Assert.Equal(7, session.Graph.GetMolecule(1).Value); // 10 - 3 = 7
    }
}
