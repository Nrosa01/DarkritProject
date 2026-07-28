using System;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.DevTools.Logger
{
    public enum LogLevel {  Trace, Debug, Info, Warning, Error } 

    public interface ILogger
    {
        internal void Log(string message, LogLevel logLevel);
        public void Trace(string message) => Log(message, LogLevel.Trace);
        public void Debug(string message) => Log(message, LogLevel.Debug);
        public void Info(string message) => Log(message, LogLevel.Info);
        public void Warning(string message) => Log(message, LogLevel.Warning);
        public void Error(string message) => Log(message, LogLevel.Error);
    }
}
