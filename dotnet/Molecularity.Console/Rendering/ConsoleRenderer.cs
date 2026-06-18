using Molecularity.Core.Domain;
using Molecularity.Core.Interfaces;

namespace Molecularity.Console.Rendering;

public class ConsoleRenderer : IGameRenderer {
    public void RenderGraph(MoleculeGraph graph) {
        System.Console.WriteLine("\n\n=== Current Molecule Graph ===");
        foreach (Molecule molecule in graph.GetAliveAll()) {
            IEnumerable<Molecule> neighbors = graph.GetAliveNeighbors(molecule.Id);
            string neighborIds = string.Join(", ", neighbors.Select(n => n.Id));
            string value = molecule.IsRevealed ? molecule.Value.ToString() : "??";

            System.Console.WriteLine($"[{molecule.Id}] | {molecule.Type} | {value} | {neighborIds}");
        }
    }

    public void RenderTurnResult(TurnResult result) {
        System.Console.WriteLine($"\nTurn executed on molecule ID: {result.RemovedMoleculeId}");

        foreach (TurnEvent ev in result.Events) {
            switch (ev) {
                case MoleculeRemovedEvent removed:
                    System.Console.WriteLine($"Molecule ID: {removed.MoleculeId} | removed");
                    break;
                case MoleculeRevealedEvent revealed:
                    System.Console.WriteLine($"Molecule ID: {revealed.MoleculeId} | revealed");
                    break;
                case ValueChangedEvent changed:
                    string newValue = changed.IsRevealed ? changed.NewValue.ToString() : "??";
                    string reason = changed.Reason == ValueChangeReason.Ability ? "ability" : "decrement";
                    System.Console.WriteLine($"Molecule ID: {changed.MoleculeId} | Delta: {changed.Delta:+0;-0;0} | New Value: {newValue} | Reason: {reason}");
                    break;
            }
        }
    }

    public void RenderVictory() {
        System.Console.WriteLine("Congratulations! You've won the game!");
    }

    public void RenderDefeat(int? culpritId) {
        System.Console.WriteLine($"Game Over! Molecule ID {culpritId} caused your defeat.");
    }

    public void RenderMessage(string message) {
        System.Console.WriteLine(message);
    }
}
