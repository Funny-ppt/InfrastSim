using InfrastSim.Algorithms;
using InfrastSim.TimeDriven.WebHelper.Enumerate;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;

namespace InfrastSim.TimeDriven.WebHelper;
internal class EnumerateContext {
    static readonly List<int> primes = EularSieve.Resolve((1 << 24) - 1);
    const int MOD = 16777213;

    int max_size;
    int ucnt = 0;
    OpEnumData[] ops = null!;
    Simulator simu = null!;
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
    bool ValidateResult(in EnumResult result) {
        if (result.eff.IsZero()) return false;
        if (result.init_size == 1) return true;

        var comb = result.comb;
        for (int i = 0; i < result.init_size; i++) {
            var eff = TestMany(simu, comb.Where(o => o != comb[i]));
            var div_eff = eff + comb[i].SingleEfficiency;
            var diff_eff = eff - div_eff;
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
    public IOrderedEnumerable<EnumResult> Enumerate(JsonDocument json) {
        var root = json.RootElement;
        var preset = root.GetProperty("preset");
        simu = InitSimulator(preset);
        baseline = simu.GetEfficiency();
        ops = root.GetProperty("ops").Deserialize<OpEnumData[]>(EnumerateHelper.Options);

        var uidmap = new Dictionary<string, int>();
        for(var i = 0; i < ops.Length; i++) {
            var op = ops[i];
            if (uidmap.TryGetValue(op.Name, out var uid)) {
                op.uid = uid;
            } else {
                op.uid = uidmap[op.Name] = ucnt++;
            }
            op.id = i;
            op.prime = primes[i];
            op.SingleEfficiency = TestSingle(simu, op);
        }

        max_size = Math.Min(32, ops.Length);
        if (root.TryGetProperty("max_size", out var max_size_elem)) {
            max_size = Math.Min(max_size, max_size_elem.GetInt32());
        }

        var tasks = new Task[ops.Length];
        foreach (var op in ops) {
            tasks[op.id] = Task.Run(() => {
                InitProc(op, InitSimulator(preset));
            });
        }
        Task.WaitAll(tasks);
        return results.Values.Where(v => ValidateResult(v)).OrderByDescending(v => v.eff.GetScore());
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
        return (comb.Length << 24) | (int)f;
    }

    void InitProc(OpEnumData op, Simulator simu) {
        RecursivelyProc(new[] { op }, 1, simu, op.SingleEfficiency);
        if (op.RelevantOps == null) {
            return;
        }
        var relevants = ops.Where(o => op.RelevantOps.Contains(o.Name)).ToArray();
        for (int i = 1; i <= relevants.Length; i++) {
            var combs = new Combination<OpEnumData>(relevants, i);
            foreach (var e in combs.ToEnumerable()) {
                var c = (OpEnumData[])e;
                var comb = new OpEnumData[i + 1];
                Array.Copy(c, comb, c.Length);
                comb[^1] = op;

                Efficiency eff;
                try {
                    eff = TestMany(simu, comb);
                } catch {
                    continue;
                }
                RecursivelyProc(comb, comb.Length, simu, eff);
            }
        }
    }
    void RecursivelyProc(OpEnumData[] comb, int init_size, Simulator simu, Efficiency eff) {
        var f = new BitArray(ucnt);
        foreach (var op in comb) {
            f[op.uid] = true;
        }
        foreach (var op in ops) {
            if (f[op.uid]) continue;
            var new_comb = new OpEnumData[comb.Length + 1];
            Array.Copy(comb, new_comb, comb.Length);
            new_comb[comb.Length] = op;
            Proc(new_comb, init_size, simu, eff);
        }
    }
    void Proc(OpEnumData[] comb, int init_size, Simulator simu, Efficiency base_eff) {
        var gid = GetGroupId(comb);
        if (!results.TryAdd(gid, default)) {
            return;
        }
        var op_data = comb.Last();
        OperatorBase op;
        try {
            op = simu.Assign(op_data);
        } catch {
            return; // 失败，所以不需要移除
        }
        var eff = simu.GetEfficiency();
        var extra_eff = eff - base_eff - op_data.SingleEfficiency;
        if (!extra_eff.IsPositive()) {
            return;
        }
        var tot_extra_eff = eff;
        foreach (var opd in comb) {
            tot_extra_eff -= opd.SingleEfficiency;
        }
        results[gid] = new(comb, init_size, eff, tot_extra_eff);

        if (comb.Length < max_size) {
            RecursivelyProc(comb, init_size, simu, base_eff);
        }
        op.ReplaceByTestOp(); // 递归完毕，移除
    }
}
