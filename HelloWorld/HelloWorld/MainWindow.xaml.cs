
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Drawing;


namespace HelloWorld
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region ATTRIBUTES
        private Thread processingThread;
        private PXCMSenseManager senseManager;

        private PXCMHandModule handAnalysis;
        private PXCMHandConfiguration handConfiguration;
        private PXCMHandData handData;

        private HandsRecognition handsRecognition;
        //private PXCMHandData.GestureData gestureData;

        //private bool handWaving;
        //private bool handTrigger;
        private bool started = false;
        #endregion ATTRIBUTES

        /// <summary>
        /// MainWindow constructor : variable instanciation and initialization
        ///     - SenseManager
        ///     - Hand module
        ///     - starting the Worker Thread
        /// </summary>
        public MainWindow()
        {
            #region ATTRIBUTES
            InitializeComponent();
            #endregion ATTRIBUTES
        }

        #region WINDOW EVENT HANDLERS
        /// <summary>
        /// Raised when the main window is loaded.
        ///     - label is initialized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        /// <summary>
        /// Manages the memory object releasal when the app is closing.
        /// The Working Thread is aborted as well.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            /*processingThread.Abort();
            if (handData != null) handData.Dispose();
            handConfiguration.Dispose();
            senseManager.Dispose();*/
        }
        #endregion WINDOW EVENT HANDLERS

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!started)
            {
                started = true;
                buttonStart.Content = "Stop";
                Console.WriteLine("start");

                /*// Instantiate and initialize the SenseManager
                senseManager = PXCMSenseManager.CreateInstance();
                senseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 640, 480, 30);
                senseManager.EnableHand();
                senseManager.Init();

                // Start the worker thread
                processingThread = new Thread(new ThreadStart(ProcessingThread));
                processingThread.Start();*/

                handsRecognition = new HandsRecognition();
                handsRecognition.NewDataEvent += Recognition_NewDataEvent;
                new Thread(() => handsRecognition.SimplePipeline()).Start();
            }
            else
            {
                buttonStart.Content = "Start";
                started = false;
                Console.WriteLine("stop");

                handsRecognition.SignalStop();
            }
        }

        #region WORKER THREAD
        /// <summary>
        /// Contains : AcquireFrame/ReleaseFrame loop (independent from the main UI thread).
        /// We access the RealSense SDK APIs from here :
        ///     - acquire color image data
        ///     - retrieve hand geture data
        ///     - updateUI call
        ///     - frame releasal
        /// Updates to the UI are synchronized with the frame loop through method calls to UpdateUI();
        /// Procedural call is used here.
        /// </summary>
        private void ProcessingThread()
        {
            // Start AcquireFrame/ReleaseFrame loop
            while (senseManager.AcquireFrame(true) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                #region ATTRIBUTES
                //capture
                PXCMCapture.Sample sample = senseManager.QuerySample();
                Bitmap colorBitmap;
                PXCMImage.ImageData colorData;
                
                #endregion ATTRIBUTES

                // Get color image data
                sample.color.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_RGB24, out colorData);
                colorBitmap = colorData.ToBitmap(0, sample.color.info.width, sample.color.info.height);

                // Update the user interface
                UpdateUI(colorBitmap);

                // Release the frame
                if (handData != null) handData.Dispose();
                colorBitmap.Dispose();
                sample.color.ReleaseAccess(colorData);
                senseManager.ReleaseFrame();
            }
        }
        #endregion WORKER THREAD

        #region DISPATCHER FOR EXECUTING OPERATIONS ON THE UI THREAD
        /// <summary>
        /// Uses the Dispatcher.Invoke method to perform operations that will be executed on the UI thread.
        ///     - display color stream via WPF Image control
        ///     - display Hello World in Label control
        ///     - reset the messsage within 50 frames using a Timer if the waving gesture is no longer detected.
        /// </summary>
        /// <param name="bitmap"></param>
        private void UpdateUI(Bitmap bitmap)
        {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
            {
                if (bitmap != null)
                {
                    // Mirror the color stream Image control
                    imgColorStream.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                    ScaleTransform mainTransform = new ScaleTransform();
                    mainTransform.ScaleX = -1;
                    mainTransform.ScaleY = 1;
                    imgColorStream.RenderTransform = mainTransform;

                    // Display the color stream using the method created in ConvertBitmap
                    imgColorStream.Source = ConvertBitmap.BitmapToBitmapSource(bitmap); 
                }
            }));
        }
        #endregion DISPATCHER FOR EXECUTING OPERATIONS ON THE UI THREAD

        private void Recognition_NewDataEvent(PXCMHandData data, int frameNumber)
        {
            this.Dispatcher.Invoke(() => DrawStuff(data, frameNumber));
        }

        private void DrawStuff(PXCMHandData data, int frameNumber)
        {
            Bitmap bitmap = new Bitmap((int)imgColorStream.Width, (int)imgColorStream.Height);

            handsRecognition.DisplayJoints(data, bitmap, frameNumber);

            imgColorStream.Source = BitmapToImageSource(bitmap);
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