using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace KeePassW10.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string name = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public Brush BackgroundColorBrush
        {
            get
            {
                return (SolidColorBrush)App.Current.Resources["WhiteBrush"];
            }
        }

        public Brush ForegroundColorBrush
        {
            get
            {
                return (SolidColorBrush)App.Current.Resources["BlackBrush"];
            }
        }
    }
}
