using System;

namespace YKnyttLib.Logging
{
    public class KnyttLogger : IKnyttLoggerTarget
    {
        public void NewMessage(KnyttLogMessage message)
        {
            Console.WriteLine(message.Render());
        }
    }
}
