using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SmartSearchScreen
{
    public class ImageLoader
    {
        private readonly string imagesFolderPath;
        private List<string> loadedImages = new List<string>();
        private List<string> allImages = new List<string>();

        public ImageLoader(string imagesFolderPath)
        {
            this.imagesFolderPath = imagesFolderPath;
            LoadAllImages();
        }

        public void LoadAllImages()
        {
            allImages = Directory.GetFiles(imagesFolderPath, "*.png")
                                 .OrderBy(f => File.GetCreationTime(f))
                                 .ToList();
        }

        public int GetTotalPages(int imagesPerPage)
        {
            return (int)Math.Ceiling((double)allImages.Count / imagesPerPage);
        }

        public string GetLastImage()
        {
            LoadAllImages();
            foreach (var image in allImages)
            {
                loadedImages.Add(image);
            }

            if (loadedImages.Count > 0)
            {
                return loadedImages.Last();
            }
            else
            {
                return null;
            }
        }

        public void LoadImagesByPage(Grid imageGrid, int pageNumber, int imagesPerPage)
        {
            imageGrid.Children.Clear();
            var imagesToLoad = allImages.Skip((pageNumber - 1) * imagesPerPage).Take(imagesPerPage).ToList();
            int row = 0, col = 0;

            foreach (var imageFile in imagesToLoad)
            {
                var image = new Image
                {
                    Source = new BitmapImage(new Uri(imageFile)),
                    Margin = new Thickness(5)
                };
                Grid.SetRow(image, row);
                Grid.SetColumn(image, col);
                imageGrid.Children.Add(image);

                col++;
                if (col >= 3)
                {
                    col = 0;
                    row++;
                }
            }
        }
    }
}