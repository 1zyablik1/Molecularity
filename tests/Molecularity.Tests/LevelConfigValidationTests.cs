using System.Collections.Generic;
using Molecularity.Core.Data;

namespace Molecularity.Tests;

public class LevelConfigValidationTests {
    private static LevelConfig SimpleChain() => new(
        LevelId: 1,
        Molecules: new List<MoleculeConfig> {
            new(1, MoleculeType.Simple, 3, true),
            new(2, MoleculeType.Simple, 2, false),
            new(3, MoleculeType.Simple, 1, false),
        },
        Connections: new List<ConnectionConfig> {
            new(1, 2),
            new(2, 3),
        }
    );

    [Fact]
    public void WellFormedLevel_IsValid() {
        LevelValidationResult result = SimpleChain().Validate();
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void EmptyMolecules_IsInvalid() {
        var level = new LevelConfig(
            LevelId: 1,
            Molecules: new List<MoleculeConfig>(),
            Connections: new List<ConnectionConfig>()
        );

        LevelValidationResult result = level.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("at least one molecule"));
    }

    [Fact]
    public void DuplicateMoleculeId_IsInvalid() {
        var level = new LevelConfig(
            LevelId: 1,
            Molecules: new List<MoleculeConfig> {
                new(1, MoleculeType.Simple, 3, true),
                new(1, MoleculeType.Simple, 2, false),
            },
            Connections: new List<ConnectionConfig>()
        );

        LevelValidationResult result = level.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Duplicate molecule id"));
    }

    [Fact]
    public void ConnectionReferencingMissingMolecule_IsInvalid() {
        var level = new LevelConfig(
            LevelId: 1,
            Molecules: new List<MoleculeConfig> {
                new(1, MoleculeType.Simple, 3, true),
            },
            Connections: new List<ConnectionConfig> {
                new(1, 99),
            }
        );

        LevelValidationResult result = level.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("unknown molecule id"));
    }

    [Fact]
    public void SelfConnection_IsInvalid() {
        var level = new LevelConfig(
            LevelId: 1,
            Molecules: new List<MoleculeConfig> {
                new(1, MoleculeType.Simple, 3, true),
            },
            Connections: new List<ConnectionConfig> {
                new(1, 1),
            }
        );

        LevelValidationResult result = level.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Self-connection"));
    }

    [Fact]
    public void DuplicateConnection_IsInvalid() {
        var level = new LevelConfig(
            LevelId: 1,
            Molecules: new List<MoleculeConfig> {
                new(1, MoleculeType.Simple, 3, true),
                new(2, MoleculeType.Simple, 3, true),
            },
            Connections: new List<ConnectionConfig> {
                new(1, 2),
                new(2, 1),
            }
        );

        LevelValidationResult result = level.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Duplicate connection"));
    }

    [Fact]
    public void MoleculeWithValueLessThanOne_IsInvalid() {
        var level = new LevelConfig(
            LevelId: 1,
            Molecules: new List<MoleculeConfig> {
                new(1, MoleculeType.Simple, 3, true),
                new(2, MoleculeType.Simple, 0, false),
            },
            Connections: new List<ConnectionConfig> {
                new(1, 2),
            }
        );

        LevelValidationResult result = level.Validate();

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("InitialValue < 1"));
    }
}
