using Molecularity.Core.Domain;
using Molecularity.Core.Interfaces;

namespace Molecularity.Console.ConsoleInputProvider;

public class ConsoleInputProvider : IInputProvider {
    public int RequestMoleculeId(MoleculeGraph graph) {
        System.Console.WriteLine("Enter the ID of the molecule you want to interact with:");
        while (true) {
            string? input = System.Console.ReadLine();

            if (int.TryParse(input, out int moleculeId)) {
                try {
                    Molecule molecule = graph.GetMolecule(moleculeId);
                    if (molecule.IsAlive) {
                        return moleculeId;
                    }

                    System.Console.WriteLine("Molecule is already removed. Try again.");
                }
                catch (Exception) {
                    System.Console.WriteLine("Invalid molecule ID. Please try again.");
                }
            }
            else {
                System.Console.WriteLine("Invalid input. Please enter a valid integer ID.");
            }
        }
    }
}
