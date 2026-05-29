using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace ImageScaller3.ViewModels;

public class WindowStartManager
{
   public static Views.ProgressBarWindow  StartProgressBarWindow(ProgressBarViewModel progressVM)
    {
        // 1. Создаем и отображаем окно прогресса
        var progressWindow = new Views.ProgressBarWindow
        {
            DataContext = progressVM
        };
        {

            // Находим главное окно приложения, чтобы сделать окно прогресса зависимым (Owner)
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Show() открывает окно, не блокируя код. Открываем его поверх главного.
                if (desktop.MainWindow != null)
                {
                    progressWindow.Show(desktop.MainWindow);
                }
            }

         
        }
        return progressWindow;
    }
}