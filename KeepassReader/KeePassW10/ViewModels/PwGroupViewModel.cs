using KeePassLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeePassW10.ViewModels
{
    /// <summary>
    /// Represents a group of password entries
    /// </summary>
    public class PwGroupViewModel : ViewModelBase
    {
        public PwGroup Model { get; private set; }

        private List<PwEntryViewModel> _entries = new List<PwEntryViewModel>();
        public List<PwEntryViewModel> Entries
        {
            get
            {
                return _entries;
            }
        }

        private ObservableCollection<PwEntryViewModel> _filteredEntries = new ObservableCollection<PwEntryViewModel>();
        public ObservableCollection<PwEntryViewModel> FilteredEntries
        {
            get
            {
                return _filteredEntries;
            }
        }

        private List<PwGroupViewModel> _groups = new List<PwGroupViewModel>();
        public List<PwGroupViewModel> Groups
        {
            get
            {
                return _groups;
            }
        }

        private int _hierarchicalLevel = 0;
        public int HierachicalLevel
        {
            get
            {
                return _hierarchicalLevel;
            }
            set
            {
                _hierarchicalLevel = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("Name");
            }
        }

        public bool FilteredEntriesEmpty
        {
            get
            {
                return FilteredEntries.Count == 0;
            }
        }

        public string Name
        {
            get
            {
                if (HierachicalLevel == 0)
                {
                    return Model.Name;
                }
                else
                {
                    // Version with tree character on first column
                    //string name = "└─";
                    //for ( int i=2; i <= HierachicalLevel; i++)
                    //{
                    //    name += "─";
                    //}
                    //name += " " + Model.Name;
                    //return name;

                    string name = "";
                    // Version with space before tree character
                    for (int i = 0; i <= HierachicalLevel - 2; i++)
                    {
                        //name += "∙ ";
                        name += "  ";
                    }
                    name += "└  " + Model.Name;
                    return name;
                }

            }
        }

        public PwGroupViewModel(PwGroup model, bool readSubgroups = false)
        {
            HierachicalLevel = 0; // default hierachical value

            Model = model;

            // Add the group entries themselves
            AddGroupEntries(Model);

            // Then flatten all the subgroup entries if asked
            // Flatten the subgroup entries if there are subgroups
            if (readSubgroups)
            {
                //foreach (var subgroup in Model.GetGroups(true))
                //{
                //    AddGroupEntries(subgroup);
                //}

                if (SettingsViewModel.Instance.ViewCollectionAsTree)
                {
                    loadGroupHierarchical(model);
                }
                else
                {
                    loadGroupFlattened(model);
                }
            }
        }

        public void UpdateFilteredEntries(string filter)
        {
            if (String.IsNullOrEmpty(filter))
            {
                // Filter entries
                FilteredEntries.Clear();
                foreach (var item in Entries)
                {
                    FilteredEntries.Add(item);
                }
            }
            else
            {
                string lowcaseFilter = filter.ToLower();
                // Take all elements
                FilteredEntries.Clear();

                foreach (var item in Entries.Where(e => e.Title.ToLower().Contains(lowcaseFilter)))
                {
                    FilteredEntries.Add(item);
                }

            }

            NotifyPropertyChanged("FilteredEntriesEmpty");
        }


        public PwEntryViewModel FindEntryInGroup(PwUuid uuid)
        {
            var entry = Entries.FirstOrDefault(e => e.Model.Uuid.Equals(uuid));
            
            if ( entry == null )
            {
                foreach (var group in Groups)
                {
                    entry = group.FindEntryInGroup(uuid);
                    if( entry != null )
                    {
                        break;
                    }
                }
            }
            return entry;
        }

        /// <summary>
        /// Load the group by flattening all subgroup entries into this group
        /// </summary>
        /// <param name="model"></param>
        private void loadGroupFlattened(PwGroup model)
        {
            foreach (var subgroup in Model.GetGroups(true))
            {
                AddGroupEntries(subgroup);
            }
        }


        private void loadGroupHierarchical(PwGroup model)
        {
            foreach (var subgroup in Model.GetGroups(false))
            {
                _groups.Add(new PwGroupViewModel(subgroup, true));
            }
        }

        private void AddGroupEntries(PwGroup group)
        {
            // Build the entries ViewModels
            if (group.GetEntriesCount(false) > 0)
            {
                foreach (var item in group.GetEntries(false))
                {
                    _entries.Add(new PwEntryViewModel(item));
                }

                if (SettingsViewModel.Instance.SortingEnabled)
                {
                    _entries = _entries.OrderBy(e => e.Title).ToList();
                }
            }
        }




    }
}
