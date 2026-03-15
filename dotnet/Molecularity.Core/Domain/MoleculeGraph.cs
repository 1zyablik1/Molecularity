using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Molecularity.Core.Domain {
    public class MoleculeGraph {
        private readonly Dictionary<int, Molecule> _molecules = new();
        private readonly Dictionary<int, HashSet<int>> _connections = new();

        public void AddMolecule(Molecule molecule) {
            if (!_molecules.TryAdd(molecule.Id, molecule)) {
                throw new Exception($"Molecule with id {molecule.Id} already exists.");
            }
        }

        public void AddConnection(int fromMoleculeId, int toMoleculeId) {
            if (!_molecules.ContainsKey(fromMoleculeId)) {
                throw new Exception($"Molecule with id {fromMoleculeId} not found.");
            }

            if (!_molecules.ContainsKey(toMoleculeId)) {
                throw new Exception($"Molecule with id {toMoleculeId} not found.");
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
                : throw new Exception($"Molecule with id {moleculeId} not found.");
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
                throw new Exception($"Molecule with id {moleculeId} not found.");
            }

            if (!molecule.IsAlive) {
                throw new Exception($"Molecule with id {moleculeId} is already removed.");
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

        public GraphSnapshot TakeSnapshot() {
            var moleculeSnapshot = new List<MoleculeSnapshot>();
            foreach (Molecule molecule in _molecules.Values) {
                moleculeSnapshot.Add(new MoleculeSnapshot(molecule.Id, molecule.Value, molecule.IsAlive, molecule.IsRevealed, molecule.ClonePassives()));
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
            foreach (MoleculeSnapshot moleculeSnapshot in snapshot.Molecules) {
                if (_molecules.TryGetValue(moleculeSnapshot.Id, out Molecule? molecule)) {
                    molecule.SetFromSnapshot(moleculeSnapshot);
                }
                else {
                    throw new Exception($"Molecule with id {moleculeSnapshot.Id} not found.");
                }
            }

            _connections.Clear();
            foreach (MoleculeConnectionSnapshot connectionSnapshot in snapshot.Connections) {
                AddConnection(connectionSnapshot.FromId, connectionSnapshot.ToId);
            }
        }
    }
}
