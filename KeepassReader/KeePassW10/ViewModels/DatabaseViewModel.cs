using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Utility;
using KeePassW10.Common;
using KeePassW10.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using WinKeeLib.Keys;

namespace KeePassW10.ViewModels
{
    /// <summary>
    /// Represents the database
    /// </summary>
    public class DatabaseViewModel : ViewModelBase
    {
        private const string FUTURE_LIST_DATABASE_TOKEN_KEY = "FutureListDatabaseToken";
        private const string LAST_DATABASE_NAME_KEY = "LastDatabaseName";
        private const string AUTOMATICALLY_OPEN_LAST_DATABASE_NAME_KEY = "AutomaticallyOpenLastDatabase";
        private const string SUSPENSION_TIMESTAMP_KEY = "SuspensionTimestamp";

        public static DatabaseViewModel Instance
        {
            get
            {
                return (DatabaseViewModel)App.Current.Resources["DatabaseViewModel"];
            }
        }

        #region Public properties

        private ObservableCollection<PwGroupViewModel> _groups = new ObservableCollection<PwGroupViewModel>();
        /// <summary>
        /// Lists all database groups and subentries entries
        /// </summary>
        public ObservableCollection<PwGroupViewModel> Groups
        {
            get
            {
                return _groups;
            }
        }

        //private ObservableCollection<PwEntryViewModel> _filteredEntriesFlattened = new ObservableCollection<PwEntryViewModel>();
        ///// <summary>
        ///// List of filtered entries from all the groups. Can be used by AutoSuggestBox
        ///// </summary>
        //public ObservableCollection<PwEntryViewModel> FilteredEntriesFlattened
        //{
        //    get
        //    {
        //        return _filteredEntriesFlattened;
        //    }
        //}

        private ObservableCollection<PwGroupViewModel> _filteredGroupedEntries = new ObservableCollection<PwGroupViewModel>();
        /// <summary>
        /// List of filtered group and entries corresponding to the current search criteria
        /// </summary>
        public ObservableCollection<PwGroupViewModel> FilteredGroupedEntries
        {
            get
            {
                return _filteredGroupedEntries;
            }
        }

        private ObservableCollection<PwGroupViewModel> _hierarchicalGroups = new ObservableCollection<PwGroupViewModel>();
        /// <summary>
        /// Lists all database groups and subentries entries
        /// </summary>
        public ObservableCollection<PwGroupViewModel> HierarchicalGroups
        {
            get
            {
                return _hierarchicalGroups;
            }
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
                NotifyPropertyChanged();
            }
        }

        public string LastDatabaseName
        {
            get
            {
                object value = String.Empty;
                bool success = ApplicationData.Current.LocalSettings.Values.TryGetValue(LAST_DATABASE_NAME_KEY, out value);
                if( success )
                {
                    return (string)value;
                }
                return String.Empty;
            }
        }

        public bool IsLastDatabaseAvailable
        {
            get
            {
                object value = String.Empty;
                bool success = ApplicationData.Current.LocalSettings.Values.TryGetValue(FUTURE_LIST_DATABASE_TOKEN_KEY, out value);
                return success;
            }
        }

        private PwEntryViewModel _currentEntry = null;
        /// <summary>
        /// Currently selected entry
        /// </summary>
        public PwEntryViewModel CurrentEntry
        {
            get
            {
                return _currentEntry;
            }
            set
            {
                _currentEntry = value;
                NotifyPropertyChanged();
            }
        }


        private PwGroupViewModel _currentGroup = null;
        /// <summary>
        /// Currently selected entry
        /// </summary>
        public PwGroupViewModel CurrentGroup
        {
            get
            {
                return _currentGroup;
            }
            set
            {
                _currentGroup = value;
                //UpdateFilterededEntries(Filter);
                NotifyPropertyChanged();

            }
        }


        public bool AutomaticallyOpenLastDatabase
        {
            get
            {
                object v;
                if( ApplicationData.Current.LocalSettings.Values.TryGetValue(AUTOMATICALLY_OPEN_LAST_DATABASE_NAME_KEY, out v) )
                {
                    return (bool)v;
                }
                return false;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values[AUTOMATICALLY_OPEN_LAST_DATABASE_NAME_KEY] = value;
            }
        }

        public string Filter { get; set; }


        private bool _databaseOpened = false;
        public bool DatabaseOpened
        {
            get
            {
                return _databaseOpened;
            }
            set
            {
                _databaseOpened = value;
                NotifyPropertyChanged();
            }
        }

        #endregion


        private StorageFile _databaseFile;

        /// <summary>
        /// Initialize the database view model with the database file as well as the password to use to decrypt it
        /// </summary>
        /// <param name="databaseFile"></param>
        /// <param name="password"></param>
        public DatabaseViewModel()
        {
            App.Current.Suspending += App_Suspending;
            App.Current.Resuming += App_Resuming;

            Filter = String.Empty;
        }

        private void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            
            // Save date of suspension if a database is opened, so that the automatic lock can be performed
            if( DatabaseOpened && SettingsViewModel.Instance.AutoDatabaseTimeLockEnabled)
            {
                ApplicationData.Current.LocalSettings.Values[SUSPENSION_TIMESTAMP_KEY] = DateTime.Now.ToString();
            }

            deferral.Complete();
        }

        private void App_Resuming(object sender, object e)
        {
            // Check if the auto database time lock is enabled
            if (SettingsViewModel.Instance.AutoDatabaseTimeLockEnabled)
            {
                // Read last date
                object o = null;
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(SUSPENSION_TIMESTAMP_KEY, out o))
                {
                    string dateValue = (String)o;
                    if (!String.IsNullOrEmpty(dateValue))
                    {
                        DateTime date = DateTime.Parse(dateValue);

                        // Now check if the suspension has been long enough
                        double nbMinutes = (DateTime.Now - date).TotalMinutes;
                        if( nbMinutes >= SettingsViewModel.Instance.AutoDatabaseTimeLockMinutesDelay)
                        {
                            Cleanup();

                            NavigationService.NavigateBackToRoot();
                        }
                    }
                }
            }
            ApplicationData.Current.LocalSettings.Values.Remove(SUSPENSION_TIMESTAMP_KEY);
        }

        /// <summary>
        /// Update the FilteredEntries property with the list of entries corresponding to the provided filter
        /// </summary>
        /// <param name="filter"></param>
        public void UpdateFilterededEntries(string filter)
        {
            Filter = filter;

            string lowcaseFilter = filter.ToLower();

            if (SettingsViewModel.Instance.ViewCollectionAsTree)
            {
                if (CurrentGroup != null)
                {
                    // Apply the filter to all subgroups
                    foreach (var group in HierarchicalGroups)
                    {
                        group.UpdateFilteredEntries(filter);    
                    }

                    // If the selected group is empty, switch to the first with a least an entry
                    if (CurrentGroup.FilteredEntriesEmpty)
                    {
                        var group = HierarchicalGroups.FirstOrDefault(g => !g.FilteredEntriesEmpty);
                        if( group != null )
                        {
                            CurrentGroup = group;
                        }
                    }
                }


            }
            else
            {

                FilteredGroupedEntries.Clear();
                // Or filter the grouped view
                foreach (var group in _groups)
                {
                    group.UpdateFilteredEntries(filter);
                    if (group.FilteredEntries.Count > 0)
                    {
                        FilteredGroupedEntries.Add(group);
                    }
                }
            }
        }

        public async Task<StorageFile> GetPreviouslyLoadedDatabaseAsync()
        {
            // Read token in settings
            object value = String.Empty;
            bool success = ApplicationData.Current.LocalSettings.Values.TryGetValue(FUTURE_LIST_DATABASE_TOKEN_KEY, out value);
            if( success )
            {
                string token = (string)value;

                return await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Loads the given database file using the provided password
        /// </summary>
        /// <param name="databaseFile"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task LoadDatabaseAsync(StorageFile databaseFile, PasswordParameter password)
        {
            _databaseFile = databaseFile;

            IsLoading = true;

            try
            {
                // Open the database (may trigger exceptions)
                await openDatabaseAsync(databaseFile, password);

                DatabaseOpened = true;
                // And only if everything went fine, store the tokens and database names

                // Store the selected file token in the future access list to easily retrieve it later
                string token = StorageApplicationPermissions.FutureAccessList.Add(databaseFile);
                // Add or replace the token value for the last database file
                ApplicationData.Current.LocalSettings.Values[FUTURE_LIST_DATABASE_TOKEN_KEY] = token;
                ApplicationData.Current.LocalSettings.Values[LAST_DATABASE_NAME_KEY] = databaseFile.Name;

            }
            finally
            {
                IsLoading = false;
            }
        }



        /// <summary>
        /// Loads the previously opened database, if possible, using the provided password
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task LoadPreviouslyOpenedDatabase(PasswordParameter password)
        {
            // Read token in settings
            object value = String.Empty;
            bool success = ApplicationData.Current.LocalSettings.Values.TryGetValue(FUTURE_LIST_DATABASE_TOKEN_KEY, out value);
            if (success)
            {
                string token = (string)value;

                StorageFile file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
                if (file != null)
                {
                    await LoadDatabaseAsync(file, password);
                }
            }
            else
            {
                throw new FileNotFoundException("Database file not found");
            }
        }


        /// <summary>
        /// Find the entry referenced by the given uuid
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public PwEntryViewModel FindEntryByUid(string uuidString)
        {
            PwUuid uuid = new PwUuid(MemUtil.HexStringToByteArray(uuidString));

            PwEntryViewModel entry = null;

            foreach (var groupVM in Groups)
            {
                entry = groupVM.FindEntryInGroup(uuid);
                if( entry != null )
                {
                    break;
                }
            }

            return entry;
        }

        /// <summary>
        /// Clean any opened database and empty all data. Called when leaving to Welcome page
        /// </summary>
        public void Cleanup()
        {
            DatabaseOpened = false;
            Filter = String.Empty;
            _groups.Clear();
            _filteredGroupedEntries.Clear();
            _hierarchicalGroups.Clear();

        }

        /// <summary>
        /// Opens the given database using the provided password, and the optional keyfile
        /// </summary>
        /// <param name="databaseFile"></param>
        /// <param name="password"></param>
        /// <param name="keyFile"></param>
        /// <returns></returns>
        private async Task openDatabaseAsync(StorageFile databaseFile, PasswordParameter password)
        {
            try
            {
                using (var stream = await databaseFile.OpenReadAsync())
                {
                    // Build up the database 
                    var db = new PwDatabase();

                    var key = new CompositeKey();

                    if (!String.IsNullOrEmpty(password.Password))
                    {
                        // Add the password
                        key.AddUserKey(new KcpPassword(password.Password));
                    }

                    if (password.KeyFile != null)
                    {
                        var keyFileStream = await password.KeyFile.OpenReadAsync();
                        if (keyFileStream != null)
                        {
                            key.AddUserKey(new KcpKeyFile(keyFileStream.AsStreamForRead()));
                        }
                    }

                    var cancelToken = new CancellationTokenSource();

                    db.Open(stream.GetInputStreamAt(0).AsStreamForRead(),
                                    key,
                                    null,
                                    cancelToken.Token);

                    // Now iterate all groups entries to create the ViewModel counterpart

                    if (db.RootGroup != null)
                    {
                        if (SettingsViewModel.Instance.ViewCollectionAsTree)
                        {
                            _groups.Add(new PwGroupViewModel(db.RootGroup, true));
                        }
                        else
                        {
                            _groups.Add(new PwGroupViewModel(db.RootGroup));

                            foreach (var group in db.RootGroup.GetGroups(false))
                            {
                                _groups.Add(new PwGroupViewModel(group, true));
                            }
                        }
                    }

                    if( SettingsViewModel.Instance.ViewCollectionAsTree )
                    {
                        // Create the hierarchical group
                        foreach (var groupVM in _groups)
                        {
                            addHierachicalGroup(groupVM, 0);
                        }
                        if( _groups.Count > 0)
                        {
                            CurrentGroup = _groups.FirstOrDefault();
                        }
                    }

                    if( SettingsViewModel.Instance.SortingEnabled )
                    {
                        // Sort groups by name
                        _groups = new ObservableCollection<PwGroupViewModel>(_groups.OrderBy(g => g.Name));
                    }

                    // Initialize the filtered list instances
                    UpdateFilterededEntries(String.Empty);

                    db.Close();
                }
            }
            catch(InvalidCompositeKeyException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void addHierachicalGroup( PwGroupViewModel group, int hierarchicalLevel )
        {
            HierarchicalGroups.Add(group);
            group.HierachicalLevel = hierarchicalLevel;

            foreach (var groupVM in group.Groups)
            {
                addHierachicalGroup(groupVM, hierarchicalLevel+1);
            }
        }


    }
}
