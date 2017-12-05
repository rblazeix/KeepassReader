using KeePassW10.Common;
using KeePassW10.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
    /// Data to represent an item in the nav menu.
    /// </summary>
    public class NavMenuItem
    {
        public string Label { get; set; }
        public Symbol Symbol { get; set; }
        public char SymbolAsChar
        {
            get
            {
                return (char)this.Symbol;
            }
        }

        public Action ClickAction { get; set; }

        public ICommand ItemSelectedCommand { get; private set; }

        public NavMenuItem()
        {
            ItemSelectedCommand = new RelayCommand(() =>
           {
               ClickAction.Invoke();

           });
        }

    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RootPage : Page
    {

        private List<NavMenuItem> _menuItems = new List<NavMenuItem>();
        public List<NavMenuItem> MenuItems
        {
            get
            {
                return _menuItems;
            }
        }

        public bool CloseButtonAvailable
        {
            get
            {
                return ContentFrame.CanGoBack;
            }
        }


        public RootPage()
        {
            this.InitializeComponent();

            NavigationService.RegisterNavigationFrame(ContentFrame, PageTitle);

            buildDrawerMenu();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Back button management
            ContentFrame.Navigated += RootFrame_Navigated;
            // Register a handler for BackRequested events and set the
            // visibility of the Back button
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            // HAndle back button showing
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ContentFrame.CanGoBack ?
                    AppViewBackButtonVisibility.Visible :
                    AppViewBackButtonVisibility.Collapsed;

            ContentFrame.Navigate(typeof(WelcomePage));
        }

        private void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Each time a navigation event occurs, update the Back button's visibility
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame)sender).CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
        }


        private void buildDrawerMenu()
        {
            // About
            _menuItems.Add(new NavMenuItem()
            {
                Symbol = Symbol.PreviewLink,
                Label = ResourceLoader.GetForCurrentView().GetString("DrawerMenu_About"),
                ClickAction = new Action(() =>
                   {
                       NavigationService.NavigateToPage(typeof(AboutPage));
                       CloseDrawerAfterClick();
                   })
            });

            // What's new
            _menuItems.Add(new NavMenuItem()
            {
                Symbol = Symbol.Message,
                Label = ResourceLoader.GetForCurrentView().GetString("DrawerMenu_NewFeatures"),
                ClickAction = new Action( async () =>
                {
                    CloseDrawerAfterClick();
                    // Show a dialog with the new features of this version
                    await new MessageDialog(
                        ResourceLoader.GetForCurrentView().GetString("NewFeatures_Dialog_Message"),
                        ResourceLoader.GetForCurrentView().GetString("NewFeatures_Dialog_Title")
                        ).ShowAsync();
                    
                })
            });

            // Contact
            _menuItems.Add(new NavMenuItem()
            {
                Symbol = Symbol.Mail,
                Label = ResourceLoader.GetForCurrentView().GetString("DrawerMenu_Contact"),
                ClickAction = new Action(async () =>
               {
                   var mailto = new Uri("mailto:rb.dev.win@outlook.com");
                   if (mailto != null)
                   {
                       await Windows.System.Launcher.LaunchUriAsync(mailto);
                   }
                   CloseDrawerAfterClick();
               })
            });

            // Rate app
            _menuItems.Add(new NavMenuItem()
            {
                Symbol = Symbol.Comment,
                Label = ResourceLoader.GetForCurrentView().GetString("DrawerMenu_Rate"),
                ClickAction = new Action(async () =>
                {
                    await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9nblggh4s820"));
                    CloseDrawerAfterClick();
                })
            });

            // Settings
            _menuItems.Add(new NavMenuItem()
            {
                Symbol = Symbol.Setting,
                Label = ResourceLoader.GetForCurrentView().GetString("DrawerMenu_Settings"),
                ClickAction = new Action(() =>
                {
                    NavigationService.NavigateToPage(typeof(SettingsPage));
                    CloseDrawerAfterClick();
                })
            });

            // Lock database
            _menuItems.Add(new NavMenuItem()
            {
                Symbol = Symbol.Permissions,
                Label = ResourceLoader.GetForCurrentView().GetString("DrawerMenu_LockDatabase"),
                ClickAction = new Action(() =>
                {
                    LockDatabaseButton_Click(this, null);
                    CloseDrawerAfterClick();
                })
            });

        }

        private void CloseDrawerAfterClick()
        {
            // Close Pane depending only when in overlay mode
            if (RootSplitView.IsPaneOpen &&
                (RootSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
                 )
            {
                RootSplitView.IsPaneOpen = false;
            }
        }


        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (ContentFrame.CanGoBack)
            {
                e.Handled = true;
                ContentFrame.GoBack();
            }
        }



        private void LockDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            DatabaseViewModel.Instance.Cleanup();

            NavigationService.NavigateBackToRoot();
        }


        #region Drawer menu handling

        private void DrawerMenuButton_Click(object sender, RoutedEventArgs e)
        {
            RootSplitView.IsPaneOpen = !RootSplitView.IsPaneOpen;
        }

        #endregion


    }
}
