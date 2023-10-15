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
    public static int GetRealGoldProductionLine(this TimeDrivenSimulator simu)
    {
        return simu.ModifiableFacilities
                   .Where(fac => fac is ManufacturingStation manufacturing
                       && manufacturing.Product == Product.Gold).Count();
    }
    public static int GetGoldProductionLine(this TimeDrivenSimulator simu)
    {
        return (int)simu.ExtraGoldProductionLine + simu.GetRealGoldProductionLine();
    }

    public static int GetPowerStations(this TimeDrivenSimulator simu)
    {
        return (int)simu.ExtraPowerStation + simu.PowerStations.Count();
    }
    public static OperatorBase? GetVip(this Dormitory dorm)
    {
        return dorm.Operators
            .OrderByDescending(op => op, new VipPriorityComparer())
            .FirstOrDefault();
    }
}
