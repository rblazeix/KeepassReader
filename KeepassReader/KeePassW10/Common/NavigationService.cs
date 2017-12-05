using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;

namespace KeePassW10.Common
{
    public class NavigationService
    {
        private static Frame _navigationFrame = null;
        private static TextBlock _pageTitleControl = null;


        public static void RegisterNavigationFrame(Frame frame, TextBlock titleControl = null)
        {
            _navigationFrame = frame;
            _pageTitleControl = titleControl;
        }

        public static void NavigateToPage(Type pageType, object parameter = null, bool clearStackAfterNavigation = false)
        {
            if (_navigationFrame == null)
            { throw new Exception("No navigation frame set"); }

            if (_navigationFrame.CurrentSourcePageType != pageType)
            {
                // Actually Request the navigation as a Task to avoid WP8.1 Navigate problem when performed without delay
                    _navigationFrame.Navigate(pageType, parameter);

                    if (clearStackAfterNavigation)
                    {
                        ClearNavigationStack();
                    }
                    

            }
        }

        public static void SetPageTitle(string title)
        {
            if( _pageTitleControl == null )
            {  return; }

            _pageTitleControl.Text = title;
        }

        public static void SetPageTitleFromResource(string resId)
        {
            if (_pageTitleControl == null)
            { return; }

            _pageTitleControl.Text = ResourceLoader.GetForCurrentView().GetString(resId);
        }

        public static void NavigateBackToRoot()
        {
            if (_navigationFrame == null)
            { throw new Exception("No navigation frame set"); }

            while (_navigationFrame.CanGoBack)
            {
                _navigationFrame.GoBack();
            }
        }

        public static void ClearNavigationStack()
        {
            if (_navigationFrame == null)
            { throw new Exception("No navigation frame set"); }

            _navigationFrame.BackStack.Clear();
        }

    }
}
