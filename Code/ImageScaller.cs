using ImageMagick;

public static class ImageScaller
{
    public static MagickImage ScaleImage(uint targetWidth, uint targetHeight, MagickImage sourceImage, FilterType interpolation)
    {

  sourceImage.FilterType = interpolation;

    // 1. Создаем геометрию с нужными размерами
    var geometry = new MagickGeometry(targetWidth, targetHeight)
    {
        // 2. Включаем флаг игнорирования пропорций (это принудительное растягивание)
        IgnoreAspectRatio = true 
    };

    // 3. Передаем объект геометрии в Resize вместо отдельных чисел
    sourceImage.Resize(geometry);
    
    // Безопасно возвращаем живой объект
    return sourceImage;
    }
}