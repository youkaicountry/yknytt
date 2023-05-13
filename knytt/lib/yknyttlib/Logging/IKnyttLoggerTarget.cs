namespace YKnyttLib.Logging
{
    public interface IKnyttLoggerTarget
    {
        void NewMessage(KnyttLogMessage message);
    }
}
