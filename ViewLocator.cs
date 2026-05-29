using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ImageScaller3.ViewModels;
using ImageScaller3.Views;

namespace ImageScaller3;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null) return null;

        // Явно связываем каждую ViewModel с её View (окном или UserControl)
        // Компилятор видит эти связи напрямую и ничего не удалит!
        return data switch
        {
            MainWindowViewModel => new MainWindow(),
            ProgressBarViewModel => new ProgressBarWindow(), // Добавьте сюда ваши ViewModel -> View
            
            _ => new TextBlock { Text = $"Not Found: {data.GetType().Name}" }
        };
    }

    public bool Match(object? data)
    {
        // Проверяем, является ли переданный объект нашей ViewModel
        return data is ViewModelBase; 
    }
}