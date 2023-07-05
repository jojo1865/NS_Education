using System;
using System.Data.Entity.Validation;
using System.Linq;
using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors.DataValidationErrors
{
    public sealed class UpdateDbFailedError : BaseDataValidationError
    {
        public UpdateDbFailedError(Exception exception)
        {
            if (exception is DbEntityValidationException efException)
            {
                Exception = String.Join(",\n",
                    efException.EntityValidationErrors.SelectMany(r => r.ValidationErrors).Select(e => e.ErrorMessage));
            }
            else
            {
                Exception = exception.GetActualMessage();
            }
        }

        public string Exception { get; }

        public override int ErrorCodeInt => 2;
        public override string ErrorMessage => "更新 DB 時發生異常！";
    }
}