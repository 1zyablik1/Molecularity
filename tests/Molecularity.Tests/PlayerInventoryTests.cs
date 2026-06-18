using Molecularity.Core.Items;
using Molecularity.Core.Items.Implementations;
using Molecularity.Core.Player;

namespace Molecularity.Tests;

public class PlayerInventoryTests {
    [Fact]
    public void Add_SameType_IncrementsCount() {
        var inventory = new PlayerInventory();
        inventory.Add(new RevealAllItem());
        inventory.Add(new RevealAllItem());

        Assert.Equal(2, inventory.Count(LevelItemType.RevealAll));
    }

    [Fact]
    public void Remove_DecrementsCount() {
        var inventory = new PlayerInventory();
        var item = new RevealAllItem();
        inventory.Add(item);

        inventory.Remove(item);

        Assert.Equal(0, inventory.Count(LevelItemType.RevealAll));
    }

    [Fact]
    public void GetItem_WhenEmpty_ReturnsNull() {
        var inventory = new PlayerInventory();
        Assert.Null(inventory.GetItem(LevelItemType.RevealAll));
    }

    [Fact]
    public void Count_MissingType_ReturnsZero() {
        var inventory = new PlayerInventory();
        Assert.Equal(0, inventory.Count(LevelItemType.Freeze));
    }

    [Fact]
    public void GetItem_ReturnsItemOfRequestedType() {
        var inventory = new PlayerInventory();
        inventory.Add(new FreezeItem());

        ILevelItem? item = inventory.GetItem(LevelItemType.Freeze);

        Assert.NotNull(item);
        Assert.Equal(LevelItemType.Freeze, item!.Type);
    }
}
