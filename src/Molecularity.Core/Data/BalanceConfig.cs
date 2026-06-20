using System.Text.Json.Serialization;

namespace Molecularity.Core.Data {
    [JsonConverter(typeof(BalanceConfigJsonConverter))]
    public record BalanceConfig(
        int BaseDecrement = GameBalance.BaseDecrement,
        int LazyStep = GameBalance.LazyStep,
        int ShieldTurns = GameBalance.ShieldTurns,
        int LockTurns = GameBalance.LockTurns,
        int FreezeTurns = GameBalance.FreezeTurns,
        int AnchorDecrement = GameBalance.AnchorDecrement,
        int AnchorHeal = GameBalance.AnchorHeal,
        int VirusBite = GameBalance.VirusBite) {
        public static readonly BalanceConfig Default = new();
    }
}
