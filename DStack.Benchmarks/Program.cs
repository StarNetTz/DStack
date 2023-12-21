using BenchmarkDotNet.Running;

namespace DStack.Benchmarks
{
    public class Program
    {
        static void Main(string[] args)
        {
           var results = BenchmarkRunner.Run<JsonTests>();
        }
    }
}
