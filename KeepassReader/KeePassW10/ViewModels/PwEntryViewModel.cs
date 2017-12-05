using KeePassLib;
using KeePassW10.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using System.Windows.Input;
using KeePassLib.Utility;
using KeePassW10.Services;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Core;
using Windows.UI.Popups;
using System.Text.RegularExpressions;

namespace KeePassW10.ViewModels
{
    /// <summary>
    /// Represents an password entry (login, password and other metadata)
    /// </summary>
    public class PwEntryViewModel : ViewModelBase
    {
        private static Regex refRegexp = new Regex(@"^{REF:[UP]@I:([A-Z0-9]+)}$");

        /// <summary>
        /// List of fields names that are aleeady handled and are not to be taken as custom fields
        /// </summary>
        private static List<string> CustomFieldsExcludedValues = new List<string>() { "Title",  "UserName", "Password", "URL", "Notes"};

        #region Public properties

        public PwEntry Model { get; private set; }


        public string Title
        {
            get
            {
                if (Model.Strings.Exists("Title"))
                {
                    return Model.Strings.Get("Title").ReadString();
                }
                return String.Empty;
            }
        }

        public string Username
        {
            get
            {
                if (Model.Strings.Exists("UserName"))
                {
                    return Model.Strings.Get("UserName").ReadString();
                }
                return String.Empty;
            }
        }

        public string Password
        {
            get
            {
                if (Model.Strings.Exists("Password"))
                {
                    return Model.Strings.Get("Password").ReadString();
                }
                return String.Empty;
            }
        }

        public bool HasUrl
        {
            get
            {
                return !String.IsNullOrEmpty(Url);
            }
        }

        public string Url
        {
            get
            {
                if (Model.Strings.Exists("URL"))
                {
                    return Model.Strings.Get("URL").ReadString();
                }
                return String.Empty;
            }
        }

        public string LastModificationDate
        {
            get
            {
                return TimeUtil.ToDisplayString(Model.LastModificationTime);
            }
        }

        public bool HasNotes
        {
            get
            {
                return !String.IsNullOrEmpty(Notes);
            }
        }

        public string Notes
        {
            get
            {
                if (Model.Strings.Exists("Notes"))
                {
                    return Model.Strings.Get("Notes").ReadString();
                }
                return String.Empty;
            }
        }

        public string Icon
        {
            get
            {
                return IconManager.GetIconPath(Model.IconId);
            }
        }


        public bool HasCustomFields
        {
            get
            {
                return _customFields.Count > 0;
            }
        }

        private List<KeyValuePair<string, string>> _customFields = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> CustomFields
        {
            get
            {
                return _customFields;
            }
        }

        private bool _showPassword = false;
        public bool ShowPassword
        {
            get
            {
                return _showPassword;
            }
            set
            {
                _showPassword = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("PasswordTextIfShown");

            }
        }
        // Specific handling for the password hiding mechanism
        public string PasswordTextIfShown
        {
            get
            {
                if( ShowPassword)
                {
                    return Password;
                }
                else
                {
                    // Return a number of bullets corresponding to the password length
                    string bulletsPassword = String.Empty;
                    foreach (char c in Password)
                    {
                        bulletsPassword += ((char)9679);
                    }
                    return bulletsPassword;
                }
            }
        }

        #endregion


        public ICommand CopyUsernameCommand { get; private set; }
        public ICommand CopyPasswordCommand { get; private set; }
        public ICommand CopyUrlCommand { get; private set; }

        public PwEntryViewModel( PwEntry model)
        {
            Model = model;

            // Check custom fields in the strings model
            foreach (var field in Model.Strings)
            {
                if (!CustomFieldsExcludedValues.Contains(field.Key))
                {
                    // This fields is a custom field
                    CustomFields.Add(new KeyValuePair<string, string>( field.Key, field.Value.ReadString()));
                }
            }

            CopyUsernameCommand = new RelayCommand(CopyUsernameAction);
            CopyPasswordCommand = new RelayCommand(CopyPasswordAction);
            CopyUrlCommand = new RelayCommand(CopyUrlAction);
        }


        private void CopyUsernameAction()
        {
            string name = Username;

            // Handle reference to another entry
            if (refRegexp.IsMatch(name))
            {
                string refId = refRegexp.Match(name).Groups[1].Value;

                var entry = DatabaseViewModel.Instance.FindEntryByUid(refId);
                if( entry != null )
                {
                    name = entry.Username;
                }
            }
            // handle this case
            ClipboardViewModel.Instance.CopyUserNameToClipboard(name);
        }

        private void CopyPasswordAction()
        {
            string value = Password;

            // Handle reference to another entry
            if (refRegexp.IsMatch(value))
            {
                string refId = refRegexp.Match(value).Groups[1].Value;

                var entry = DatabaseViewModel.Instance.FindEntryByUid(refId);
                if (entry != null)
                {
                    value = entry.Password;
                }
            }

            ClipboardViewModel.Instance.CopyPasswordToClipboard(value);
        }

        private void CopyUrlAction()
        {
            ClipboardViewModel.Instance.CopyPasswordToClipboard(Url);
        }




    }
}
