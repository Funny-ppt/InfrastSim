using System.Diagnostics;

namespace InfrastSim.TimeDriven;
internal class VipPriorityComparer : IComparer<OperatorBase> {
    public int Compare(OperatorBase? x, OperatorBase? y) {
        Debug.Assert(x != null && y != null);
        if (x.IsFullOfEnergy) return -1;
        if (y.IsFullOfEnergy) return 1;
        if (x.DormVipPriority != y.DormVipPriority) return x.DormVipPriority - y.DormVipPriority;
        if (Util.Equals(x.Mood, y.Mood)) return 0;
        return y.Mood > x.Mood ? 1 : -1;
    }
}
