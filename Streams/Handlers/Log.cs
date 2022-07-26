public class Log : IHandler
{
    public Task Handle(Event evt)
    {
        Console.WriteLine(evt.Payload.ToString());

        return Task.CompletedTask;
    }
}