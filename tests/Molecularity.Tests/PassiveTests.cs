using Molecularity.Core.Domain.Passives;

namespace Molecularity.Tests;

public class PassiveTests {
    [Fact]
    public void LazyPassive_DecrementsByArithmeticProgression() {
        var lazy = new LazyPassive(1);

        Assert.Equal(-1, lazy.ModifyDelta(-1, null!, null!)); // turn 1: -1
        lazy.OnPassiveApply(null!, null!);
        Assert.Equal(-2, lazy.ModifyDelta(-1, null!, null!)); // turn 2: -2
        lazy.OnPassiveApply(null!, null!);
        Assert.Equal(-3, lazy.ModifyDelta(-1, null!, null!)); // turn 3: -3
        Assert.False(lazy.IsExpired); // never expires — keeps accelerating
    }

    [Fact]
    public void LazyPassive_StepIsTheCommonDifference() {
        var lazy = new LazyPassive(2); // 2, 4, 6, …

        Assert.Equal(-2, lazy.ModifyDelta(-1, null!, null!));
        lazy.OnPassiveApply(null!, null!);
        Assert.Equal(-4, lazy.ModifyDelta(-1, null!, null!));
    }

    [Fact]
    public void LazyPassive_Clone_PreservesProgressionPosition() {
        var lazy = new LazyPassive(1);
        lazy.OnPassiveApply(null!, null!); // current 1 -> 2

        IPassiveProperty clone = lazy.Clone();

        Assert.Equal(-2, clone.ModifyDelta(-1, null!, null!)); // clone continues at -2
        clone.OnPassiveApply(null!, null!);
        Assert.Equal(-3, clone.ModifyDelta(-1, null!, null!));
    }

    [Fact]
    public void LazyPassive_PreventsRemoval_IsFalse() {
        var lazy = new LazyPassive(1);
        Assert.False(lazy.PreventsRemoval);
    }

    [Fact]
    public void ShieldPassive_BlocksDelta_AndPreventsRemoval_WhileActive() {
        var shield = new ShieldPassive(2);

        Assert.True(shield.PreventsRemoval);
        Assert.Equal(0, shield.ModifyDelta(-1, null!, null!));

        shield.OnPassiveApply(null!, null!); // 2 -> 1
        Assert.True(shield.PreventsRemoval);

        shield.OnPassiveApply(null!, null!); // 1 -> 0
        Assert.False(shield.PreventsRemoval);
        Assert.Equal(-1, shield.ModifyDelta(-1, null!, null!));

        shield.OnPassiveApply(null!, null!);
        Assert.True(shield.IsExpired);
    }

    [Fact]
    public void ShieldPassive_Clone_PreservesState() {
        var shield = new ShieldPassive(2);
        shield.OnPassiveApply(null!, null!); // 2 -> 1

        IPassiveProperty clone = shield.Clone();

        Assert.True(clone.PreventsRemoval); // still has 1 turn left
        clone.OnPassiveApply(null!, null!); // 1 -> 0
        Assert.False(clone.PreventsRemoval);
    }

    [Fact]
    public void LockPassive_DoesNotBlockDelta_ButPreventsRemoval() {
        var lockP = new LockPassive(2);

        Assert.True(lockP.PreventsRemoval);
        Assert.Equal(-1, lockP.ModifyDelta(-1, null!, null!)); // delta passes through

        lockP.OnPassiveApply(null!, null!); // 2 -> 1
        Assert.True(lockP.PreventsRemoval);

        lockP.OnPassiveApply(null!, null!); // 1 -> 0
        Assert.False(lockP.PreventsRemoval);

        lockP.OnPassiveApply(null!, null!);
        Assert.True(lockP.IsExpired);
    }

    [Fact]
    public void LockPassive_Clone_PreservesState() {
        var lockP = new LockPassive(2);
        lockP.OnPassiveApply(null!, null!); // 2 -> 1

        IPassiveProperty clone = lockP.Clone();

        Assert.True(clone.PreventsRemoval); // still 1 turn left
        clone.OnPassiveApply(null!, null!); // 1 -> 0
        Assert.False(clone.PreventsRemoval);
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
