using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Platform.Storage;
using ImageMagick;
using Avalonia.Platform;
using System;

namespace ImageScaller3.ViewModels;

using Bitmap = Avalonia.Media.Imaging.Bitmap;

public partial class MainWindowViewModel : ViewModelBase
{
    #region Data
    private string imagesPath = "Путь к папке с картинками не выбран";

    private bool IsImagePathNull { get => imagesPath == "Путь к папке с картинками не выбран" || imagesPath == string.Empty; }

    public string ImagesPath
    {
        get
        {
            return imagesPath;
        }
        private set
        {
            if (value == string.Empty)
            {
                value = "Путь к папке с картинками не выбран";
            }
            imagesPath = value;
            OnPropertyChanged(nameof(ImagesPath));
        }
    }

    public bool IsCustomScaleSettings
    {
        get
        {
            if (IsExtended == false)
            {
                return false;
            }
            return SelectedScaleToImageOption.Type == ScaleToImage.Custom;
        }
    }

    public bool ShowInterpolationSettings
    {
        get
        {
            if (!IsExtended)
            {
                return false;
            }
            else if (IsDontScaleOption)
            {
                return false;
            }
            return true;
        }
    }
    public bool IsDontScaleOption
    {
        get
        {
            return SelectedScaleToImageOption.Type == ScaleToImage.DontScale;
        }
    }

    #region AtlasState
    private bool isAtlasSaving = false;

    public bool IsAtlasSaving
    {
        get
        {
            return isAtlasSaving;
        }
        private set
        {
            isAtlasSaving = value;
            OnPropertyChanged(nameof(IsAtlasSaving));
            OnPropertyChanged(nameof(IsAtlasReadyToGenerate));
        }

    }
    private bool isAtlasChanged = false;

    private bool isAtlasReadyToGenerate = false;

    public bool IsAtlasReadyToGenerate
    {
        get
        {
            isAtlasReadyToGenerate = IsImagesLoaded && !isAtlasSaving;
            return isAtlasReadyToGenerate;

        }
    }

    public bool IsAtlasReadyToSave
    {
        get
        {
            return isAtlasChanged;
        }
    }
    #endregion

    private bool isImagesLoaded = false;

    public bool IsImagesLoaded
    {
        get
        {
            return isImagesLoaded;
        }
        private set
        {
            isImagesLoaded = value;
            OnPropertyChanged(nameof(IsImagesLoaded));
            OnPropertyChanged(nameof(IsAtlasReadyToGenerate));
        }

    }

    [ObservableProperty]
    private DescriptionInfo<FilterType> selectedFilter = FilterOptions[0];


    private uint? customImageSizeHorizontal = 2;

    public uint? CustomImageSizeHorizontal
    {
        get
        {
            if (IsExtended == false)
            {
                customImageSizeHorizontal = 2;

            }
            return customImageSizeHorizontal;
        }
        set
        {
            customImageSizeHorizontal = SetNumberWithRules(value);
            OnPropertyChanged(nameof(CustomImageSizeHorizontal));

        }
    }

    private uint? customImageSizeVertical = 2;

    public uint? CustomImageSizeVertical
    {
        get
        {
            if (IsExtended == false)
            {
                customImageSizeVertical = 2;

            }
            return customImageSizeVertical;

        }
        set
        {
            customImageSizeVertical = SetNumberWithRules(value);
            OnPropertyChanged(nameof(CustomImageSizeVertical));
        }
    }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowInterpolationSettings))]
    [NotifyPropertyChangedFor(nameof(IsCustomScaleSettings))]
    private DescriptionInfo<ScaleToImage> selectedScaleToImageOption = ScaleToImageOptions[0];



    public static List<DescriptionInfo<ScaleToImage>> ScaleToImageOptions { get; } = new List<DescriptionInfo<ScaleToImage>>
        {
            new DescriptionInfo<ScaleToImage> { Type = ScaleToImage.DontScale, Name = "Без масштабирования", },
            new DescriptionInfo<ScaleToImage> { Type = ScaleToImage.ScaleToBigger, Name = "Самой большой картинке в папке", },
            new DescriptionInfo<ScaleToImage> { Type = ScaleToImage.ScaleToSmaller, Name = "Самой маленькой картинке в папке", },
            new DescriptionInfo<ScaleToImage> { Type = ScaleToImage.Custom, Name = "Пользовательский масштаб",  }
        };


    // Заполняем список популярными фильтрами
    public static List<DescriptionInfo<FilterType>> FilterOptions { get; } = new List<DescriptionInfo<FilterType>>
        {
            new DescriptionInfo<FilterType> {
                Type = FilterType.Lanczos,
                Name = "Lanczos (Высокое качество)",
                Description = "Лучший выбор для уменьшения картинок и текстур. Сохраняет высокую резкость и мелкие детали, но работает чуть медленнее остальных."
            },
            new DescriptionInfo<FilterType>  {
                Type = FilterType.Mitchell, // В Magick.NET он называется Mitchel с одной 'l'
                Name = "Mitchell (Сглаженный)",
                Description = "Идеален для фотографий и плавных градиентов. Дает хороший баланс между резкостью и сглаживанием без появления резких 'ступенек'."
            },
            new DescriptionInfo<FilterType> {
                Type = FilterType.Triangle,
                Name = "Bilinear / Triangle (Быстрый)",
                Description = "Простая линейная интерполяция. Работает очень быстро, но результат может выглядеть слегка размытым. Подходит для черновых набросков."
            },
            new DescriptionInfo<FilterType> {
                Type = FilterType.Point,
                Name = "Nearest Neighbor / Point (Пиксельный)",
                Description = "Попиксельное масштабирование без сглаживания. Идеально для Pixel-Art, так как сохраняет жесткие границы пикселей и не размывает их."
            }
        };



    public static List<MagickImage> images { get; private set; } = new List<MagickImage>();

    private Bitmap atlas = new Bitmap(AssetLoader.Open(new Uri("avares://ImageScaller3/Assets/NoAtlas.png")));
    
    public Bitmap Atlas
    {
        get => atlas;
        private set
        {
            atlas = value;
            isAtlasChanged = true;
            OnPropertyChanged(nameof(Atlas));
            OnPropertyChanged(nameof(IsAtlasReadyToSave));
        }
    }

    public uint? spacingWithImagesHorizontal = 0;
    public uint? SpacingWithImagesHorizontal
    {
        get => spacingWithImagesHorizontal;
        set
        {
            spacingWithImagesHorizontal = SetNumberWithRules(value);
            OnPropertyChanged(nameof(SpacingWithImagesHorizontal));
        }
    }

    public uint? spacingWithImagesVertical = 0;
    public uint? SpacingWithImagesVertical
    {
        get => spacingWithImagesVertical;
        set
        {
            spacingWithImagesVertical = SetNumberWithRules(value);
            OnPropertyChanged(nameof(SpacingWithImagesVertical));
        }
    }
    public uint? imagesInRow = 3;
    public uint? ImagesInRow
    {
        get => imagesInRow;
        set
        {
            imagesInRow = SetNumberWithRules(value, min: 1);
            OnPropertyChanged(nameof(ImagesInRow));
        }
    }
    private bool isExtended = false;

    public bool IsExtended
    {
        get => isExtended;
        set
        {
            isExtended = value;
            OnPropertyChanged(nameof(IsExtended));
            OnPropertyChanged(nameof(ShowInterpolationSettings));
            OnPropertyChanged(nameof(IsCustomScaleSettings));
        }
    }

    #endregion

    #region Functions



    [RelayCommand]
    private async Task LoadImagesFromDisk(Window ownerWindow)
    {
        ImagesPath = await OpenFolderDialog(ownerWindow);
        if (!IsImagePathNull)
        {
            var loader = new ImageFileLoader(ImagesPath);
            var progressBar = new ProgressBarViewModel();
            var window = WindowStartManager.StartProgressBarWindow(progressBar);

            progressBar.OnLoadingChanged += (newValue) => IsImagesLoaded = !newValue;

            var loadedImage = await progressBar.CreateProgressBar(loader, loader.LoadImagesFromDiskAsync);

            if (loadedImage != null)
            {
                images.AddRange(loadedImage);
            }
            window.Close();
            progressBar.OnLoadingChanged -= (newValue) => IsImagesLoaded = !newValue;
        }
        else
        {
            ImagesPath = string.Empty;
        }


    }

    private async Task<string> OpenFolderDialog(Window ownerWindow)
    {
        string selectedPath = string.Empty;
        if (ownerWindow == null) return string.Empty;

        // 1. Настраиваем параметры диалогового окна
        var options = new FolderPickerOpenOptions
        {
            Title = "Выберите папку с картинками для атласа",
            AllowMultiple = false // Нам нужна только одна папка
        };

        // 2. Открываем окно выбора папки
        var result = await ownerWindow.StorageProvider.OpenFolderPickerAsync(options);

        // 3. Проверяем, выбрал ли пользователь что-то или просто закрыл окно
        if (result != null && result.Count > 0)
        {
            // Получаем объект папки
            IStorageFolder folder = result[0];

            // Переводим его в привычный локальный путь (string)
            string localPath = folder.Path.LocalPath;

            // Сохраняем путь во ViewModel
            selectedPath = localPath;
        }
        return selectedPath;
    }

    [RelayCommand]
    private async Task GenerateAtlas()
    {
#pragma warning disable CS8629 // Тип значения, допускающего NULL, может быть NULL.
        if (IsCustomScaleSettings)
        {
            var temp = await PhotoGenerator.GenerateAtlasAsync(images, (uint)ImagesInRow, (uint)SpacingWithImagesHorizontal, (uint)SpacingWithImagesVertical, SelectedFilter.Type, SelectedScaleToImageOption.Type, customImageSizeHorizontal, customImageSizeVertical);
            Atlas = await PhotoCoverter.ConvertMagickImageToBitmapAsync(temp);
        }
        else
        {
            var temp = await PhotoGenerator.GenerateAtlasAsync(images, (uint)ImagesInRow, (uint)SpacingWithImagesHorizontal, (uint)SpacingWithImagesVertical, SelectedFilter.Type, SelectedScaleToImageOption.Type);
            Atlas = await PhotoCoverter.ConvertMagickImageToBitmapAsync(temp);
        }

#pragma warning restore CS8629 // Тип значения, допускающего NULL, может быть NULL.
    }

    [RelayCommand]
    private async Task SaveAtlas(Window ownerWindow)
    {
        if (Atlas == null) return;

        // 1. Настраиваем параметры диалогового окна сохранения
        var options = new FilePickerSaveOptions
        {
            Title = "Сохранить атлас",
            DefaultExtension = "png",
            SuggestedFileName = "Atlas.png"
        };

        // 2. Открываем окно сохранения файла
        IStorageFile? path = await ownerWindow.StorageProvider.SaveFilePickerAsync(options);
        

        if (path is not null)
        {
            // 1. Инициализируем ViewModel прогресс-бара СРАЗУ в главном потоке
            var progressBar = new ProgressBarViewModel();

            // Сразу же подписываемся на событие (теперь компилятор спокоен — переменная точно существует!)
            progressBar.OnLoadingChanged += (newValue) => IsAtlasSaving = newValue;

          
            var window = WindowStartManager.StartProgressBarWindow(progressBar);

            // 4. Получаем путь и запускаем загрузчик файлов
#pragma warning disable CS8602
            string localPath = path.Path.LocalPath;
#pragma warning restore CS8602

            // Создаем лоадер (если он делает тяжелое сохранение на диск, 
            // его методы тоже стоит вызывать через await)
            var loader = new ImageFileLoader(atlas);

            progressBar.OnLoadingChanged += (newValue) => IsAtlasSaving = newValue;
            progressBar.ProgressBarName = "Конвертация картинки в png...";

            await progressBar.CreateProgressBar(loader, loader.SaveImageWithProgressAsync, localPath);
            window.Close();
            progressBar.OnLoadingChanged -= (newValue) => IsAtlasSaving = newValue;

        }
    }

    private uint? SetNumberWithRules(uint? value, uint min = 0, uint max = 1000)
    {
        uint? result = value;

        if (value == null)
        {

            result = min;
        }
        else if (value < min)
        {
            result = min;
        }
        else if (value > max)
        {
            result = max;
        }
        else
        {
            result = value;
        }

        return result;
    }
    #endregion
}