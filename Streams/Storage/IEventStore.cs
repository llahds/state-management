using Newtonsoft.Json.Linq;

public interface IEventStore
{
    Task<Event> Add(JObject evt);
    Task<bool> IsValid(long id);
    Task<JObject[]> Get(long[] eventIds);
}
