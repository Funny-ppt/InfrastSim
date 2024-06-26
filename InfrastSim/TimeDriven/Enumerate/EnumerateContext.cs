using InfrastSim.Algorithms;
using InfrastSim.TimeDriven.WebHelper;
using System.Collections;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;

namespace InfrastSim.TimeDriven.Enumerate;
internal class EnumerateContext {
    static readonly List<int> primes = EularSieve.Resolve((1 << 24) - 1);
    const int MOD = 16777213;

    int max_size;
    int ucnt = 0;
    OpEnumData[] ops = null!;
    Efficiency baseline;
    ConcurrentDictionary<int, EnumResult> results = new();

    Efficiency TestSingle(Simulator simu, OpEnumData data) {
        var op = simu.Assign(data);
        var diff = simu.GetEfficiency() - baseline;
        op.ReplaceByTestOp();
        return diff;
    }
    Efficiency TestMany(Simulator simu, IEnumerable<OpEnumData> datas) {
        try {
            foreach (var data in datas) {
                simu.Assign(data);
            }
            var diff = simu.GetEfficiency() - baseline;
            return diff;
        } finally {
            simu.FillTestOp();
        }
    }
    bool ValidateResult(Simulator simu, EnumResult result) {
        if (result.eff.IsZero()) return false;
        if (result.init_size == 1) return true;

        var comb = result.comb;
        for (int i = 0; i < result.init_size; i++) {
            var eff = TestMany(simu, comb.Where(o => o != comb[i]));
            var div_eff = eff + comb[i].SingleEfficiency;
            var diff_eff = result.eff - div_eff;
            if (!diff_eff.IsPositive()) return false;
        }
        return true;
    }

    //class Comparer : IComparer<(OpEnumData[] comb, Efficiency eff, Efficiency extra_eff)> {
    //    public int Compare((OpEnumData[] comb, Efficiency eff, Efficiency extra_eff) x, (OpEnumData[] comb, Efficiency eff, Efficiency extra_eff) y) {
    //        double x_score = x.eff.GetScore(), y_score = y.eff.GetScore();
    //        if (!Util.Equals(x_score, y_score)) return x_score - y_score < 0 ? -1 : 1;
    //        x_score = x.extra_eff.GetScore() / x.comb.Length;
    //        y_score = y.extra_eff.GetScore() / y.comb.Length;
    //        if (!Util.Equals(x_score, y_score)) return x_score - y_score < 0 ? -1 : 1;
    //        return 0;
    //    }
    //}

    private EnumerateContext() { }
    public static IOrderedEnumerable<EnumResult> Enumerate(JsonDocument json) {
        return new EnumerateContext().EnumerateImpl(json);
    }

    public IOrderedEnumerable<EnumResult> EnumerateImpl(JsonDocument json) {
        var root = json.RootElement;
        var preset = root.GetProperty("preset");
        var simu1 = InitSimulator(preset);
        baseline = simu1.GetEfficiency();
        ops = root.GetProperty("ops").Deserialize<OpEnumData[]>(EnumerateHelper.Options)
            ?? throw new InvalidOperationException("expected 'ops' as an array, readed null");

        var uidmap = new Dictionary<string, int>();
        for (var i = 0; i < ops.Length; i++) {
            var op = ops[i];
            if (uidmap.TryGetValue(op.Name, out var uid)) {
                op.uid = uid;
            } else {
                op.uid = uidmap[op.Name] = ucnt++;
            }
            op.id = i;
            op.prime = primes[i];
            op.SingleEfficiency = TestSingle(simu1, op);
        }

        max_size = Math.Min(32, ops.Length);
        if (root.TryGetProperty("max_size", out var max_size_elem)) {
            max_size = Math.Min(max_size, max_size_elem.GetInt32());
        }

        var channel = Channel.CreateUnbounded<Frame>();
        foreach (var op in ops)
            foreach (var frame in GenerateFrames(simu1, op))
                channel.Writer.TryWrite(frame);

        var tasks = new Task[Environment.ProcessorCount];
        for (int i = 0; i < tasks.Length; i++) {
            tasks[i] = Task.Run(() => {
                var simu = InitSimulator(preset);
                while (channel.Reader.TryRead(out var frame)) {
                    foreach (var outFrame in ProcessFrame(simu, frame)) {
                        channel.Writer.TryWrite(outFrame);
                    }
                }
            });
        }
        Task.WaitAll(tasks);

        return results.Values
            .Where(v => ValidateResult(simu1, v))
            .OrderByDescending(v => v.eff.GetScore());
    }
    static Simulator InitSimulator(JsonElement elem) {
        var simu = new Simulator();
        foreach (var prop in elem.EnumerateObject()) {
            simu.SetFacilityState(prop.Name, prop.Value);
        }
        simu.FillTestOp();
        return simu;
    }
    static int GetGroupId(OpEnumData[] comb) {
        long f = 1;
        foreach (var op in comb) {
            f = f * op.prime % MOD;
        }
        return comb.Length << 24 | (int)f;
    }

    readonly struct Frame {
        public OpEnumData[] comb { get; init; } = null!;
        public BitArray uset { get; init; } = null!;
        public int gid { get; init; }
        public int init_size { get; init; }
        public Efficiency base_eff { get; init; }

        public Frame(OpEnumData[] comb, Efficiency base_eff, int ucnt) {
            this.comb = comb;
            this.base_eff = base_eff;
            gid = GetGroupId(comb);
            init_size = comb.Length;
            uset = new(ucnt);
            foreach (var op in comb) {
                uset[op.uid] = true;
            }
        }
        public Frame FromComb(OpEnumData[] new_comb, Efficiency base_eff, int gid) {
            var op = new_comb[^1];
            BitArray new_uset = new(uset);
            new_uset[op.uid] = true;
            return new Frame {
                comb = new_comb,
                base_eff = base_eff,
                gid = gid,
                init_size = init_size,
                uset = new_uset
            };
        }
    }

    IEnumerable<Frame> GenerateFrames(Simulator simu, OpEnumData op) {
        yield return new([op], op.SingleEfficiency, ucnt);

        if (op.RelevantOps == null) {
            yield break;
        }

        var relevants = ops.Where(o => op.RelevantOps.Contains(o.Name)).ToArray();
        for (int i = 1; i <= relevants.Length; i++) {
            var combs = new Combination<OpEnumData>(relevants, i);
            foreach (var e in combs.ToEnumerable()) {
                OpEnumData[] comb = [.. (OpEnumData[])e, op]; // 这里利用了Combination实现返回值为内部数组的特性

                Efficiency eff;
                try {
                    eff = TestMany(simu, comb);
                } catch {
                    continue;
                }
                yield return new(comb, eff, ucnt);
            }
        }
    }
    IEnumerable<Frame> ProcessFrame(Simulator simu, Frame frame) {
        foreach (var op in ops) {
            if (frame.uset[op.uid]) continue; // 处理干员表中有同一干员的不同位置

            var gid = (frame.comb.Length + 1 << 24) | (frame.gid & 0xffffff) * op.prime % MOD;
            if (!results.TryAdd(gid, default)) {
                continue;
            }

            OpEnumData[] next_comb = [.. frame.comb, op];
            Efficiency eff;
            try { // 检验组合是否能被基建容纳，如果可以，则计算其效率
                eff = TestMany(simu, next_comb);
            } catch {
                continue;
            }

            // 检验新加入组合的干员是否贡献了额外效率
            var extra_eff = eff - frame.base_eff - op.SingleEfficiency;
            if (!extra_eff.IsPositive()) {
                continue;
            }
            // 计算相较于单干员效率总和的额外效率
            var tot_extra_eff = eff;
            foreach (var opd in next_comb) {
                tot_extra_eff -= opd.SingleEfficiency;
            }
            results[gid] = new(next_comb, frame.init_size, eff, tot_extra_eff);
            if (next_comb.Length < max_size) yield return frame.FromComb(next_comb, eff, gid);
        }
    }
}
