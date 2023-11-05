namespace InfrastSim.TimeDriven.WebHelper;
public record struct Efficiency(double TradEff, double ManuEff, double PowerEff) {
    public static Efficiency operator -(Efficiency a, Efficiency b) {
        return new Efficiency(a.TradEff - b.TradEff, a.ManuEff - b.ManuEff, a.PowerEff - b.PowerEff);
    }
    public static Efficiency operator +(Efficiency a, Efficiency b) {
        return new Efficiency(a.TradEff + b.TradEff, a.ManuEff + b.ManuEff, a.PowerEff + b.PowerEff);
    }

    public double GetScore() {
        return TradEff * 5 + ManuEff * 6 + PowerEff * 3;
    }
}