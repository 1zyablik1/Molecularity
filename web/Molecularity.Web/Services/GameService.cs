using System;
using System.Collections.Generic;
using System.Linq;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;
using Molecularity.Core.Domain.Exceptions;
using Molecularity.Core.Items;
using Molecularity.Core.Items.Implementations;
using Molecularity.Core.Player;

namespace Molecularity.Web.Services {

    /// <summary>
    /// View-model representation of an alive molecule with its graph position info.
    /// </summary>
    public sealed record MoleculeViewModel(int Id, string Type, int Value, bool IsRevealed, bool IsRemovable);

    /// <summary>
    /// View-model representation of a connection between two alive molecules.
    /// </summary>
    public sealed record ConnectionViewModel(int FromId, int ToId);

    /// <summary>
    /// Snapshot of inventory counts keyed by item type name.
    /// </summary>
    public sealed record InventoryViewModel(IReadOnlyDictionary<string, int> Counts);

    /// <summary>
    /// Result returned after every mutating action.
    /// </summary>
    public sealed record ActionResult(
        string Message,
        GameStatus Status,
        int? CulpritId,
        IReadOnlyList<MoleculeViewModel> Molecules,
        IReadOnlyList<ConnectionViewModel> Connections,
        InventoryViewModel Inventory,
        int TurnsTaken,
        int ItemsUsed,
        bool CanUndo);

    /// <summary>
    /// In-browser game logic service.  Wraps GameSession + PlayerInventory.
    /// All game rules are delegated entirely to Molecularity.Core — this class
    /// only marshals calls and converts results to view-model types.
    /// </summary>
    public sealed class GameService {
        private const int ItemsPerType = 10;

        private GameSession? _session;
        private PlayerInventory? _inventory;
        private int _activeLevelId;

        public bool HasActiveGame => _session != null;

        /// <summary>Start (or restart) a level.</summary>
        public ActionResult Start(LevelConfig level) {
            _inventory = CreateInventory(level);
            _session = new GameSession(level, _inventory);
            _activeLevelId = level.LevelId;
            return BuildResult("Уровень начат", null);
        }

        /// <summary>Take a regular turn: remove the molecule with the given id.</summary>
        public ActionResult Turn(int moleculeId) {
            EnsureSession();
            EnsureInProgress();

            Molecule target = _session!.Graph.GetMolecule(moleculeId);
            if (!target.IsAlive) {
                throw new InvalidOperationException("Эта молекула уже удалена.");
            }

            TurnResult result;
            try {
                result = _session.TakeTurn(moleculeId);
            } catch (MoleculeShieldedException) {
                return BuildResult($"Молекула {moleculeId} защищена и не может быть удалена сейчас.", null);
            }

            string message = _session.Status switch {
                GameStatus.Win  => "Уровень пройден",
                GameStatus.Lose => "Молекула распалась. Попробуйте ещё раз",
                _               => $"Молекула {moleculeId} удалена"
            };

            return BuildResult(message, result.CulpritId);
        }

        /// <summary>Use an instant item (RevealAll, PlusOneAll, Undo).</summary>
        public ActionResult UseInstantItem(LevelItemType type) {
            EnsureSession();
            EnsureInProgress();
            _session!.UseInstantItem(type);
            return BuildResult($"Использован предмет {type}", null);
        }

        /// <summary>Use a single-target item (Freeze) on the given molecule.</summary>
        public ActionResult UseSingleTargetItem(LevelItemType type, int targetId) {
            EnsureSession();
            EnsureInProgress();
            EnsureAlive(targetId);
            _session!.UseSingleTargetItem(type, targetId);
            return BuildResult($"Использован предмет {type}", null);
        }

        /// <summary>Use a double-target item (ChainBreak) on the given connected pair.</summary>
        public ActionResult UseDoubleTargetItem(LevelItemType type, int fromId, int toId) {
            EnsureSession();
            EnsureInProgress();
            EnsureAlive(fromId);
            EnsureAlive(toId);
            EnsureConnected(fromId, toId);
            _session!.UseDoubleTargetItem(type, fromId, toId);
            return BuildResult($"Использован предмет {type}", null);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private ActionResult BuildResult(string message, int? culpritId) {
            GraphSnapshot snapshot = _session!.Graph.TakeSnapshot();

            var molecules = snapshot.Molecules
                .Where(m => m.IsAlive)
                .Select(m => {
                    Molecule source = _session.Graph.GetMolecule(m.Id);
                    return new MoleculeViewModel(m.Id, source.Type.ToString(), m.Value, m.IsRevealed, source.IsRemovable);
                })
                .ToList();

            var connections = snapshot.Connections
                .Select(c => new ConnectionViewModel(c.FromId, c.ToId))
                .ToList();

            var inventoryCounts = Enum.GetValues<LevelItemType>()
                .ToDictionary(t => t.ToString(), t => _inventory!.Count(t));

            return new ActionResult(
                message,
                _session.Status,
                culpritId,
                molecules,
                connections,
                new InventoryViewModel(inventoryCounts),
                _session.TurnsTaken,
                _session.ItemsUsed,
                _session.CanUndo);
        }

        private void EnsureSession() {
            if (_session == null) throw new InvalidOperationException("Нет активной игры.");
        }

        private void EnsureInProgress() {
            if (_session!.Status != GameStatus.InProgress)
                throw new InvalidOperationException("Игра уже завершена. Перезапустите уровень.");
        }

        private void EnsureAlive(int id) {
            if (!_session!.Graph.GetMolecule(id).IsAlive)
                throw new InvalidOperationException("Цель уже удалена.");
        }

        private void EnsureConnected(int from, int to) {
            GraphSnapshot snapshot = _session!.Graph.TakeSnapshot();
            bool connected = snapshot.Connections.Any(c =>
                (c.FromId == from && c.ToId == to) || (c.FromId == to && c.ToId == from));
            if (!connected)
                throw new InvalidOperationException("Между выбранными молекулами нет связи.");
        }

        private static PlayerInventory CreateInventory(LevelConfig level) {
            var inventory = new PlayerInventory();
            int freezeTurns = (level.Balance ?? BalanceConfig.Default).FreezeTurns;
            for (int i = 0; i < ItemsPerType; i++) {
                inventory.Add(new RevealAllItem());
                inventory.Add(new PlusOneAllItem());
                inventory.Add(new FreezeItem(freezeTurns));
                inventory.Add(new ChainBreakItem());
                inventory.Add(new UndoItem());
            }
            return inventory;
        }
    }
}
