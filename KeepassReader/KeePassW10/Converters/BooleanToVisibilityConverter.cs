using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace KeePassW10
{
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, String language)
        {
            Boolean? b = value as Boolean?;
            Boolean val = b != null && (Boolean)b;

            if (parameter != null && (String)parameter == "invert")
                return !val ? Visibility.Visible : Visibility.Collapsed;

            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, String language)
        {
            throw new NotImplementedException();
        }
    }
}
