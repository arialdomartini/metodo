using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using FluentAssertions;
using Metodo.CommandLine;
using Xunit;

namespace Metodo.Test
{
    public class SpanTest
    {
        [Fact]
        public void regex_should_detect_from_to_values()
        {
            var diffLine = "@@ -1,8 +12,998 @@";
            var template = @"@@\s\-(?<oldFrom>\d+),(?<oldLength>\d+)\s\+(?<newFrom>\d+),(?<newLength>\d+).*\s@@";
            var regex = new Regex(template);
            var matchCollection = regex.Matches(diffLine);
            var first = matchCollection.First();

            first.Groups["oldFrom"].Value.Should().Be("1");
            first.Groups["oldLength"].Value.Should().Be("8");
            first.Groups["newFrom"].Value.Should().Be("12");
            first.Groups["newLength"].Value.Should().Be("998");

            first.Should().NotBeNull();
        }
        
        [Fact]
        public void should_extract_a_span_from_the_diff_string_relevant_line()
        {
            var diffLine = "@@ -1,8 +1,9 @@";

            var result = Span.SingleFrom(diffLine);

            result.Should().BeEquivalentTo(new Span
            {
                OldFrom = 1,
                OldTo = 1 + 8,
                NewFrom = 1,
                NewTo = 1 + 9
            });
        }
        
        [Fact]
        public void should_extract_spans_from_diff_string()
        {
            var diff = @"diff --git a/builtin-http-fetch.c b/http-fetch.c
similarity index 95%
rename from builtin-http-fetch.c
rename to http-fetch.c
index f3e63d7..e8f44ba 100644
--- a/builtin-http-fetch.c
+++ b/http-fetch.c
@@ -1,8 +1,9 @@
 #include ""cache.h""
#include ""walker.h""

                -int cmd_http_fetch(int argc, const char **argv, const char *prefix)
            +int main(int argc, const char **argv)
            {
                +       const char *prefix;
                    struct walker *walker;
                int commits_on_stdin = 0;
                int commits;
                    @@ -18,6 +19,8 @@ int cmd_http_fetch(int argc, const char **argv, const char *prefix)
                int get_verbosely = 0;
                int get_recover = 0;

                +       prefix = setup_git_directory();
                +
                    git_config(git_default_config, NULL);

                while (arg < argc && argv[arg][0] == '-') {";
            
            var result = Span.From(diff).ToList();

            result.Count.Should().Be(2);
            result[0].Should().BeEquivalentTo(new Span
            {
                OldFrom = 1,
                OldTo = 1 + 8,
                NewFrom = 1,
                NewTo = 1 + 9
            });
            result[1].Should().BeEquivalentTo(new Span
            {
                OldFrom = 18,
                OldTo = 18 + 6,
                NewFrom = 19,
                NewTo = 19 + 8
            });
        }
        
        
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
            var span1 = new Span {OldFrom = from1, OldTo = to1};
            var span2 = new Span {OldFrom = from2, OldTo = to2};

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
                        Name = "foo", Span = new Span {OldFrom = 10, OldTo = 20}
                    },
                    new Method
                    {
                        Name = "bar", Span = new Span {OldFrom = 21, OldTo = 30}
                    },
                    new Method
                    {
                        Name = "baz", Span = new Span {OldFrom = 31, OldTo = 40}
                    },
                    new Method
                    {
                        Name = "barbaz", Span = new Span {OldFrom = 50, OldTo = 100}
                    }
                }
            };

            var change = new Change {Spans = new List<Span>
            {
                new Span {OldFrom = 8, OldTo = 11},
                new Span {OldFrom = 29, OldTo = 30},
                new Span {OldFrom = 200, OldTo = 300}
            }};

            var result = methods.GetAffectedBy(change).ToList();

            result.Should().Contain("foo");
            result.Should().Contain("bar");
            result.Should().NotContain("baz");
        }   
    }
}