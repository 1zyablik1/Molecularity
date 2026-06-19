namespace Molecularity.Solver;

public record SolveReport(
    bool Solvable,
    long WinningLines,
    bool WinningLinesCapped,
    int SafeFirstMoves,
    int FirstMoveCount,
    int MoleculeCount,
    bool Truncated,
    // Winning orderings as a fraction of all N! orderings (size-normalized difficulty):
    // 1.0 = any order wins (trivial), →0 = a very narrow solution funnel.
    // 0.0 when unsolvable; double.NaN when the exact count is unknown (capped or truncated).
    double SolutionDensity,
    // Fog-of-war fairness: can the level be won by clicking ONLY molecules whose value is
    // currently visible (initially revealed, or revealed by an earlier removal)? If a level
    // is Solvable but NOT VisibleOnlySolvable, winning requires a blind click on a hidden
    // molecule (or a RevealAll item) — usually unfair for early/free levels.
    bool VisibleOnlySolvable);
