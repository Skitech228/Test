namespace ANG24.Sys.Application.Core
{
    /// <summary>
    /// Своё внутреннее исключение, бросать при выходе за пределы значений объекта
    /// </summary>
    public class ParameterException : Exception
    {
        public ParameterException(string details = "") : base("Выход за пределы допустимых значений")
            => this.Details = details;

        public int StatusCode { get; set; }
        public string Details { get; set; }
    }
}
