using Newtonsoft.Json.Linq;

public class EmptyWindow : IEventStore
{
    public Task<Event> Add(JObject evt)
    {
        return Task.FromResult(new Event { Id = 1, Payload = evt });
    }

    public Task<JObject[]> Get(long[] eventIds)
    {
        return Task.FromResult(new JObject[0]);
    }

    public Task<bool> IsValid(long id)
    {
        return Task.FromResult(false);
    }
}
