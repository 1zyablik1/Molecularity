# web/

## Molecularity.Web — Blazor WebAssembly client

A standalone Blazor WebAssembly application that runs the `Molecularity.Core` game engine
directly in the browser via .NET/WASM. **No game rules are reimplemented in JavaScript** —
all logic (turns, items, win/lose detection) comes from `src/Molecularity.Core`.

### Architecture

```
src/Molecularity.Core  ← single source of truth for game rules
        ↑ ProjectReference
web/Molecularity.Web   ← Blazor WASM; renders UI in Razor, calls Core
```

Key files:

| Path | Purpose |
|---|---|
| `Services/GameService.cs` | In-browser session: wraps `GameSession` + `PlayerInventory`, exposes view-model types |
| `Services/LevelService.cs` | Fetches level JSON via `HttpClient`, parses via `LevelJson.Parse` |
| `Services/GraphLayout.cs` | Force-directed layout (pure presentation, no game rules) |
| `Services/ItemMeta.cs` | Display metadata (icon, name, target count) for each item type |
| `Pages/Index.razor` | Main page: level list ↔ game view state machine |
| `Components/GameBoard.razor` | Game UI: graph, inventory, result overlay |
| `Components/MoleculeGraphView.razor` | SVG molecule graph rendered from C# |

### Levels

Levels are sourced from `content/levels/*.json` at build time — **no committed copy**.
The csproj links them into `wwwroot/levels/` and generates `wwwroot/levels-index.json`
(a JSON array of ids) via an inline MSBuild task.

### Running locally

```sh
dotnet run --project web/Molecularity.Web
```

Then open `https://localhost:5000` (or whatever port is printed).

### Deploying to GitHub Pages

Push to `main`. The `.github/workflows/deploy-pages.yml` workflow:
1. Runs `dotnet publish` to produce a static `wwwroot/` output.
2. Rewrites `<base href>` from `/` to `/<repo-name>/` for the Pages sub-path.
3. Uploads and deploys via `actions/upload-pages-artifact` + `actions/deploy-pages`.
