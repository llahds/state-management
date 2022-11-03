
namespace Streams.Events.Handlers
{
    public interface IHandler
    {
        Task Handle(Event evt);
    }
}