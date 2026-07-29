using System;
using System.Collections.Generic;
using System.Text;
using Darkrit.DataStructures;
using Microsoft.Xna.Framework;

namespace Darkrit.DevTools.Logger
{
    // Fromat is like this [00:03:42] INFO: Cleaned up old log: user://rvr_logs/2026-07-21T22.57.30.log
    // [hh:mm:ss] LogLevel: msg
    // Messages are colored based on the log level
    internal struct LogEntry
    {
        public string message;
        public LogLevel logLevel;
        public DateTime firstDate;
        public DateTime lastDate;
        public int repeatCount;
        public Color color;
    }

    internal class ImGuiLogger : ILogger
    {
        readonly RingBuffer<LogEntry> _buffer = new (2048);

        internal Color GetColorByLevel(LogLevel level) => level switch
        {
            LogLevel.Trace => Color.Beige,
            LogLevel.Debug => Color.Azure,
            LogLevel.Info => Color.LightGreen,
            LogLevel.Warning => Color.Yellow,
            LogLevel.Error => Color.Red,
            _ => throw new NotImplementedException()
        };

        public RingBuffer<LogEntry> Buffer => _buffer;

        public bool CollapseRepeated { get; set; }

        void ILogger.Log(string message, LogLevel logLevel)
        {
            var now = DateTime.Now;

            if (_buffer.Size > 0 && CollapseRepeated)
            {
                ref var last = ref _buffer.Back();

                if (last.message == message && last.logLevel == logLevel)
                {
                    last.repeatCount++;
                    last.lastDate = now;
                    return;
                }
            }

            _buffer.PushBack(new LogEntry
            {
                message = message,
                logLevel = logLevel,
                color = GetColorByLevel(logLevel),
                firstDate = now,
                lastDate = now,
                repeatCount = 1
            });
        }
    }
}
