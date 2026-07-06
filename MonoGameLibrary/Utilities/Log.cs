using System;
using System.Diagnostics;
namespace MonoGameLibrary.Utilities
{
    public static class Log
    {
        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
            Debug.WriteLine(message);
        }
    }
}
