namespace elbgb_core
{
    public interface IVideoFrameSink
    {
        void AppendFrame(byte[] frame);
    }
}
