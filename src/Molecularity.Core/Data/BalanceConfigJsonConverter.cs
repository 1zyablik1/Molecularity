using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Molecularity.Core.Data {
    /// <summary>
    /// Deserializes <see cref="BalanceConfig"/> from a partial JSON object,
    /// filling any omitted field with the corresponding <see cref="GameBalance"/> default.
    /// This ensures <c>{ "shieldTurns": 3 }</c> correctly yields BaseDecrement=-1, FreezeTurns=3, etc.
    /// </summary>
    public class BalanceConfigJsonConverter : JsonConverter<BalanceConfig> {
        public override BalanceConfig Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.StartObject) {
                throw new JsonException("Expected start of object for BalanceConfig.");
            }

            int baseDecrement = GameBalance.BaseDecrement;
            int lazyStep = GameBalance.LazyStep;
            int shieldTurns = GameBalance.ShieldTurns;
            int lockTurns = GameBalance.LockTurns;
            int freezeTurns = GameBalance.FreezeTurns;
            int anchorDecrement = GameBalance.AnchorDecrement;
            int anchorHeal = GameBalance.AnchorHeal;
            int virusBite = GameBalance.VirusBite;

            while (reader.Read()) {
                if (reader.TokenType == JsonTokenType.EndObject) {
                    break;
                }

                if (reader.TokenType != JsonTokenType.PropertyName) {
                    throw new JsonException("Expected property name in BalanceConfig.");
                }

                string propertyName = reader.GetString()!;
                reader.Read();

                if (string.Equals(propertyName, "baseDecrement", StringComparison.OrdinalIgnoreCase)) {
                    baseDecrement = reader.GetInt32();
                } else if (string.Equals(propertyName, "lazyStep", StringComparison.OrdinalIgnoreCase)) {
                    lazyStep = reader.GetInt32();
                } else if (string.Equals(propertyName, "shieldTurns", StringComparison.OrdinalIgnoreCase)) {
                    shieldTurns = reader.GetInt32();
                } else if (string.Equals(propertyName, "lockTurns", StringComparison.OrdinalIgnoreCase)) {
                    lockTurns = reader.GetInt32();
                } else if (string.Equals(propertyName, "freezeTurns", StringComparison.OrdinalIgnoreCase)) {
                    freezeTurns = reader.GetInt32();
                } else if (string.Equals(propertyName, "anchorDecrement", StringComparison.OrdinalIgnoreCase)) {
                    anchorDecrement = reader.GetInt32();
                } else if (string.Equals(propertyName, "anchorHeal", StringComparison.OrdinalIgnoreCase)) {
                    anchorHeal = reader.GetInt32();
                } else if (string.Equals(propertyName, "virusBite", StringComparison.OrdinalIgnoreCase)) {
                    virusBite = reader.GetInt32();
                } else {
                    reader.Skip();
                }
            }

            return new BalanceConfig(baseDecrement, lazyStep, shieldTurns, lockTurns, freezeTurns, anchorDecrement, anchorHeal, virusBite);
        }

        public override void Write(Utf8JsonWriter writer, BalanceConfig value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            writer.WriteNumber("baseDecrement", value.BaseDecrement);
            writer.WriteNumber("lazyStep", value.LazyStep);
            writer.WriteNumber("shieldTurns", value.ShieldTurns);
            writer.WriteNumber("lockTurns", value.LockTurns);
            writer.WriteNumber("freezeTurns", value.FreezeTurns);
            writer.WriteNumber("anchorDecrement", value.AnchorDecrement);
            writer.WriteNumber("anchorHeal", value.AnchorHeal);
            writer.WriteNumber("virusBite", value.VirusBite);
            writer.WriteEndObject();
        }
    }
}
