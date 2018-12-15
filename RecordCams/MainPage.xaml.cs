using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace RecordCams
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private bool isRecording;
        private string _ProjectNameText;
        private int sequenceNumber = 0;
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private int numCameras;

        public MainPage()
        {
            if(localSettings.Values["ProjectName"] != null)
            {
                _ProjectNameText = (string) localSettings.Values["ProjectName"];
            }

            if(localSettings.Values["SequenceNumber"] != null)
            {
                sequenceNumber = (int) localSettings.Values["SequenceNumber"];
            }

            if (localSettings.Values["NumCameras"] != null)
            {
                numCameras = (int)localSettings.Values["NumCameras"];
            } else
            {
                numCameras = 2;
                localSettings.Values["NumCameras"] = numCameras;
            }

            this.InitializeComponent();
        }

        void QuitButtonClicked(object sender, RoutedEventArgs e)
        {
            CoreApplication.Exit();
        }

        void RecordButtonClicked(object sender, RoutedEventArgs e)
        {
            if(isRecording)
            {
                Camera1.StopRecordingAsync();
                Camera2.StopRecordingAsync();

                RecordStop.Content = "Record";
                Cancel.Visibility = Visibility.Visible;

                isRecording = false;
            }
            else
            {
                sequenceNumber++;
                localSettings.Values["SequenceNumber"] = sequenceNumber;

                Camera1.StartRecordingAsync(ProjectNameText, sequenceNumber);
                Camera2.StartRecordingAsync(ProjectNameText, sequenceNumber);

                RecordStop.Content = "Stop";
                Cancel.Visibility = Visibility.Visible;

                isRecording = true;
            }
        }

        async void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            await Camera1.CancelRecordingAsync();
            await Camera2.CancelRecordingAsync();

            sequenceNumber--;
            localSettings.Values["SequenceNumber"] = sequenceNumber;

            RecordStop.Content = "Record";
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
