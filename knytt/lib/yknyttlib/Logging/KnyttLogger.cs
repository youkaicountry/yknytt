using System;
using System.Collections.Generic;

namespace YKnyttLib.Logging
{
    public static class KnyttLogger
    {
        private static List<IKnyttLoggerTarget> targets;
        private static Level logLevel;

        public enum Level
        {
            TRACE = 0,
            DEBUG = 1,
            INFO = 2,
            WARN = 3,
            ERROR = 4,
            FATAL = 5,
            OFF = 6,
        }

        static KnyttLogger()
        {
            targets = new List<IKnyttLoggerTarget>();
            logLevel = Level.INFO;
        }

        public static void AddTarget(IKnyttLoggerTarget target)
        {
            targets.Add(target);
        }

        public static void SetLogLevel(Level level)
        {
            logLevel = level;
        }

        public static void Log(Level level, string message, KnyttPoint? worldPos=null, KnyttPoint? screenPos=null)
        {
            // Ensure logger is showing messages of this level
            if (level < logLevel) { return; }

            // Construct the message
            var m = new KnyttLogMessage() {Timestamp=DateTime.Now, Level=level, Value=message, WorldPos=worldPos, ScreenPos=screenPos};

            // Pass it along
            foreach (var target in targets)
            {
                target.NewMessage(m);
            }
        }

        // Convenience functions

        public static void Trace(string message, KnyttPoint? worldPos=null, KnyttPoint? screenPos=null)
        {
            Log(Level.TRACE, message, worldPos, screenPos);
        }

        public static void Debug(string message, KnyttPoint? worldPos=null, KnyttPoint? screenPos=null)
        {
            Log(Level.DEBUG, message, worldPos, screenPos);
        }

        public static void Info(string message, KnyttPoint? worldPos=null, KnyttPoint? screenPos=null)
        {
            Log(Level.INFO, message, worldPos, screenPos);
        }

        public static void Warn(string message, KnyttPoint? worldPos=null, KnyttPoint? screenPos=null)
        {
            Log(Level.WARN, message, worldPos, screenPos);
        }

        public static void Error(string message, KnyttPoint? worldPos=null, KnyttPoint? screenPos=null)
        {
            Log(Level.ERROR, message, worldPos, screenPos);
        }

        public static void Fatal(string message, KnyttPoint? worldPos=null, KnyttPoint? screenPos=null)
        {
            Log(Level.FATAL, message, worldPos, screenPos);
        }
    }
}
