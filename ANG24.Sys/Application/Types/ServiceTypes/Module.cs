using System.ComponentModel;

namespace ANG24.Sys.Application.Types.ServiceTypes
{
    public class Module : INotifyPropertyChanged
    {
        private TimeSpan time;

        public string Synonym { get; set; }
        public TimeSpan Time { get => time; set { time = value; NotifyPropertyChanged(); } }
        public string Name { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
