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
    }
}
