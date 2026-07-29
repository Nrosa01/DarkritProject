using System;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.DevTools.Logger
{
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
}
