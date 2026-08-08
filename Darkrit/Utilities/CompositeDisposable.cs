// Darkrit - Copyright (C) Nicolás Rosa (@nrosa01)
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Darkrit.Utilities;

/// <summary>
/// Container class that holds a list of diposables. When this class is disposes, all if elements are disposed too.
/// </summary>
public class CompositeDisposable : IDisposable
{
    readonly List<IDisposable> _disposables = [];

    public void Dispose()
    {
        int iterations = _disposables.Count;
        for (int i = 0; i < iterations; i++)
            _disposables[i].Dispose();

        _disposables.Clear();
        GC.SuppressFinalize(this);
    }

    public void Add(IDisposable disposable) => _disposables.Add(disposable);
}

public static class DisposableExtensions
{
    /// <summary>
    /// Adds a new disposable to <see cref="CompositeDisposable"/>
    /// </summary>
    /// <param name="disposable">The disposable to add to the <see cref="CompositeDisposable"/></param>
    /// <param name="compositeDisposable">The composite disposable that holds the disposables</param>
    public static void AddTo(this IDisposable disposable, CompositeDisposable compositeDisposable) => compositeDisposable.Add(disposable);
}
