using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Animation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kicker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPageViewModel vm;

        public SettingsPage() : this((Application.Current as App)?.SettingsPageViewModel!)
        {
        }
        public SettingsPage(SettingsPageViewModel vm)
        {
            this.InitializeComponent();
            this.vm = vm;
        }

        private void RescanButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPageViewModel.Rescan();
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPageViewModel.LaunchVP();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(
                typeof(MainPage),
                null,
                new SlideNavigationTransitionInfo()
                { Effect = SlideNavigationTransitionEffect.FromLeft }
            );
        }
    }
}
