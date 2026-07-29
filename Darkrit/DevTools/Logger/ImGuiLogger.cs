using System;
using System.Collections.Generic;
using System.Text;
using Darkrit.DataStructures;

namespace Darkrit.DevTools.Logger
{
    // Fromat is like this [00:03:42] INFO: Cleaned up old log: user://rvr_logs/2026-07-21T22.57.30.log
    // [hh:mm:ss] LogLevel: msg
    // Messages are colored based on the log level
    internal class LogEntry
    {
        public string message;
        public LogLevel logLevel;
        public DateTime date;
    }

    internal class ImGuiLogger : ILogger
    {
        readonly RingBuffer<LogEntry> buffer = new (2048);

        void ILogger.Log(string message, LogLevel logLevel)
        {
            throw new NotImplementedException();
        }
    }
}
