using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RecordCams
{
    public sealed partial class RecorderControls : UserControl
    {
        private bool isRecording;
        private string _ProjectNameText;
        private int sequenceNumber = 0;
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private BitmapImage RECORD_ICON = new BitmapImage(new Uri("ms-appx:///Assets/IconRecord.png"));
        private BitmapImage STOP_ICON = new BitmapImage(new Uri("ms-appx:///Assets/IconStop.png"));

        public RecorderControls()
        {
            if (localSettings.Values["ProjectName"] != null)
            {
                _ProjectNameText = (string)localSettings.Values["ProjectName"];
            }

            if (localSettings.Values["SequenceNumber"] != null)
            {
                sequenceNumber = (int)localSettings.Values["SequenceNumber"];
            }

            this.InitializeComponent();
        }

        internal IMultiCamRecorder Recorder {
            get;
            set;
        }

        async void RecordButtonClicked(object sender, RoutedEventArgs e)
        {
            if (isRecording)
            {
                await Recorder.StopRecordingAsync();

                RecordStopImage.Source = RECORD_ICON;
                Cancel.Visibility = Visibility.Collapsed;

                isRecording = false;
            }
            else
            {
                sequenceNumber++;
                localSettings.Values["SequenceNumber"] = sequenceNumber;

                await Recorder.StartRecordingAsync(ProjectNameText, sequenceNumber);

                RecordStopImage.Source = STOP_ICON;
                Cancel.Visibility = Visibility.Visible;

                isRecording = true;
            }
        }

        async void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            await Recorder.CancelRecordingAsync();

            sequenceNumber--;
            localSettings.Values["SequenceNumber"] = sequenceNumber;

            RecordStopImage.Source = RECORD_ICON;
            Cancel.Visibility = Visibility.Collapsed;

            isRecording = false;
        }

        public string ProjectNameText
        {
            get { return _ProjectNameText; }
            set
            {
                if (_ProjectNameText != value)
                {
                    localSettings.Values["ProjectName"] = value;
                    localSettings.Values["SequenceNumber"] = 0;

                    sequenceNumber = 0;
                    _ProjectNameText = value;
                }
            }
        }

    }
}
