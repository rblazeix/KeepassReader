using KeePassW10.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace KeePassW10.Views
{
    public sealed partial class PasswordDialog : ContentDialog
    {
        public PasswordDialogViewModel ViewModel {  get { return this.DataContext as PasswordDialogViewModel; } }

        /// <summary>
        /// Result of the dialog -> true if validated (ok or return pressed), false otherwise
        /// </summary>
        public bool Result = false;

        public PasswordDialog(string databaseName)
        {
            this.InitializeComponent();

            this.DataContext = new PasswordDialogViewModel(databaseName);

            // Set database name
            var res = ResourceLoader.GetForCurrentView();
            DatabaseNameTextBlock.Text = String.Format(res.GetString("PasswordDialog_DatabaseName"), databaseName);

        }
        
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Result = true;

            ViewModel.CheckKeyFileInUse();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Result = false;
        }

        private void CheckBox_ShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            PasswordField.PasswordRevealMode = PasswordRevealMode.Visible;
        }

        private void CheckBox_ShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordField.PasswordRevealMode = PasswordRevealMode.Peek;
        }

        private async void KeyFile_Button_Click(object sender, RoutedEventArgs e)
        {
            // Open a file selector
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add("*");

            try
            {
                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    ViewModel.SelectKeyFile(file);
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Validation du formulaire quand l'utilisateur appuie sur Entrée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasswordField_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Result = true;
                e.Handled = true;
                this.Hide();
            }
        }
    }
}
