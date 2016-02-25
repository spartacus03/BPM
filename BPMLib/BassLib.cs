using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass;
using System.Threading;

namespace BPMLib
{
    public class BassLib: IDisposable
    {
        private BassLib(BASSInit options, IntPtr hwind)
        {
            Bass.BASS_Init(-1, 44100, options, hwind);
        }

        private static BassLib instance = null;
        public static BassLib BASSInitialize(BASSInit options, IntPtr hwind)
        {
            if (instance == null)
            {
                instance = new BassLib(options, hwind);
            }
            return instance;
        }

        public void Dispose()
        {
            if(instance != null)
            {
                Bass.BASS_Free();
                instance = null;
            }
        }

        public int OpenFile(string loc)
        {
            int chan = Bass.BASS_StreamCreateFile(loc, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT);
            if (chan == 0)
            {
                GetBassErrorTrow("Could not open file: " + loc);
            }
            return chan;
        }

        public int OpenFile(string loc, BASSFlag flags)
        {
            int chan = Bass.BASS_StreamCreateFile(loc, 0, 0, flags | BASSFlag.BASS_STREAM_PRESCAN | BASSFlag.BASS_STREAM_DECODE);
            if (chan == 0)
            {
                GetBassErrorTrow("Could not open file: " + loc);
            }
            return chan;
        }

        public void SetPosSeconds(int chan, double seconds)
        {
            long curPos = Bass.BASS_ChannelGetPosition(chan);
            long pos = Bass.BASS_ChannelSeconds2Bytes(chan, seconds);

            

            if(!Bass.BASS_ChannelSetPosition(chan, pos, BASSMode.BASS_POS_BYTES))
            {
                GetBassErrorTrow("Could not seek to " + seconds);
            }
        }

        public void GetBassErrorTrow(string msg)
        {
            var err = Bass.BASS_ErrorGetCode();
            //BassException e = new BassException(err, msg);
            Console.WriteLine(err.ToString() + " " + msg);
        }

        public float CheckTempoRange(float tempo)
        {
            float TEMPO_MAX = 300;
            float TEMPO_MIN = 5;
            if (tempo > TEMPO_MAX) { tempo = TEMPO_MAX; }
            if (tempo < TEMPO_MIN) { tempo = TEMPO_MIN; }
            return tempo;
        }

        public int ChangeSpeed(int chan, string loc, float speed)
        {
            // get position to resume playback at
            long pos = Bass.BASS_ChannelGetPosition(chan, BASSMode.BASS_POS_BYTES);

            if(!Bass.BASS_StreamFree(chan))
            {
                GetBassErrorTrow("Could not free stream: " + chan);
            }

            chan = OpenFile(loc, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT);
            
            chan = BassFx.BASS_FX_TempoCreate(chan, BASSFlag.BASS_SAMPLE_LOOP);
            if(chan == 0)
            {
                GetBassErrorTrow("Could not open file: " + loc);
            }

            if(!Bass.BASS_ChannelSetPosition(chan, pos, BASSMode.BASS_POS_BYTES))
            {
                GetBassErrorTrow("Could not seek to " + pos + " bytes");
            }

            if(!Bass.BASS_ChannelSetAttribute(chan, BASSAttribute.BASS_ATTRIB_TEMPO, CheckTempoRange(speed) - 100))
            {
                GetBassErrorTrow("Could not set attribute: " + BASSAttribute.BASS_ATTRIB_TEMPO);
            }

            return chan;
        }

        public double GetPositionInSeconds(int chan)
        {
            long posBytes = Bass.BASS_ChannelGetPosition(chan, BASSMode.BASS_POS_BYTES);
            double seconds = Bass.BASS_ChannelBytes2Seconds(chan, posBytes);
            return seconds;
        }
    }
}
