#region Using derectives

using System.Threading.Tasks;
using System.Windows.Input;

#endregion

namespace SysTest.Win.AsyncConmands
{
    internal interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync();

        bool CanExecute();
    }
}