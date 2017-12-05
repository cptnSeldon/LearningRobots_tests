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
        private int maxHistory = 5;

        private HandMetadata lastHandMetadata;
        #endregion ATTRIBUTES


        public MainWindow()
        {
            InitializeComponent();
            buttonStart.Content = "Start";
        }

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

        private void Recognition_NewRGBImageEvent(Bitmap obj)
        {
            this.Dispatcher.Invoke(() => DrawRGBImage(obj));
        }

        private void Recognition_NewDataEvent(PXCMHandData data, int frameNumber, HandMetadata handMetadata)
        {
            this.Dispatcher.Invoke(()=>SaveData(data,frameNumber, handMetadata));
        }

        private void DrawRGBImage(Bitmap bitmap)
        {
            //mirror
            image.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            ScaleTransform scaleTransform = new ScaleTransform();
            scaleTransform.ScaleX = -1;
            scaleTransform.ScaleY = 1;
            image.RenderTransform = scaleTransform;

            labelLeftHand_.Content = $"{lastHandMetadata.LeftHandPosition[0]}, {lastHandMetadata.LeftHandPosition[1]}";
            labelRightHand_.Content = $"{lastHandMetadata.RightHandPosition[0]}, {lastHandMetadata.RightHandPosition[1]}";

            DrawHands(bitmap);
            //DrawHistory(bitmap);
            DrawVector(bitmap);

            image.Source = BitmapToImageSource(bitmap);
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

        private void DrawHistory(Bitmap bitmap)
        {
            Graphics g = Graphics.FromImage(bitmap);
            System.Drawing.Pen penLeft = new System.Drawing.Pen(System.Drawing.Color.Red, 3.0f);
            System.Drawing.Pen penRight = new System.Drawing.Pen(System.Drawing.Color.Blue, 3.0f);

            lock (history)
            {
                if(history.Count > 1)
                {
                    System.Drawing.Point[] leftPoints = new System.Drawing.Point[history.Count];

                    System.Drawing.Point[] rightPoints = new System.Drawing.Point[history.Count];

                    for (int i = 0; i < history.Count; i++)
                    {
                        leftPoints[i] = new System.Drawing.Point(history[i].LeftHandPosition[0], history[i].LeftHandPosition[1]);
                        rightPoints[i] = new System.Drawing.Point(history[i].RightHandPosition[0], history[i].RightHandPosition[1]);
                    }

                    g.DrawLines(penLeft, leftPoints);
                    g.DrawLines(penRight, rightPoints);
                }
                
            }
        }

        private int[][] CalculateNaiveVector(int scale)
        {
            lock (history)
            {
                if (history.Count > 1)
                {
                    int[][] leftPoints = new int[2][];
                    int[][] rightPoints = new int[2][];
                    
                    leftPoints[1] = new int[] { history[history.Count - 1].LeftHandPosition[0], history[history.Count - 1].LeftHandPosition[1] };
                    rightPoints[1] = new int[] { history[history.Count - 1].RightHandPosition[0], history[history.Count - 1].RightHandPosition[1] };
                    leftPoints[0] = new int[] { history[0].LeftHandPosition[0], history[0].LeftHandPosition[1] };
                    rightPoints[0] = new int[] { history[0].RightHandPosition[0], history[0].RightHandPosition[1] };

                    int[][] vector = new int[2][];

                    vector[0] = new int[] { (leftPoints[1][0] - leftPoints[0][0]) * scale, (leftPoints[1][1] - leftPoints[0][1]) * scale };
                    vector[1] = new int[] { (rightPoints[1][0] - rightPoints[0][0]) * scale, (rightPoints[1][1] - rightPoints[0][1]) * scale };

                    return vector;
                }
                return null;
            }
           
        }

        private void DrawVector(Bitmap bitmap)
        {
            //vector = 0B - 0A
            int[][] v = CalculateNaiveVector(5);

            if( v != null){
                Graphics g = Graphics.FromImage(bitmap);
                System.Drawing.Pen penLeft = new System.Drawing.Pen(System.Drawing.Color.Red, 3.0f);
                System.Drawing.Pen penRight = new System.Drawing.Pen(System.Drawing.Color.Blue, 3.0f);

                int leftX = lastHandMetadata.LeftHandPosition[0];
                int leftY = lastHandMetadata.LeftHandPosition[1];
                int rightX = lastHandMetadata.RightHandPosition[0];
                int rightY = lastHandMetadata.RightHandPosition[1];

                //v[left or right][point1 or point2][x or y]
                g.DrawLine(penLeft, new System.Drawing.Point(leftX, leftY), new System.Drawing.Point(leftX + v[0][0], leftY + v[0][1]));
                g.DrawEllipse(penLeft, new RectangleF(leftX + v[0][0], leftY + v[0][1], 5, 5));
                g.DrawLine(penRight, new System.Drawing.Point(rightX, rightY), new System.Drawing.Point(rightX + v[1][0], rightY + v[1][1]));
                g.DrawEllipse(penRight, new RectangleF(rightX + v[1][0], rightY + v[1][1], 5, 5));
            }
        }

        private void DrawHands(Bitmap bitmap) {
            if (lastData == null)
                return;
            //Bitmap bitmap = new Bitmap((int)image.Width, (int)image.Height);

            recognition.DisplayJoints(lastData, bitmap, lastFrameNumber);
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
