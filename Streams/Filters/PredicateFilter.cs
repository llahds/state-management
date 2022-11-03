using Newtonsoft.Json.Linq;

namespace Streams.Events.Filters
{
    public class PredicateFilter : IFilter
    {
        private readonly Func<JObject, bool> predicate;

        public PredicateFilter(
            Func<JObject, bool> predicate)
        {
            this.predicate = predicate;
        }

        public ValueTask<bool> Evaluate(JObject evt)
        {
            return ValueTask.FromResult(this.predicate(evt));
        }
    }
}