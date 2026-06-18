using System.Text.Json.Serialization;

namespace Molecularity.Core.Data {
    [JsonConverter(typeof(BalanceConfigJsonConverter))]
    public record BalanceConfig(
        int BaseDecrement = GameBalance.BaseDecrement,
        int ShieldTurns = GameBalance.ShieldTurns,
        int FreezeTurns = GameBalance.FreezeTurns,
        int AnchorDecrement = GameBalance.AnchorDecrement,
        int AnchorHeal = GameBalance.AnchorHeal) {
        public static readonly BalanceConfig Default = new();
    }
}
