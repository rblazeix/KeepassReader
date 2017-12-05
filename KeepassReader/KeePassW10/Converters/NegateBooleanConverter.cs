using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace KeePassW10
{
    public sealed class NegateBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, String language)
        {
            Boolean? b = value as Boolean?;
            Boolean val = b != null && (Boolean)b;

            return !val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, String language)
        {
            return Convert(value, targetType, parameter, language);
        }
    }
}
