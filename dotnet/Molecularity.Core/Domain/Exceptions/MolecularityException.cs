using System;

namespace Molecularity.Core.Domain.Exceptions {
    public class MolecularityException : Exception {
        public MolecularityException(string message) : base(message) {
        }
    }
}
