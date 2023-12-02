using InfrastSim.TimeDriven.WebHelper.Enumerate;
using System.Text.Json.Serialization;

namespace InfrastSim.TimeDriven.WebHelper;

[JsonSerializable(typeof(OpEnumData[]))]
[JsonSerializable(typeof(OpEnumData))]
[JsonSerializable(typeof(Efficiency))]
[JsonSerializable(typeof(OpConf))]
[JsonSerializable(typeof(PosConf))]
[JsonSerializable(typeof(FacConf))]
[JsonSerializable(typeof(DispatchGroup))]
internal partial class SourceGenerationContext : JsonSerializerContext {
}
