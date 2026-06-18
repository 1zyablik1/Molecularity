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
    double SolutionDensity);
