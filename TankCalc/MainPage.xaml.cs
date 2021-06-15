using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.ViewManagement;
using Windows.Graphics.Display;
using Xamarin.Forms;
using Page = Windows.UI.Xaml.Controls.Page;
using Size = Windows.Foundation.Size;


// Die Elementvorlage "NavigationView" wird unter https://docs.microsoft.com/de-de/windows/uwp/design/controls-and-patterns/navigationview dokumentiert.

namespace TankCalc
{
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
        }


        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(Views.Durchschnittsverbrauch));
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Maximized;
        }



        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            NavigationViewItem item = args.SelectedItem as NavigationViewItem;

            switch (item.Tag.ToString())
            {
                case "dverbrauch":
                    ContentFrame.Navigate(typeof(Views.Durchschnittsverbrauch));
                    break;
                case "fahrtkosten":
                    ContentFrame.Navigate(typeof(Views.Fahrtkosten));
                    break;
            }
        }
    }
}
