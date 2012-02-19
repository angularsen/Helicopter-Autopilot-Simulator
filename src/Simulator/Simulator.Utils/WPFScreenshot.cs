#if !XBOX

using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Simulator.Utils
{
    public static class WPFScreenshot
    {
        /// <summary>
        /// Gets a PNG "screenshot" of the current UIElement
        /// </summary>
        /// <param name="source">UIElement to screenshot</param>
        /// <param name="scale">Scale to render the screenshot</param>
        /// <returns>Byte array of JPG data</returns>
        public static byte[] GetPngImage(this UIElement source, double scale)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Interlace = PngInterlaceOption.Off;
            return GetImage(source, encoder, scale);
        }

        /// <summary>
        /// Gets a JPG "screenshot" of the current UIElement
        /// </summary>
        /// <param name="source">UIElement to screenshot</param>
        /// <param name="scale">Scale to render the screenshot</param>
        /// <param name="quality">JPG Quality</param>
        /// <returns>Byte array of JPG data</returns>
        public static byte[] GetJpegImage(this UIElement source, double scale, int quality)
        {
            var encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = quality;
            return GetImage(source, new JpegBitmapEncoder(), scale);
        }


        private static byte[] GetImage(UIElement source, BitmapEncoder bitmapEncoder, double scale)
        {
            double actualHeight = source.RenderSize.Height;
            double actualWidth = source.RenderSize.Width;

            double renderHeight = actualHeight * scale;
            double renderWidth = actualWidth * scale;

            var renderTarget = new RenderTargetBitmap((int)renderWidth, (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
            var sourceBrush = new VisualBrush(source);

            var drawingVisual = new DrawingVisual();
            var drawingContext = drawingVisual.RenderOpen();

            using (drawingContext)
            {
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
                drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(actualWidth, actualHeight)));
            }
            renderTarget.Render(drawingVisual);

            bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTarget));

            Byte[] imageArray;

            using (var outputStream = new MemoryStream())
            {
                bitmapEncoder.Save(outputStream);
                imageArray = outputStream.ToArray();
            }

            return imageArray;
        }
    }
}
#endif