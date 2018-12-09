using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml.Data;
using Windows.Devices.Enumeration;

namespace RecordCams
{
    public class DeviceInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value == null)
            {
                return null;
            }
            return value as DeviceInformation;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value as DeviceInformation;
        }
    }
    public class DeviceNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as DeviceInformation).Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
