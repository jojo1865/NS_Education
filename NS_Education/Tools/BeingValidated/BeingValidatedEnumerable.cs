using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NS_Education.Tools.BeingValidated
{
    public class BeingValidatedEnumerable<TElement, TEnumerable> : IBeingValidated<TElement, TEnumerable>
        where TEnumerable : IEnumerable<TElement>
    {
        private readonly IBeingValidated<TEnumerable, TEnumerable> _inner;
        private readonly TEnumerable _targetEnumerable;

        public BeingValidatedEnumerable(TEnumerable target, bool skipIfAlreadyInvalid = false)
        {
            _targetEnumerable = target;
            _inner = target.StartValidate(skipIfAlreadyInvalid);
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
            foreach (TElement element in _targetEnumerable)
            {
                _inner.Validate(_ => validation.Invoke(element),
                    _ => onFail?.Invoke(element),
                    (_, e) => onException?.Invoke(element, e));
            }

            return this;
        }

        /// <summary>
        /// 驗證集合中的元素。
        /// </summary>
        /// <param name="validation">欲執行的 void 驗證方法</param>
        /// <param name="onException">（可選）驗證過程拋錯時應執行的行為。忽略時，向外拋出</param>
        /// <returns>此物件本身</returns>
        public IBeingValidated<TElement, TEnumerable> Validate(Action<TElement> validation,
            Action<TElement, Exception> onException = null)
        {
            foreach (TElement element in _targetEnumerable)
            {
                _inner.Validate(_ => validation.Invoke(element),
                    (_, e) => onException?.Invoke(element, e));
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
            foreach (TElement element in _targetEnumerable)
            {
                await _inner.ValidateAsync(async _ => await validation.Invoke(element),
                    _ => onFail?.Invoke(element),
                    (_, e) => onException?.Invoke(element, e));
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
        public async Task<IBeingValidated<TElement, TEnumerable>> ValidateAsync(Func<TElement, Task> validation,
            Action<TElement, Exception> onException)
        {
            foreach (TElement element in _targetEnumerable)
            {
                await _inner.ValidateAsync(async _ => await validation.Invoke(element),
                    (_, e) => onException?.Invoke(element, e));
            }

            return this;
        }

        public bool IsValid()
        {
            return _inner.IsValid();
        }

        public IBeingValidated<TElement, TEnumerable> SkipIfAlreadyInvalid(bool setTo = true)
        {
            _inner.SkipIfAlreadyInvalid(setTo);
            return this;
        }

        /// <inheritdoc />
        /// <remarks>這將檢查所有元素，只要有任一者滿足條件，就跳過後續驗證。</remarks>
        public IBeingValidated<TElement, TEnumerable> SkipIf(Predicate<TElement> predicate)
        {
            _inner.SkipIf(_ => _targetEnumerable.Any(predicate.Invoke));
            return this;
        }

        /// <inheritdoc />
        public IBeingValidated<TElement, TEnumerable> StopSkipping()
        {
            SkipIfAlreadyInvalid(false);
            return this;
        }
    }
}