using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;

namespace Streams.Events.Handlers
{
    public class Join
    {
        private readonly EventStream mergeStream;
        private readonly Func<JObject, object> leftKeyExtractor;
        private readonly Func<JObject, object> rightKeyExtractor;
        private readonly Func<JObject, JObject, JObject> transform;
        private readonly TimeSpan validFor;
        private readonly JoinListener leftListener;
        private readonly JoinListener rightListener;

        private readonly MemoryCache leftCache;
        private readonly MemoryCache rightCache;

        public Join(
            EventStream mergeStream,
            Func<JObject, object> leftKeyExtractor,
            Func<JObject, object> rightKeyExtractor,
            Func<JObject, JObject, JObject> transform,
            EventStream left,
            EventStream right,
            TimeSpan validFor)
        {
            this.mergeStream = mergeStream;
            this.leftKeyExtractor = leftKeyExtractor;
            this.rightKeyExtractor = rightKeyExtractor;
            this.transform = transform;
            this.validFor = validFor;
            this.leftListener = new JoinListener(this.LeftHandler, left);
            this.rightListener = new JoinListener(this.RightHandler, right);

            this.leftCache = new MemoryCache(new MemoryCacheOptions());
            this.rightCache = new MemoryCache(new MemoryCacheOptions());
        }

        protected async Task LeftHandler(Event evt)
        {
            var key = this.leftKeyExtractor(evt.Payload);

            await this.leftCache.GetOrCreateAsync(
                key,
                cacheEntry =>
                {
                    cacheEntry.AbsoluteExpiration = DateTimeOffset.UtcNow.Add(this.validFor);

                    return Task.FromResult(new JoinCacheEntry
                    {
                        Payload = evt.Payload,
                        ExpiresOn = DateTime.UtcNow.Add(this.validFor)
                    });
                });

            JoinCacheEntry joinValue = null;

            if (this.rightCache.TryGetValue(key, out joinValue) && joinValue?.ExpiresOn > DateTime.UtcNow)
            {
                var payload = this.transform(evt.Payload, joinValue.Payload);

                await this.mergeStream.Emit(payload);
            }
        }

        protected async Task RightHandler(Event evt)
        {
            var key = this.rightKeyExtractor(evt.Payload);

            await this.rightCache.GetOrCreateAsync(
                key,
                cacheEntry =>
                {
                    cacheEntry.AbsoluteExpiration = DateTimeOffset.UtcNow.Add(this.validFor);

                    return Task.FromResult(new JoinCacheEntry
                    {
                        Payload = evt.Payload,
                        ExpiresOn = DateTime.UtcNow.Add(this.validFor)
                    });
                });

            JoinCacheEntry joinValue = null;

            if (this.leftCache.TryGetValue(key, out joinValue) && joinValue?.ExpiresOn > DateTime.UtcNow)
            {
                var payload = this.transform(evt.Payload, joinValue.Payload);

                await this.mergeStream.Emit(payload);
            }
        }

        internal class JoinListener : IHandler
        {
            private readonly Func<Event, Task> action;

            public JoinListener(
                Func<Event, Task> action,
                EventStream eventStream)
            {
                this.action = action;
                eventStream.Subscribe(this);
            }

            public async Task Handle(Event evt)
            {
                await this.action(evt);
            }
        }

        internal class JoinCacheEntry
        {
            public DateTime ExpiresOn { get; set; }
            public JObject Payload { get; set; }
        }
    }
}
