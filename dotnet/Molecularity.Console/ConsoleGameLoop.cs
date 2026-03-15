using Molecularity.Core.Data;
using Molecularity.Core.Domain;
using Molecularity.Core.Interfaces;
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
        var inventory = new PlayerInventory();
        inventory.Add(new RevealAllItem());
        inventory.Add(new PlusOneAllItem());
        inventory.Add(new FreezeItem());
        inventory.Add(new ChainBreakItem());
        inventory.Add(new UndoItem());

        var session = new GameSession(level, inventory);

        while (session.Status == GameStatus.InProgress) {
            _renderer.RenderGraph(session.Graph);
            int moleculeId = _input.RequestMoleculeId(session.Graph);
            TurnResult result = session.TakeTurn(moleculeId);
            _renderer.RenderTurnResult(result);

            switch (session.Status) {
                case GameStatus.Win:
                    _renderer.RenderVictory();
                    break;
                case GameStatus.Lose:
                    _renderer.RenderDefeat(result.CulpritId);
                    break;
            }
        }
    }
}
