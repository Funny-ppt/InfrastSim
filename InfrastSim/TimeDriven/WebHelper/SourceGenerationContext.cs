using System.Text.Json.Serialization;

namespace InfrastSim.TimeDriven.WebHelper;

[JsonSerializable(typeof(OpEnumData))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
internal partial class SourceGenerationContext : JsonSerializerContext {
}
