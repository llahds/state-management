
using Newtonsoft.Json.Linq;

namespace Streams.Events.Handlers
{
    public class SelectHandler : IHandler
    {
        private readonly Func<JObject, JObject> transform;
        private readonly EventStream selectStream;

        public SelectHandler(
            Func<JObject, JObject> transform,
            EventStream selectStream)
        {
            this.transform = transform;
            this.selectStream = selectStream;
        }

        public Task Handle(Event evt)
        {
            return this.selectStream.Emit(this.transform(evt.Payload));
        }
    }
}