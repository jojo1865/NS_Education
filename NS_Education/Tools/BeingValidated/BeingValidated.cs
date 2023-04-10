using System;

namespace NS_Education.Tools.BeingValidated
{
    public class BeingValidated<T> : IBeingValidated<T>
    {
        private readonly T _target;
        private bool _isInvalid;
        private readonly bool _lazy;

        public BeingValidated(T target, bool lazy = false)
        {
            _target = target;
            _lazy = lazy;
        }

        public IBeingValidated<T> Validate(Func<T, bool> validation, Action onFail = null, Action<Exception> onException = null)
        {
            if (_lazy && _isInvalid)
                return this;

            try
            {
                // 驗證，並且在遇到失敗時執行 onFail。
                if (!validation.Invoke(_target))
                {
                    onFail?.Invoke();
                    _isInvalid = true;
                }
            }
            catch (Exception e)
            {
                _isInvalid = true;

                if (onException == null) throw;
                onException.Invoke(e);
            }

            return this;
        }

        public bool Result()
        {
            return !_isInvalid;
        }
    }
}