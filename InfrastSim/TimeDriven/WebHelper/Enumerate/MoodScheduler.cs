namespace InfrastSim.TimeDriven.WebHelper.Enumerate;
internal class MoodScheduler {
    DispatchGroup[] groups;
    Dictionary<string, OpConf> opSettings;
    TimeSpan[] times;
    TimeSpan cycle;

    Simulator sim;
    List<(TimeSpan time, Simulator arr)> arrangements;

    public MoodScheduler(DispatchGroup[] groups, Dictionary<string, OpConf> opSettings, TimeSpan[] times) {
        this.groups = groups;
        this.opSettings = opSettings;
        this.times = times;
        this.cycle = TimeSpan.Zero;
        foreach (var time in times) {
            cycle += time;
        }

        sim = new();
        foreach (var (name, sett) in opSettings) {
            var op = sim.GetOperator(name);
            op.SetMood(sett.InitMood);
        }
    }

    public void Calculate() {
        var cur_sim = sim.Clone();
        foreach (var time in times) {

        }
    }
}
