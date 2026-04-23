
using BenchmarkDotNet.Attributes;
using DStack.Aggregates;
using DStack.TestObjects;

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
