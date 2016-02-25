using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BPMLib
{
    public class Preset : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _Name = "";
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                NotifyPropertyChanged();
            }
        }

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public double Begin { get; set; }
        public double End { get; set; }
    }
}