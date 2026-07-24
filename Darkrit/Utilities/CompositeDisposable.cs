using System;
using System.Collections.Generic;
using System.Text;

namespace Darkrit.Utilities
{
    /// <summary> Clase contenedora de lista de disposables. Cuando esta clase se disposa, disposa todos sus elementos.</summary>
    public class CompositeDisposable : IDisposable
    {
        List<IDisposable> disposables = [];

        public void Dispose()
        {
            int iterations = disposables.Count;
            for (int i = 0; i < iterations; i++)
            {
                disposables[i].Dispose();
            }

            disposables.Clear();
        }

        public void Add(IDisposable disposable) => disposables.Add(disposable);
    }

    public static class DisposableExtensions
    {
        ///<summary> Añade un disposable al <see cref="CompositeDisposable"/>. </summary>
        public static void AddTo(this IDisposable disposable, CompositeDisposable compositeDisposable) => compositeDisposable.Add(disposable);
    }
}
