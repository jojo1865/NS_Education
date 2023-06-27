using System;
using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors.DataValidationErrors
{
    public sealed class UpdateDbFailedError : BaseDataValidationError
    {
        public UpdateDbFailedError(Exception exception)
        {
            Exception = exception.GetActualMessage();
        }

        public string Exception { get; }

        public override int ErrorCodeInt => 2;
        public override string ErrorMessage => "更新 DB 時發生異常！";
    }
}