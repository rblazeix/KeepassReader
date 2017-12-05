using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace KeePassW10.ViewModels
{
    public class PasswordDialogViewModel : ViewModelBase
    {
        private const string FUTURE_LIST_LAST_KEYFILE_TOKEN_KEY = "FutureListLastKeyFileToken";
        private const string DATABASENAME_FOR_LAST_KEYFILE_KEY = "DatabaseNameForLastKeyFile";

        private string _databaseName;

        #region Public properties
        private bool _usePassword;
        public bool UsePassword
        {
            get
            {
                return _usePassword;
            }
            set
            {
                _usePassword = value;
                NotifyPropertyChanged();
            }
        }

        private string _password;
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                if (String.IsNullOrEmpty(_password))
                {
                    UsePassword = false;
                }
                else
                {
                    UsePassword = true;
                }
                NotifyPropertyChanged();
            }
        }

        private bool _useKeyFile;
        public bool UseKeyFile
        {
            get
            {
                return _useKeyFile;
            }
            set
            {
                _useKeyFile = value;
                NotifyPropertyChanged();
            }
        }


        private StorageFile _keyFile;
        public StorageFile KeyFile
        {
            get
            {
                return _keyFile;
            }
            set
            {
                _keyFile = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("KeyFilePath");
                NotifyPropertyChanged("KeyFilePathVisible");
            }
        }

        public string KeyFilePath
        {
            get
            {
                if( KeyFile != null )
                {
                    return KeyFile.Path;
                }
                return String.Empty;
            }
        }


        public bool KeyFilePathVisible
        {
            get
            {
                return !String.IsNullOrEmpty(KeyFilePath);
            }
        }


        #endregion

        public PasswordDialogViewModel(string databaseName)
        {
            _databaseName = databaseName;

#if DEBUG
            Password = "testPassword";
#endif
            // Retrieve previously used keyfile, if any
            initAsync();
        }

        private async void initAsync()
        {
            try
            {
                if (SettingsViewModel.Instance.RememberLastSelectedKeyFile)
                {
                    // Check if a keyfile token was lastly used
                    object value = String.Empty;
                    bool success = ApplicationData.Current.LocalSettings.Values.TryGetValue(FUTURE_LIST_LAST_KEYFILE_TOKEN_KEY, out value);
                    if (success)
                    {
                        string token = (string)value;
                        KeyFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);

                        UseKeyFile = true;
                    }
                }
            }
            catch(Exception) { } // Error loading the last keyfile, ignore it
        }

        public void SelectKeyFile(StorageFile keyFile)
        {
            UseKeyFile = true;

            KeyFile = keyFile;
            
            try
            {
                // Store the keyfile to the future access list for automatic access for next opening
                string token = StorageApplicationPermissions.FutureAccessList.Add(KeyFile);
                // Add or replace the token value for the last database file
                ApplicationData.Current.LocalSettings.Values[FUTURE_LIST_LAST_KEYFILE_TOKEN_KEY] = token;

                // Save the name of the database the keyfile was last associated to
                ApplicationData.Current.LocalSettings.Values[DATABASENAME_FOR_LAST_KEYFILE_KEY] = _databaseName;
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Check if a key file is in use. If so, save it for future use (the next password dialog display).
        /// If not, remove it so that it is not proposed the next time
        /// </summary>
        public void CheckKeyFileInUse()
        {
            if (!UseKeyFile && KeyFile != null)
            {
                // A keyfile is currently selected but not used. Remove it from FutureAccessList
                object value = String.Empty;
                bool success = ApplicationData.Current.LocalSettings.Values.TryGetValue(FUTURE_LIST_LAST_KEYFILE_TOKEN_KEY, out value);
                if (success)
                {
                    try
                    {
                        string token = (string)value;
                        StorageApplicationPermissions.FutureAccessList.Remove(token);
                        ApplicationData.Current.LocalSettings.Values.Remove(FUTURE_LIST_LAST_KEYFILE_TOKEN_KEY);
                        ApplicationData.Current.LocalSettings.Values.Remove(DATABASENAME_FOR_LAST_KEYFILE_KEY);
                    }
                    catch (Exception) { }
                }
            }

        }

        /// <summary>
        /// Return the stored keyfile, or null if none is available
        /// </summary>
        public async static Task<StorageFile> GetStoredKeyFileAsync(string databaseName)
        {
            object value = String.Empty;
            bool success = ApplicationData.Current.LocalSettings.Values.TryGetValue(FUTURE_LIST_LAST_KEYFILE_TOKEN_KEY, out value);
            if (success)
            {
                string token = (string)value;

                // Now try to retrieve the name of the database this keyfile was lastrly used with
                value = String.Empty;
                success = ApplicationData.Current.LocalSettings.Values.TryGetValue(DATABASENAME_FOR_LAST_KEYFILE_KEY, out value);
                if (success)
                {
                    try
                    {
                        string lastName = (string)value;
                        if (databaseName.CompareTo(lastName) == 0)
                        {
                            // The keyFile was used for this same database, so use it
                            return await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
                        }
                    }
                    catch (Exception) { }
                }
            }
            return null;
        }
    }
}
