namespace InfrastSim;

internal static class Util {
    public static bool Equals(double self, double other, double epsilon = 1e-9) {
        return Math.Abs(self - other) < epsilon;
    }
}
    