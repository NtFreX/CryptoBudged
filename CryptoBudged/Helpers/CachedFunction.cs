using System;

namespace CryptoBudged.Helpers
{
    public class CachedFunction<T>
    {
        private readonly Func<T> _func;
        private readonly TimeSpan _minAllowedInterval;

        private DateTime? _lastExecution;
        private T _lastResult;

        public CachedFunction(Func<T> func, TimeSpan minAllowedInterval)
        {
            _func = func;
            _minAllowedInterval = minAllowedInterval;
        }

        public T Execute()
        {
            if (_lastExecution.HasValue && _lastExecution >= DateTime.Now - _minAllowedInterval)
            {
                return _lastResult;
            }

            _lastResult = _func();
            _lastExecution = DateTime.Now;

            return _lastResult;
        }
    }
}