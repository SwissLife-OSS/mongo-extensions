using System;
using BenchmarkDotNet.Attributes;

namespace Prime.Extensions.Benchmarks
{
    [MemoryDiagnoser]
    public class FindIdsBenchmarks
    {
        [Benchmark(Baseline = true)]
        public void FindIdsViaMongoDbClassicCalls()
        {
            Console.WriteLine("dd");
        }
    }
}
