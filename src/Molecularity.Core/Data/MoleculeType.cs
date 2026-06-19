namespace Molecularity.Core.Data {
    /// <summary>
    /// Defines the type of molecule. This can be used to determine how the molecule processes data and interacts with other molecules.
    /// </summary>
    public enum MoleculeType {
        Simple,
        Lazy,
        Anchor,
        Parasite,
        Shield,
        Lock,
        Bomb,
        Splitter
    }
}
