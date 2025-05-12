using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Kicker
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel vm;

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
        
            FileScanner.Instance.ScanCompleted += (sender, tables) =>
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    vm.Tables = tables;
                    vm.StartButtonEnabled = TablesGridView.SelectedItem != null;
                });
            };
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (TablesGridView.SelectedItem != null)
            {
                if (TablesGridView.SelectedItem is TableEntry table)
                {
                    Launch(table);
                }
            }
            else
            {
                ContentDialog dialog = new()
                {
                    Title = "No table selected.",
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary,
                    Content = "Please select a table first.",
                    XamlRoot = this.XamlRoot
                };

                _ = dialog.ShowAsync();
            }
        }

        private void TablesGridView_ProcessKeyboardAccelerators(UIElement sender, ProcessKeyboardAcceleratorEventArgs args)
        {
            if (args.Key == Windows.System.VirtualKey.Enter && TablesGridView.SelectedItem != null)
            {
                args.Handled = true;
                if (TablesGridView.SelectedItem is TableEntry table)
                {
                    Launch(table);
                }
            }
        }

        private void Table_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (TablesGridView.SelectedItem != null)
            {
                if (TablesGridView.SelectedItem is TableEntry table)
                {
                    Launch(table, 1000);
                }
            }
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            var window = (Application.Current as App)?.Window as MainWindow;
            if (window != null)
            {
                Random random = new();
                var rnd = random.Next(0, vm.Tables.Count - 1);
                Launch(vm.Tables[rnd]);
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

        private void Page_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Get the element that was clicked
            var clickedElement = e.OriginalSource as FrameworkElement;

            // Check if the clicked element is not part of the GridView
            if (clickedElement != null && !IsElementInGridView(clickedElement))
            {
                TablesGridView.SelectedItem = null; // Clear the selection
            }
        }

        private bool IsElementInGridView(FrameworkElement? element)
        {
            // Traverse the visual tree to check if the element is within the GridView
            while (element != null)
            {
                if (element == TablesGridView)
                {
                    return true;
                }
                element = VisualTreeHelper.GetParent(element) as FrameworkElement;
            }
            return false;
        }

        private static void Launch(TableEntry table, int sleep = 0)
        {
            var window = (Application.Current as App)?.Window as MainWindow;
            if (window != null)
            {
                Microsoft.UI.Windowing.AppWindow appWindow = window.AppWindow;
                if (appWindow != null)
                {
                    if (sleep > 0)
                    {
                        Thread.Sleep(sleep);
                    }
                    appWindow.Hide();
                    TableLauncher.Instance.LaunchTable(table.Path);
                    appWindow.Show();
                }
            }
        }

        private void TablesGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.StartButtonEnabled = TablesGridView.SelectedItem != null;
        }
    }
}
