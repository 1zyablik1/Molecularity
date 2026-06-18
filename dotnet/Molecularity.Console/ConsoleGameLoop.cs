using Molecularity.Core.Data;
using Molecularity.Core.Domain;
using Molecularity.Core.Interfaces;
using Molecularity.Core.Items;
using Molecularity.Core.Items.Implementations;
using Molecularity.Core.Player;

namespace Molecularity.Console;

public class ConsoleGameLoop {
    private readonly IGameRenderer _renderer;
    private readonly IInputProvider _input;
    private readonly ILevelRepository _repository;

    public ConsoleGameLoop(ILevelRepository repository, IGameRenderer renderer, IInputProvider input) {
        _repository = repository;
        _renderer = renderer;
        _input = input;
    }

    public void Run() {
        //TODO: allow user to select level
        LevelConfig level = _repository.Get(1);
        //TODO: update inventory
        PlayerInventory inventory = MockInventory();
        var session = new GameSession(level, inventory);

        Loop(session, inventory);
    }

    private void Loop(GameSession session, PlayerInventory inventory) {
        while (session.Status == GameStatus.InProgress) {
            _renderer.RenderGraph(session.Graph);
            RenderInventory(inventory);

            PlayerAction action = _input.RequestAction(session);
            switch (action) {
                case PlayerAction.Click:
                    TakeTurn(session);
                    break;
                case PlayerAction.UseItem:
                    UseItem(session, inventory);
                    break;
                case PlayerAction.Quit:
                    _renderer.RenderMessage("Bye!");
                    return;
            }

            switch (session.Status) {
                case GameStatus.Win:
                    _renderer.RenderVictory();
                    break;
                case GameStatus.Lose:
                    _renderer.RenderDefeat(_lastCulpritId);
                    break;
            }
        }
    }

    private int? _lastCulpritId;

    private void TakeTurn(GameSession session) {
        int moleculeId = _input.RequestMoleculeId(session.Graph);
        TurnResult result = session.TakeTurn(moleculeId);
        _lastCulpritId = result.CulpritId;
        _renderer.RenderTurnResult(result);
    }

    private void UseItem(GameSession session, PlayerInventory inventory) {
        if (inventory.Items.All(kvp => kvp.Value.Count == 0)) {
            _renderer.RenderMessage("No items available.");
            return;
        }

        LevelItemType type = _input.RequestItem(inventory);
        ILevelItem? item = inventory.GetItem(type);

        try {
            switch (item) {
                case IDoubleTargetItem:
                    int fromId = _input.RequestMoleculeId(session.Graph);
                    int toId = _input.RequestMoleculeId(session.Graph);
                    session.UseDoubleTargetItem(type, fromId, toId);
                    break;
                case ISingleTargetItem:
                    int targetId = _input.RequestMoleculeId(session.Graph);
                    session.UseSingleTargetItem(type, targetId);
                    break;
                case IInstantItem:
                    session.UseInstantItem(type);
                    break;
            }

            _renderer.RenderMessage($"Used {type}.");
        }
        catch (InvalidOperationException ex) {
            _renderer.RenderMessage(ex.Message);
        }
    }

    private void RenderInventory(PlayerInventory inventory) {
        System.Console.WriteLine("Inventory:");
        foreach (KeyValuePair<LevelItemType, IReadOnlyList<ILevelItem>> item in inventory.Items) {
            System.Console.WriteLine($"- {item.Key}: {item.Value.Count}");
        }
    }

    private PlayerInventory MockInventory() {
        var inventory = new PlayerInventory();
        inventory.Add(new RevealAllItem());
        inventory.Add(new PlusOneAllItem());
        inventory.Add(new FreezeItem());
        inventory.Add(new ChainBreakItem());
        inventory.Add(new UndoItem());

        return inventory;
    }
}
