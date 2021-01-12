using Xunit;

namespace DStack.Aggregates.UnitTests
{
    public class DomainErrorTests
    {
        [Fact]
        public void Should_Create_Using_Default_Constructor()
        {
            var e = new DomainError();
        }

        [Fact]
        public void Should_Create_Named_DomainError()
        {
            var e = DomainError.Named("SampleDomainError", "Something went wrong");

            Assert.Equal("SampleDomainError", e.Name);
        }
    }
}