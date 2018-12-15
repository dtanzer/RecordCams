using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace RecordCams
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page, IMultiCamRecorder
    {

        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private int numCameras;

        internal IMultiCamRecorder Recorder { get { return this; } }

        public MainPage()
        {
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

        public async Task CancelRecordingAsync()
        {
            await Camera1.CancelRecordingAsync();
            //await Camera2.CancelRecordingAsync();
        }

        public async Task StartRecordingAsync(string projectNameText, int sequenceNumber)
        {
            await Camera1.StartRecordingAsync(projectNameText, sequenceNumber);
            //await Camera2.StartRecordingAsync(projectNameText, sequenceNumber);
        }

        public async Task StopRecordingAsync()
        {
            await Camera1.StopRecordingAsync();
            //await Camera2.StopRecordingAsync();
        }
    }
}
