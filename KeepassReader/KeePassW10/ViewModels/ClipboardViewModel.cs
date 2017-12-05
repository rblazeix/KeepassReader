using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace KeePassW10.ViewModels
{
    public class ClipboardViewModel : ViewModelBase
    {
        public static ClipboardViewModel Instance { get { return (ClipboardViewModel)App.Current.Resources["ClipboardViewModel"]; } }

        private CoreDispatcher _dispatcher = Window.Current.Dispatcher;

        private ThreadPoolTimer _notificationTimer;


        private bool _clipboardClearNotificationDisplayed = false;
        public bool ClipboardClearNotificationDisplayed
        {
            get
            {
                return _clipboardClearNotificationDisplayed; 
            }
            set
            {
                _clipboardClearNotificationDisplayed = value;
                NotifyPropertyChanged();
            }
        }



        private String _clipboardMessageText;
        public String ClipboardMessageText
        {
            get
            {
                return _clipboardMessageText;
            }
            private set
            {
                _clipboardMessageText = value;
                NotifyPropertyChanged();
            }
        }

        private bool _clipboardCopied = false;


        public ClipboardViewModel()
        {
            App.Current.Resuming += App_Resuming;
            App.Current.Suspending += App_Suspending;
        }

        /// <summary>
        /// Called when the application is resuming.
        /// Use this moment to clear the clipboard if necessary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_Resuming(object sender, object e)
        {
            if( SettingsViewModel.Instance.ClearClipboardOnResumeEnabled &&
                _clipboardCopied)
            {
                _clipboardCopied = false;

                if (ClearClipboard())
                {
                    showClipboardNotification("Root_ClipboardClear_Notification");
                }
            }
        }

        private void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            hideClipboardClearNotification();
        }

        public void CopyUserNameToClipboard(string value)
        {
            CopyToClipboard(value);

            hideClipboardClearNotification();
            showClipboardNotification("Root_ClipboardCopyUsername_Notification");
        }

        public void CopyPasswordToClipboard(string value)
        {
            CopyToClipboard(value);

            hideClipboardClearNotification();
            showClipboardNotification("Root_ClipboardCopyPassword_Notification");
        }

        public void CopyToClipboard(string value)
        {
            DataPackage dataPackage = new DataPackage();

            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(value);

            Clipboard.SetContent(dataPackage);

            _clipboardCopied = true;
        }
        
        public bool ClearClipboard()
        {
            try
            {
                Clipboard.Clear();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Hide the clipboard clear notification and disables the timer to make it disappear
        /// </summary>
        private void hideClipboardClearNotification()
        {
            if (_notificationTimer != null)
            {
                _notificationTimer.Cancel();
                _notificationTimer = null;
            }
            ClipboardClearNotificationDisplayed = false;
        }

        /// <summary>
        /// Display a toast notification about the clipboard
        /// </summary>
        /// <param name="text"></param>
        public void showClipboardNotification(String resourceId)
        {
            ClipboardMessageText = ResourceLoader.GetForCurrentView().GetString(resourceId);
            ClipboardClearNotificationDisplayed = true;

            if (_notificationTimer != null)
            {
                _notificationTimer.Cancel();
            }
            // Start the timer to hide the clipboard clear notification
            _notificationTimer = ThreadPoolTimer.CreateTimer(
                async (source) =>
                {
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ClipboardClearNotificationDisplayed = false;
                    });
                }, TimeSpan.FromSeconds(2));
        }
    }
}
