# content/

Canonical game content — the single source of truth shared across runners, tools and Unity.

- **levels/** — level configs as JSON (schema in `docs/GAME-CORE.md` §3а: molecules +
  connections + optional `balance` + optional `layoutSeed`). Consumed by
  `JsonLevelRepository`. The console runner links these into its output folder; the future
  Unity client and the solver/level-tool read the same files.
