using System;
using System.Threading.Tasks;

namespace NS_Education.Tools.BeingValidated
{
    // 用於驗證物件狀態時的 Wrapper，可以透過 ExtensionMethods 的 StartValidate() 取得。
    public class BeingValidated<T> : IBeingValidated<T, T>
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

        public IBeingValidated<T, T> Validate(Func<T, bool> validation, Action<T> onFail = null, Action<T, Exception> onException = null)
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

        private void DoWhenException(Action<T, Exception> onException, Exception e)
        {
            _isInvalid = true;

            if (onException == null) throw e;
            onException.Invoke(_target, e);
        }

        private void DoWhenFail(Action<T> onFail)
        {
            onFail?.Invoke(_target);
            _isInvalid = true;
        }

        private bool IsLazyAndInvalid() => _skipIfInvalid && _isInvalid;

        public IBeingValidated<T, T> Validate(Action<T> validation, Action<T, Exception> onException = null)
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

        public async Task<IBeingValidated<T, T>> ValidateAsync(Func<T, Task<bool>> validation, Action<T> onFail = null, Action<T, Exception> onException = null)
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

        public async Task<IBeingValidated<T, T>> ValidateAsync(Func<T, Task> validation, Action<T, Exception> onException = null)
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

        public IBeingValidated<T, T> SkipIfAlreadyInvalid(bool setTo = true)
        {
            _skipIfInvalid = setTo;
            return this;
        }
    }
}