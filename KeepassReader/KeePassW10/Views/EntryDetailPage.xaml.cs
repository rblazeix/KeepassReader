using KeePassW10.Common;
using KeePassW10.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace KeePassW10.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EntryDetailPage : Page
    {
        public DatabaseViewModel ViewModel { get { return this.DataContext as DatabaseViewModel; } }

        public EntryDetailPage()
        {
            this.InitializeComponent();

            NavigationService.SetPageTitleFromResource("EntryDetailPage_Title");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            DataTransferManager.GetForCurrentView().DataRequested += Share_DataRequested;

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            DataTransferManager.GetForCurrentView().DataRequested -= Share_DataRequested;
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Try to add
                Uri uri = null;
                if( !Uri.TryCreate(ViewModel.CurrentEntry.Url, UriKind.Absolute, out uri))
                {
                    // Try to add http at the start
                    if (!Uri.TryCreate("http://" + ViewModel.CurrentEntry.Url, UriKind.Absolute, out uri))
                    {
                        // Error again
                        uri = null;
                    }
                }

                if (uri != null)
                {
                    await Windows.System.Launcher.LaunchUriAsync(uri);
                }

            } catch(Exception ex)
            {
                // Error opening the URL, notiy user
            }
        }


        private void ShareAppButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private void Share_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.Properties.Title = ViewModel.CurrentEntry.Title;
            string text = ViewModel.CurrentEntry.Title + "\n" + ViewModel.CurrentEntry.Username + "\n" + ViewModel.CurrentEntry.Password;
            args.Request.Data.SetText(text);
        }


    }
}
