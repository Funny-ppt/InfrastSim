using System.Collections;

namespace InfrastSim.TimeDriven.WebHelper;
internal class Combination<T> {
    public Combination(IList<T> values, int n) {
        this.values = values;
        stack = new int[n];
        arr = new T[n];
        N = n;
        M = values.Count;
        cur = 1;
        p = 1;
        arr[0] = values[0];
    }

    IList<T> values;
    int[] stack;
    T[] arr;
    int cur;
    int p;
    public int N { get; }
    public int M { get; }

    public IEnumerable<T>? Enumerate() {
        while (cur > 0 && M - p < N - cur) {
            p = stack[--cur] + 1; // 弹出最后的数，并递增上一个的位置
        }
        if (M - p < N - cur) { // 剩余的数不够，返回null
            return null;
        }
        while (cur < N) { // 取数，填充arr
            stack[cur] = p;
            arr[cur] = values[p];
            cur++;
            p++;
        }
        p = stack[--cur] + 1; // 弹出最后的数，并递增上一个的位置
        return arr;
    }

    public IEnumerator<IEnumerable<T>> GetEnumerator() {
        return new Enumerator(this);
    }

    public IEnumerable<IEnumerable<T>> ToEnumerable() {
        IEnumerable<T>? cur = null;
        while ((cur = Enumerate()) != null) {
            yield return cur;
        }
    }

    class Enumerator : IEnumerator<IEnumerable<T>> {
        Combination<T> _combination;
        IEnumerable<T>? _cur = null;

        public Enumerator(Combination<T> combination) {
            _combination = combination;
        }

        public IEnumerable<T> Current => _cur;
        object IEnumerator.Current => _cur;

        public bool MoveNext() {
            _cur = _combination.Enumerate();
            return _cur != null;
        }

        public void Reset() {
            _combination = new Combination<T>(_combination.values, _combination.N);
            _cur = null;
        }

        public void Dispose() {
        }
    }
}
