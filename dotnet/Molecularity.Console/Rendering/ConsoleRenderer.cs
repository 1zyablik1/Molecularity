using Molecularity.Core.Domain;
using Molecularity.Core.Interfaces;

namespace Molecularity.Console.Rendering;

public class ConsoleRenderer : IGameRenderer {
    public void RenderGraph(MoleculeGraph graph) {
        System.Console.WriteLine("\n\n=== Current Molecule Graph ===");
        foreach (Molecule molecule in graph.GetAliveAll()) {
            IEnumerable<Molecule> neighbors = graph.GetAliveNeighbors(molecule.Id);
            string neighborIds = string.Join(", ", neighbors.Select(n => n.Id));

            System.Console.WriteLine($"[{molecule.Id}] | {molecule.Type} | {molecule.Value} | {neighborIds}");
        }
    }

    public void RenderTurnResult(TurnResult result) {
        System.Console.WriteLine($"\nTurn executed on molecule ID: {result.RemovedMoleculeId}");

        foreach (MoleculeValueChange change in result.Changes) {
            System.Console.WriteLine($"Molecule ID: {change.MoleculeId} | Value Change: {change.Delta} | New Value: {change.NewValue}");
        }
    }

    public void RenderVictory() {
        System.Console.WriteLine("Congratulations! You've won the game!");
    }

    public void RenderDefeat(int culpritId) {
        System.Console.WriteLine($"Game Over! Molecule ID {culpritId} caused your defeat.");
    }

    public void RenderMessage(string message) {
        System.Console.WriteLine(message);
    }
}
