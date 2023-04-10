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

        public BeingValidated<T> Validate(Func<T, bool> validation)
        {
            if (_lazy && _isInvalid)
                return this;
            
            if (!validation.Invoke(_target))
                _isInvalid = true;

            return this;
        }

        public bool Result()
        {
            return _isInvalid;
        }
    }
}