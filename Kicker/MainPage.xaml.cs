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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.WindowManagement;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Animation;
using System.Drawing;
using Microsoft.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kicker
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel vm;

        private TableEntry? _SelectedTable;
        private TableEntry? SelectedTable
        {
            get
            {
                return _SelectedTable;
            }
            
            set
            {
                _SelectedTable = value;
                vm.StartButtonEnabled = value != null;
            }
        }

        public MainPage() : this((Application.Current as App)?.MainViewModel!)
        {

        }
        public MainPage(MainViewModel vm)
        {
            this.InitializeComponent();
            this.vm = vm;
            this.Loaded += (sender, e) =>
            {
                Frame.Background = new SolidColorBrush(Colors.Black);
            };
        }

        private void BasicGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SelectedTable = e.ClickedItem as TableEntry;
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTable != null)
            {
                var window = (Application.Current as App)?.Window as MainWindow;
                if (window != null)
                {
                    Microsoft.UI.Windowing.AppWindow appWindow = window.AppWindow;
                    if (appWindow != null)
                    {
                        appWindow.Hide();

                        TableLauncher.Instance.LaunchTable(SelectedTable.Path);
                        appWindow.Show();
                    }
                }
            }
            else
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "No table selected.";
                dialog.PrimaryButtonText = "OK";
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = "Please select a table first.";
                dialog.XamlRoot = this.XamlRoot;

                _ = dialog.ShowAsync();
            }
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            var window = (Application.Current as App)?.Window as MainWindow;
            if (window != null)
            {
                Microsoft.UI.Windowing.AppWindow appWindow = window.AppWindow;
                if (appWindow != null)
                {
                    appWindow.Hide();

                    Random random = new();
                    var rnd = random.Next(0, vm.Tables.Count - 1);

                    TableLauncher.Instance.LaunchTable(vm.Tables[rnd].Path);

                    appWindow.Show();
                }
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(
                typeof(SettingsPage),
                null,
                new SlideNavigationTransitionInfo()
                { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            switch (((MenuFlyoutItem)sender).Tag)
            {
                case "SortByName":
                    vm.Sort(SortType.Name);
                    break;
                case "SortByYear":
                    vm.Sort(SortType.Year);
                    break;
                case "SortByManufacturer":
                    vm.Sort(SortType.Manufacturer);
                    break;
            }
        }
    }
}
