# unity/

The Unity game client (visualization + meta-game). **Not created yet.**

When added, the Unity project lives here (`Assets/`, `Packages/`, `ProjectSettings/`).
It consumes the engine-agnostic logic from `src/Molecularity.Core` (as a compiled DLL or
shared source) and drives it from event handlers — primarily rendering
`TurnResult.Events`. See `docs/GAME-CORE.md` for the Core↔Unity boundary.

Unity-generated files (`Library/`, `Temp/`, `obj/`, generated `*.csproj`/`*.sln`, …) are
ignored by the root `.gitignore`; only hand-authored `src`/`tests`/`tools` projects and
the root `Molecularity.sln` are tracked.
