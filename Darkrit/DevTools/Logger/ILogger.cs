// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Darkrit.DevTools.Logger;

public enum LogLevel {  Trace, Debug, Info, Warning, Error } 

/// <summary>
/// Simple logger interface
/// </summary>
public interface ILogger
{
    void Log(string message, LogLevel logLevel);
    void Trace(string message) => Log(message, LogLevel.Trace);
    void Debug(string message) => Log(message, LogLevel.Debug);
    void Info(string message) => Log(message, LogLevel.Info);
    void Warning(string message) => Log(message, LogLevel.Warning);
    void Error(string message) => Log(message, LogLevel.Error);
}
