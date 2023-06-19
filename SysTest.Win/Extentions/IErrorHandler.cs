#region Using derectives

using System;

#endregion

namespace SysTest.Win.Extentions
{
    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }
}