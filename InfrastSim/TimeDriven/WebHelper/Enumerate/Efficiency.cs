namespace InfrastSim.TimeDriven.WebHelper;
public record struct Efficiency(double TradEff, double ManuEff, double PowerEff) {
    public static Efficiency operator -(Efficiency a, Efficiency b) {
        return new Efficiency(a.TradEff - b.TradEff, a.ManuEff - b.ManuEff, a.PowerEff - b.PowerEff);
    }
    public static Efficiency operator +(Efficiency a, Efficiency b) {
        return new Efficiency(a.TradEff + b.TradEff, a.ManuEff + b.ManuEff, a.PowerEff + b.PowerEff);
    }

    public readonly double GetScore() {
        return TradEff * 5 + ManuEff * 6 + PowerEff * 3;
    }

    public readonly bool IsZero() {
        return Util.Equals(0, TradEff) && Util.Equals(0, ManuEff) && Util.Equals(0, PowerEff);
    }
}