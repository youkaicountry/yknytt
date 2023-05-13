using System;

namespace YKnyttLib.Logging
{
    public struct KnyttLogMessage
    {
        private const string dateFormat = "yyyy-M-dd HH:mm:ss.fff";

        public KnyttLogger.Level Level;
        public string Value;
        public KnyttPoint? WorldPos;
        public KnyttPoint? ScreenPos;
        public DateTime Timestamp;

        public string Render()
        {
            return $"{Timestamp.ToString(dateFormat)} <{logLevelToString(Level)}> {Value}";
        }

        public string RenderBBCode()
        {
            return $"[color=#cccccc]{Timestamp.ToString(dateFormat)}[/color] <[color=#{logLevelToBBCodeColor(Level)}]{logLevelToString(Level)}[/color]> {Value}";
        }

        private string logLevelToString(KnyttLogger.Level level)
        {
            switch(level)
            {
                case KnyttLogger.Level.TRACE:
                    return "trace";

                case KnyttLogger.Level.DEBUG:
                    return "debug";

                case KnyttLogger.Level.INFO:
                    return "info";

                case KnyttLogger.Level.WARN:
                    return "warn";

                case KnyttLogger.Level.ERROR:
                    return "error";

                case KnyttLogger.Level.FATAL:
                    return "fatal";
            }

            return "";
        }

        private string logLevelToBBCodeColor(KnyttLogger.Level level)
        {
            switch(level)
            {
                case KnyttLogger.Level.TRACE:
                    return "08ff00";

                case KnyttLogger.Level.DEBUG:
                    return "06c800";

                case KnyttLogger.Level.INFO:
                    return "048400";

                case KnyttLogger.Level.WARN:
                    return "d4ff00";

                case KnyttLogger.Level.ERROR:
                    return "ff0000";

                case KnyttLogger.Level.FATAL:
                    return "a00000";
            }

            return "";
        }
    }
}
