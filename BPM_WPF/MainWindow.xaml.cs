using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BPMLib;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass;
using System.Windows.Interop;
using System.Text.RegularExpressions;

namespace BPM_WPF
{
    class SongPosition
    {
        public int secs;
        public int mins;
        public int ms;

        public SongPosition(double seconds)
        {
            mins = (int)(seconds / 60);
            secs = (int)(seconds % 60);
            ms = (int)((seconds - ((int)seconds)) * 100);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BPMViewModel viewModel = null;
        public MainWindow()
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            viewModel = new BPMViewModel(helper.Handle);
            DataContext = viewModel;
            InitializeComponent();
        }

        private void SongPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetTimerText(e.NewValue);
        }

        private void SongPosition_DragStarted(object sender, RoutedEventArgs e)
        {
            viewModel.dispatcherTimer.IsEnabled = false;
        }

        private void SongPosition_DragCompleted(object sender, RoutedEventArgs e)
        {
            viewModel.SetPosSeconds(((Slider)sender).Value);
            viewModel.dispatcherTimer.IsEnabled = true;
        }

        private void SetTimerText(double seconds)
        {
            var pos = new SongPosition(seconds);
            viewModel.PositionTxt =
                pos.mins.ToString().PadLeft(2, '0') + ":" +
                pos.secs.ToString().PadLeft(2, '0') + ":" +
                pos.ms.ToString("F0").PadLeft(2, '0');
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (viewModel.SelectedSong != null)
            {
                viewModel.ChangeSpeed((float)e.NewValue);
            }
            ((BPMViewModel)DataContext).SpeedTxt = e.NewValue.ToString("F2") + "%";
        }

        private void Open_Clicked(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".mp3";
            dlg.Filter = "Music Files (.mp3)|*.mp3;*.m4a;";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                viewModel.OpenFile(dlg.FileName);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            viewModel.SongManager.Dispose();
        }

        private Preset selectedPreset = null;
        private void presetListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = (ListBox)sender;
             
            if (list.SelectedItems.Count > 0)
            {
                // loop current selection
                selectedPreset = (Preset)list.SelectedItem;
                viewModel.Loop(selectedPreset);
                viewModel.NewPresetName = selectedPreset.Name;

                var startPos = new SongPosition(selectedPreset.Begin);
                var stopPos = new SongPosition(selectedPreset.End);

                viewModel.StartMin = startPos.mins.ToString();
                viewModel.StartSec = startPos.secs.ToString();
                viewModel.StartMSec = startPos.ms.ToString();

                viewModel.StopMin = stopPos.mins.ToString();
                viewModel.StopSec = stopPos.secs.ToString();
                viewModel.StopMSec = stopPos.ms.ToString();
            }
            else
            {
                // loop entire song
                selectedPreset = null;
            }
        }

        private void addPreset_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPreset == null)
            {
                Preset p = new Preset
                {
                    Name = viewModel.NewPresetName,
                    Begin = (double.Parse(viewModel.StartMin) * 60) + double.Parse(viewModel.StartSec) + (double.Parse(viewModel.StartMSec) / 100),
                    End = (double.Parse(viewModel.StopMin) * 60) + double.Parse(viewModel.StopSec) + (double.Parse(viewModel.StopMSec) / 100)
                };

                viewModel.SelectedSong.Presets.Add(p);
            }
            else
            {
                selectedPreset.Name = viewModel.NewPresetName;
                selectedPreset.Begin = (double.Parse(viewModel.StartMin) * 60) + double.Parse(viewModel.StartSec) + (double.Parse(viewModel.StartMSec) / 100);
                selectedPreset.End = (double.Parse(viewModel.StopMin) * 60) + double.Parse(viewModel.StopSec) + (double.Parse(viewModel.StopMSec) / 100);

                viewModel.Loop(selectedPreset);
            }
        }

        private void captureEnd_Click(object sender, RoutedEventArgs e)
        {
            string[] time = viewModel.PositionTxt.Split(new char[] { ':' });
            viewModel.StopMin = time[0];
            viewModel.StopSec = time[1];
            viewModel.StopMSec = time[2];
        }

        private void captureStart_Click(object sender, RoutedEventArgs e)
        {
            string[] time = viewModel.PositionTxt.Split(new char[] { ':' });
            viewModel.StartMin = time[0];
            viewModel.StartSec = time[1];
            viewModel.StartMSec = time[2];
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void PreviewNumberTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void deletePreset_Click(object sender, RoutedEventArgs e)
        {
            if(selectedPreset != null)
            {
                viewModel.SelectedSong.Presets.Remove(selectedPreset);
            }
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if(viewModel.PlayPause == "Pause")
            {
                viewModel.Pause();
            }
            else
            {
                viewModel.Play();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Play:
                    viewModel.Play();
                    break;
                case Key.Pause:
                case Key.MediaStop:
                    viewModel.Pause();
                    break;
                case Key.Left:
                    // rewind song 5 seconds
                    viewModel.SongPos = viewModel.SongPos - 5;
                    if(viewModel.SongPos < 0)
                    {
                        viewModel.SongPos = 0;
                    }
                    SetTimerText(viewModel.SongPos);
                    viewModel.SetPosSeconds(viewModel.SongPos);
                    break;
                case Key.Right:
                    // fast forward song 5 seconds
                    viewModel.SongPos = viewModel.SongPos + 5;
                    if (viewModel.SongPos > viewModel.SongLength)
                    {
                        viewModel.SongPos = viewModel.SongLength;
                    }
                    SetTimerText(viewModel.SongPos);
                    viewModel.SetPosSeconds(viewModel.SongPos);
                    break;
                case Key.Down:
                    // slow down 2.5%
                    viewModel.SongSpeed = viewModel.SongSpeed - 2.5;
                    break;
                case Key.Up:
                    // speed up 2.5%
                    viewModel.SongSpeed = viewModel.SongSpeed + 2.5;
                    break;
                case Key.MediaPlayPause:
                case Key.Space:
                    this.PlayPause_Click(sender, e);
                    break;
            }
        }

        private void songListBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;//prevent this key press
        }
    }
}
