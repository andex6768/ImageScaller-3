using System.Threading.Tasks;
using System.Collections.Generic;
using ImageMagick;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using System;
using System.Diagnostics;
using System.Linq;

public static class PhotoGenerator
{

    #region Structs
    private struct AtlasScale
    {
        public uint atlasHeight;
        public uint atlasWeignt;
        public ushort currentImagesInRow;
        public ushort currentImagesInColunm;

        public Queue<int> indexOfBiggestImagesInRows;

        public AtlasScale()
        {
            atlasHeight = 0;
            atlasWeignt = 0;
            currentImagesInRow = 0;
            currentImagesInColunm = 0;
            indexOfBiggestImagesInRows = new Queue<int>();
        }
    }
    private struct Pointer
    {
        public int x;
        public int y;
        public Pointer()
        {
            x = 0;
            y = 0;
        }
    }
    private struct PhotoScale
    {
        public uint height;
        public uint width;
        public PhotoScale(uint width, uint height)
        {
            this.width = width;
            this.height = height;
        }

    }
    #endregion

    public static Task<MagickImage> GenerateAtlasAsync(
    IReadOnlyList<MagickImage> images,
    uint imagesInRow, uint imageSpacingHorizontal,
    uint imageSpacingVertical,
    FilterType interpolation,
    ScaleToImage scaleToImageOption,
    uint? customWidth = null,
   uint? customHeight = null
    )
    {
        // Запускаем тяжелую склейку в фоновом потоке
        return Task.Run(() =>
        {


            if (scaleToImageOption == ScaleToImage.Custom && (customWidth == null || customHeight == null))
            {
                throw new ArgumentException("Было выбрано ScaleToImage.Custom но не были переданы customWidth и customHeight");
            }
            if (images.Count == 0)
            {
                throw new ArgumentException("Список картинок пустой");
            }
            bool isCustomSize = scaleToImageOption == ScaleToImage.Custom && customWidth != null && customHeight != null;
            // Копирование по значению всех картинок
            List<MagickImage> tempImages = images.Select(img => new MagickImage(img)).ToList();

            AtlasScale atlasScale;
            Pointer pointer = new Pointer();

            ushort drawedImagesInRow = 0;
#pragma warning disable CS8629 // Тип значения, допускающего NULL, может быть NULL.
            PhotoScale startPhoto;
            if (isCustomSize)
            {
                startPhoto = new PhotoScale((uint)customWidth, (uint)customHeight);
                atlasScale = GetAtlasScale(tempImages, (ushort)imagesInRow, (ushort)imageSpacingHorizontal, (ushort)imageSpacingVertical, scaleToImageOption, startPhoto);
            }
            else
            {
                atlasScale = GetAtlasScale(tempImages, (ushort)imagesInRow, (ushort)imageSpacingHorizontal, (ushort)imageSpacingVertical, scaleToImageOption);
                startPhoto = new PhotoScale(tempImages[0].Width, tempImages[0].Height);
            }

            var atlas = new MagickImage(MagickColors.Transparent, atlasScale.atlasWeignt, atlasScale.atlasHeight);

            for (int i = 0; i <= tempImages.Count - 1; i++)
            {
                MagickImage currentImage = tempImages[i];
                if (scaleToImageOption != ScaleToImage.DontScale)
                {
                    if (currentImage.Width != startPhoto.width || currentImage.Height != startPhoto.height)
                    {
                        currentImage = ImageScaller.ScaleImage(startPhoto.width, startPhoto.height, currentImage, interpolation);
                        tempImages[i] = currentImage;
                    }
                }

                atlas.Composite(currentImage, pointer.x, pointer.y, CompositeOperator.Over);
                pointer.x += (int)currentImage.Width;
                drawedImagesInRow++;

                if (drawedImagesInRow == atlasScale.currentImagesInRow)
                {
                    // Перейти на новый ряд
                    pointer.x = 0;

                    // Присваиваем высоту первой картинки из ряда
                    pointer.y += (int)(tempImages[atlasScale.indexOfBiggestImagesInRows.Dequeue()].Height + imageSpacingVertical);
                    drawedImagesInRow = 0;
                }
                // Иначе продолжение ряда
                else
                {
                    pointer.x += (int)imageSpacingHorizontal;
                }
            }
            return atlas;

        }

            );

    }


    private static void SortImagesByMaxArea(List<MagickImage> images)
    {
        images.Sort((a, b) =>
        {
            int FirsArea = (int)(a.Width * a.Height);
            int SecondArea = (int)(b.Width * b.Height);
            return SecondArea.CompareTo(FirsArea);
        });
    }
    private static void SortImagesByMinArea(List<MagickImage> images)
    {
        images.Sort((a, b) =>
        {
            int FirsArea = (int)(a.Width * a.Height);
            int SecondArea = (int)(b.Width * b.Height);
            return FirsArea.CompareTo(SecondArea);
        });

    }

    private static AtlasScale GetAtlasScale(List<MagickImage> images,
    ushort imagesInRow, ushort
    indentationWithImagesHorizontal,
    ushort indentationWithImagesVertical,
     ScaleToImage scaleToImageOption,
     PhotoScale? constStartPhoto = null)
    {
        // Отсортируем массив по самым большим или самым маленьким картинкам
        switch (scaleToImageOption)
        {
            case ScaleToImage.ScaleToBigger:
                SortImagesByMaxArea(images);

                break;
            case ScaleToImage.ScaleToSmaller:
                SortImagesByMinArea(images);
                break;
            default:
                SortImagesByMaxArea(images);
                break;
        }

        if (scaleToImageOption != ScaleToImage.Custom && scaleToImageOption != ScaleToImage.DontScale)
        {
            constStartPhoto = new PhotoScale(images[0].Width, images[0].Height);
        }

        #region Weight
        AtlasScale scale = new AtlasScale();

        // Определяем сколько реально помещается в ряд. Тоесть если ImagesInRow больше чем картинок в масиве.

        for (int i = 0; i < imagesInRow; i++)
        {
            if (i <= images.Count - 1)
            {
                scale.currentImagesInRow++;
            }
            else
            {
                break;
            }
        }

        for (int i = 0; i < scale.currentImagesInRow; i++)
        {

            if (constStartPhoto != null)
            {
                scale.atlasWeignt += constStartPhoto.Value.width + indentationWithImagesHorizontal;
            }
            else
            {
                scale.atlasWeignt += images[i].Width + indentationWithImagesHorizontal;
            }

        }
        scale.atlasWeignt -= indentationWithImagesHorizontal; // чтобы в по бокам фото не было лишних пикселей
        #endregion
        #region Height


        scale.currentImagesInColunm = (ushort)(images.Count / scale.currentImagesInRow);
        if (images.Count % scale.currentImagesInRow != 0)
        {
            scale.currentImagesInColunm++;
        }


        int biggestImageInRowIndex = 0;
        for (int j = 0; j < scale.currentImagesInColunm; j++)
        {
            if (j < images.Count - 1)
            {

                if (constStartPhoto != null)
                {
                    scale.atlasHeight += constStartPhoto.Value.height + indentationWithImagesVertical;
                    scale.indexOfBiggestImagesInRows.Enqueue(biggestImageInRowIndex);
                    biggestImageInRowIndex += scale.currentImagesInRow ;
                }
                else
                {
                    scale.atlasHeight += images[biggestImageInRowIndex].Height + indentationWithImagesVertical;
                    scale.indexOfBiggestImagesInRows.Enqueue(biggestImageInRowIndex);
                    biggestImageInRowIndex += scale.currentImagesInRow ;
                }

            }
        }
        scale.atlasHeight -= indentationWithImagesVertical;


        #endregion
        return scale;
    }



}