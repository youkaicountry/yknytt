using System;

namespace YKnyttLib.Logging
{
    public class ConsoleLoggerTarget : IKnyttLoggerTarget
    {
        public void NewMessage(KnyttLogMessage message)
        {
            System.Console.WriteLine(message.Render());
        }
    }
}
