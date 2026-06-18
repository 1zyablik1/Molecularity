using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Molecularity.Core.Data;

namespace Molecularity.Tests;

public class JsonLevelRepositoryTests : IDisposable {
    private readonly List<string> _tempDirs = new List<string>();

    private string CreateTempDir() {
        string dir = Path.Combine(Path.GetTempPath(), "MolecularityTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        _tempDirs.Add(dir);
        return dir;
    }

    public void Dispose() {
        foreach (string dir in _tempDirs) {
            if (Directory.Exists(dir)) {
                Directory.Delete(dir, recursive: true);
            }
        }
    }

    private static void WriteJson(string dir, string fileName, string json) {
        File.WriteAllText(Path.Combine(dir, fileName), json);
    }

    private static readonly string ValidLevel1Json = @"{
  ""levelId"": 1,
  ""layoutSeed"": 10,
  ""molecules"": [
    { ""id"": 1, ""type"": ""Simple"", ""initialValue"": 3, ""isInitiallyRevealed"": true },
    { ""id"": 2, ""type"": ""Simple"", ""initialValue"": 2, ""isInitiallyRevealed"": false }
  ],
  ""connections"": [
    { ""fromId"": 1, ""toId"": 2 }
  ]
}";

    private static readonly string ValidLevel2Json = @"{
  ""levelId"": 2,
  ""molecules"": [
    { ""id"": 1, ""type"": ""Parasite"", ""initialValue"": 5, ""isInitiallyRevealed"": true },
    { ""id"": 2, ""type"": ""Simple"",   ""initialValue"": 4, ""isInitiallyRevealed"": false }
  ],
  ""connections"": [
    { ""fromId"": 1, ""toId"": 2 }
  ]
}";

    [Fact]
    public void LoadsMultipleLevels_GetAllReturnsTheirIds() {
        string dir = CreateTempDir();
        WriteJson(dir, "level_1.json", ValidLevel1Json);
        WriteJson(dir, "level_2.json", ValidLevel2Json);

        var repo = new JsonLevelRepository(dir);
        List<int> ids = repo.GetAll().ToList();

        Assert.Contains(1, ids);
        Assert.Contains(2, ids);
        Assert.Equal(2, ids.Count);
    }

    [Fact]
    public void Get_ReturnsLevelWithCorrectMoleculesAndConnections() {
        string dir = CreateTempDir();
        WriteJson(dir, "level_1.json", ValidLevel1Json);

        var repo = new JsonLevelRepository(dir);
        LevelConfig level = repo.Get(1);

        Assert.Equal(1, level.LevelId);
        Assert.Equal(2, level.Molecules.Count);
        Assert.Single(level.Connections);
        Assert.Equal(1, level.Connections[0].FromId);
        Assert.Equal(2, level.Connections[0].ToId);
    }

    [Fact]
    public void MoleculeType_ParsedFromString() {
        string dir = CreateTempDir();
        WriteJson(dir, "level_2.json", ValidLevel2Json);

        var repo = new JsonLevelRepository(dir);
        LevelConfig level = repo.Get(2);

        Assert.Equal(MoleculeType.Parasite, level.Molecules[0].Type);
        Assert.Equal(MoleculeType.Simple, level.Molecules[1].Type);
    }

    [Fact]
    public void LayoutSeed_ParsedWhenPresent() {
        string dir = CreateTempDir();
        WriteJson(dir, "level_1.json", ValidLevel1Json);

        var repo = new JsonLevelRepository(dir);
        LevelConfig level = repo.Get(1);

        Assert.Equal(10, level.LayoutSeed);
    }

    [Fact]
    public void LayoutSeed_NullWhenOmitted() {
        string dir = CreateTempDir();
        WriteJson(dir, "level_2.json", ValidLevel2Json);

        var repo = new JsonLevelRepository(dir);
        LevelConfig level = repo.Get(2);

        Assert.Null(level.LayoutSeed);
    }

    [Fact]
    public void Get_UnknownId_ThrowsKeyNotFoundException() {
        string dir = CreateTempDir();
        WriteJson(dir, "level_1.json", ValidLevel1Json);

        var repo = new JsonLevelRepository(dir);

        Assert.Throws<KeyNotFoundException>(() => repo.Get(999));
    }

    [Fact]
    public void InvalidLevelFile_ConstructorThrowsInvalidOperationException() {
        string dir = CreateTempDir();
        // Duplicate molecule id makes it invalid
        string invalidJson = @"{
  ""levelId"": 5,
  ""molecules"": [
    { ""id"": 1, ""type"": ""Simple"", ""initialValue"": 3, ""isInitiallyRevealed"": true },
    { ""id"": 1, ""type"": ""Simple"", ""initialValue"": 2, ""isInitiallyRevealed"": false }
  ],
  ""connections"": []
}";
        WriteJson(dir, "level_invalid.json", invalidJson);

        Assert.Throws<InvalidOperationException>(() => new JsonLevelRepository(dir));
    }

    [Fact]
    public void DuplicateLevelId_AcrossFiles_ThrowsInvalidOperationException() {
        string dir = CreateTempDir();
        WriteJson(dir, "level_a.json", ValidLevel1Json);
        WriteJson(dir, "level_b.json", ValidLevel1Json); // same levelId 1

        Assert.Throws<InvalidOperationException>(() => new JsonLevelRepository(dir));
    }

    [Fact]
    public void NonExistentDirectory_ThrowsDirectoryNotFoundException() {
        string nonExistent = Path.Combine(Path.GetTempPath(), "does_not_exist_" + Guid.NewGuid().ToString("N"));

        Assert.Throws<DirectoryNotFoundException>(() => new JsonLevelRepository(nonExistent));
    }

    [Fact]
    public void AllMoleculeTypes_ParseCorrectly() {
        string dir = CreateTempDir();
        string allTypesJson = @"{
  ""levelId"": 10,
  ""molecules"": [
    { ""id"": 1, ""type"": ""Simple"",  ""initialValue"": 5, ""isInitiallyRevealed"": true },
    { ""id"": 2, ""type"": ""Parasite"",""initialValue"": 5, ""isInitiallyRevealed"": true },
    { ""id"": 3, ""type"": ""Shield"",  ""initialValue"": 5, ""isInitiallyRevealed"": true },
    { ""id"": 4, ""type"": ""Anchor"",  ""initialValue"": 5, ""isInitiallyRevealed"": true }
  ],
  ""connections"": [
    { ""fromId"": 1, ""toId"": 2 },
    { ""fromId"": 2, ""toId"": 3 },
    { ""fromId"": 3, ""toId"": 4 }
  ]
}";
        WriteJson(dir, "level_10.json", allTypesJson);

        var repo = new JsonLevelRepository(dir);
        LevelConfig level = repo.Get(10);

        Assert.Equal(MoleculeType.Simple,  level.Molecules[0].Type);
        Assert.Equal(MoleculeType.Parasite, level.Molecules[1].Type);
        Assert.Equal(MoleculeType.Shield,  level.Molecules[2].Type);
        Assert.Equal(MoleculeType.Anchor,  level.Molecules[3].Type);
    }
}
