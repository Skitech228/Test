using System.Text.Json;

namespace ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base
{
    public interface IOperatorScript
    {
        string? Name { get; set; }
        string Script { set; }
        public IEnumerable<IMethodExecuter>? Methods { get; set; }
    }
    /// <summary>
    /// Одна из реализаций скриптов, на удаление
    /// </summary>
    [Obsolete]
    public class OperatorScript : IOperatorScript
    {
        public string? Name { get; set; }
        public string Script
        {
            get => JsonSerializer.Serialize(Methods, new JsonSerializerOptions { WriteIndented = true });
            set => Methods = JsonSerializer.Deserialize<IEnumerable<IMethodExecuter>>(value);
        }
        public IEnumerable<IMethodExecuter>? Methods { get; set; }
    }
}
