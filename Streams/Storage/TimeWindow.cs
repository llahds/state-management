using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace Streams.Events.Storage
{
    public class TimeWindow : IEventStore
    {
        private readonly TimeSpan validFor;
        private long count = 0;
        private readonly ConcurrentDictionary<long, Event> window = new ConcurrentDictionary<long, Event>();

        public TimeWindow(
            TimeSpan validFor
            )
        {
            this.validFor = validFor;
        }

        public Task Evict()
        {
            var expired = this.window.Where(K => K.Value.ExpiresOn < DateTime.UtcNow).ToArray();

            foreach (var ex in expired)
            {
                this.window.TryRemove(ex);
            }

            return Task.CompletedTask;
        }

        public Task<Event> Add(JObject evt)
        {
            var count = Interlocked.Increment(ref this.count);

            var @event = new Event { Id = count, Payload = evt, ExpiresOn = DateTime.UtcNow.Add(this.validFor) };

            this.window.TryAdd(count, @event);

            return Task.FromResult(@event);
        }

        public Task<JObject[]> Get(long[] eventIds)
        {
            var e = new List<JObject>();

            foreach (var eventId in eventIds)
            {
                Event evt = null;

                if (this.window.TryGetValue(eventId, out evt) && evt.ExpiresOn > DateTime.UtcNow)
                {
                    e.Add(evt.Payload);
                }
            }

            return Task.FromResult(e.ToArray());
        }

        public Task<bool> IsValid(long id)
        {
            var isValid = false;

            Event evt = null;

            if (this.window.TryGetValue(id, out evt) && evt.ExpiresOn > DateTime.UtcNow)
            {
                isValid = true;
            }

            return Task.FromResult(isValid);
        }
    }
}