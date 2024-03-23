namespace InfrastSim;
internal static class TicksHelper {
    public const long TimeSpanTicksPerTick = 100000L;
    public const int TicksPerSecond = 100;
    public const int TicksPerMinute = TicksPerSecond * 60;
    public const int TicksPerHours = TicksPerMinute * 24;
    public const int TicksPerDrone = TicksPerMinute * 3; // 3 minute
    public const int TicksToProduceDrone = TicksPerMinute * 6; // 6 minute
    public const int TicksToProducerefresh = TicksPerHours * 12; // 12 hours

    public static int ToSimuTicks(this TimeSpan ts) => (int)(ts.Ticks / TimeSpanTicksPerTick);
    public static int TotalSeconds(this TimeSpan ts) => (int)(ts.Ticks / TimeSpan.TicksPerSecond);
    public static int HoursNotExceed(this TimeSpan ts, int n) => Math.Min(n, (int)(ts.Ticks / TimeSpan.TicksPerHour));
    public static TimeSpan SimuTicksToTimeSpan(this int simuTicks) => TimeSpan.FromTicks(simuTicks * TimeSpanTicksPerTick);
}
