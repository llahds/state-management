using Newtonsoft.Json.Linq;
using Streams.Events.Filters;
using Streams.Events.Handlers;
using Streams.Events.Storage;

namespace Streams.Events
{

    public static class StreamExtensions
    {
        public static async Task<EventStream> GroupBy(this EventStream stream, Func<JObject, object> keyExtractor, IFilter filter)
        {
            var groupByStream = new EventStream(new EmptyWindow(), filter);

            await stream.Subscribe(new GroupByHandler(keyExtractor, stream.EventStore, groupByStream));

            return groupByStream;
        }

        public static async Task<EventStream> Select(this EventStream stream, Func<JObject, JObject> transform)
        {
            var selectStream = new EventStream(new EmptyWindow(), new MatchAllFilter());

            await stream.Subscribe(new SelectHandler(transform, selectStream));

            return selectStream;
        }
    }
}