using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace BPMLib
{
    public class SongManager: IDisposable
    {
        private const string SongManagerFilename = "Songs.xml";

        private SongManager()
        {
            this.CreateConfigFile();
            this.ReadValues();
        }

        private static SongManager _instance = null;
        public static SongManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SongManager();
                }
                return _instance;
            }
        }

        private ObservableCollection<Song> _songs = new ObservableCollection<Song>();
        public ObservableCollection<Song> Songs
        {
            get { return _songs; }
            set {
                _songs = value;
            }
        }

        private string SongMangerFilePath
        {
            get
            {
                return Path.Combine(BPMLib.Constants.BPMDataDir, SongManagerFilename);
            }
        }

        private void CreateConfigFile()
        {
            if (!Directory.Exists(BPMLib.Constants.BPMDataDir))
            {
                Directory.CreateDirectory(BPMLib.Constants.BPMDataDir);
            }

            if(!File.Exists(SongMangerFilePath))
            {
                WriteValues();
            }
        }

        private void ReadValues()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Song>));
            using (FileStream stream = File.OpenRead(SongMangerFilePath))
            {
                this.Songs = (ObservableCollection<Song>)serializer.Deserialize(stream);
            }
        }

        private void WriteValues()
        {
            if (File.Exists(SongMangerFilePath))
            {
                File.Delete(SongMangerFilePath);
                CreateConfigFile();
            }

            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Song>));
            using (FileStream stream = File.OpenWrite(SongMangerFilePath))
            {
                serializer.Serialize(stream, this.Songs);
            }
        }

        public bool IsSongManaged(string filepath)
        {
            foreach(var song in Songs)
            {
                if(song.FilePath == filepath)
                {
                    return true;
                }
            }
            return false;
        }

        public Song ManageSong(string filepath)
        {
            Song s = null;
            foreach (var song in Songs)
            {
                if (song.FilePath == filepath)
                {
                    s = song;
                    break;
                }
            }

            if (s == null)
            {
                s = new Song
                {
                    FilePath = filepath,
                    Presets = new ObservableCollection<Preset>()
                };
                Songs.Add(s);
            }

            return s;
        }

        public void Dispose()
        {
            WriteValues();
        }
    }
}
