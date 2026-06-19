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
        Assert.Equal(1, BalanceConfig.Default.LazyStep);
        Assert.Equal(2, BalanceConfig.Default.ShieldTurns);
        Assert.Equal(2, BalanceConfig.Default.LockTurns);
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

    // ── Behavioral: custom LazyStep = 2 (decrements 2, 4, 6, …) ─────────────

    [Fact]
    public void Lazy_WithStep2_DecrementsByTwoFourSix() {
        // Star: lazy (id 1) with 3 simple neighbours; click one per turn and watch the
        // lazy molecule's accelerating decay with a common difference of 2.
        var balance = new BalanceConfig(LazyStep: 2);
        var levelConfig = new LevelConfig(
            LevelId: 99,
            Molecules: new List<MoleculeConfig> {
                TestData.Lazy(1, 20),
                TestData.Simple(2, 10),
                TestData.Simple(3, 10),
                TestData.Simple(4, 10),
            },
            Connections: new List<ConnectionConfig> { new(1, 2), new(1, 3), new(1, 4) },
            Balance: balance);

        var session = new GameSession(levelConfig, new Molecularity.Core.Player.PlayerInventory());

        session.TakeTurn(2);
        Assert.Equal(18, session.Graph.GetMolecule(1).Value); // turn 1: -2

        session.TakeTurn(3);
        Assert.Equal(14, session.Graph.GetMolecule(1).Value); // turn 2: -4

        session.TakeTurn(4);
        Assert.Equal(8, session.Graph.GetMolecule(1).Value);  // turn 3: -6
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
