using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Molecularity.Core.Data;

namespace Molecularity.Web.Services {

    public sealed record LevelSummary(int Id, int MoleculeCount, int ConnectionCount, IReadOnlyList<string> Types);

    public sealed class LevelService {
        private readonly HttpClient _http;
        private readonly Dictionary<int, LevelConfig> _configs = new();
        private IReadOnlyList<LevelSummary>? _summaries;

        public LevelService(HttpClient http) => _http = http;

        public bool IsLoaded => _summaries != null;
        public IReadOnlyList<LevelSummary> Summaries => _summaries ?? Array.Empty<LevelSummary>();

        /// <summary>
        /// Load the levels index and then fetch each level JSON.
        /// Probes level_1.json, level_2.json... until 404 if index is unavailable.
        /// </summary>
        public async Task LoadAsync() {
            if (_summaries != null) return;

            List<int> ids = await LoadIdsAsync();
            var summaries = new List<LevelSummary>(ids.Count);

            foreach (int id in ids) {
                LevelConfig? config = await TryFetchLevelAsync(id);
                if (config == null) continue;
                _configs[id] = config;
                var types = new List<string>();
                foreach (var mol in config.Molecules) {
                    string t = mol.Type.ToString();
                    if (!types.Contains(t)) types.Add(t);
                }
                summaries.Add(new LevelSummary(id, config.Molecules.Count, config.Connections.Count, types));
            }

            _summaries = summaries;
        }

        public LevelConfig? Get(int id) => _configs.TryGetValue(id, out var cfg) ? cfg : null;

        // ── Internals ──────────────────────────────────────────────────────

        private async Task<List<int>> LoadIdsAsync() {
            try {
                int[]? ids = await _http.GetFromJsonAsync<int[]>("levels-index.json");
                if (ids != null && ids.Length > 0) return new List<int>(ids);
            }
            catch { /* fall through to probe */ }

            // Fallback: probe sequential ids until first 404
            var probed = new List<int>();
            for (int id = 1; id <= 100; id++) {
                var resp = await _http.GetAsync($"levels/level_{id}.json");
                if (!resp.IsSuccessStatusCode) break;
                probed.Add(id);
            }
            return probed;
        }

        private async Task<LevelConfig?> TryFetchLevelAsync(int id) {
            try {
                var resp = await _http.GetAsync($"levels/level_{id}.json");
                if (!resp.IsSuccessStatusCode) return null;
                string json = await resp.Content.ReadAsStringAsync();
                return LevelJson.Parse(json);
            }
            catch {
                return null;
            }
        }
    }
}
