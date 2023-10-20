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
    public static bool IsProduceGold(this ManufacturingStation manufacturing) {
        return manufacturing.Product == Product.Gold;
    }
    public static bool IsProduceCombatRecord(this ManufacturingStation manufacturing) {
        return Product.CombatRecords.Contains(manufacturing.Product);
    }
    public static bool IsProduceOriginStone(this ManufacturingStation manufacturing) {
        return Product.StoneFragment.Contains(manufacturing.Product);
    }
}
