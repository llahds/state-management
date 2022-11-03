using Newtonsoft.Json.Linq;

namespace Streams.Events.Filters
{
    public interface IFilter
    {
        ValueTask<bool> Evaluate(JObject evt);
    }
}