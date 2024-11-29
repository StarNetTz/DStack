
using BenchmarkDotNet.Attributes;
using DStack.Aggregates;

namespace DStack.Benchmarks;

[MemoryDiagnoser]
public class AggregateBenchmarks
{
    readonly PersonAggregateState State = new();

    [Benchmark]
    public void WithStateSetter()
    {
        new PersonAggregate().SetState(State);
    }
}
