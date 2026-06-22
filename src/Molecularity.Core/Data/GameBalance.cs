namespace Molecularity.Core.Data {
    /// <summary>
    /// Central place for tunable gameplay constants.
    /// These are defaults; they can be overridden per-level via <see cref="BalanceConfig"/> (JSON <c>balance</c> block).
    /// </summary>
    public static class GameBalance {
        public const int BaseDecrement = -1;
        public const int LazyStep = 1;
        public const int ShieldTurns = 2;
        public const int LockTurns = 2;
        public const int FreezeTurns = 3;
        public const int AnchorDecrement = -2;
        public const int AnchorHeal = 1;
        public const int VirusBite = 2;
        public const int SplitterChildValue = 3;
    }
}
