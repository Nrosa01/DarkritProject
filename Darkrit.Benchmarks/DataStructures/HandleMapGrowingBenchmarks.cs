using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Darkrit.Base;
using Darkrit.DataStructures;
using DarkritBenchmarks.Common;

namespace DarkritBenchmarks.DataStructures;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.NativeAot10_0)]
public class HandleMapGrowingIterationBenchmarks
{
    private const int Size = 10_000_000;

    private HandleMapGrowing<LargeStruct> _map = null!;

    [GlobalSetup]
    public void Setup()
    {
        _map = new HandleMapGrowing<LargeStruct>(Size);

        for (int i = 0; i < Size; i++)
        {
            _map.Add(new LargeStruct
            {
                A = i,
                B = i + 1,
                C = i + 2,
                D = i + 3,
                E = i + 4,
                F = i + 5,
                G = i + 6,
                H = i + 7
            });
        }
    }

    [Benchmark]
    public long ForeachRef()
    {
        long sum = 0;

        foreach (ref var handle in _map)
            sum += Calculate(handle.Item);

        return sum;
    }

    [Benchmark]
    public long ForeachValue()
    {
        long sum = 0;

        foreach (var handle in _map)
            sum += Calculate(handle.Item);

        return sum;
    }

    [Benchmark]
    public long For()
    {
        long sum = 0;
        var items = _map.Items;

        for (int i = 1; i < items.Length; i++)
        {
            ref readonly var entry = ref items[i];

            if (entry.Handle.Id == 0)
                continue;

            sum += Calculate(entry.Item);
        }

        return sum;
    }

    [Benchmark]
    public long ForeachItemValueBoxing()
    {
        long sum = 0;

        foreach (LargeStruct item in (IEnumerable<LargeStruct>)_map)
            sum += Calculate(item);

        return sum;
    }

    [Benchmark]
    public long GetByHandle()
    {
        long sum = 0;

        var items = _map.Items;

        for (int i = 1; i < items.Length; i++)
        {
            ref readonly var entry = ref items[i];

            if (entry.Handle.Id == 0)
                continue;

            sum += Calculate(_map.Get(entry.Handle));
        }

        return sum;
    }

    private static long Calculate(in LargeStruct item) => item.H - item.G - item.F + item.E +item.D - item.C + item.B + item.A;
}