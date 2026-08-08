// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Darkrit.DataStructures;
using Microsoft.Xna.Framework;

namespace Darkrit.DevTools.Logger;

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

/// <summary>
/// Logger that internally uses a ring buffer. Can optionally use RLE to group similar logs
/// </summary>
internal class CompactLogger : ILogger
{
    private readonly RingBuffer<LogEntry> _buffer = new (2048);

    internal static Color GetColorByLevel(LogLevel level) => level switch
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

        // Groups similar logs. But the only downside is that
        // this way the full logs that are grouped are lost
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
