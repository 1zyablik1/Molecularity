namespace Molecularity.Core.Domain.Abilities {
    /// <summary>
    /// Anchor on-click ability: heals every alive neighbor by <see cref="GameBalance.AnchorHeal"/>.
    /// Fires before the molecule is removed and before the global decrement.
    /// </summary>
    public class HealNeighborsAbility : IAbility {
        public void Execute(Molecule source, MoleculeGraph graph) {
            foreach (Molecule neighbor in graph.GetAliveNeighbors(source.Id)) {
                neighbor.ApplyDelta(GameBalance.AnchorHeal);
            }
        }
    }
}
