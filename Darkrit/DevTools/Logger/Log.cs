// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Darkrit.DevTools.Logger;

/// <summary>
/// Simple log class used by the Framework as main logger
/// </summary>
public class Log
{
    private static CompositeLogger Backend { get; set; } =  new CompositeLogger();

    public static void AddLogger(ILogger logger) => CompositeLogger.AddLogger(logger);

    public static void Trace(string message) => Backend.Trace(message);
    public static void Debug(string message) => Backend.Debug(message);
    public static void Info(string message) => Backend.Info(message);
    public static void Warning(string message) => Backend.Warning(message);
    public static void Error(string message) => Backend.Error(message);
}
