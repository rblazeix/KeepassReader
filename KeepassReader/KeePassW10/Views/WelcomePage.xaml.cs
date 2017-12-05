using KeePassW10.Common;
using KeePassW10.Services;
using KeePassW10.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
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
    public sealed partial class WelcomePage : Page
    {
        private const string WHATS_NEW_DISPLAYED_KEY = "WhatsNewDisplayed";

        public DatabaseViewModel ViewModel { get { return this.DataContext as DatabaseViewModel; } }
        
        public WelcomePage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavigationService.SetPageTitleFromResource("WelcomePage_Title");



            // What's new messagebox
            PackageVersion version = Package.Current.Id.Version;
            string versionKey = WHATS_NEW_DISPLAYED_KEY + version.Major + version.Minor + version.Build;
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(versionKey))
            {
                ApplicationData.Current.LocalSettings.Values[versionKey] = true;

                // Show What's new dialog
                await new MessageDialog(
                                        ResourceLoader.GetForCurrentView().GetString("NewFeatures_Dialog_Message"),
                                        ResourceLoader.GetForCurrentView().GetString("NewFeatures_Dialog_Title")
                                        ).ShowAsync();
            }

            // Cleanup DatabaseViewModel to ensure a clean startup
            ViewModel.Cleanup();

            // Open the last database if navigating to this page for the first time 
            //  and if the option is eselected
            if( ViewModel.AutomaticallyOpenLastDatabase && 
                ViewModel.IsLastDatabaseAvailable &&
                e.NavigationMode == NavigationMode.New
                )
            {
                // Desynchronize the password popup  little, to avoid that the bottom application bar is active
                // and on top of the dialog
                await Task.Delay(100); // wait duration has no meaning, only the desynchronizarion
                await LoadPreviousDatabaseAsync();
            }

        }

        /// <summary>
        /// Asks the user to selecte a database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OpenDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".kdbx");

            string databaseName = String.Empty;

            try
            {
                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    databaseName = file.Name;
                    // Ask for password
                    PasswordParameter password = await DatabasePasswordProvider.GetDatabasePasswordAsync(databaseName);

                    // Then load the database
                    await ViewModel.LoadDatabaseAsync(file, password);

                    // Once loaded, navigate to the collection page
                    if (SettingsViewModel.Instance.ViewCollectionAsTree)
                    {
                        this.Frame.Navigate(typeof(EntryCollectionTreeViewPage));
                    }
                    else
                    {
                        this.Frame.Navigate(typeof(EntryCollectionPage));
                    }
                }
            }
            catch(WinKeeLib.Keys.InvalidCompositeKeyException)
            {
                DatabasePasswordProvider.ClearSavedPasswordForDatabase(databaseName);
                MessageDialog err = new MessageDialog( 
                    ResourceLoader.GetForCurrentView().GetString("MessageDialog_IncorrectKey")  );
                await err.ShowAsync();
            }
            catch(TaskCanceledException)
            {
                // User cancelled password entering
            }
            catch (Exception ex)
            {
                // Error opening the database
                MessageDialog err = new MessageDialog(
                    ResourceLoader.GetForCurrentView().GetString("MessageDialog_ErrorOpeningDatabase") + "\n\"" + ex.Message + "\"");
                await err.ShowAsync();
            }
        }

      

        private async void PreviousDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            // Ensure a database is not already loading
            await LoadPreviousDatabaseAsync();

        }

        private async Task LoadPreviousDatabaseAsync()
        {
            try
            {

                // Ask for password
                PasswordParameter password = await DatabasePasswordProvider.GetDatabasePasswordAsync(ViewModel.LastDatabaseName);

                // Then load the database
                await ViewModel.LoadPreviouslyOpenedDatabase(password);

                // Once loaded, navigate to the collection page
                if (SettingsViewModel.Instance.ViewCollectionAsTree)
                {
                    this.Frame.Navigate(typeof(EntryCollectionTreeViewPage));
                }
                else
                {
                    this.Frame.Navigate(typeof(EntryCollectionPage));
                }
            }
            catch (WinKeeLib.Keys.InvalidCompositeKeyException)
            {
                // Invalid key, ensure it is not saved
                DatabasePasswordProvider.ClearSavedPasswordForDatabase(ViewModel.LastDatabaseName);
                MessageDialog err = new MessageDialog(
                    ResourceLoader.GetForCurrentView().GetString("MessageDialog_IncorrectKey"));
                await err.ShowAsync();
            }
            catch (TaskCanceledException)
            {
                // User cancelled password entering
            }
            catch (Exception ex)
            {
                // Error opening the database
                MessageDialog err = new MessageDialog(
                    ResourceLoader.GetForCurrentView().GetString("MessageDialog_ErrorOpeningDatabase") + "\n\"" + ex.Message + "\"");
                await err.ShowAsync();
            }
        }

        
    }
}
