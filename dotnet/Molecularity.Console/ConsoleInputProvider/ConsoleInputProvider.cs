using Molecularity.Core.Domain;
using Molecularity.Core.Interfaces;
using Molecularity.Core.Items;
using Molecularity.Core.Player;

namespace Molecularity.Console.ConsoleInputProvider;

public class ConsoleInputProvider : IInputProvider {
    public PlayerAction RequestAction(GameSession session) {
        while (true) {
            string undoHint = session.CanUndo ? " (undo available)" : "";
            System.Console.WriteLine($"\nAction: [c] click  [i] use item{undoHint}  [q] quit");
            string? input = System.Console.ReadLine()?.Trim().ToLowerInvariant();

            switch (input) {
                case "c": return PlayerAction.Click;
                case "i": return PlayerAction.UseItem;
                case "q": return PlayerAction.Quit;
                default:
                    System.Console.WriteLine("Unknown action. Try again.");
                    break;
            }
        }
    }

    public int RequestMoleculeId(MoleculeGraph graph) {
        System.Console.WriteLine("Enter the ID of the molecule you want to interact with:");
        while (true) {
            string? input = System.Console.ReadLine();

            if (int.TryParse(input, out int moleculeId)) {
                if (graph.TryGetMolecule(moleculeId, out Molecule? molecule)) {
                    if (molecule!.IsAlive) {
                        return moleculeId;
                    }

                    System.Console.WriteLine("Molecule is already removed. Try again.");
                }
                else {
                    System.Console.WriteLine("Invalid molecule ID. Please try again.");
                }
            }
            else {
                System.Console.WriteLine("Invalid input. Please enter a valid integer ID.");
            }
        }
    }

    public LevelItemType RequestItem(PlayerInventory inventory) {
        List<LevelItemType> available = inventory.Items
            .Where(kvp => kvp.Value.Count > 0)
            .Select(kvp => kvp.Key)
            .ToList();

        System.Console.WriteLine("Choose an item:");
        for (int i = 0; i < available.Count; i++) {
            System.Console.WriteLine($"[{i}] {available[i]} x{inventory.Count(available[i])}");
        }

        while (true) {
            string? input = System.Console.ReadLine();
            if (int.TryParse(input, out int index) && index >= 0 && index < available.Count) {
                return available[index];
            }

            System.Console.WriteLine("Invalid choice. Enter the number next to the item.");
        }
    }
}
