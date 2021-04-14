using BenchmarkDotNet.Running;

namespace Prime.Extensions.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //BenchmarkSwitcher
            //    .FromAssembly(typeof(Program).Assembly)
            //    .Run(args, new DebugInProcessConfig());
            BenchmarkRunner.Run<FindIdsBenchmarks>();
        }
    }
}
