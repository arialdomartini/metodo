using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Metodo.CommandLine
{
    public class Span
    {
        public int OldFrom { get; set; }
        public int OldTo { get; set; }
        
        public int NewFrom { get; set; }
        public int NewTo { get; set; }

        public bool OverlapsWith(Span span) =>
            span.Contains(OldFrom) || span.Contains(OldTo) ||
            Contains(span.OldFrom) || Contains(span.OldTo)
        ;

        private bool Contains(int value) =>
            value >= OldFrom && value <= OldTo;

        private const string Template = @"@@\s\-(?<oldFrom>\d+),(?<oldLength>\d+)\s\+(?<newFrom>\d+),(?<newLength>\d+).*\s@@";

        public static Span SingleFrom(string diffLine)
        {
            var regex = new Regex(Template);
            var matchCollection = regex.Matches(diffLine);

            return ToSpan(matchCollection.First());
        }

        private static Span ToSpan(Match first)
        {
            var oldFrom = int.Parse(first.Groups["oldFrom"].Value);
            var oldLength = int.Parse(first.Groups["oldLength"].Value);
            var newFrom = int.Parse(first.Groups["newFrom"].Value);
            var newLength = int.Parse(first.Groups["newLength"].Value);
            return new Span
            {
                OldFrom = oldFrom,
                OldTo = oldFrom + oldLength,
                NewFrom = newFrom,
                NewTo = newFrom + newLength
            };
        }

        public static IEnumerable<Span> From(string diff)
        {
            var regex = new Regex(Template, RegexOptions.Multiline);
            var matchCollection = regex.Matches(diff);
            return matchCollection.Select(ToSpan);
        }
    }
}