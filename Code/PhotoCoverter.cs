using System.IO;
using System.Threading.Tasks;
using ImageMagick;
using Bitmap = Avalonia.Media.Imaging.Bitmap; 


public static class PhotoCoverter
{
  public static MagickImage ConvertBitmapToMagickImage(Bitmap bitmap)
    {
        using (var stream = new MemoryStream())
        {
            bitmap.Save(stream);
            stream.Position = 0; // Сброс позиции потока
            return new MagickImage(stream);
        }
    }
   public static Bitmap ConvertMagickImageToBitmap(MagickImage magickImage)
    {
        using (var stream = new MemoryStream())
        {
            magickImage.Write(stream, MagickFormat.Png);
            stream.Position = 0; // Сброс позиции потока
            return new Bitmap(stream);
        }
    }


       public static Task<Bitmap> ConvertMagickImageToBitmapAsync(MagickImage magickImage)
    {
        return Task.Run(() =>
        {
             using (var stream = new MemoryStream())
        {
            magickImage.Write(stream, MagickFormat.Png);
            stream.Position = 0; // Сброс позиции потока
            return new Bitmap(stream);
        }
        });
       
    }
}