using System;
using System.Threading.Tasks;

namespace NsEduCore_Tools.BeingValidated
{
    public class BeingValidated<T> : IBeingValidated<T>
    {
        private readonly T _target;
        private bool _isInvalid;
        private readonly bool _skipIfInvalid;

        public BeingValidated(T target, bool skipIfInvalid = false)
        {
            _target = target;
            _skipIfInvalid = skipIfInvalid;
        }

        public IBeingValidated<T> Validate(Func<T, bool> validation, Action onFail = null, Action<Exception> onException = null)
        {
            if (_skipIfInvalid && _isInvalid)
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

        public async Task<IBeingValidated<T>> ValidateAsync(Func<T, Task<bool>> validation, Action onFail = null, Action<Exception> onException = null)
        {
            if (_skipIfInvalid && _isInvalid)
                return this;

            try
            {
                // 驗證，並且在遇到失敗時執行 onFail。
                if (!await validation.Invoke(_target))
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

        public bool IsValid()
        {
            return !_isInvalid;
        }
    }
}