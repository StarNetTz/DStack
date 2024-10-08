using BenchmarkDotNet.Running;

namespace DStack.Benchmarks
{
    public class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<JsonTests>();
            BenchmarkRunner.Run<AggregateBenchmarks>();
        }
    }
}
