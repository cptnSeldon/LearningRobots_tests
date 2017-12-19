using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;

namespace DisplayHands
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region ATTRIBUTES
        bool started = false;
        HandsRecognition recognition;
        private int lastFrameNumber=0;
        private PXCMHandData lastData;

        private HandMetadata lastHandMetadata;
        private RectangleManager rectangleManager;
        private DrawingManager drawingManager;
        private History history = new History();
        #endregion ATTRIBUTES


        public MainWindow()
        {
            InitializeComponent();
            buttonStart.Content = "Start";
        }

        #region USER EVENT MANAGER
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!started)
            {
                started = true;
                buttonStart.Content = "Stop";
                Console.WriteLine("start");
                recognition = new HandsRecognition();
                recognition.NewDataEvent += Recognition_NewDataEvent;
                recognition.NewRGBImageEvent += Recognition_NewRGBImageEvent;
                new Thread(()=>recognition.SimplePipeline()).Start();
            }
            else
            {
                recognition.NewDataEvent -= Recognition_NewDataEvent;
                recognition.NewRGBImageEvent -= Recognition_NewRGBImageEvent;
                buttonStart.Content = "Start";
                started = false;
                Console.WriteLine("stop");
                recognition.SignalStop();
                  
            }
        }

        private void sliderRectangles_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (rectangleManager != null)
                rectangleManager.DestroyAll();
            rectangleManager = new RectangleManager((int)sliderRectangles.Value);
        }
        #endregion USER EVENT MANAGER

        #region EVENT
        private void Recognition_NewRGBImageEvent(Bitmap obj)
        {
            this.Dispatcher.Invoke(() => DrawRGBImage(obj));
        }

        private void Recognition_NewDataEvent(PXCMHandData data, int frameNumber, HandMetadata handMetadata)
        {
            this.Dispatcher.Invoke(()=>SaveData(data,frameNumber, handMetadata));
        }
        #endregion EVENT

        private void SaveData(PXCMHandData data, int frameNumber, HandMetadata handMetadata)
        {
            lastData = data;
            lastFrameNumber = frameNumber;
            lastHandMetadata = handMetadata;

            history.Save(handMetadata);
        }

        private void DrawRGBImage(Bitmap bitmap)
        {
            drawingManager = new DrawingManager();
            //mirror
            image.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            ScaleTransform scaleTransform = new ScaleTransform();
            scaleTransform.ScaleX = -1;
            scaleTransform.ScaleY = 1;
            image.RenderTransform = scaleTransform;

            labelLeftHand_.Content = $"{lastHandMetadata.LeftHandPosition[0]}, {lastHandMetadata.LeftHandPosition[1]}";
            labelRightHand_.Content = $"{lastHandMetadata.RightHandPosition[0]}, {lastHandMetadata.RightHandPosition[1]}";

            //draw stuff
            drawingManager.DrawHands(bitmap, recognition, lastData, lastFrameNumber);
            drawingManager.DrawHistory(bitmap, history);
            drawingManager.DrawVector(bitmap, lastHandMetadata, history);
            drawingManager?.Rectangles_DrawAll(bitmap, lastHandMetadata, rectangleManager);

            image.Source = BitmapToImageSource(bitmap);
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        
    }
}
