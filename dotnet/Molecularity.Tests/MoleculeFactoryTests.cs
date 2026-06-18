using Molecularity.Core.Data;
using Molecularity.Core.Domain;

namespace Molecularity.Tests;

public class MoleculeFactoryTests {
    [Fact]
    public void Create_Simple_CopiesConfigFields() {
        Molecule molecule = MoleculeFactory.Create(new MoleculeConfig(7, MoleculeType.Simple, 4, IsInitiallyRevealed: true));

        Assert.Equal(7, molecule.Id);
        Assert.Equal(MoleculeType.Simple, molecule.Type);
        Assert.Equal(4, molecule.Value);
        Assert.True(molecule.IsRevealed);
        Assert.True(molecule.IsAlive);
    }

    [Fact]
    public void Create_HiddenMolecule_IsNotRevealed() {
        Molecule molecule = MoleculeFactory.Create(new MoleculeConfig(1, MoleculeType.Simple, 3, IsInitiallyRevealed: false));
        Assert.False(molecule.IsRevealed);
    }

    [Fact]
    public void Create_Anchor_Succeeds() {
        Molecule molecule = MoleculeFactory.Create(new MoleculeConfig(1, MoleculeType.Anchor, 3, true));

        Assert.Equal(MoleculeType.Anchor, molecule.Type);
        Assert.True(molecule.IsAlive);
    }
}
