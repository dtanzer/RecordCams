using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    partial class Camera
    {
        private MediaCapture mediaCapture;
        private bool isPreviewing;
        LowLagMediaRecording _mediaRecording;

        public ObservableCollection<DeviceInformation> VideoSources = new ObservableCollection<DeviceInformation>();
        private DeviceInformation _SelectedVideoSource;

        public DeviceInformation SelectedVideoSource
        {
            get
            {
                return _SelectedVideoSource;
            }
            set
            {
                this._SelectedVideoSource = value;
                this.StartPreviewAsync();
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
                this._SelectedAudioSource = value;
                this.StartPreviewAsync();
            }
        }

        internal async void StopRecording()
        {
            await _mediaRecording.StopAsync();
            await _mediaRecording.FinishAsync();
            _mediaRecording = null;
        }

        internal async void StartRecording()
        {
            var myVideos = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Videos);
            StorageFile file = await myVideos.SaveFolder.CreateFileAsync(this.Name + ".mp4", CreationCollisionOption.GenerateUniqueName);
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
            this.AddVideoSourcesAsync();
            this.InitializeVideo();
        }

        private async void InitializeVideo()
        {
            await this.StartPreviewAsync();
        }

        private async void AddVideoSourcesAsync()
        {
            await this.InitializeVideoSourcesAsync();
        }

        private async Task InitializeVideoSourcesAsync()
        {
            DeviceInformationCollection videoCollection = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            foreach (var deviceInfo in videoCollection)
            {
                VideoSources.Add(deviceInfo);
            }

            DeviceInformationCollection audioCollection = await DeviceInformation.FindAllAsync(DeviceClass.AudioCapture);

            foreach (var deviceInfo in audioCollection)
            {
                AudioSources.Add(deviceInfo);
            }
        }

        private async Task StartPreviewAsync()
        {
            if(_SelectedVideoSource == null)
            {
                return;
            }
            try
            {
                if (mediaCapture != null)
                {
                    await mediaCapture.StopPreviewAsync();
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
    }
}
