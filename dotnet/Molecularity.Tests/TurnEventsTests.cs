using System.Collections.Generic;
using System.Linq;
using Molecularity.Core.Data;
using Molecularity.Core.Domain;

namespace Molecularity.Tests;

public class TurnEventsTests {
    [Fact]
    public void ClickSimple_EmitsRemovedAndDecrementEvents_NoAbilityEvents() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Simple(1, 5),
                TestData.Simple(2, 5),
                TestData.Simple(3, 5),
            },
            new List<ConnectionConfig> { new(1, 2), new(2, 3) });

        TurnResult result = session.TakeTurn(1);

        // Clicked molecule removed event is present
        Assert.Contains(result.Events.OfType<MoleculeRemovedEvent>(), e => e.MoleculeId == 1);

        // Decrement events for remaining alive molecules
        var decrementEvents = result.Events.OfType<ValueChangedEvent>()
            .Where(e => e.Reason == ValueChangeReason.Decrement)
            .ToList();
        Assert.Contains(decrementEvents, e => e.MoleculeId == 2);
        Assert.Contains(decrementEvents, e => e.MoleculeId == 3);

        // No ability events at all
        Assert.Empty(result.Events.OfType<ValueChangedEvent>().Where(e => e.Reason == ValueChangeReason.Ability));
    }

    [Fact]
    public void ClickAnchor_EmitsAbilityEventsBeforeDecrementEvents_AndRemovedEvent() {
        // Anchor (id 1) connected to neighbors 2 and 3; click anchor
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Anchor(1, 5),
                TestData.Simple(2, 5),
                TestData.Simple(3, 5),
            },
            new List<ConnectionConfig> { new(1, 2), new(1, 3) });

        TurnResult result = session.TakeTurn(1);

        // Ability events present for each alive neighbor with Reason=Ability and Delta=+1
        var abilityEvents = result.Events.OfType<ValueChangedEvent>()
            .Where(e => e.Reason == ValueChangeReason.Ability)
            .ToList();
        Assert.Contains(abilityEvents, e => e.MoleculeId == 2 && e.Delta == 1);
        Assert.Contains(abilityEvents, e => e.MoleculeId == 3 && e.Delta == 1);

        // Removed event for the anchor
        Assert.Contains(result.Events.OfType<MoleculeRemovedEvent>(), e => e.MoleculeId == 1);

        // All ability events appear before any decrement event (index ordering)
        var allEvents = result.Events.ToList();
        int lastAbilityIndex = allEvents
            .Select((e, i) => (e, i))
            .Where(t => t.e is ValueChangedEvent vc && vc.Reason == ValueChangeReason.Ability)
            .Max(t => t.i);
        int firstDecrementIndex = allEvents
            .Select((e, i) => (e, i))
            .Where(t => t.e is ValueChangedEvent vc && vc.Reason == ValueChangeReason.Decrement)
            .Min(t => t.i);
        Assert.True(lastAbilityIndex < firstDecrementIndex, "All ability events must appear before any decrement event.");
    }

    [Fact]
    public void ClickMolecule_WithHiddenNeighbor_EmitsRevealedEvent() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Simple(1, 5, revealed: true),
                TestData.Simple(2, 5, revealed: false),
            },
            new List<ConnectionConfig> { new(1, 2) });

        TurnResult result = session.TakeTurn(1);

        Assert.Contains(result.Events.OfType<MoleculeRevealedEvent>(), e => e.MoleculeId == 2);
    }

    [Fact]
    public void ClickMolecule_WithAlreadyRevealedNeighbor_EmitsNoRevealedEvent() {
        GameSession session = TestData.Session(
            new List<MoleculeConfig> {
                TestData.Simple(1, 5, revealed: true),
                TestData.Simple(2, 5, revealed: true),
            },
            new List<ConnectionConfig> { new(1, 2) });

        TurnResult result = session.TakeTurn(1);

        Assert.Empty(result.Events.OfType<MoleculeRevealedEvent>().Where(e => e.MoleculeId == 2));
    }
}
