public interface IUIEventHandler
{
    bool Handle(UIEventType type, object payload);
}
