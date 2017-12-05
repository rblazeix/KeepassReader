using KeePassW10.Common;
using KeePassW10.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class EntryCollectionTreeViewPage : Page
    {
        public DatabaseViewModel ViewModel { get { return this.DataContext as DatabaseViewModel; } }

        private bool _treeViewExpanded = true;

        public EntryCollectionTreeViewPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            TreeViewColumn.Width = new GridLength(_treeViewExpanded ?
                (double)this.Resources["TreePaneWidthExpanded"] :
                (double)this.Resources["TreePaneWidthReduced"]);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavigationService.SetPageTitle(ViewModel.LastDatabaseName);
            EntryCollection_SearchBox.Text = ViewModel.Filter;

        }

        private void EntryList_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Button b = (Button)sender;
            b.Flyout.ShowAt((FrameworkElement)sender);
        }


        private void MenuFlyoutItem_ShowDetails_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem m = (MenuFlyoutItem)sender;

            PwEntryViewModel vm = m.DataContext as PwEntryViewModel;

            ViewModel.CurrentEntry = vm;

            // Navigate to entry detail page
            this.Frame.Navigate(typeof(EntryDetailPage));
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if( args.Reason == AutoSuggestionBoxTextChangeReason.UserInput )
            {
                // Filter the entries according to user input
                ViewModel.UpdateFilterededEntries(sender.Text);
            }
        }

        private void SplitPane_Button_Click(object sender, RoutedEventArgs e)
        {
            _treeViewExpanded = !_treeViewExpanded;
            TreeViewColumn.Width = new GridLength(_treeViewExpanded ? 
                (double)this.Resources["TreePaneWidthExpanded"] :
                (double)this.Resources["TreePaneWidthReduced"]);

            ExpandIcon.Visibility = _treeViewExpanded ? Visibility.Collapsed : Visibility.Visible;
            ReduceIcon.Visibility = _treeViewExpanded ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
