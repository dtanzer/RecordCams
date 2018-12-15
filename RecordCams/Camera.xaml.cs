using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Display;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Core;

namespace RecordCams
{
    partial class Camera : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MediaCapture mediaCapture;
        private bool isPreviewing;
        LowLagMediaRecording _mediaRecording;

        public ObservableCollection<DeviceInformation> VideoSources = new ObservableCollection<DeviceInformation>();
        private DeviceInformation _SelectedVideoSource;

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public DeviceInformation SelectedVideoSource
        {
            get
            {
                return _SelectedVideoSource;
            }
            set
            {
                if (value != _SelectedVideoSource)
                {
                    this._SelectedVideoSource = value;
                    localSettings.Values[this.Name + "Video"] = value.Id;
                    this.InitializeVideo();
                    OnPropertyChanged("SelectedVideoSource");
                }
            }
        }

        public ObservableCollection<DeviceInformation> AudioSources = new ObservableCollection<DeviceInformation>();
        private DeviceInformation _SelectedAudioSource;

        public DeviceInformation SelectedAudioSource
        {
            get
            {
                return _SelectedAudioSource;
            }
            set
            {
                if (value != _SelectedAudioSource)
                {
                    this._SelectedAudioSource = value;
                    localSettings.Values[this.Name + "Audio"] = value.Id;
                    this.InitializeVideo();
                    OnPropertyChanged("SelectedAudioSource");
                }
            }
        }

        internal async void StopRecording()
        {
            await _mediaRecording.StopAsync();
            await _mediaRecording.FinishAsync();
            _mediaRecording = null;
        }

        private string _CameraNameText = "";
        string CameraNameText {
            get
            {
                return _CameraNameText;
            }

            set
            {
                this._CameraNameText = value;
                localSettings.Values[this.Name + "Name"] = value;
                OnPropertyChanged("CameraNameText");
            }
        }

        internal async void StartRecording(string projectName, int sequenceNumber)
        {
            bool hasCameraName = _CameraNameText != null && _CameraNameText.Length > 0;
            string baseName = hasCameraName ? _CameraNameText : this.Name;

            bool hasProjectName = projectName != null && projectName.Length > 0;
            string project = hasProjectName ? projectName : "RecordCamsProject";

            string name = project + "\\" + sequenceNumber.ToString("000") + "_" + baseName + ".mp4";

            var myVideos = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            StorageFile file = await myVideos.SaveFolder.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);
            _mediaRecording = await mediaCapture.PrepareLowLagRecordToStorageFileAsync(
                    MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p), file);
            mediaCapture.RecordLimitationExceeded += MediaCapture_RecordLimitationExceeded;
            _mediaRecording.StartAsync();
        }

        private void MediaCapture_RecordLimitationExceeded(MediaCapture sender)
        {
            this.StopRecording();
            System.Diagnostics.Debug.WriteLine("Record limitation exceeded.");
        }

        public Camera()
        {
            this.InitializeComponent();
            this.Loaded += Camera_Loaded;
        }

        private void Camera_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (localSettings.Values[this.Name + "Name"] != null)
            {
                CameraNameText = (string)localSettings.Values[this.Name + "Name"];
            }
            this.AddVideoSourcesAsync();
        }

        private async void InitializeVideo()
        {
            if (_SelectedVideoSource != null && _SelectedAudioSource != null)
            {
                await this.StartPreviewAsync();
            }
        }

        private async void AddVideoSourcesAsync()
        {
            await this.InitializeVideoSourcesAsync();
        }

        private async Task InitializeVideoSourcesAsync()
        {
            DeviceInformationCollection videoCollection = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            string selectedVideoId = (string) localSettings.Values[this.Name + "Video"];

            DeviceInformation currentSelectedVideoSource = null;
            foreach (var deviceInfo in videoCollection)
            {
                VideoSources.Add(deviceInfo);
                if(deviceInfo.Id == selectedVideoId)
                {
                    currentSelectedVideoSource = deviceInfo;
                }
            }
            if(currentSelectedVideoSource != null)
            {
                SelectedVideoSource = currentSelectedVideoSource;
            }

            DeviceInformationCollection audioCollection = await DeviceInformation.FindAllAsync(DeviceClass.AudioCapture);
            string selectedAudioId = (string)localSettings.Values[this.Name + "Audio"];

            DeviceInformation currentSelectedAudioSource = null;
            foreach (var deviceInfo in audioCollection)
            {
                AudioSources.Add(deviceInfo);
                if (deviceInfo.Id == selectedAudioId)
                {
                    currentSelectedAudioSource = deviceInfo;
                }
            }
            if(currentSelectedAudioSource != null)
            {
                SelectedAudioSource = currentSelectedAudioSource;
            }
        }

        private async Task StartPreviewAsync()
        {
            try
            {
                if (mediaCapture != null)
                {
                    await mediaCapture.StopPreviewAsync();
                    mediaCapture.Dispose();
                }
                mediaCapture = new MediaCapture();

                MediaCaptureInitializationSettings settings = new Windows.Media.Capture.MediaCaptureInitializationSettings();
                settings.AudioDeviceId = "";
                settings.VideoDeviceId = _SelectedVideoSource.Id;
                settings.StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo;
                settings.PhotoCaptureSource = PhotoCaptureSource.VideoPreview;

                await mediaCapture.InitializeAsync(settings);
                mediaCapture.Failed += MediaCapture_Failed;

                DisplayRequest displayRequest = new DisplayRequest();
                displayRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }
            catch (UnauthorizedAccessException)
            {
                // This will be thrown if the user denied access to the camera in privacy settings
                ShowMessageToUser("The app was denied access to the camera");
                return;
            }

            try
            {
                PreviewControl.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
                isPreviewing = true;
            }
            catch (System.IO.FileLoadException)
            {
                mediaCapture.CaptureDeviceExclusiveControlStatusChanged += _mediaCapture_CaptureDeviceExclusiveControlStatusChanged;
            }
        }

        private async void _mediaCapture_CaptureDeviceExclusiveControlStatusChanged(MediaCapture sender, MediaCaptureDeviceExclusiveControlStatusChangedEventArgs args)
        {
            if (args.Status == MediaCaptureDeviceExclusiveControlStatus.SharedReadOnlyAvailable)
            {
                ShowMessageToUser("The camera preview can't be displayed because another app has exclusive access");
            }
            else if (args.Status == MediaCaptureDeviceExclusiveControlStatus.ExclusiveControlAvailable && !isPreviewing)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await StartPreviewAsync();
                });
            }
        }

        private void ShowMessageToUser(string v)
        {
            throw new NotImplementedException();
        }

        private void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            throw new NotImplementedException();
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
