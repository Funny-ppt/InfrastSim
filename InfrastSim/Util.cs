using System.Text;
using System.Text.Json;

namespace InfrastSim;

public static class Util {
    public const double Epsilon = 1e-9;
    public static bool Equals(double self, double other, double epsilon = Epsilon) {
        return Math.Abs(self - other) < epsilon;
    }
    public static double Align(double a, double b) {
        return Math.Floor(a / b + Epsilon) * b;
    }


    public static void WriteItem(this Utf8JsonWriter writer, string propertyName, IJsonSerializable? serializable, bool detailed = false) {
        writer.WritePropertyName(propertyName);
        if (serializable != null) serializable.ToJson(writer, detailed);
        else writer.WriteNullValue();
    }
    public static void WriteItemValue(this Utf8JsonWriter writer, IJsonSerializable? serializable, bool detailed = false) {
        if (serializable != null) serializable.ToJson(writer, detailed);
        else writer.WriteNullValue();
    }
    public static string ToJson(this IJsonSerializable serializable, bool detailed = false) {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        serializable.ToJson(writer, detailed);
        writer.Flush();
        return Encoding.UTF8.GetString(ms.ToArray()) ?? string.Empty;
    }
}
