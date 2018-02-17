using System;
using System.Linq;

namespace TzIdentityManager.Core.Extensions
{
    public static class IdentityManagerResultExtensions
    {
        public static ErrorModel ToError(this IdentityManagerResult result)
        {
            if (result == null) throw new ArgumentNullException("result");

            return new ErrorModel
            {
                Errors = result.Errors.ToArray()
            };
        }
    }
}
