
using BenchmarkDotNet.Attributes;
using DStack.Aggregates;

namespace DStack.Benchmarks;

[MemoryDiagnoser]
public class AggregateBenchmarks
{
    PersonAggregateState State = new PersonAggregateState();

    [Benchmark]
    public void WithConstrutor()
    {
        var agg = new PersonAggregate();

        agg?.SetState(State);
    }

    [Benchmark(Baseline = true)]
    public void WithActivator()
    {
        Activator.CreateInstance(typeof(PersonAggregate), State);
    }
}
