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
using Microsoft.Win32;

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
        private SequenceManager sequenceManager;
        private Utils utils;
        private State currentStateLeft;
        private State currentStateRight;
        private int directedRectangle;
        private int leftRectIndex;
        private int rightRectIndex;
        private History history = new History();
        private String filename = null;
        #endregion ATTRIBUTES


        public MainWindow()
        {
            InitializeComponent();
            buttonStart.Content = "Start";
            sliderRectangles.IsEnabled = false;
            rectangleManager = new RectangleManager(0);
        }

        #region USER EVENT MANAGER
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!started)
            {
                started = true;
                buttonStart.Content = "Stop";
                Console.WriteLine("start");

                sequenceManager = new SequenceManager();
                drawingManager = new DrawingManager();

                recognition = new HandsRecognition();
                recognition.NewDataEvent += Recognition_NewDataEvent;
                recognition.NewRGBImageEvent += Recognition_NewRGBImageEvent;

                bool check = Record.IsChecked;
                Record.IsEnabled = false;

                new Thread(()=>recognition.SimplePipeline(filename, check)).Start();

                sliderRectangles.IsEnabled = true;
            }
            else
            {
                recognition.NewDataEvent -= Recognition_NewDataEvent;
                recognition.NewRGBImageEvent -= Recognition_NewRGBImageEvent;
                sliderRectangles.IsEnabled = false;
                buttonStart.Content = "Start";
                started = false;

                Console.WriteLine("stop");
                recognition.SignalStop();
                Record.IsChecked = false;
                Record.IsEnabled = true;
            }
        }

        private void sliderRectangles_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (rectangleManager != null)
                rectangleManager.DestroyAll();
            rectangleManager = new RectangleManager((int)sliderRectangles.Value);

            #region SEQUENCE INITIALIZATION
            if ((int) sliderRectangles.Value == 0)
                sequenceManager.GenerateSequence(0, 1);
            else
                sequenceManager.GenerateSequence((int)sliderRectangles.Value, (int)sliderRectangles.Value);

            currentStateLeft = State.UNDEFINED;
            currentStateRight = State.UNDEFINED;

            sequenceManager.PrintSequence();
            #endregion SEQUENCE INITIALIZATION
        }

        #region MENU
        private void Record_Click(object sender, RoutedEventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = @"RSSDK clip|*.rssdk|All files|*.*",
                CheckPathExists = true,
                OverwritePrompt = true
            };

            if (saveFileDialog.ShowDialog() == true)
                filename = saveFileDialog.FileName;
        }

        private void Replay_Click(object sender, RoutedEventArgs e)
        {
            //TODO
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = @"RSSDK clip|*.rssdk|All files|*.*",
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openFileDialog.ShowDialog() == true)
                filename = openFileDialog.FileName;
        }

        private void JSON_Click(object sender, RoutedEventArgs e)
        {
            //https://www.newtonsoft.com/json/help/html/Introduction.htm

           

        }

        private void AppExit_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Do you want to quit the application?", "Quit", MessageBoxButton.OK, MessageBoxImage.Information);
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, "" +
                "Developped by Julia Németh, 3dlma\n" +
                "HE-ARC, Fall semester, January 2018",
                "About Learning Robots", MessageBoxButton.OK, MessageBoxImage.Information
                );
        }
        #endregion MENU

        #endregion USER EVENT MANAGER

        #region EVENT
        private void Recognition_NewRGBImageEvent(Bitmap obj)
        {
            this.Dispatcher.Invoke(() => DrawRGBImage(obj));
        }

        private void Recognition_NewDataEvent(PXCMHandData data, int frameNumber, HandMetadata handMetadata)
        {
            this.Dispatcher.Invoke(()=>SaveData(data,frameNumber, handMetadata));

            utils = new Utils();

            #region SEQUENCE MANAGEMENT
            directedRectangle = -1;
            //check if direction vector is contained in one of the rectangles
            if (utils.CalculateNaiveVector(drawingManager.scale, history) != null)
                directedRectangle = rectangleManager.GetPointInRectangle(utils.CalculateNaiveVector(drawingManager.scale, history));
            leftRectIndex = -1;
            rightRectIndex = -1;
            //check if hand is contained in one of the rectangles
            if (lastHandMetadata.LeftHandPosition[0] + lastHandMetadata.LeftHandPosition[1] > 0)
                leftRectIndex = rectangleManager.GetPointInRectangle(lastHandMetadata.LeftHandPosition[0], lastHandMetadata.LeftHandPosition[1]);
            if (lastHandMetadata.RightHandPosition[0] + lastHandMetadata.RightHandPosition[1] > 0)
                rightRectIndex = rectangleManager.GetPointInRectangle(lastHandMetadata.RightHandPosition[0], lastHandMetadata.RightHandPosition[1]);

            Console.WriteLine(rightRectIndex + ", " + directedRectangle);

            //test if sequense is ok
            currentStateLeft = sequenceManager.GetState(leftRectIndex, directedRectangle, true);
            currentStateRight = sequenceManager.GetState(rightRectIndex, directedRectangle, false);

            Console.WriteLine("Current rectangle: " + sequenceManager.GetCurrent());

            Console.WriteLine("Left: " + currentStateLeft + "\nRight: " + currentStateRight);
            #endregion SEQUENCE MANAGEMENT
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
            //mirror
            image.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            ScaleTransform scaleTransform = new ScaleTransform
            {
                ScaleX = -1,
                ScaleY = 1
            };
            image.RenderTransform = scaleTransform;

            labelLeftHand_.Content = $"{lastHandMetadata.LeftHandPosition[0]}, {lastHandMetadata.LeftHandPosition[1]}";
            labelRightHand_.Content = $"{lastHandMetadata.RightHandPosition[0]}, {lastHandMetadata.RightHandPosition[1]}";

            //draw stuff
            drawingManager.DrawHands(bitmap, recognition, lastData, lastFrameNumber);
            drawingManager.DrawHistory(bitmap, history);
            drawingManager.DrawVector(bitmap, lastHandMetadata, history);
            drawingManager?.Rectangles_DrawAll(bitmap, lastHandMetadata, rectangleManager, currentStateLeft, currentStateRight, leftRectIndex, rightRectIndex, directedRectangle);

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
