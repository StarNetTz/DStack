using Starnet.ObjectComparer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace DStack.Aggregates.Testing
{
    //ncrunch: no coverage start
    public abstract class AggregateTesterBase<TCommand, TEvent> : List<TCommand, TEvent>, IDisposable
    {
        bool ThenWasCalled = false;
        readonly List<TEvent> GivenEvents = new List<TEvent>();
        TCommand WhenCommand;
        readonly List<TEvent> ThenEvents = new List<TEvent>();

        protected abstract Task<ExecuteCommandResult<TEvent>> ExecuteCommand(TEvent[] store, TCommand cmd);

        public void Given(params TEvent[] g)
        {
            GivenEvents.AddRange(g);
        }

        protected void When(TCommand command)
        {
            WhenCommand = command;
        }

        public async Task ExpectNoEvents()
        {
            await Expect(NoProducedEvents, NoPublishedEvents);
        }

        public async Task Expect(params TEvent[] g)
        {
            await Expect(g.ToList(), new List<TEvent>());
        }

        public async Task Expect(List<TEvent> producedEvents, List<TEvent> publishedEvents)
        {
            ThenWasCalled = true;
            ThenEvents.AddRange(producedEvents);

            
            var givenEvents = GivenEvents.ToArray();
            var res = await ExecuteCommand(givenEvents, WhenCommand);
            TEvent[] actualProduced = res.ProducedEvents;
            TEvent[] actualPublished = res.PublishedEvents;

            var producedEventsResults = CompareAssert(ThenEvents.ToArray(), actualProduced).ToArray();
            var producedResults = GetFormattedResults(producedEventsResults);
            Console.Write(producedResults);

            var publishedEventsResults = CompareAssert(publishedEvents.ToArray(), actualPublished).ToArray();
            var publishedResults = GetFormattedResults(publishedEventsResults);
            Console.Write(publishedResults);

            if (producedEventsResults.Any(r => r.Failure != null))
                throw new XunitException($"Specification failed on produced events:\n{producedResults}");

            if (publishedEventsResults.Any(r => r.Failure != null))
                throw new XunitException($"Specification failed on published events:\n{publishedResults}");
        }

        protected static string GetFormattedResults(ICollection<ExpectResult> exs)
        {
            var results = exs.ToArray();
            var failures = results.Where(f => f.Failure != null).ToArray();
            if (!failures.Any())
                return "\nResults: [Passed]";

            var sb = new StringBuilder();
            sb.Append("\nResults: [Failed]");
           

            for (int i = 0; i < results.Length; i++)
            {
                sb.AppendLine(GetAdjusted(string.Format("  {0}. ", (i + 1)), results[i].Expectation));
                sb.AppendLine(GetAdjusted("     ", results[i].Failure ?? "PASS"));
            }
            return sb.ToString();
        }

        public async Task ExpectError(string name)
        {
            ThenWasCalled = true;
            try
            {
                await ExecuteCommand(GivenEvents.ToArray(), WhenCommand);
            }
            catch (DomainError e)
            {
                if (e.Name.Equals(name))
                    return;
            }
            throw new XunitException($"Specification failed on expected error: {name}");
        }

        protected static IEnumerable<ExpectResult> CompareAssert(TEvent[] expected, TEvent[] actual)
        {
            var max = Math.Max(expected.Length, actual.Length);

            for (int i = 0; i < max; i++)
            {
                var ex = expected.Skip(i).FirstOrDefault();
                var ac = actual.Skip(i).FirstOrDefault();

                var expectedString = ex == null ? "No event expected" : ex.ToString();
                var actualString = ac == null ? "No event actually" : ac.ToString();

                var result = new ExpectResult { Expectation = expectedString };

                var realDiff = ObjectComparer.FindDifferences(ex, ac);
                if (!string.IsNullOrEmpty(realDiff))
                {
                    var stringRepresentationsDiffer = expectedString != actualString;

                    result.Failure = stringRepresentationsDiffer ?
                        GetAdjusted("Was:  ", actualString) :
                        GetAdjusted("Diff: ", realDiff);
                }

                yield return result;
            }
        }

        public static string GetAdjusted(string adj, string text)
        {
            var first = true;
            var builder = new StringBuilder();
            foreach (var s in text.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                builder.Append(first ? adj : new string(' ', adj.Length));
                builder.AppendLine(s);
                first = false;
            }
            return builder.ToString();
        }

       
        protected AggregateTesterBase()
        {
            WhenCommand = default(TCommand);
            GivenEvents.Clear();
            ThenEvents.Clear();
        }

        
        public void TeardownSpecification()
        {
            if (!ThenWasCalled)
                throw new XunitException("THEN was not called from the unit test");
        }

        public IEnumerable<SpecificationInfo<TCommand, TEvent>> ListSpecifications()
        {
            throw new NotImplementedException();
        }

        public static List<TEvent> ToEventList(TEvent ev)
        {
            return new List<TEvent>() { ev };
        }

        public void Dispose()
        {
            if (!ThenWasCalled)
                throw new XunitException("THEN was not called from the unit test");
        }

        public static List<TEvent> NoProducedEvents  => new List<TEvent>();
        public static List<TEvent> NoPublishedEvents => new List<TEvent>();
    }

    public class ExecuteCommandResult<TEvent>
    {
        public TEvent[] ProducedEvents { get; set; }
        public TEvent[] PublishedEvents { get; set; }
    }
    //ncrunch: no coverage end
}