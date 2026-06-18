using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;
using Molecularity.Core.Items;
using Molecularity.Core.Items.Implementations;
using Molecularity.Core.Player;

var builder = WebApplication.CreateBuilder(args);
var levels = new JsonLevelRepository(Path.Combine(AppContext.BaseDirectory, "levels"));
builder.Services.AddSingleton<ILevelRepository>(levels);
builder.Services.AddSingleton<GameStore>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/levels", (ILevelRepository repository) =>
    repository.GetAll().Select(id => {
        LevelConfig level = repository.Get(id);
        return new {
            id,
            molecules = level.Molecules.Count,
            connections = level.Connections.Count,
            types = level.Molecules.Select(m => m.Type.ToString()).Distinct()
        };
    }));

app.MapPost("/api/games", (StartGameRequest request, GameStore games) =>
    Try(() => Results.Ok(games.Start(request.LevelId))));

app.MapPost("/api/games/{gameId:guid}/turn", (Guid gameId, TurnRequest request, GameStore games) =>
    Try(() => Results.Ok(games.Turn(gameId, request.MoleculeId))));

app.MapPost("/api/games/{gameId:guid}/items", (Guid gameId, ItemRequest request, GameStore games) =>
    Try(() => Results.Ok(games.UseItem(gameId, request))));

app.MapFallbackToFile("index.html");
app.Run();

static IResult Try(Func<IResult> action) {
    try {
        return action();
    }
    catch (KeyNotFoundException ex) {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (Exception ex) when (ex is InvalidOperationException or ArgumentException) {
        return Results.BadRequest(new { error = ex.Message });
    }
}

record StartGameRequest(int LevelId);
record TurnRequest(int MoleculeId);
record ItemRequest(LevelItemType Type, int? FirstTargetId, int? SecondTargetId);

sealed class GameStore {
    private const int ItemsPerType = 10;
    private readonly ILevelRepository _levels;
    private readonly ConcurrentDictionary<Guid, ActiveGame> _games = new();

    public GameStore(ILevelRepository levels) => _levels = levels;

    public object Start(int levelId) {
        LevelConfig level = _levels.Get(levelId);
        PlayerInventory inventory = CreateInventory(level);
        var active = new ActiveGame(levelId, new GameSession(level, inventory), inventory);
        Guid id = Guid.NewGuid();
        _games[id] = active;
        return ToResponse(id, active, "Уровень начат");
    }

    public object Turn(Guid id, int moleculeId) {
        ActiveGame game = Get(id);
        lock (game.Sync) {
            EnsureInProgress(game.Session);
            Molecule molecule = game.Session.Graph.GetMolecule(moleculeId);
            if (!molecule.IsAlive) throw new InvalidOperationException("Эта молекула уже удалена.");

            TurnResult result = game.Session.TakeTurn(moleculeId);
            string message = game.Session.Status switch {
                GameStatus.Win => "Уровень пройден",
                GameStatus.Lose => "Молекула распалась. Попробуйте ещё раз",
                _ => $"Молекула {moleculeId} удалена"
            };
            return ToResponse(id, game, message, result.CulpritId);
        }
    }

    public object UseItem(Guid id, ItemRequest request) {
        ActiveGame game = Get(id);
        lock (game.Sync) {
            EnsureInProgress(game.Session);
            switch (request.Type) {
                case LevelItemType.RevealAll:
                case LevelItemType.PlusOneAll:
                case LevelItemType.Undo:
                    game.Session.UseInstantItem(request.Type);
                    break;
                case LevelItemType.Freeze:
                    int target = RequireTarget(request.FirstTargetId);
                    EnsureAlive(game.Session.Graph, target);
                    game.Session.UseSingleTargetItem(request.Type, target);
                    break;
                case LevelItemType.ChainBreak:
                    int from = RequireTarget(request.FirstTargetId);
                    int to = RequireTarget(request.SecondTargetId);
                    EnsureConnection(game.Session.Graph, from, to);
                    game.Session.UseDoubleTargetItem(request.Type, from, to);
                    break;
                default:
                    throw new ArgumentException("Неизвестный предмет.");
            }

            return ToResponse(id, game, $"Использован предмет {request.Type}");
        }
    }

    private ActiveGame Get(Guid id) => _games.TryGetValue(id, out ActiveGame? game)
        ? game
        : throw new KeyNotFoundException("Игровая сессия не найдена.");

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

    private static object ToResponse(Guid id, ActiveGame game, string message, int? culpritId = null) {
        GraphSnapshot snapshot = game.Session.Graph.TakeSnapshot();
        return new {
            gameId = id,
            levelId = game.LevelId,
            status = game.Session.Status.ToString(),
            turns = game.Session.TurnsTaken,
            itemsUsed = game.Session.ItemsUsed,
            canUndo = game.Session.CanUndo,
            message,
            culpritId,
            molecules = snapshot.Molecules.Where(m => m.IsAlive).Select(m => {
                Molecule source = game.Session.Graph.GetMolecule(m.Id);
                return new { m.Id, type = source.Type.ToString(), m.Value, m.IsRevealed };
            }),
            connections = snapshot.Connections.Select(c => new { c.FromId, c.ToId }),
            inventory = Enum.GetValues<LevelItemType>().ToDictionary(t => t.ToString(), game.Inventory.Count)
        };
    }

    private static int RequireTarget(int? target) => target
        ?? throw new ArgumentException("Для предмета нужно выбрать цель.");

    private static void EnsureAlive(MoleculeGraph graph, int id) {
        if (!graph.GetMolecule(id).IsAlive) throw new InvalidOperationException("Цель уже удалена.");
    }

    private static void EnsureConnection(MoleculeGraph graph, int from, int to) {
        EnsureAlive(graph, from);
        EnsureAlive(graph, to);
        bool connected = graph.TakeSnapshot().Connections.Any(c =>
            (c.FromId == from && c.ToId == to) || (c.FromId == to && c.ToId == from));
        if (!connected) throw new InvalidOperationException("Между выбранными молекулами нет связи.");
    }

    private static void EnsureInProgress(GameSession session) {
        if (session.Status != GameStatus.InProgress)
            throw new InvalidOperationException("Игра уже завершена. Перезапустите уровень.");
    }

    private sealed record ActiveGame(int LevelId, GameSession Session, PlayerInventory Inventory) {
        public object Sync { get; } = new();
    }
}
