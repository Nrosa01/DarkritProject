using System;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.DevTools.Logger
{
    internal class CompositeLogger() : ILogger
    {
        private static List<ILogger> Loggers { get; set; } = [];
        public static void AddLogger(ILogger logger) => Loggers.Add(logger);

        void ILogger.Log(string message, LogLevel logLevel) => Loggers.ForEach(x => x.Log(message, logLevel));
    }
}
