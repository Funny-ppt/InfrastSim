using System.Text.Json.Serialization;

namespace InfrastSimServer {
    [JsonSerializable(typeof(Dictionary<string, int>))]
    [JsonSerializable(typeof(SelectOperatorsData))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(int))]
    internal partial class SourceGenerationContext : JsonSerializerContext {
    }
}
