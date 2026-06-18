using Molecularity.Core.Domain.Passives;

namespace Molecularity.Tests;

public class PassiveTests {
    [Fact]
    public void ShieldPassive_BlocksDelta_WhileShieldRemains_ThenLetsThrough() {
        var shield = new ShieldPassive(2);

        Assert.Equal(0, shield.ModifyDelta(-1, null!, null!)); // turn 1: blocked
        shield.OnPassiveApply(null!, null!);
        Assert.False(shield.IsExpired);

        Assert.Equal(0, shield.ModifyDelta(-1, null!, null!)); // turn 2: blocked
        shield.OnPassiveApply(null!, null!);
        Assert.False(shield.IsExpired);

        Assert.Equal(-1, shield.ModifyDelta(-1, null!, null!)); // turn 3: damage passes
        shield.OnPassiveApply(null!, null!);
        Assert.True(shield.IsExpired);
    }

    [Fact]
    public void ShieldPassive_Clone_PreservesRemainingShield() {
        var shield = new ShieldPassive(2);
        shield.OnPassiveApply(null!, null!); // 2 -> 1

        IPassiveProperty clone = shield.Clone();

        Assert.Equal(0, clone.ModifyDelta(-1, null!, null!)); // still shielded (1 left)
        clone.OnPassiveApply(null!, null!); // 1 -> 0
        Assert.Equal(-1, clone.ModifyDelta(-1, null!, null!)); // now lets damage through
    }

    [Fact]
    public void FreezePassive_BlocksDelta_AndExpiresAfterNTurns() {
        var freeze = new FreezePassive(2);

        Assert.Equal(0, freeze.ModifyDelta(-1, null!, null!));
        freeze.OnPassiveApply(null!, null!); // 2 -> 1
        Assert.False(freeze.IsExpired);

        freeze.OnPassiveApply(null!, null!); // 1 -> 0
        Assert.True(freeze.IsExpired);
    }

    [Fact]
    public void FreezePassive_OneTurn_ExpiresImmediately() {
        var freeze = new FreezePassive(1);

        freeze.OnPassiveApply(null!, null!); // 1 -> 0
        Assert.True(freeze.IsExpired);
    }

    [Fact]
    public void NoPassive_DoesNotModifyDelta_AndNeverExpires() {
        var passive = new NoPassive();

        Assert.Equal(-1, passive.ModifyDelta(-1, null!, null!));
        passive.OnPassiveApply(null!, null!);
        Assert.False(passive.IsExpired);
    }
}
