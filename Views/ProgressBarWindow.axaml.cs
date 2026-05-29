using Avalonia.Controls;
using ImageScaller3.ViewModels;

namespace ImageScaller3.Views;

public partial class ProgressBarWindow : Window
{
    public ProgressBarWindow()
    {
        InitializeComponent();
    }

    // Вспомогательное свойство для быстрого доступа к ViewModel
    public ProgressBarViewModel? ViewModel => DataContext as ProgressBarViewModel;
}