namespace Molecularity.Core.Domain {
    public enum GameStatus {
        InProgress,
        Win,
        Lose,

        /// <summary>
        /// No legal move: molecules remain but none are removable (all are protected, e.g.
        /// an active Shield/Lock), so the field can never be cleared. Terminal — restart.
        /// </summary>
        Stuck
    }
}
