using System.Collections;

namespace InfrastSim.Algorithms;
internal static class EularSieve {
    public static List<int> Resolve(int n) {
        var primes = new List<int>();
        var isPrime = new BitArray(n + 1);

        for (int i = 2; i <= n; i++) {
            if (!isPrime[i]) {
                primes.Add(i);
            }

            for (int j = 0; j < primes.Count && i * primes[j] <= n; j++) {
                isPrime[i * primes[j]] = true;

                if (i % primes[j] == 0) {
                    break;
                }
            }
        }

        return primes;
    }
}
