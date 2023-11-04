using System.Text.Json.Serialization;

namespace InfrastSim.TimeDriven.WebHelper;
public class OpEnumData {
    public string Name { get; set; }
    public string Fac { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Product { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Strategy { get; set; }
}
