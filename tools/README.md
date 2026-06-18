# tools/

Author-time / analysis tooling. **Not shipped with the game.**

Planned:
- **Molecularity.Solver** — auto-solves levels (DFS over the deterministic core) to verify
  solvability and estimate difficulty (winning-orderings count, per-step "forgiveness",
  forced-move depth). Reuses `Molecularity.Core` primitives; no core changes.
- **Molecularity.LevelTool** — exports level JSON from Google Sheets/Drive into
  `content/levels/`, validating each via `LevelConfig.Validate()` before writing.

Conventions:
- .NET tools live here as projects and are added to the root `Molecularity.sln`; they may
  reference `src/Molecularity.Core` but **Core never references a tool**.
- Non-.NET tools (if any) live in their own self-contained subfolder here.
