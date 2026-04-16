using System;
using UniRx;

namespace Common.ReactiveUtils
{
    public class ReactiveEvent : IObservable<Unit>
    {
        private event Action<Unit> Action;
        
        public void Invoke()
        {
            Action?.Invoke(default);
        }

        public IObservable<Unit> AsObservable() => Observable.FromEvent<Unit>(
            addHandler => Action += addHandler,
            removeHandler => Action -= removeHandler);

        public IDisposable Subscribe(IObserver<Unit> observer)
        {
            return AsObservable().Subscribe(observer);
        }
    }
    
    /// <typeparam name="T"></typeparam>
    public class ReactiveEvent<T> : IObservable<T>
    {
        private event Action<T> Action;

        public void Invoke(T value)
        {
            Action?.Invoke(value);
        }

        public IObservable<T> AsObservable() => Observable.FromEvent<T>(
            addHandler => Action += addHandler,
            removeHandler => Action -= removeHandler);

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return AsObservable().Subscribe(observer);
        }
    }
}