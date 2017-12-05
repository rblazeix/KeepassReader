using KeePassW10.Common;
using KeePassW10.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace KeePassW10.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {
            this.InitializeComponent();

            PackageVersion version = Package.Current.Id.Version;
            VersionText.Text = string.Format("Version {0}.{1}.{2}", version.Major, version.Minor, version.Build);

            if( !SettingsViewModel.Instance.IsLightTheme )
            {
                // Use the other image
                Logo.Source = new BitmapImage(new Uri("ms-appx://KeePassW10/Assets/IconsSVG/AboutLogoWhite.png"));
            }

            NavigationService.SetPageTitleFromResource("AboutPage_Title");
        }
    }
}
