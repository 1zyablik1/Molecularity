# tools/

Author-time / analysis tooling. **Not shipped with the game.**

## Molecularity.Solver (implemented)

A CLI that loads every level from `content/levels/` and runs a depth-first exhaustive analysis
to determine solvability and estimate difficulty.

**What it reports per level:**

| Field | Description |
|---|---|
| N | Molecule count |
| Solvable | Whether at least one winning click-ordering exists |
| WinLines | Number of distinct winning orderings (capped at 100 000; shown with `+` if capped) |
| SafeFirst | How many of the N opening clicks still lead to a win |
| Density | **Winning orderings / N!** — solution density, size-normalized (0–100%). Shown as `—` when the exact count is unavailable (capped or truncated) |
| Fair | Fog-of-war fairness: `✓` if the level can be won clicking **only currently-visible** molecules; `✗` if it is solvable only with a blind click on a hidden molecule (or a `RevealAll` item). Such levels print `[needs blind click/RevealAll]` — usually a problem for early/free levels |
| Difficulty | Heuristic label derived from Density (see below) |

**Difficulty labels (size-aware, from Density):**

Because Density divides winning orderings by `N!`, the grade is comparable across level
sizes — a tight 4-molecule level and a tight 12-molecule level score similarly.

- `Unsolvable` — no winning ordering exists
- `Unknown` — search truncated (level too large for the node budget)
- `Trivial` — every order wins (Density ≈ 100%), or ≥100 000 winning orderings (capped)
- `Easy` — Density ≥ 50%
- `Medium` — Density ≥ 20%
- `Hard` — Density ≥ 5%
- `VeryHard` — Density < 5%

**How to run** (from the repo root):

```
dotnet run --project tools/Molecularity.Solver
```

Pass an optional levels directory as the first argument:

```
dotnet run --project tools/Molecularity.Solver -- /path/to/levels
```

If no argument is given the tool walks up from its own output directory until it finds
`content/levels` inside the repository.

---

Planned:
- **Molecularity.LevelTool** — exports level JSON from Google Sheets/Drive into
  `content/levels/`, validating each via `LevelConfig.Validate()` before writing.

## Conventions

- .NET tools live here as projects and are added to the root `Molecularity.sln`; they may
  reference `src/Molecularity.Core` but **Core never references a tool**.
- Non-.NET tools (if any) live in their own self-contained subfolder here.
