using System.Collections.Generic;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;
using Molecularity.Core.Items;
using Molecularity.Core.Player;

namespace Molecularity.Tests;

/// <summary>
/// Helpers to build levels, graphs and sessions for tests.
/// </summary>
internal static class TestData {
    public static MoleculeConfig Simple(int id, int value, bool revealed = true) =>
        new(id, MoleculeType.Simple, value, revealed);

    public static MoleculeConfig Shield(int id, int value, bool revealed = true) =>
        new(id, MoleculeType.Shield, value, revealed);

    public static MoleculeConfig Parasite(int id, int value, bool revealed = true) =>
        new(id, MoleculeType.Parasite, value, revealed);

    public static MoleculeConfig Anchor(int id, int value, bool revealed = true) =>
        new(id, MoleculeType.Anchor, value, revealed);

    public static LevelConfig Level(List<MoleculeConfig> molecules, List<ConnectionConfig> connections) =>
        new(LevelId: 1, molecules, connections);

    public static MoleculeGraph Graph(List<MoleculeConfig> molecules, List<ConnectionConfig> connections) =>
        LevelBuilder.Build(Level(molecules, connections));

    public static GameSession Session(
        List<MoleculeConfig> molecules,
        List<ConnectionConfig> connections,
        PlayerInventory? inventory = null) =>
        new(Level(molecules, connections), inventory ?? new PlayerInventory());

    public static PlayerInventory InventoryWith(params ILevelItem[] items) {
        var inventory = new PlayerInventory();
        foreach (ILevelItem item in items) {
            inventory.Add(item);
        }

        return inventory;
    }
}
