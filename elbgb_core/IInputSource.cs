namespace elbgb_core
{
    public interface IInputSource
    {
        GBCoreInput PollInput();
    }
}
