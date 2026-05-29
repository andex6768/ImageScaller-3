using Avalonia;
using System;
using System.IO;

namespace ImageScaller3;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            // Если приложение упало при старте — записываем всё в файл error.txt
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
            File.WriteAllText(logPath, $"Критическая ошибка АОТ:\n{ex.Message}\n\nСтек вызовов:\n{ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                File.AppendAllText(logPath, $"\n\nВнутренняя ошибка:\n{ex.InnerException.Message}");
            }
        }
    }
      
      

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
