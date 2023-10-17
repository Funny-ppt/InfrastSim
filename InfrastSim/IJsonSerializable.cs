using System.Text.Json;

namespace InfrastSim;
public interface IJsonSerializable {
    public void ToJson(Utf8JsonWriter writer, bool detailed = false);
}
