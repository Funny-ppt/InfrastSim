using System.Text.Json.Serialization;

namespace InfrastSim.TimeDriven.WebHelper;
public class OpEnumData {
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public int id { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public int uid { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public int prime { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public Efficiency SingleEfficiency { get; set; } = null!;


    public string Name { get; set; } = null!;
    public string Fac { get; set; } = null!;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Product { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Strategy { get; set; }
}
