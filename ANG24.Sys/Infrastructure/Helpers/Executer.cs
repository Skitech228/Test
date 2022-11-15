using Autofac;

namespace ANG24.Sys.Infrastructure.Helpers
{
    public class Executer : IExecuteableCreator
    {
        public IMethodExecuter CurrentProcess { get; protected set; }

        private readonly ILifetimeScope container;
        public Executer(ILifetimeScope container) => this.container = container;
        public async Task<bool> ExecuteAsync(Action<IMethodExecuter> executable)
        {
            return await Task.Factory.StartNew(() =>
            {
                var ex = container.Resolve<IMethodExecuter>();
                executable?.Invoke(ex);
                ex.Execute(this, new CancellationToken());
                return true;
            });
        }
        public bool ExecuteCommand(Action<IMethodExecuter> command) => ExecuteAsync(command).Result;
    }
}
