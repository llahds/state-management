using Newtonsoft.Json.Linq;

namespace Streams.Events.Filters
{
    public class MatchAllFilter : IFilter
    {
        public ValueTask<bool> Evaluate(JObject evt)
        {
            return ValueTask.FromResult(true);
        }
    }
}