namespace ANG24.Sys.Application.Core.DTO
{
    /// <summary>
    /// Объект данных контроллера для передачи через API
    /// </summary>
    public struct OperatorMessageDto
    {
        /// <summary>
        /// название оператора, отправляющего пакет данных
        /// </summary>
        public string OperatorName { get; set; }
        /// <summary>
        /// Собранная строка данных от контроллера (что бы клиент мог её распарсить)
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Дополнительная информация для парсинга объекта
        /// </summary>
        public string Optional { get; set; }
    }
}
