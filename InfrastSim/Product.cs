namespace InfrastSim;
public record Product(string Name, int Volume, int RequiredLevel, TimeSpan ProduceTime, Material[]? Consumes = null) {
    public readonly static Product[] CombatRecords = {
        new("基础作战记录", 2, 1, TimeSpan.FromMinutes(45)),
        new("初级作战记录", 3, 2, TimeSpan.FromMinutes(80)),
        new("中级作战记录", 5, 3, TimeSpan.FromMinutes(180)),
    };
    public readonly static Product Gold = new("赤金", 2, 1, TimeSpan.FromMinutes(72));
    public readonly static Product[] StoneFragment = {
        new("源石碎片", 3, 3, TimeSpan.FromMinutes(60),
            new[]{ new Material("龙门币", 1600),
                   new Material("固源岩", 2),}),
        new("源石碎片", 3, 3, TimeSpan.FromMinutes(60),
            new[]{ new Material("龙门币", 1000),
                   new Material("装置", 1),}),
    };

    public readonly static Product[] AllProducts =
        CombatRecords.Append(Gold).Concat(StoneFragment).ToArray();
}