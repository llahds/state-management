using Newtonsoft.Json.Linq;

public static class StreamExtensions
{
    public static async Task<EventStream> GroupBy(this EventStream stream, Func<JObject, object> keyExtractor, IFilter filter)
    {
        var groupByStream = new EventStream(new EmptyWindow(), filter);

        await stream.Subscribe(new GroupByHandler(keyExtractor, stream.EventStore, groupByStream));

        return groupByStream;
    }
}
