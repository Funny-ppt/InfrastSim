namespace InfrastSim.TimeDriven.WebHelper.Enumerate;
internal struct EnumResult {
    public EnumResult(OpEnumData[] comb, int init_size, in Efficiency eff, in Efficiency extra_eff) {
        this.comb = comb;
        this.init_size = init_size;
        this.eff = eff;
        this.extra_eff = extra_eff;
    }

    public OpEnumData[] comb { get; set; }
    public int init_size {  get; set; }
    public Efficiency eff { get; set; }
    public Efficiency extra_eff { get; set; }
}
