using System.Collections.Generic;

namespace Darkrit.DevTools.Logger;

/// <summary>
/// Logger that contains other loggers and forwards messages to them.
/// </summary>
internal class CompositeLogger() : ILogger
{
    private static List<ILogger> Loggers { get; set; } = [];
    public static void AddLogger(ILogger logger) => Loggers.Add(logger);

    public void Trace(string message) => Log(message, LogLevel.Trace);
    public void Debug(string message) => Log(message, LogLevel.Debug);
    public void Info(string message) => Log(message, LogLevel.Info);
    public void Warning(string message) => Log(message, LogLevel.Warning);
    public void Error(string message) => Log(message, LogLevel.Error);
    public void Log(string message, LogLevel logLevel) => Loggers.ForEach(x => x.Log(message, logLevel));
}
