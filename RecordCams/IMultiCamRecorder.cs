using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordCams
{
    interface IMultiCamRecorder
    {
        Task CancelRecordingAsync();
        Task StartRecordingAsync(string projectNameText, int sequenceNumber);
        Task StopRecordingAsync();
    }
}
