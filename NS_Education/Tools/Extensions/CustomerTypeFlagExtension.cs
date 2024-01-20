using System;
using NS_Education.Variables;

namespace NS_Education.Tools.Extensions
{
    public static class CustomerTypeFlagExtension
    {
        public static string GetTypeFlagName(this CustomerType customerType)
        {
            switch (customerType)
            {
                case CustomerType.Internal:
                    return "內部客戶";
                case CustomerType.External:
                    return "外部客戶";
                case CustomerType.CommDept:
                    return "通訊處";
                default:
                    throw new ArgumentOutOfRangeException(nameof(customerType));
            }
        }
    }
}