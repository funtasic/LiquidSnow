﻿using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Thismaker.Goro.Utilities
{
    public static class BitmapUtility
    {
        /// <summary>
        /// Converts a System.Drawing.Bitmap into a WPF BitmapImage
        /// </summary>
        /// <param name="bitmap">The bitmap to convert</param>
        /// <returns></returns>
        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            var image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            return image;
        }
    }
}
