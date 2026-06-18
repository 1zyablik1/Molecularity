namespace Molecularity.Core.Domain {
    /// <summary>
    /// Central place for tunable gameplay constants.
    /// TODO: move to JSON level/config files (see GAME-CORE.md).
    /// </summary>
    public static class GameBalance {
        public const int BaseDecrement = -1;
        public const int ShieldTurns = 2;
        public const int FreezeTurns = 3;
        public const int AnchorDecrement = -2;
        public const int AnchorHeal = 1;
    }
}
