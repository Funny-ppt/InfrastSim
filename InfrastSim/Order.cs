using InfrastSim.TimeDriven;
using System.Reflection.Emit;
using System.Text.Json;

namespace InfrastSim;

public record Order(int RequiredLevel, TimeSpan ProduceTime, Material Consumes, Material Earns) : IJsonSerializable {
    public readonly static Order[] Gold = {
        new(1, TimeSpan.FromMinutes(144), new Material("赤金", 2),  new Material("龙门币", 1000)),
        new(2, TimeSpan.FromMinutes(210), new Material("赤金", 3),  new Material("龙门币", 1500)),
        new(3, TimeSpan.FromMinutes(276), new Material("赤金", 4),  new Material("龙门币", 2000)),
    };
    public readonly static Order OriginStone = new(3, TimeSpan.FromMinutes(120), new Material("源石碎片", 2), new Material("合成玉", 20));

    public void ToJson(Utf8JsonWriter writer, bool detailed = false) {
        writer.WriteStartObject();
        writer.WriteNumber("produce-time", ProduceTime.Ticks);
        writer.WriteString("consume", Consumes.Name);
        writer.WriteNumber("consume-count", Consumes.Count);
        writer.WriteString("earn", Earns.Name);
        writer.WriteNumber("earn-count", Earns.Count);
        writer.WriteEndObject();
    }
    public static Order? FromJson(JsonElement elem) {
        if (elem.ValueKind == JsonValueKind.Null) return null;

        var produceTime = new TimeSpan(elem.GetProperty("produce-time").GetInt64());
        var consume = elem.GetProperty("consume").GetString();
        var consumeCount = elem.GetProperty("consume-count").GetInt32();
        var earn = elem.GetProperty("earn").GetString();
        var earnCount = elem.GetProperty("earn-count").GetInt32();
        return new Order(0, produceTime, new(consume, consumeCount), new(earn, earnCount));
    }
}