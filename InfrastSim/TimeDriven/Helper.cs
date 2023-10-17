using System.Text;
using System.Text.Json;

namespace InfrastSim.TimeDriven;
internal static class Helper
{
    public static bool HasGroupMember(this FacilityBase facility, string group)
    {
        return facility.Operators.Where(op => op.Groups.Contains(group)).Any();
    }
    public static int GroupMemberCount(this FacilityBase facility, string group)
    {
        return facility.Operators.Where(op => op.Groups.Contains(group)).Count();
    }
    public static int GetRealGoldProductionLine(this Simulator simu)
    {
        return simu.ModifiableFacilities
                   .Where(fac => fac is ManufacturingStation manufacturing
                       && manufacturing.Product == Product.Gold).Count();
    }
    public static int GetGoldProductionLine(this Simulator simu)
    {
        return (int)simu.ExtraGoldProductionLine + simu.GetRealGoldProductionLine();
    }

    public static int GetPowerStations(this Simulator simu)
    {
        return (int)simu.ExtraPowerStation + simu.PowerStations.Count();
    }
    public static OperatorBase? GetVip(this Dormitory dorm)
    {
        return dorm.Operators
            .OrderByDescending(op => op, new VipPriorityComparer())
            .FirstOrDefault();
    }


    public static void WriteItem(this Utf8JsonWriter writer, string propertyName, IJsonSerializable serializable, bool detailed = false) {
        writer.WritePropertyName(propertyName);
        serializable.ToJson(writer, detailed);
    }
    public static void WriteItemValue(this Utf8JsonWriter writer, IJsonSerializable serializable, bool detailed = false) {
        serializable.ToJson(writer, detailed);
    }
    public static string ToJson(this IJsonSerializable serializable, bool detailed = false) {
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        serializable.ToJson(writer, detailed);
        writer.Flush();
        return Encoding.UTF8.GetString(ms.ToArray()) ?? string.Empty;
    }
}
