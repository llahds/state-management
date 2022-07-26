using Newtonsoft.Json.Linq;

public class EventStream
{
    private readonly IEventStore store;
    private readonly IFilter filter;
    private readonly List<IHandler> subscribers;

    public IEventStore EventStore => this.store;

    public EventStream(
        IEventStore store,
        IFilter filter)
    {
        this.store = store;
        this.filter = filter;

        this.subscribers = new List<IHandler>();
    }

    public async Task Emit(JObject evt)
    {
        var e = await this.store.Add(evt);

        if (await this.filter.Evaluate(evt))
        {
            await Task.WhenAll(this.subscribers.Select(F => F.Handle(e)));
        }
    }

    public Task Subscribe(IHandler handler)
    {
        this.subscribers.Add(handler);

        return Task.CompletedTask;
    }
}
