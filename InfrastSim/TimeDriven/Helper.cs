using System.Text;
using System.Text.Json;

namespace InfrastSim.TimeDriven;
internal static class Helper {
    public static bool VisibleToLookupSkill(this OperatorBase op) {
        if (op.Facility == null) return false;
        if (op.Facility.IsWorking) {
            return !op.IsTired;
        } else {
            return !op.IsExausted;
        }
    }
    public static OperatorBase? FindOp(this FacilityBase facility, string opName)
    {
        return facility.Operators.Where(op => op.Name == opName).FirstOrDefault();
    }
    /// <summary>
    /// 适用于 xxx干员是否在xxx位置, 不考虑红脸
    /// </summary>
    public static bool IsOpInFacility(this Simulator simu, string name, FacilityType type) {
        return simu.GetOperator(name).Facility?.Type == type;
    }
    /// <summary>
    /// 适用于 基建中xxx组干员的数量, 考虑红脸
    /// </summary>
    public static int GroupMemberCount(this Simulator simu, string group) {
        return simu.OperatorsInFacility.Where(op => op.VisibleToLookupSkill() && op.HasGroup(group)).Count();
    }
    /// <summary>
    /// 适用于 设施中xxx组干员, 考虑红脸
    /// </summary>
    public static IEnumerable<OperatorBase> GroupMembers(this FacilityBase facility, string group) {
        return facility.Operators.Where(op => op.VisibleToLookupSkill() && op.HasGroup(group));
    }
    /// <summary>
    /// 适用于 设施中是否有xxx组干员, 考虑红脸
    /// </summary>
    public static bool HasGroupMember(this FacilityBase facility, string group)
    {
        return facility.GroupMembers(group).Any();
    }
    /// <summary>
    /// 适用于 设施中xxx组干员的数量, 考虑红脸
    /// </summary>
    public static int GroupMemberCount(this FacilityBase facility, string group)
    {
        return facility.GroupMembers(group).Count();
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

    public static int PowerStationsCount(this Simulator simu)
    {
        return (int)simu.ExtraPowerStation + simu.PowerStations.Count();
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
