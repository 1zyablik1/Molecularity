using Molecularity.Core.Data;
using Molecularity.Solver;

string levelsDir = ResolvelevelsDirectory(args);

var repo = new JsonLevelRepository(levelsDir);
var solver = new LevelSolver();

Console.WriteLine($"{"Level",-8} {"N",3}  {"Solvable",8}  {"WinLines",10}  {"SafeFirst",10}  {"Density",8}  {"Fair",5}  Difficulty");
Console.WriteLine(new string('-', 82));

foreach (int id in repo.GetAll()) {
    LevelConfig level = repo.Get(id);
    SolveReport report = solver.Analyze(level);

    string solvable = report.Solvable ? "✓" : "✗";
    string winLines = report.WinningLinesCapped
        ? $"{report.WinningLines}+"
        : report.WinningLines.ToString();
    string safeFirst = $"{report.SafeFirstMoves}/{report.FirstMoveCount}";
    string density = double.IsNaN(report.SolutionDensity) ? "—" : report.SolutionDensity.ToString("P1");
    string fair = !report.Solvable ? "—" : report.VisibleOnlySolvable ? "✓" : "✗";
    string difficulty = GetDifficultyLabel(report);
    string blind = report.Solvable && !report.VisibleOnlySolvable ? " [needs blind click/RevealAll]" : "";
    string truncated = report.Truncated ? " [truncated]" : "";

    Console.WriteLine($"level_{id,-3} {report.MoleculeCount,3}  {solvable,8}  {winLines,10}  {safeFirst,10}  {density,8}  {fair,5}  {difficulty}{blind}{truncated}");
}

static string ResolvelevelsDirectory(string[] args) {
    if (args.Length > 0) {
        return args[0];
    }

    // Walk up from AppContext.BaseDirectory looking for a folder containing content/levels
    string? dir = AppContext.BaseDirectory;
    while (dir != null) {
        string candidate = Path.Combine(dir, "content", "levels");
        if (Directory.Exists(candidate)) {
            return candidate;
        }
        dir = Directory.GetParent(dir)?.FullName;
    }

    throw new DirectoryNotFoundException(
        "Could not locate content/levels directory. " +
        "Pass the levels directory path as the first argument.");
}

// Size-aware difficulty: derived from solution density (winning orderings / N!),
// so a level's grade is comparable across different molecule counts.
static string GetDifficultyLabel(SolveReport report) {
    if (!report.Solvable) return "Unsolvable";
    if (report.Truncated) return "Unknown";       // search incomplete (too big)
    if (report.WinningLinesCapped) return "Trivial"; // ≥100k winning orderings — extremely forgiving

    double d = report.SolutionDensity;
    if (d >= 0.999) return "Trivial";  // every order wins
    if (d >= 0.5) return "Easy";
    if (d >= 0.2) return "Medium";
    if (d >= 0.05) return "Hard";
    return "VeryHard";
}
