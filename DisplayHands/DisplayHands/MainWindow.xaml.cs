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

        private IList<HandMetadata> history = new List<HandMetadata>();
        private Object lockHistory = new Object();
        private int maxHistory = 20;

        private HandMetadata lastHandMetadata;
        private RectangleManager rectangleManager;
        private DrawingManager drawingManager;
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

        
        public double[][] CalculateNaiveVector(int scale)
        {
            lock (history)
            {
                if (history.Count > maxHistory - 1)
                {
                    double[][] leftPoints = new double[2][];
                    double[][] rightPoints = new double[2][];

                    leftPoints[1] = new double[] { history[history.Count - 1].LeftHandPosition[0], history[history.Count - 1].LeftHandPosition[1] };
                    rightPoints[1] = new double[] { history[history.Count - 1].RightHandPosition[0], history[history.Count - 1].RightHandPosition[1] };
                    leftPoints[0] = new double[] { history[0].LeftHandPosition[0], history[0].LeftHandPosition[1] };
                    rightPoints[0] = new double[] { history[0].RightHandPosition[0], history[0].RightHandPosition[1] };

                    double[][] vector = new double[2][];

                    vector[0] = new double[] { (leftPoints[1][0] - leftPoints[0][0]) * scale, (leftPoints[1][1] - leftPoints[0][1]) * scale };
                    vector[1] = new double[] { (rightPoints[1][0] - rightPoints[0][0]) * scale, (rightPoints[1][1] - rightPoints[0][1]) * scale };

                    return vector;
                }
                return null;
            }

        }

        public void DrawHistory(Bitmap bitmap)
        {
            Graphics g = Graphics.FromImage(bitmap);
            System.Drawing.Pen penLeft = new System.Drawing.Pen(System.Drawing.Color.Red, 3.0f);
            System.Drawing.Pen penRight = new System.Drawing.Pen(System.Drawing.Color.Blue, 3.0f);

            lock (history)
            {
                if (history.Count > 1)
                {
                    System.Drawing.Point[] leftPoints = new System.Drawing.Point[history.Count];

                    System.Drawing.Point[] rightPoints = new System.Drawing.Point[history.Count];

                    for (int i = 0; i < history.Count; i++)
                    {
                        leftPoints[i] = new System.Drawing.Point((int)history[i].LeftHandPosition[0], (int)history[i].LeftHandPosition[1]);
                        rightPoints[i] = new System.Drawing.Point((int)history[i].RightHandPosition[0], (int)history[i].RightHandPosition[1]);
                    }

                    if (leftPoints.Last().X + leftPoints.Last().Y > 0)
                        g.DrawLines(penLeft, leftPoints);
                    if (rightPoints.Last().X + rightPoints.Last().Y > 0)
                        g.DrawLines(penRight, rightPoints);
                }
            }
        }

        private void SaveData(PXCMHandData data, int frameNumber, HandMetadata handMetadata)
        {
            lastData = data;
            lastFrameNumber = frameNumber;
            lastHandMetadata = handMetadata;

            lock (lockHistory)
            {
                if (history.Count == maxHistory)
                {
                    history.RemoveAt(0);
                }
                history.Add(handMetadata);  //adds at end of list
            }

        }

        private void DrawVector(Bitmap bitmap)
        {
            //vector = 0B - 0A
            double[][] v = CalculateNaiveVector(2);

            if( v != null){
                Graphics g = Graphics.FromImage(bitmap);
                System.Drawing.Pen penLeft = new System.Drawing.Pen(System.Drawing.Color.Red, 3.0f);
                System.Drawing.Pen penRight = new System.Drawing.Pen(System.Drawing.Color.Blue, 3.0f);

                double leftX = lastHandMetadata.LeftHandPosition[0];
                double leftY = lastHandMetadata.LeftHandPosition[1];
                double rightX = lastHandMetadata.RightHandPosition[0];
                double rightY = lastHandMetadata.RightHandPosition[1];

                //v[left or right][point1 or point2][x or y]
                if(leftX + leftY > 0)
                {
                    g.DrawLine(penLeft, new System.Drawing.Point((int)leftX, (int)leftY), new System.Drawing.Point((int)leftX + (int)v[0][0], (int)leftY + (int)v[0][1]));
                    g.DrawEllipse(penLeft, new RectangleF((float)leftX + (float)v[0][0], (float)leftY + (float)v[0][1], 5, 5));
                }
                
                if(rightX + rightY > 0)
                {
                    g.DrawLine(penRight, new System.Drawing.Point((int)rightX, (int)rightY), new System.Drawing.Point((int)rightX + (int)v[1][0], (int)rightY + (int)v[1][1]));
                    g.DrawEllipse(penRight, new RectangleF((float)rightX + (float)v[1][0], (float)rightY + (float)v[1][1], 5, 5));
                }
            }
        }

 

        private void DrawHands(Bitmap bitmap) {
            if (lastData == null)
                return;
            //Bitmap bitmap = new Bitmap((int)image.Width, (int)image.Height);

            recognition.DisplayJoints(lastData, bitmap, lastFrameNumber);
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

            DrawHands(bitmap);
            DrawHistory(bitmap);
            DrawVector(bitmap);
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
