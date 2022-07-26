using Newtonsoft.Json.Linq;

public class GroupByHandler : IHandler
{
    private readonly Func<JObject, object> keyExtractor;
    private readonly IEventStore store;
    private readonly EventStream stream;
    private readonly Dictionary<object, HashSet<long>> groups;
    private readonly AsyncLockPool locks;

    public GroupByHandler(
        Func<JObject, object> key,
        IEventStore store,
        EventStream stream)
    {
        this.keyExtractor = key;
        this.store = store;
        this.stream = stream;
        this.groups = new Dictionary<object, HashSet<long>>();

        this.locks = new AsyncLockPool(128);
    }

    public async Task Handle(Event evt)
    {
        var key = this.keyExtractor(evt.Payload);

        await this.locks.Wait(key);

        try
        {
            if (this.groups.ContainsKey(key) == false)
            {
                this.groups.Add(key, new HashSet<long>());
            }

            var group = this.groups[key];

            // evict expired events
            var ids = group.ToArray();
            var checkIds = await Task.WhenAll(ids.Select(T => this.store.IsValid(T)));

            for (var i = 0; i < ids.Length; i++)
            {
                if (checkIds[i] == false)
                {
                    group.Remove(ids[i]);
                }
            }

            group.Add(evt.Id);

            await this.stream.Emit(JObject.FromObject(new { key = key, events = await this.store.Get(group.ToArray()) }));
        }
        finally
        {
            await this.locks.Release(key);
        }
    }
}
