using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Molecularity.Core.Domain.Exceptions;

namespace Molecularity.Core.Domain {
    public class MoleculeGraph {
        private readonly Dictionary<int, Molecule> _molecules = new();
        private readonly Dictionary<int, HashSet<int>> _connections = new();

        public void AddMolecule(Molecule molecule) {
            if (!_molecules.TryAdd(molecule.Id, molecule)) {
                throw new DuplicateMoleculeException(molecule.Id);
            }
        }

        public void AddConnection(int fromMoleculeId, int toMoleculeId) {
            if (!_molecules.ContainsKey(fromMoleculeId)) {
                throw new MoleculeNotFoundException(fromMoleculeId);
            }

            if (!_molecules.ContainsKey(toMoleculeId)) {
                throw new MoleculeNotFoundException(toMoleculeId);
            }

            if (!_connections.ContainsKey(fromMoleculeId)) {
                _connections[fromMoleculeId] = new HashSet<int>();
            }

            if (!_connections.ContainsKey(toMoleculeId)) {
                _connections[toMoleculeId] = new HashSet<int>();
            }

            _connections[fromMoleculeId].Add(toMoleculeId);
            _connections[toMoleculeId].Add(fromMoleculeId);
        }

        public void RemoveConnection(int fromMoleculeId, int toMoleculeId) {
            if (_connections.TryGetValue(fromMoleculeId, out HashSet<int>? fromNeighbors)) {
                fromNeighbors.Remove(toMoleculeId);
            }

            if (_connections.TryGetValue(toMoleculeId, out HashSet<int>? toNeighbors)) {
                toNeighbors.Remove(fromMoleculeId);
            }
        }

        [return: NotNull]
        public Molecule GetMolecule(int moleculeId) {
            return _molecules.TryGetValue(moleculeId, out Molecule? molecule)
                ? molecule
                : throw new MoleculeNotFoundException(moleculeId);
        }

        public bool TryGetMolecule(int moleculeId, out Molecule? molecule) {
            return _molecules.TryGetValue(moleculeId, out molecule);
        }

        [return: NotNull]
        public IEnumerable<Molecule> GetAliveNeighbors(int moleculeId) {
            if (!_connections.TryGetValue(moleculeId, out HashSet<int>? neighbors)) {
                return Enumerable.Empty<Molecule>();
            }

            return neighbors
                .Select(id => _molecules[id])
                .Where(m => m.IsAlive)
                .ToList();
        }

        [return: NotNull]
        public IEnumerable<Molecule> GetAliveAll() {
            return _molecules.Values.Where(molecule => molecule.IsAlive).ToList();
        }

        public void RemoveMolecule(int moleculeId) {
            if (!_molecules.TryGetValue(moleculeId, out Molecule? molecule)) {
                throw new MoleculeNotFoundException(moleculeId);
            }

            if (!molecule.IsAlive) {
                throw new MoleculeAlreadyRemovedException(moleculeId);
            }

            molecule.Remove();

            if (!_connections.TryGetValue(moleculeId, out HashSet<int>? neighbors)) {
                return;
            }

            foreach (int neighborId in neighbors) {
                _molecules[neighborId].Reveal();
                _connections[neighborId].Remove(moleculeId);
            }

            _connections.Remove(moleculeId);
        }

        public bool IsEmpty() {
            return _molecules.Values.All(m => !m.IsAlive);
        }

        /// <summary>
        /// Returns the next free molecule id: max id across ALL molecules (including removed
        /// ones whose entries stay in _molecules) plus 1. Returns 1 if the graph is empty.
        /// Considers removed molecules so spawned children never collide with removed ids.
        /// </summary>
        public int NextId() {
            if (_molecules.Count == 0) return 1;
            int max = 0;
            foreach (int id in _molecules.Keys) {
                if (id > max) max = id;
            }
            return max + 1;
        }

        public GraphSnapshot TakeSnapshot() {
            var moleculeSnapshot = new List<MoleculeSnapshot>();
            foreach (Molecule molecule in _molecules.Values) {
                moleculeSnapshot.Add(molecule.ToSnapshot());
            }

            List<MoleculeConnectionSnapshot> connectionSnapshot = new();
            foreach (KeyValuePair<int, HashSet<int>> kvp in _connections) {
                int fromId = kvp.Key;
                foreach (int toId in kvp.Value) {
                    if (fromId < toId) {
                        connectionSnapshot.Add(new MoleculeConnectionSnapshot(fromId, toId));
                    }
                }
            }

            return new GraphSnapshot(moleculeSnapshot, connectionSnapshot);
        }

        public void RestoreSnapshot(GraphSnapshot snapshot) {
            // Build a set of ids present in the snapshot.
            var snapshotIds = new HashSet<int>();
            foreach (MoleculeSnapshot moleculeSnapshot in snapshot.Molecules) {
                snapshotIds.Add(moleculeSnapshot.Id);
            }

            // Remove any molecules that were spawned AFTER the snapshot was taken
            // (e.g. Splitter children). Their ids won't be in snapshotIds.
            var idsToRemove = new List<int>();
            foreach (int id in _molecules.Keys) {
                if (!snapshotIds.Contains(id)) {
                    idsToRemove.Add(id);
                }
            }
            foreach (int id in idsToRemove) {
                _molecules.Remove(id);
                _connections.Remove(id);
            }

            // Restore state of all molecules present in the snapshot.
            foreach (MoleculeSnapshot moleculeSnapshot in snapshot.Molecules) {
                if (_molecules.TryGetValue(moleculeSnapshot.Id, out Molecule? molecule)) {
                    molecule.SetFromSnapshot(moleculeSnapshot);
                }
                else {
                    throw new MoleculeNotFoundException(moleculeSnapshot.Id);
                }
            }

            _connections.Clear();
            foreach (MoleculeConnectionSnapshot connectionSnapshot in snapshot.Connections) {
                AddConnection(connectionSnapshot.FromId, connectionSnapshot.ToId);
            }
        }
    }
}
