using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FluentAssertions;
using Xunit;

namespace Metodo.Test
{
    public class Span
    {
        public int From { private get; set; }
        public int To { private get; set; }

        public bool OverlapsWith(Span span) =>
            span.Contains(From) || span.Contains(To) ||
            Contains(span.From) || Contains(span.To)
        ;

        private bool Contains(int value) =>
            value >= From && value <= To;
    }

    public class SpanTest
    {
        [Theory]
        [InlineData(10, 20, 10, 20, true)]
        [InlineData(10, 20, 12, 18, true)]
        [InlineData(10, 20, 8, 12, true)]
        [InlineData(8, 12, 10, 20, true)]
        [InlineData(10, 20, 18, 22, true)]
        [InlineData(18, 22, 10, 20, true)]
        [InlineData(10, 20, 5, 8, false)]
        [InlineData(10, 20, 5, 10, true)]
        [InlineData(10, 20, 20, 30, true)]
        public void overlap_test(int from1, int to1, int from2, int to2, bool expected)
        {
            var span1 = new Span {From = from1, To = to1};
            var span2 = new Span {From = from2, To = to2};

            var result = span1.OverlapsWith(span2);

            result.Should().Be(expected);
        }
        
        [Fact]
        public void should_detect_affected_method()
        {
            var methods = new MethodCollection
            {
                Methods = new List<Method>
                {
                    new Method
                    {
                        Name = "foo", Span = new Span {From = 10, To = 20}
                    },
                    new Method
                    {
                        Name = "bar", Span = new Span {From = 21, To = 30}
                    },
                    new Method
                    {
                        Name = "baz", Span = new Span {From = 31, To = 40}
                    },
                    new Method
                    {
                        Name = "barbaz", Span = new Span {From = 50, To = 100}
                    }
                }
            };

            var change = new Change {Spans = new List<Span>
            {
                new Span {From = 8, To = 11},
                new Span {From = 29, To = 30},
                new Span {From = 200, To = 300}
            }};

            var result = methods.GetAffectedBy(change).ToList();

            result.Should().Contain("foo");
            result.Should().Contain("bar");
            result.Should().NotContain("baz");
        }
        
    }

    public class MethodCollection
    {
        public IEnumerable<string> GetAffectedBy(Change change) => 
            Methods
                .Where(m => 
                    change.Spans.Any(s => s.OverlapsWith(m.Span)))
                .Select(x => x.Name);

        public List<Method> Methods { get; set; }
    }
    
    public class Change
    {
        public List<Span> Spans { get; set; }
    }

    public class Method
    {
        public string Name { get; set; }
        public Span Span { get; set; }
    }
}