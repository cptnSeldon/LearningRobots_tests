using System;
using System.Windows.Media.Imaging;

namespace P3_LearningRobots
{
    /// <summary>
    /// Bitmap conversion management class
    /// Source: IntelRealSense SDK - DisplayHands sample
    /// Utility class which supports BITMAP conversions for the color video stream
    ///     -> GDI bitmap (Graphics Device Interface) : universal set of routines that can
    ///         be used to draw onto a screen, printer, plotter or bitmap image in memory.
    ///     link : https://www.codeproject.com/Articles/356/Bitmap-Basics-A-GDI-tutorial
    ///     -> ameliorating the code like below
    ///     link : https://www.codeproject.com/Articles/104929/Bitmap-to-BitmapSource
    /// </summary>
    class ConvertBitmap
    {
        //GDI dll
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        //handling the pointer
        public static extern bool DeleteObject(IntPtr handle);
        //set of pixels
        public static BitmapSource bitmapSource;
        //pointer to an unmanaged bitmap/palette information
        public static IntPtr intPointer;
        /// <summary>
        /// Returns a managed BitmapSource
        /// </summary>
        /// <param name="bitmap">object used to wirk with images defined by pixel data</param>
        /// <returns></returns>
        public static BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            intPointer = bitmap.GetHbitmap();

            bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                intPointer,
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(intPointer);

            return bitmapSource;
        }
    }
}
