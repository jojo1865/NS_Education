using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NS_Education.Tools.BeingValidated
{
    public class BeingValidatedEnumerable<TElement, TEnumerable> : IBeingValidated<TElement, TEnumerable>
        where TEnumerable : IEnumerable<TElement>
    {
        private readonly TEnumerable _collection;
        private bool _isInvalid;
        private bool _skipIfAlreadyInvalid;
        private bool IsLazyAndInvalid => _skipIfAlreadyInvalid && _isInvalid;
        
        public BeingValidatedEnumerable(TEnumerable target, bool skipIfAlreadyInvalid = false)
        {
            _collection = target;
            _skipIfAlreadyInvalid = skipIfAlreadyInvalid;
        }
        
        /// <summary>
        /// 驗證集合中的元素。
        /// </summary>
        /// <param name="validation">欲執行的驗證方法，應接收此集合元素的類型，並回傳 bool 表示驗證結果</param>
        /// <param name="onFail">（可選）驗證回傳 false 時應執行的行為。忽略時，只記錄驗證結果</param>
        /// <param name="onException">（可選）驗證過程拋錯時應執行的行為。忽略時，向外拋出</param>
        /// <returns>此物件本身</returns>
        public IBeingValidated<TElement, TEnumerable> Validate(Func<TElement, bool> validation,
            Action<TElement> onFail = null, Action<TElement, Exception> onException = null)
        {
            foreach (TElement element in _collection)
            {
                try
                {
                    if (IsLazyAndInvalid) return this;
                    if (validation.Invoke(element)) continue;
                    
                    DoWhenFailWithElement(onFail, element);
                }
                catch (Exception e)
                {
                    DoWhenExceptionWithElement(onException, e, element);
                }
            }

            return this;
        }

        private void DoWhenFailWithElement(Action<TElement> onFail, TElement element)
        {
            _isInvalid = true;
            onFail?.Invoke(element);
        }
        
        private void DoWhenExceptionWithElement(Action<TElement, Exception> onException, Exception e, TElement element)
        {
            _isInvalid = true;
            if (onException is null)
                throw e;
            
            onException.Invoke(element, e);
        }

        /// <summary>
        /// 驗證集合中的元素。
        /// </summary>
        /// <param name="validation">欲執行的 void 驗證方法</param>
        /// <param name="onException">（可選）驗證過程拋錯時應執行的行為。忽略時，向外拋出</param>
        /// <returns>此物件本身</returns>
        public IBeingValidated<TElement, TEnumerable> Validate(Action<TElement> validation, Action<TElement, Exception> onException = null)
        {
            foreach (TElement element in _collection)
            {
                try
                {
                    if (IsLazyAndInvalid) return this;
                    validation.Invoke(element);
                }
                catch (Exception e)
                {
                    DoWhenExceptionWithElement(onException, e, element);
                }
            }

            return this;
        }

        /// <summary>
        /// 異步驗證集合中的元素。
        /// </summary>
        /// <param name="validation">欲執行的 void 驗證方法</param>
        /// <param name="onFail">（可選）驗證回傳 false 時應執行的行為。忽略時，只記錄驗證結果</param>
        /// <param name="onException">（可選）驗證過程拋錯時應執行的行為。忽略時，向外拋出</param>
        /// <returns>此物件本身</returns>
        /// <remarks><b>此處的異步實作並不是所有元素同時執行，仍然會照順序執行。異步的 scope 在於整組驗證，而非單個元素。</b></remarks>
        public async Task<IBeingValidated<TElement, TEnumerable>> ValidateAsync(Func<TElement, Task<bool>> validation,
            Action<TElement> onFail = null, Action<TElement, Exception> onException = null)
        {
            foreach (TElement element in _collection)
            {
                try
                {
                    if (IsLazyAndInvalid) return this;
                    if (await validation.Invoke(element)) continue;

                    DoWhenFailWithElement(onFail, element);
                }
                catch (Exception e)
                {
                    DoWhenExceptionWithElement(onException, e, element);
                }
            }

            return this;
        }
        
        /// <summary>
        /// 異步驗證集合中的元素。
        /// </summary>
        /// <param name="validation">欲執行的 void 驗證方法</param>
        /// <param name="onException">（可選）驗證過程拋錯時應執行的行為。忽略時，向外拋出</param>
        /// <returns>此物件本身</returns>
        /// <remarks><b>此處的異步實作並不是所有元素同時執行，仍然會照順序執行。異步的 scope 在於整組驗證，而非單個元素。</b></remarks>
        public async Task<IBeingValidated<TElement, TEnumerable>> ValidateAsync(Func<TElement, Task> validation, Action<TElement, Exception> onException)
        {
            foreach (TElement element in _collection)
            {
                try
                {
                    if (IsLazyAndInvalid) return this;
                    await validation.Invoke(element);
                }
                catch (Exception e)
                {
                    DoWhenExceptionWithElement(onException, e, element);
                }
            }

            return this;
        }

        public bool IsValid()
        {
            return !_isInvalid;
        }

        public IBeingValidated<TElement, TEnumerable> SkipIfAlreadyInvalid(bool setTo = true)
        {
            _skipIfAlreadyInvalid = setTo;
            return this;
        }
    }
}