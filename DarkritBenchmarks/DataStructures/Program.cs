using BenchmarkDotNet.Running;
namespace DarkritBenchmarks.DataStructures;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        //BenchmarkRunner.Run<GrowableArrayIterationBenchmarks>();
        //BenchmarkRunner.Run<HandleMapGrowingIterationBenchmarks>();
    }
}