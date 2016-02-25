using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BPMLib;
using Un4seen.Bass;
using System.Collections.ObjectModel;
using Un4seen.Bass.AddOn.Fx;

namespace BPM_WPF
{
    public class BPMViewModel : INotifyPropertyChanged
    {
        public System.Windows.Threading.DispatcherTimer dispatcherTimer;
        private BPMLib.BassLib bass = null;
        private int chan = 0;

        public BPMViewModel(IntPtr hwnd)
        {
            LoopSync = new SYNCPROC(ProcSync);
            bass = BPMLib.BassLib.BASSInitialize(BASSInit.BASS_DEVICE_DEFAULT, hwnd);
        }

        #region Properties
        public event PropertyChangedEventHandler PropertyChanged;

        private SongManager _SongManager = BPMLib.SongManager.Instance;
        public SongManager SongManager
        {
            get { return _SongManager; }
            set {
                NotifyPropertyChanged();
            }
        }

        private String _NewPresetName = "";
        public String NewPresetName
        {
            get
            {
                return _NewPresetName;
            }
            set
            {
                _NewPresetName = value;
                NotifyPropertyChanged();
            }
        }

        private String _StartMin = "00";
        public String StartMin
        {
            get
            {
                return _StartMin;
            }
            set
            {
                _StartMin = value;
                NotifyPropertyChanged();
            }
        }
        private String _StartSec = "00";
        public String StartSec
        {
            get
            {
                return _StartSec;
            }
            set
            {
                _StartSec = value;
                NotifyPropertyChanged();
            }
        }
        private String _StartMSec = "00";
        public String StartMSec
        {
            get
            {
                return _StartMSec;
            }
            set
            {
                _StartMSec = value;
                NotifyPropertyChanged();
            }
        }
        private String _StopMin = "00";
        public String StopMin
        {
            get
            {
                return _StopMin;
            }
            set
            {
                _StopMin = value;
                NotifyPropertyChanged();
            }
        }
        private String _StopSec = "00";
        public String StopSec
        {
            get
            {
                return _StopSec;
            }
            set
            {
                _StopSec = value;
                NotifyPropertyChanged();
            }
        }
        private String _StopMSec = "00";
        public String StopMSec
        {
            get
            {
                return _StopMSec;
            }
            set
            {
                _StopMSec = value;
                NotifyPropertyChanged();
            }
        }

        private String _speedTxt = "100%";
        public String SpeedTxt
        {
            get
            {
                return _speedTxt;
            }
            set
            {
                _speedTxt = value;
                NotifyPropertyChanged();
            }
        }

        private String _posTxt = "00:00:00";
        public String PositionTxt
        {
            get
            {
                return _posTxt;
            }
            set
            {
                _posTxt = value;
                NotifyPropertyChanged();
            }
        }

        private int _songLength = 60 * 3;
        public int SongLength
        {
            get
            {
                return _songLength;
            }
            set
            {
                _songLength = value;
                NotifyPropertyChanged();
            }
        }

        private double _songPos = 0;
        public double SongPos
        {
            get
            {
                return _songPos;
            }
            set
            {
                _songPos = value;
                NotifyPropertyChanged();
            }
        }

        private double _songSpeed = 100;
        public double SongSpeed
        {
            get
            {
                return _songSpeed;
            }
            set
            {
                _songSpeed = value;
                NotifyPropertyChanged();
            }
        }

        private Song _selectedSong = null;
        public Song SelectedSong
        {
            get
            {
                return _selectedSong;
            }
            set
            {
                _selectedSong = value;
                if(_selectedSong != null)
                {
                    ChangeSong(_selectedSong);
                    if (this.speed != 100)
                    {
                        this.ChangeSpeed(this.speed);
                    }
                    this.PlayPause = "Pause";
                }
                else
                {
                    this.PlayPause = "Play";
                }
                NotifyPropertyChanged();
            }
        }

        private string _playPause = "Play";
        public string PlayPause
        {
            get
            {
                return _playPause;
            }
            set
            {
                _playPause = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        public void OpenFile(string fileName)
        {
            SelectedSong = SongManager.ManageSong(fileName);
        }

        public void SetPosSeconds(double value)
        {
            bass.SetPosSeconds(chan, value);
        }

        public void ChangeSong(Song song, Preset preset = null)
        {
            this.Pause();

            if (chan != 0)
            {
                Bass.BASS_ChannelStop(chan);
                Bass.BASS_StreamFree(chan);
            }

            chan = bass.OpenFile(song.FilePath);
            this.ChangeSpeed((float)this.SongSpeed);

            long pos = Bass.BASS_ChannelGetLength(chan);
            double seconds = Bass.BASS_ChannelBytes2Seconds(chan, pos);
            SongLength = (int)seconds;

            this.Play();
        }

        public void ChangeSpeed(float newValue)
        {
            speed = newValue;
            if (lysnc != 0) { Bass.BASS_ChannelRemoveSync(chan, lysnc); }

            chan = bass.ChangeSpeed(chan, SelectedSong.FilePath, newValue);

            if (lysnc != 0)
            {
                lysnc = Bass.BASS_ChannelSetSync(chan, BASSSync.BASS_SYNC_POS, endPos, LoopSync, (IntPtr)0);
            }

            if (this.PlayPause == "Pause")
            {
                Bass.BASS_ChannelPlay(chan, false);
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            SongPos = bass.GetPositionInSeconds(chan);
        }

        protected virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public float speed = 100;
        private long startPos = 0;
        private long endPos = 0;
        private int lysnc = 0;

        public void Loop(Preset preset)
        {
            this.Pause();
            
            startPos = Bass.BASS_ChannelSeconds2Bytes(chan, preset.Begin);
            endPos = Bass.BASS_ChannelSeconds2Bytes(chan, preset.End);

            LoopSync = new SYNCPROC(ProcSync);
            if (lysnc != 0) { Bass.BASS_ChannelRemoveSync(chan, lysnc); }

            lysnc = Bass.BASS_ChannelSetSync(chan, BASSSync.BASS_SYNC_POS, endPos, LoopSync, (IntPtr)0);

            long curPos = Bass.BASS_ChannelGetPosition(chan);
            if (curPos < startPos || curPos >= endPos)
            {
                Bass.BASS_ChannelSetPosition(chan, startPos, BASSMode.BASS_POS_BYTES);
            }

            this.Play();
        }

        private SYNCPROC LoopSync;
        private void ProcSync(int handle, int channel, int data, IntPtr user)
        {
            if (!Bass.BASS_ChannelSetPosition(channel, startPos, BASSMode.BASS_POS_BYTES))// try seeking to loop start
            {
                //Bass.BASS_ChannelSetPosition(channel, 0, BASSMode.BASS_POS_BYTES); // failed, go to start of file instead
            }
        }

        public void Play()
        {
            if(this.PlayPause == "Pause" || SelectedSong == null || chan == 0)
            {
                return;
            }

            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
            }

            Bass.BASS_ChannelPlay(chan, false);

            this.PlayPause = "Pause";

            if (dispatcherTimer == null)
            {
                dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            }

            dispatcherTimer.Start();
        }

        public void Pause()
        {
            if (this.PlayPause == "Play" || SelectedSong == null || chan == 0)
            {
                return;
            }

            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
            }

            Bass.BASS_ChannelStop(chan);

            this.PlayPause = "Play";
        }
    }
}
