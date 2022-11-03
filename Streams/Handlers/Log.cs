namespace Streams.Events.Handlers
{
    public class Log : IHandler
    {
        private readonly Action<Event> action;

        public Log(Action<Event> action)
        {
            this.action = action;
        }

        public Task Handle(Event evt)
        {
            this.action(evt);

            return Task.CompletedTask;
        }
    }
}