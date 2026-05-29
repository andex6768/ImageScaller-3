using Avalonia.Controls;

namespace ImageScaller3.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
       

        
        DataContext = new ViewModels.MainWindowViewModel();
    
 
       


    }

    private void Grid_LayoutUpdated(object? sender, System.EventArgs e)
    {
    }

    private void NumericOnlyBox_OnTextInput(object? sender, Avalonia.Input.TextInputEventArgs e)
    {

    // Если вводимый символ НЕ цифра — помечаем событие как обработанное (Handled)
    // Это предотвращает появление символа в поле
    if (!char.IsDigit(e.Text![0]))
    {
        e.Handled = true;
    }

    }
}