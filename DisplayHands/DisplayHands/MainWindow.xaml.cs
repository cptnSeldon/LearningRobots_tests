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
        bool started = false;
        HandsRecognition recognition;

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
                new Thread(()=>recognition.SimplePipeline()).Start();
            }
            else
            {
                recognition.NewDataEvent += Recognition_NewDataEvent;
                buttonStart.Content = "Start";
                started = false;
                Console.WriteLine("stop");
                recognition.SignalStop();
                  
            }
        }

        private void Recognition_NewDataEvent(PXCMHandData data, int frameNumber)
        {
            this.Dispatcher.Invoke(()=>DrawStuff(data,frameNumber));
        }

        private void DrawStuff(PXCMHandData data, int frameNumber) {
            Bitmap bitmap = new Bitmap((int)image.Width, (int)image.Height);

            recognition.DisplayJoints(data, bitmap, frameNumber);

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
