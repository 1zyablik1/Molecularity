using System;
using Molecularity.Core.Data;
using Molecularity.Core.Items;
using Molecularity.Core.Player;

namespace Molecularity.Core.Domain {
    public class GameSession {
        public GameStatus Status { get; private set; }
        private readonly TurnExecutor _turnExecutor;

        public MoleculeGraph Graph { get; private set; }

        private readonly PlayerInventory _inventory;

        private GraphSnapshot? _previousSnapshot;
        private bool _lastActionWasUndo;

        public bool CanUndo => _previousSnapshot != null
                               && !_lastActionWasUndo
                               && Status == GameStatus.InProgress
                               && _inventory.Count(LevelItemType.Undo) > 0;

        public GameSession(LevelConfig levelConfig, PlayerInventory inventory) {
            _inventory = inventory;
            Graph = LevelBuilder.Build(levelConfig);
            Status = GameStatus.InProgress;

            _turnExecutor = new TurnExecutor(Graph);
        }

        public TurnResult TakeTurn(int moleculeId) {
            if (Status != GameStatus.InProgress) {
                throw new InvalidOperationException("Game is already over.");
            }

            _previousSnapshot = Graph.TakeSnapshot();
            _lastActionWasUndo = false;
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

        public void UseInstantItem(LevelItemType type) {
            ILevelItem? iLevelItem = _inventory.GetItem(type);
            if (iLevelItem == null) {
                throw new InvalidOperationException($"No instant item of type {type} available.");
            }

            if (iLevelItem is not IInstantItem item) {
                throw new InvalidOperationException($"Item of type {type} is not an instant item.");
            }

            // Undo is a special instant item: it does not act on the graph via Use(),
            // it consumes the snapshot taken before the last turn and reverts it immediately.
            if (type == LevelItemType.Undo) {
                if (!CanUndo) {
                    throw new InvalidOperationException("Undo not available.");
                }

                _inventory.Remove(item);
                Graph.RestoreSnapshot(_previousSnapshot!);
                _previousSnapshot = null;
                _lastActionWasUndo = true;
                return;
            }

            _inventory.Remove(item);
            item.Use(Graph);
        }

        public void UseSingleTargetItem(LevelItemType type, int targetId) {
            ILevelItem? iLevelItem = _inventory.GetItem(type);
            if (iLevelItem == null) {
                throw new InvalidOperationException($"No single target item of type {type} available.");
            }

            if (iLevelItem is not ISingleTargetItem item) {
                throw new InvalidOperationException($"Item of type {type} is not a single target item.");
            }

            _inventory.Remove(item);
            item.Use(targetId, Graph);
        }

        public void UseDoubleTargetItem(LevelItemType type, int fromId, int toId) {
            ILevelItem? iLevelItem = _inventory.GetItem(type);
            if (iLevelItem == null) {
                throw new InvalidOperationException($"No double target item of type {type} available.");
            }

            if (iLevelItem is not IDoubleTargetItem item) {
                throw new InvalidOperationException($"Item of type {type} is not a double target item.");
            }

            _inventory.Remove(item);
            item.Use(fromId, toId, Graph);
        }
    }
}
