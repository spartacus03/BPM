using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMLib
{
    public class Song
    {
        public string FilePath { get; set; }

        private ObservableCollection<Preset> _presets = new ObservableCollection<Preset>();
        public ObservableCollection<Preset> Presets { get { return _presets; } set { _presets = value; } }

        public override string ToString()
        {
            return Path.GetFileName(FilePath);
        }
    }
}
