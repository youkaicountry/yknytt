using System;

namespace YKnyttLib.Logging
{
    public class ConsoleLoggerTarget : IKnyttLoggerTarget
    {
        public void NewMessage(KnyttLogMessage message)
        {
            Console.WriteLine(message.Render());
        }
    }
}
