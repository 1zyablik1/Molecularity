using System;
using Molecularity.Core.Data;

namespace Molecularity.Core.Domain {
    public class GameSession {
        public GameStatus Status { get; private set; }
        private readonly TurnExecutor _turnExecutor;

        public MoleculeGraph Graph { get; private set; }

        public GameSession(LevelConfig levelConfig) {
            Graph = LevelBuilder.Build(levelConfig);
            Status = GameStatus.InProgress;

            _turnExecutor = new TurnExecutor(Graph);
        }

        public TurnResult TakeTurn(int moleculeId) {
            if (Status != GameStatus.InProgress) {
                throw new InvalidOperationException("Game is already over.");
            }

            TurnResult result = _turnExecutor.Execute(moleculeId);

            (bool IsLoss, int? CulpritId) lose = GameRules.IsLose(Graph);
            if (lose.IsLoss) {
                Status = GameStatus.Lose;
                return result with { CulpritId = lose.CulpritId };
            }

            if (GameRules.IsWin(Graph)) {
                Status = GameStatus.Win;
            }

            return result;
        }
    }
}
