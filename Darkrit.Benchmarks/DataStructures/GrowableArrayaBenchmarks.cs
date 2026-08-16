using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DarkritBenchmarks.Common;

namespace DarkritBenchmarks.DataStructures;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.NativeAot10_0)]
public class GrowableArrayIterationBenchmarks
{
    private const int Size = 10_000_000;

    private Darkrit.DataStructures.GrowableArray<LargeStruct> _array = null!;

    [GlobalSetup]
    public void Setup()
    {
        _array = new Darkrit.DataStructures.GrowableArray<LargeStruct>(Size);

        for (int i = 0; i < Size; i++)
            _array.Add(new LargeStruct
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

    [Benchmark]
    public long ForeachRef()
    {
        long sum = 0;

        foreach (ref var item in _array)
            sum += item.H - item.G - item.F + item.E + item.D - item.C + item.B + item.A;

        return sum;
    }

    [Benchmark]
    public long ForeachValue()
    {
        long sum = 0;

        foreach (var item in _array)
            sum += item.H - item.G - item.F + item.E + item.D - item.C + item.B + item.A;

        return sum;
    }

    [Benchmark]
    public long For()
    {
        long sum = 0;

        for (int i = 0; i < _array.Count; i++)
            sum += _array[i].H - _array[i].G - _array[i].F + _array[i].E + _array[i].D - _array[i].C + _array[i].B + _array[i].A;

        return sum;
    }
}
