namespace Molecularity.Core.Domain {
    /// <summary>
    /// Per-session statistics for the meta layer (star ratings, achievements).
    /// <see cref="TurnsTaken"/> is the net number of committed turns (an Undo reverts one);
    /// <see cref="ItemsUsed"/> counts every consumed item, including Undo.
    /// </summary>
    public record SessionStats(int TurnsTaken, int ItemsUsed);
}
