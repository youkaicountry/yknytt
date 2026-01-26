using System;

namespace YKnyttLib.Logging
{
    public partial class ConsoleLoggerTarget : IKnyttLoggerTarget
    {
        public void NewMessage(KnyttLogMessage message)
        {
            System.Console.WriteLine(message.Render());
        }
    }
}
