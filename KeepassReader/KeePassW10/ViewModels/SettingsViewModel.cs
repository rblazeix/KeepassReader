using KeePassW10.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace KeePassW10.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private const string SELECTED_THEME_KEY = "SelectedThemeKey";
        private const string SORT_ENABLED_KEY = "SortingEnabled";
        private const string VIEW_COLLECTION_AS_TREE_KEY = "ViewCollectionAsTree";
        private const string CLIPBOARD_CLEAR_ENABLED_KEY = "ClearClipboardEnabled";
        private const string WINDOWS_HELLO_ENABLED_KEY = "WindowsHelloEnabled";
        private const string AUTO_DATABASE_TIME_LOCK_KEY = "AutoDatabaseTimeLockEnabled";
        private const string REMEMBER_SELECTED_KEYFILE_KEY = "RememberSelectedKeyFile";


        public static SettingsViewModel Instance
        {
            get
            {
                return (SettingsViewModel)App.Current.Resources["SettingsViewModel"];
            }
        }

        #region Settings properties

        private List<string> _themeList = new List<string>();
        public List<string> ThemeList
        {
            get
            {
                return _themeList;
            }
        }

        public int SelectedThemeIndex
        {
            get
            {
                object indexObj = ApplicationData.Current.LocalSettings.Values[SELECTED_THEME_KEY];
                if( indexObj != null )
                {
                    return (int)indexObj;
                }
                return 0;
            }

            set
            {
                ApplicationData.Current.LocalSettings.Values[SELECTED_THEME_KEY] = value;

                NotifyPropertyChanged("SelectedTheme");

                UpdateStatusBarTheme();
            }
        }

        public ElementTheme SelectedTheme
        {
            get
            {
                // Apply the new style
                if (SelectedThemeIndex == 0)
                {
                    return Windows.UI.Xaml.ElementTheme.Light;
                }
                else
                {
                    return Windows.UI.Xaml.ElementTheme.Dark;
                }
            }
        }

        public bool IsLightTheme
        {
            get
            {
                return (SelectedThemeIndex == 0);
            }
        }

        public bool SortingEnabled
        {
            get
            {
                object indexObj = ApplicationData.Current.LocalSettings.Values[SORT_ENABLED_KEY];
                if (indexObj != null)
                {
                    return (bool)indexObj;
                }
                return false; // Default is false
            }

            set
            {
                ApplicationData.Current.LocalSettings.Values[SORT_ENABLED_KEY] = value;
            }
        }

        public bool ViewCollectionAsTree
        {
            get
            {
                object indexObj = ApplicationData.Current.LocalSettings.Values[VIEW_COLLECTION_AS_TREE_KEY];
                if (indexObj != null)
                {
                    return (bool)indexObj;
                }
                return false; // Default is false
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values[VIEW_COLLECTION_AS_TREE_KEY] = value;
            }
        }


        public bool ClearClipboardOnResumeEnabled
        {
            get
            {
                object indexObj = ApplicationData.Current.LocalSettings.Values[CLIPBOARD_CLEAR_ENABLED_KEY];
                if (indexObj != null)
                {
                    return (bool)indexObj;
                }
                return true; // default for the clipboard is true
            }

            set
            {
                ApplicationData.Current.LocalSettings.Values[CLIPBOARD_CLEAR_ENABLED_KEY] = value;
            }
        }

        public bool WindowsHelloEnabled
        {
            get
            {
                object indexObj = ApplicationData.Current.LocalSettings.Values[WINDOWS_HELLO_ENABLED_KEY];
                if (indexObj != null)
                {
                    return (bool)indexObj;
                }
                return false; // Default is false
            }

            set
            {
                ApplicationData.Current.LocalSettings.Values[WINDOWS_HELLO_ENABLED_KEY] = value;

                // Each time Windows Hello is disabled, erases the saved credentials
                if( !value)
                {
                    DatabasePasswordProvider.ClearAllSavedPasswords();
                }
            }
        }


        public bool AutoDatabaseTimeLockEnabled
        {
            get
            {
                object indexObj = ApplicationData.Current.LocalSettings.Values[AUTO_DATABASE_TIME_LOCK_KEY];
                if (indexObj != null)
                {
                    return (bool)indexObj;
                }
                return false; // Default is false
            }

            set
            {
                ApplicationData.Current.LocalSettings.Values[AUTO_DATABASE_TIME_LOCK_KEY] = value;
            }
        }

        public bool RememberLastSelectedKeyFile
        {
            get
            {
                object indexObj = ApplicationData.Current.LocalSettings.Values[REMEMBER_SELECTED_KEYFILE_KEY];
                if (indexObj != null)
                {
                    return (bool)indexObj;
                }
                return true; // Default is true
            }

            set
            {
                ApplicationData.Current.LocalSettings.Values[REMEMBER_SELECTED_KEYFILE_KEY] = value;
            }
        }

        /// <summary>
        /// Delay after which the database is automatically locked (if enabled), in minutes
        /// </summary>
        public double AutoDatabaseTimeLockMinutesDelay
        {
            get
            {
                return 5.0; // 5 minutes
            }
        }

        #endregion

        public void UpdateStatusBarTheme()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                if (statusBar != null)
                {
                    statusBar.BackgroundOpacity = 1;
                    statusBar.BackgroundColor = IsLightTheme ? Colors.White : Colors.Black;
                    statusBar.ForegroundColor = IsLightTheme ? Colors.Black : Colors.White;
                }
            }
        }

        public SettingsViewModel()
        {
            var res = ResourceLoader.GetForCurrentView();

            _themeList.Add(res.GetString("Settings_Light_Theme"));
            _themeList.Add(res.GetString("Settings_Dark_Theme"));
        }

    }
}
