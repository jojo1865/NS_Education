using System;

namespace NS_Education.Tools
{
    public static class ChainValidateHelperExtensionMethods
    {
        public static BeingValidated<T> StartValidate<T>(T target)
        {
            return new BeingValidated<T>(target);
        }
    }
}