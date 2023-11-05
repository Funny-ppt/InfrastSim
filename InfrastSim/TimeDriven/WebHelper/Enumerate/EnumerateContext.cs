using InfrastSim.Algorithms;
using System.Collections;
using System.Collections.Concurrent;
using System.Text.Json;

namespace InfrastSim.TimeDriven.WebHelper;
internal class EnumerateContext {
    static readonly List<int> primes = EularSieve.Resolve((1 << 24) - 1);
    const int MOD = 16777213;

    int max_size;
    int ucnt = 0;
    OpEnumData[] ops = null!;
    Efficiency baseline;
    ConcurrentDictionary<int, (OpEnumData[] comb, Efficiency eff, Efficiency extra_eff)> results = new();

    Efficiency TestSingle(Simulator simu, OpEnumData data) {
        simu.Assign(data);
        var diff = simu.GetEfficiency() - baseline;
        var op = simu.GetOperator(data.Name);
        op.Facility?.FillTestOp();
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
    public IOrderedEnumerable<(OpEnumData[] comb, Efficiency eff, Efficiency extra_eff)> Enumerate(JsonDocument json) {
        var root = json.RootElement;
        var preset = root.GetProperty("preset");
        var simu = InitSimulator(preset);

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
                Proc(new[] { op }, InitSimulator(preset), op.SingleEfficiency);
            });
        }
        Task.WaitAll(tasks);
        return results.Values.OrderByDescending(v => v.eff.GetScore());
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

    void Proc(OpEnumData[] comb, Simulator simu, Efficiency base_eff) {
        var gid = GetGroupId(comb);

        Efficiency eff = base_eff;
        if (comb.Length > 1) {
            if (!results.TryAdd(gid, (comb, default, default))) {
                return;
            }
            try {
                eff = TestMany(simu, comb);
            } catch {
                return;
            }
            var extra_eff = eff - base_eff;
            if (extra_eff.TradEff < -Util.Epsilon || extra_eff.ManuEff < -Util.Epsilon || extra_eff.PowerEff < -Util.Epsilon) {
                return;
            }
            if (extra_eff.IsZero()) {
                return;
            }

            var tot_extra_eff = eff;
            foreach (var op in comb) {
                tot_extra_eff -= op.SingleEfficiency;
            }
            results[gid] = (comb, eff, tot_extra_eff);
        }

        var f = new BitArray(ucnt);
        foreach (var op in comb) {
            f[op.uid] = true;
        }
        if (comb.Length < max_size) {
            foreach (var op in ops) {
                if (f[op.uid]) continue;
                var new_comb = new OpEnumData[comb.Length + 1];
                Array.Copy(comb, new_comb, comb.Length);
                new_comb[comb.Length] = op;
                Proc(new_comb, simu, eff);
            }
        }
    }
}
