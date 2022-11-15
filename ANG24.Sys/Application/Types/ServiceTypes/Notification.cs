namespace ANG24.Sys.Application.Types.ServiceTypes
{
    public class Notification
    {
        public int Id { get; set; }
        public string MessageTitle { get; set; } = "Внимание!";
        public string Message { get; set; }
        public bool IsOKButton { get; set; }
        public bool IsCancelButton { get; set; }
        public int DismissDelay { get; set; }
        [NonSerialized]
        public Action OnOK;
        [NonSerialized]
        public Action OnCancel;
    }
}
