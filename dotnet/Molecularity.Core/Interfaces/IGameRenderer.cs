using Molecularity.Core.Domain;

namespace Molecularity.Core.Interfaces {
    public interface IGameRenderer {
        void RenderGraph(MoleculeGraph graph);
        void RenderTurnResult(TurnResult result);
        void RenderVictory();
        void RenderDefeat(int culpritId);
        void RenderMessage(string message);
    }
}
