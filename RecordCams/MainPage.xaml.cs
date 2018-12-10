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

        public MainPage()
        {
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
                Camera1.StartRecording();
                Camera2.StartRecording();

                RecordStop.Content = "Stop";

                isRecording = true;
            }
        }
    }
}
