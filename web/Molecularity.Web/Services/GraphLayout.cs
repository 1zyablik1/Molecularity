using System;
using System.Collections.Generic;

namespace Molecularity.Web.Services {

    /// <summary>
    /// Force-directed graph layout computed in C#.
    /// Pure presentation logic — no game rules.
    /// </summary>
    public static class GraphLayout {
        public static Dictionary<int, (double X, double Y)> Compute(
            IReadOnlyList<int> nodeIds,
            IReadOnlyList<(int FromId, int ToId)> edges,
            double width = 1000,
            double height = 650) {

            var pos = new Dictionary<int, (double X, double Y)>();

            // Initial positions using a golden-angle spiral
            for (int i = 0; i < nodeIds.Count; i++) {
                double angle = i * 2.399963 + nodeIds[i] * 0.17;
                double r = 90 + 25 * Math.Sqrt(i);
                pos[nodeIds[i]] = (
                    width / 2 + Math.Cos(angle) * r,
                    height / 2 + Math.Sin(angle) * r);
            }

            // Force-directed iterations
            for (int step = 0; step < 170; step++) {
                var force = new Dictionary<int, (double X, double Y)>();
                foreach (int id in nodeIds) force[id] = (0, 0);

                // Repulsion between all pairs
                for (int i = 0; i < nodeIds.Count; i++) {
                    for (int j = i + 1; j < nodeIds.Count; j++) {
                        int a = nodeIds[i], b = nodeIds[j];
                        double dx = pos[a].X - pos[b].X;
                        double dy = pos[a].Y - pos[b].Y;
                        double d2 = Math.Max(2500, dx * dx + dy * dy);
                        double f = 9000 / d2;
                        force[a] = (force[a].X + dx * f, force[a].Y + dy * f);
                        force[b] = (force[b].X - dx * f, force[b].Y - dy * f);
                    }
                }

                // Attraction along edges
                foreach (var (from, to) in edges) {
                    if (!pos.ContainsKey(from) || !pos.ContainsKey(to)) continue;
                    double dx = pos[to].X - pos[from].X;
                    double dy = pos[to].Y - pos[from].Y;
                    double d = Math.Max(1, Math.Sqrt(dx * dx + dy * dy));
                    double f = (d - 190) * 0.018;
                    force[from] = (force[from].X + dx / d * f, force[from].Y + dy / d * f);
                    force[to]   = (force[to].X   - dx / d * f, force[to].Y   - dy / d * f);
                }

                // Apply forces with bounds and center pull
                double cx = width / 2, cy = height / 2;
                double minX = 80, maxX = width - 80, minY = 75, maxY = height - 75;
                foreach (int id in nodeIds) {
                    double nx = Math.Max(minX, Math.Min(maxX, pos[id].X + force[id].X + (cx - pos[id].X) * 0.003));
                    double ny = Math.Max(minY, Math.Min(maxY, pos[id].Y + force[id].Y + (cy - pos[id].Y) * 0.003));
                    pos[id] = (nx, ny);
                }
            }

            return pos;
        }
    }
}
