namespace ANG24.Sys.Communication.Helpers
{
    //Представляет команду контроллера
    public class ControllerCommand
    {
        public string Command { get; set; }
        // команда с каким кодом должен прийти ответ
        public string CommandResponse { get; set; }
        public string Message { get; set; }
        //метод, выполняющий включение события
        public bool CommandDone { get; set; } = false;
        public void Complete() => OnCompleted?.Invoke(CommandDone);

        //сигнальное событие для указания выполненой команды
        public event Action<bool> OnCompleted;
    }
}