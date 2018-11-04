using System.Collections.Generic;
using System.Linq;

namespace Metodo.CommandLine
{
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