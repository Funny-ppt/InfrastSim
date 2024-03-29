namespace InfrastSim.TimeDriven.Enumerate;
public record struct Efficiency(int TradEff, int ManuEff, int PowerEff) {
    public static Efficiency operator -(Efficiency a, Efficiency b) {
        return new Efficiency(a.TradEff - b.TradEff, a.ManuEff - b.ManuEff, a.PowerEff - b.PowerEff);
    }
    public static Efficiency operator +(Efficiency a, Efficiency b) {
        return new Efficiency(a.TradEff + b.TradEff, a.ManuEff + b.ManuEff, a.PowerEff + b.PowerEff);
    }

    public readonly int GetScore() {
        return TradEff * 5 + ManuEff * 6 + PowerEff * 3;
    }

    public readonly bool IsZero() {
        return TradEff == 0 && ManuEff == 0 && PowerEff == 0;
    }

    public readonly bool IsPositive() {
        return TradEff >= 0 && ManuEff >= 0 && PowerEff >= 0 && !IsZero();
    }
}