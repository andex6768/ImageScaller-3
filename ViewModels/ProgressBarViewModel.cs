using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using System;
using Avalonia.Threading;
using Avalonia.Controls.Documents;
namespace ImageScaller3.ViewModels;

public partial class ProgressBarViewModel : ViewModelBase
{

    [ObservableProperty]
    private ulong currentProgress = 0;
    [ObservableProperty]
    private ulong maxProgress;
    [ObservableProperty]
    private string? progressText;
    [ObservableProperty]
    private string progressBarName = "Загрузка";

    private bool _isLoading = true;

    // Событие, на которое можно подписаться. Передает bool.
    public event Action<bool>? OnLoadingChanged;

    public bool isLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnLoadingChanged?.Invoke(_isLoading); // Вызываем событие при изменении значения
            }
        }
    }



    void UpdateProgress(ulong progress, string? text = null)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (text != null)
            {
                ProgressText = text;
            }


            CurrentProgress = progress * 100 / MaxProgress;
            if (CurrentProgress >= 100)
            {
                EndProgress();
            }
        });


    }
    void EndProgress()
    {

    }



    private Task InitializeProgressBarAsync(IProgressBarUsable progressBar, string actionName)
    {
        return Task.Run(() =>
         {

             progressBar.CalculateMaxProgress(actionName);
             MaxProgress = progressBar.MaxProgress;
             if (progressBar.ProgressBarName != string.Empty)
             {
                 ProgressBarName = progressBar.ProgressBarName;
             }
             progressBar.OnProgressUpdated = UpdateProgress;
         });

    }

    void SetLoadingState(ref bool loadingState)
    {
        loadingState = isLoading;
    }

    public async Task<TResult> CreateProgressBar<TResult>(IProgressBarUsable progressBar, Func<Task<TResult>> func)
    {
        await InitializeProgressBarAsync(progressBar, func.Method.Name);
        isLoading = true;

        TResult result = await func();
        isLoading = false;
        return result;
    }

    public async Task<TResult> CreateProgressBar<TResult, T1>(IProgressBarUsable progressBar, Func<T1, Task<TResult>> func, T1 arg1)
    {
        await InitializeProgressBarAsync(progressBar, func.Method.Name);
        isLoading = true;
        TResult result = await func(arg1);
        isLoading = false;
        return result;
    }

    public async Task<TResult> CreateProgressBar<TResult, T1, T2>(IProgressBarUsable progressBar, Func<T1, T2, Task<TResult>> func, T1 arg1, T2 arg2)
    {
        await InitializeProgressBarAsync(progressBar, func.Method.Name);
        isLoading = true;
        TResult result = await func(arg1, arg2);
        isLoading = false;
        return result;
    }
    public async Task<TResult> CreateProgressBar<TResult, T1, T2, T3>(IProgressBarUsable progressBar,
    Func<T1, T2, T3, Task<TResult>> func, T1 arg1, T2 arg2, T3 arg3)
    {
        await InitializeProgressBarAsync(progressBar, func.Method.Name);
        isLoading = true;
        TResult result = await func(arg1, arg2, arg3);
        isLoading = false;
        return result;
    }
}