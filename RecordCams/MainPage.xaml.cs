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
                Camera1.StopRecording();
                Camera2.StopRecording();

                RecordStop.Content = "Record";

                isRecording = false;
            }
            else
            {
                sequenceNumber++;
                localSettings.Values["SequenceNumber"] = sequenceNumber;

                Camera1.StartRecording(ProjectNameText, sequenceNumber);
                Camera2.StartRecording(ProjectNameText, sequenceNumber);

                RecordStop.Content = "Stop";

                isRecording = true;
            }
        }

        public string ProjectNameText
        {
            get { return _ProjectNameText; }
            set
            {
                localSettings.Values["ProjectName"] = value;
                localSettings.Values["SequenceNumber"] = 0;

                sequenceNumber = 0;
                _ProjectNameText = value;
            }
        }
    }
}
