using Molecularity.Core.Data;
using Molecularity.Core.Domain;

namespace Molecularity.Solver;

public class LevelSolver {
    private const long WinningLinesCap = 100_000;
    private const long NodeBudget = 5_000_000;

    private long _nodesRemaining;
    private bool _truncated;

    public SolveReport Analyze(LevelConfig level) {
        _nodesRemaining = NodeBudget;
        _truncated = false;

        MoleculeGraph graph = LevelBuilder.Build(level);
        BalanceConfig balance = level.Balance ?? BalanceConfig.Default;
        var executor = new TurnExecutor(graph, balance.BaseDecrement);

        List<int> rootIds = graph.GetAliveAll().Select(m => m.Id).ToList();
        int moleculeCount = rootIds.Count;
        int firstMoveCount = rootIds.Count;

        long totalWinning = 0;
        int safeFirstMoves = 0;

        foreach (int id in rootIds) {
            if (_truncated) break;

            GraphSnapshot snap = graph.TakeSnapshot();
            executor.Execute(id);

            long contribution;
            (bool isLoss, int? _) = GameRules.IsLose(graph);
            if (isLoss) {
                contribution = 0;
            } else if (graph.IsEmpty()) {
                contribution = 1;
            } else {
                contribution = CountWinningLines(graph, executor);
            }

            graph.RestoreSnapshot(snap);

            if (contribution > 0) {
                safeFirstMoves++;
            }

            totalWinning += contribution;
            if (totalWinning >= WinningLinesCap) {
                totalWinning = WinningLinesCap;
                break;
            }
        }

        bool capped = totalWinning >= WinningLinesCap;

        double density;
        if (totalWinning == 0) {
            density = 0.0;
        } else if (capped || _truncated) {
            density = double.NaN; // exact count unavailable
        } else {
            density = totalWinning / Factorial(moleculeCount);
        }

        return new SolveReport(
            Solvable: totalWinning > 0,
            WinningLines: totalWinning,
            WinningLinesCapped: capped,
            SafeFirstMoves: safeFirstMoves,
            FirstMoveCount: firstMoveCount,
            MoleculeCount: moleculeCount,
            Truncated: _truncated,
            SolutionDensity: density);
    }

    // N! as a double (exact for the N we care about; avoids overflow for larger N).
    private static double Factorial(int n) {
        double result = 1;
        for (int i = 2; i <= n; i++) {
            result *= i;
        }

        return result;
    }

    private long CountWinningLines(MoleculeGraph graph, TurnExecutor executor) {
        if (_truncated) return 0;

        List<int> ids = graph.GetAliveAll().Select(m => m.Id).ToList();
        long total = 0;

        foreach (int id in ids) {
            if (_truncated) break;

            _nodesRemaining--;
            if (_nodesRemaining <= 0) {
                _truncated = true;
                break;
            }

            GraphSnapshot snap = graph.TakeSnapshot();
            executor.Execute(id);

            long contribution;
            (bool isLoss, int? _) = GameRules.IsLose(graph);
            if (isLoss) {
                contribution = 0;
            } else if (graph.IsEmpty()) {
                contribution = 1;
            } else {
                contribution = CountWinningLines(graph, executor);
            }

            graph.RestoreSnapshot(snap);

            total += contribution;
            if (total >= WinningLinesCap) {
                total = WinningLinesCap;
                break;
            }
        }

        return total;
    }
}
