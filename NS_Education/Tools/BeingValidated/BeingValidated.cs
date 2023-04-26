using System;
using System.Threading.Tasks;

namespace NS_Education.Tools.BeingValidated
{
    public class BeingValidated<T> : IBeingValidated<T>
    {
        public bool IsValid() => !_isInvalid;
        
        private readonly T _target;
        private bool _isInvalid;
        private bool _skipIfInvalid;

        public BeingValidated(T target, bool skipIfInvalid = false)
        {
            _target = target;
            _skipIfInvalid = skipIfInvalid;
        }

        public IBeingValidated<T> Validate(Func<T, bool> validation, Action onFail = null, Action<Exception> onException = null)
        {
            if (IsLazyAndInvalid()) return this;

            try
            {
                // 驗證，並且在遇到失敗時執行 onFail。
                if (!validation.Invoke(_target))
                {
                    DoWhenFail(onFail);
                }
            }
            catch (Exception e)
            {
                DoWhenException(onException, e);
            }

            return this;
        }

        private void DoWhenException(Action<Exception> onException, Exception e)
        {
            _isInvalid = true;

            if (onException == null) throw e;
            onException.Invoke(e);
        }

        private void DoWhenFail(Action onFail)
        {
            onFail?.Invoke();
            _isInvalid = true;
        }

        private bool IsLazyAndInvalid() => _skipIfInvalid && _isInvalid;

        public IBeingValidated<T> Validate(Action<T> validation, Action<Exception> onException = null)
        {
            if (IsLazyAndInvalid()) return this;

            try
            {
                validation.Invoke(_target);
            }
            catch (Exception e)
            {
                DoWhenException(onException, e);
            }

            return this;
        }

        public async Task<IBeingValidated<T>> ValidateAsync(Func<T, Task<bool>> validation, Action onFail = null, Action<Exception> onException = null)
        {
            if (IsLazyAndInvalid()) return this;

            try
            {
                // 驗證，並且在遇到失敗時執行 onFail。
                if (!await validation.Invoke(_target))
                {
                    DoWhenFail(onFail);
                }
            }
            catch (Exception e)
            {
                DoWhenException(onException, e);
            }

            return this;
        }

        public async Task<IBeingValidated<T>> ValidateAsync(Func<T, Task> validation, Action<Exception> onException = null)
        {
            if (IsLazyAndInvalid()) return this;

            try
            {
                await validation.Invoke(_target);
            }
            catch (Exception e)
            {
                DoWhenException(onException, e);
            }

            return this;
        }
        
        public IBeingValidated<T> SkipIfAlreadyInvalid(bool setTo = true)
        {
            _skipIfInvalid = setTo;
            return this;
        }
    }
}