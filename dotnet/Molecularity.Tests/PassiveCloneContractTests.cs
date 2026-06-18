using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Molecularity.Core.Domain.Passives;

namespace Molecularity.Tests;

public class PassiveCloneContractTests {
    private static readonly Dictionary<Type, Func<IPassiveProperty>> KnownPassives = new() {
        { typeof(NoPassive), () => new NoPassive() },
        { typeof(ShieldPassive), () => new ShieldPassive(2) },
        { typeof(FreezePassive), () => new FreezePassive(3) },
        { typeof(NeighborCountDecrementPassive), () => new NeighborCountDecrementPassive() },
        { typeof(FlatDecrementPassive), () => new FlatDecrementPassive(-2) },
    };

    [Fact]
    public void AllConcretePassiveTypes_AreRegisteredInContractDictionary() {
        Assembly assembly = typeof(IPassiveProperty).Assembly;
        IEnumerable<Type> concretePassiveTypes = assembly.GetTypes()
            .Where(t => typeof(IPassiveProperty).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        HashSet<Type> registered = new(KnownPassives.Keys);
        HashSet<Type> found = new(concretePassiveTypes);

        var missing = found.Except(registered).ToList();
        var extra = registered.Except(found).ToList();

        Assert.True(missing.Count == 0,
            $"New IPassiveProperty implementations found that are not registered in PassiveCloneContractTests.KnownPassives. " +
            $"Please add them to ensure Clone() coverage: {string.Join(", ", missing.Select(t => t.Name))}");
        Assert.True(extra.Count == 0,
            $"KnownPassives dictionary references types that no longer exist: {string.Join(", ", extra.Select(t => t.Name))}");
    }

    [Fact]
    public void AllRegisteredPassives_Clone_ReturnsDifferentReferenceOfSameType() {
        foreach (KeyValuePair<Type, Func<IPassiveProperty>> entry in KnownPassives) {
            IPassiveProperty instance = entry.Value();
            IPassiveProperty clone = instance.Clone();

            Assert.NotSame(instance, clone);
            Assert.Equal(instance.GetType(), clone.GetType());
        }
    }
}
