using System.Diagnostics;

namespace ANG24.Sys.Infrastructure.Helpers
{
    public class MethodExecuter : IMethodExecuter
    {
        private CommandState state;
        private JsonElement objects;
        private byte progressPercent;
        public event Action<CommandState> StateChanged;
        public event Action<byte> ProgressPercentChanged;
        public string OperatorName { get; set; }
        public bool IsAsync { get; set; }
        public string MethodName { get; set; }
        public Action<bool> Callback { get; set; }
        public CommandState State
        {
            get => state;
            private set
            {
                StateChanged?.Invoke(state = value);
                if (value != CommandState.InProgress)
                    Callback?.Invoke(value switch
                    {
                        CommandState.Сompleted => true,
                        _ => false
                    });
            }
        }
        public byte ProgressPercent
        {
            get => progressPercent;
            private set => ProgressPercentChanged?.Invoke(progressPercent = value);
        }
        public IList<object> Parameters { get; set; } = new List<object>();
        public JsonElement JObjects
        {
            get => objects;
            set
            {
                objects = value;
                Parameters = ParseJObjects(value);
            }
        }
        internal IList<object> ParseJObjects(JsonElement array)
        {
            var objects = new List<object>();
            foreach (var parameter in array.EnumerateArray())
                switch (parameter.ValueKind)
                {
                    case JsonValueKind.String: objects.Add(parameter.GetString()); break;
                    case JsonValueKind.Number:
                        if (parameter.TryGetInt32(out var value)) { objects.Add(value); break; } // int
                        if (parameter.TryGetDouble(out var valueD)) { objects.Add(valueD); break; } // decimal
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False: objects.Add(parameter.GetBoolean()); break;
                    case JsonValueKind.Array: //не работает, т.к. нужен конкретный тип при проверке (string[] \ list<double>...)
                        var objArray = new List<object>();
                        foreach (var item in parameter.EnumerateArray())
                            foreach (var itemD in ParseJObjects(item))
                                objArray.Add(itemD);

                        objects.Add(objArray.ToArray());
                        break;
                }
            return objects;
        }
        public void Execute(object sender, CancellationToken token)
        {
            State = CommandState.InProgress;
            var parameterTypes = new Type[Parameters.Count];
            for (int i = 0; i < Parameters.Count; i++)
                parameterTypes[i] = Parameters[i].GetType();
            try
            {
                System.Reflection.MethodInfo method;
                method = sender.GetType().GetMethod(MethodName, parameterTypes);
                if (method is null)
                    try // может быть совпадение по именам
                    {
                        method = sender.GetType().GetMethod(MethodName);
                    }
                    catch { }


                //добавить ReportProgress в IControllerOperator и подписаться на него

                method?.Invoke(sender, Parameters.ToArray());


                if (token.IsCancellationRequested) State = CommandState.Canceled;
                else State = CommandState.Сompleted;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                if (token.IsCancellationRequested) State = CommandState.Canceled;
                else State = CommandState.Failed;
            }
        }
    }
}
