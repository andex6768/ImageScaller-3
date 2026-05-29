using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using ImageMagick;
using System.IO;
public class ImageFileLoader : IProgressBarUsable
{

    public ImageFileLoader(string _folderPath)
    {
        folderPath = _folderPath;
    }
    public ImageFileLoader(Bitmap image)
    {
        bitmapImage = image;
    }
    private Bitmap? bitmapImage = null;
    byte[]? rawImageSizeInBytes = null;

    private string? folderPath;
    private List<string> imageFilePaths = new List<string>();

    public ulong CurrentProgress { get; set; }
    public ulong MaxProgress { get; set; }
    public string ProgressBarName { get; } = "Загрузка картинок из диска";
    public Action<ulong, string?>? OnProgressUpdated { get; set; } = null;




    public void CalculateMaxProgress(string actionName)
    {
        if (actionName == nameof(LoadImagesFromDiskAsync))
        {
            MaxProgress = (ulong)CountImagesInDirectory();
        }
        else if (actionName == nameof(SaveImageWithProgressAsync))
        {
            MaxProgress = CalculateImageSize();
        }

    }

    public ulong CalculateImageSize()
    {
        ulong exactBytesSize = 0;
        if (bitmapImage != null)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Кодируем пиксели Bitmap в формат PNG прямо внутри памяти
                bitmapImage.Save(memoryStream);

                // Вытаскиваем готовый массив байт PNG
                rawImageSizeInBytes = memoryStream.ToArray();
            }

            exactBytesSize = (ulong)rawImageSizeInBytes.Length;
        }
        else
        {
            throw new InvalidOperationException("MagickImage не инициализирован. Невозможно рассчитать размер.");
        }

        return exactBytesSize;
    }
    public int CountImagesInDirectory()
    {
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException($"Папка не найдена: {folderPath}");
        }

        // 1. Получаем пути ко ВСЕМ файлам в папке (быстрая операция)
        string[] allFiles = Directory.GetFiles(folderPath);
        int imageCount = 0;

        // 2. В фоновом потоке проверяем, являются ли они картинками

        foreach (var filePath in allFiles)
        {
            if (IsImageFile(filePath))
            {
                imageFilePaths.Add(filePath);
                imageCount++;
            }
        }
        return imageCount;
    }

    public static bool IsImageFile(string filePath)
    {
        try
        {
            // Пытаемся прочитать только информацию о формате (без загрузки пикселей)
            var info = new MagickImageInfo(filePath);

            if (info.Format != MagickFormat.Unknown)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }
    public async Task<List<MagickImage>> LoadImagesFromDiskAsync()
    {
        return await Task.Run(() =>
           {
               List<MagickImage> result = new List<MagickImage>();

               for (int i = 0; i < imageFilePaths.Count; i++)
               {
                   MagickImage image = new MagickImage(imageFilePaths[i]);

                   result.Add(image);
                   OnProgressUpdated?.Invoke((uint)(i + 1), $"Загружено {i + 1} из {imageFilePaths.Count} картинок");

               }

               return result;

           });
    }
    public async Task<bool> SaveImageWithProgressAsync(string destinationPath)
    {
        if (rawImageSizeInBytes == null)
        {
            throw new InvalidOperationException("Размер изображения не рассчитан. Невозможно сохранить изображение.");
        }
        // Буфер в 64 КБ (оптимально для большинства дисков)
        byte[] buffer = new byte[64 * 1024];
        long totalBytes = rawImageSizeInBytes.Length;
        long bytesWritten = 0;


        // Открываем поток для записи на диск
        using (FileStream destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
        {
            // Записываем данные по частям
            while (bytesWritten < totalBytes)
            {
                int bytesToWrite = (int)Math.Min(buffer.Length, totalBytes - bytesWritten);
                Array.Copy(rawImageSizeInBytes, bytesWritten, buffer, 0, bytesToWrite);

                await destinationStream.WriteAsync(buffer, 0, bytesToWrite);
                bytesWritten += bytesToWrite;

                OnProgressUpdated?.Invoke((ulong)bytesWritten, $"Сохранено {bytesWritten} из {totalBytes} байт");
            }
        }
        return true;
    }

}