namespace InfrastSim;

internal record Order(int RequiredLevel, TimeSpan ProduceTime, Material Consumes, Material Earns) {
    public readonly static Order[] Gold = {
        new(1, TimeSpan.FromMinutes(144), new Material("赤金", 2),  new Material("龙门币", 1000)),
        new(2, TimeSpan.FromMinutes(210), new Material("赤金", 3),  new Material("龙门币", 1500)),
        new(3, TimeSpan.FromMinutes(276), new Material("赤金", 4),  new Material("龙门币", 2000)),
    };
    public readonly static Order OriginStone = new(3, TimeSpan.FromMinutes(120), new Material("源石碎片", 2), new Material("合成玉", 20));
}