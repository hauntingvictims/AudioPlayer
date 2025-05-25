using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace AudioPlayer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnTrackDoubleTapped(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.DataContext is string trackPath)
            {
                var viewModel = DataContext as ViewModels.MainWindowViewModel;
                viewModel?.PlayTrackCommand.Execute(trackPath).Subscribe();
            }
        }
    }
}