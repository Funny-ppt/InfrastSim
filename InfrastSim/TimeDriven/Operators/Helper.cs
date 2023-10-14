namespace InfrastSim.TimeDriven.Operators;
internal static class Helper {
    public static bool HasGroupMember(this FacilityBase facility, string group) {
        return facility.Operators.Where(op => op.Groups.Contains(group)).Any();
    }
    public static int GroupMemberCount(this FacilityBase facility, string group) {
        return facility.Operators.Where(op => op.Groups.Contains(group)).Count();
    }
}
