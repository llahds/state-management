public interface IHandler
{
    Task Handle(Event evt);
}
