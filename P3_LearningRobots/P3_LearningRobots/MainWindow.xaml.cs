﻿using Microsoft.Win32;
using P3_LearningRobots.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace P3_LearningRobots
{
    /// <summary>
    /// Interaction logics for MainWindow.xaml
    /// The core example comes from IntelRealSense SDK sample pack 'DisplayHands' 
    /// -> HandRecognition
    /// </summary>
    public partial class MainWindow : Window
    {

        #region ATTRIBUTES
        bool started = false;
        HandsRecognition recognition;
        private int lastFrameNumber = 0;
        private PXCMHandData lastData;

        private HandMetadata lastHandMetadata;
        private RectangleManager rectangleManager;
        private DrawingManager drawingManager = new DrawingManager();
        private SequenceManager sequenceManager = new SequenceManager();
        private State currentStateLeft;
        private State currentStateRight;
        private int directedRectangleLeft;
        private int directedRectangleRight;
        private int leftRectIndex;
        private int rightRectIndex;
        private History history = new History();
        private String filename = null;

        #endregion ATTRIBUTES

        /// <summary>
        /// Initializes components: default
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            //default controls handling
            button_start.Content = "Start";
            slider_rectangles.IsEnabled = false;

            //managers initialization
            rectangleManager = new RectangleManager(0);
        }

        #region CONTROLS

        #region CONTROL PANEL
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StartButton_Click(object sender, RoutedEventArgs e)
        {
            //Starts the processes
            if (!started)
            {
                started = true;
                button_start.Content = "Stop";

                #region USER CONTROLS
                input_sequence.IsEnabled = true;

                //menu
                record.IsEnabled = false;
                bool check = record.IsChecked;
                #endregion USER CONTROLS

                //start video + recognition processes
                StartOrStop(true);
                new Thread(() => recognition.SimplePipeline(filename, check)).Start();

                #region USER CONTROLS - suite
                //controls -> has to be done last
                slider_rectangles.IsEnabled = true;
                #endregion USER CONTROLS - suite

            }
            //Stops the processes
            else
            {
                //stop video + recognition processes
                StartOrStop(false);

                started = false;
                button_start.Content = "Start";

                #region USER CONTROLS
                //controls
                slider_rectangles.IsEnabled = false;
                input_sequence.IsEnabled = false;

                //menu
                record.IsChecked = false;
                record.IsEnabled = true;
                #endregion USER CONTROLS
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isStart"></param>
        public void StartOrStop(bool isStart)
        {
            if (isStart)
            {
                recognition = new HandsRecognition();
                recognition.NewDataEvent += Recognition_NewDataEvent;
                recognition.NewRGBImageEvent += Recognition_NewRGBImageEvent;
            }
            else
            {
                recognition.NewDataEvent -= Recognition_NewDataEvent;
                recognition.NewRGBImageEvent -= Recognition_NewRGBImageEvent;
                recognition.SignalStop();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SliderRectangles_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            input_sequence.Text = "";

            //create zones
            if (rectangleManager != null)
                rectangleManager.DestroyAll();
            rectangleManager = new RectangleManager((int)slider_rectangles.Value);

            //initialize sequences
            if ((int) slider_rectangles.Value == 0)
                sequenceManager.GenerateSequence(0, 1, "");
            else
                sequenceManager.GenerateSequence((int)slider_rectangles.Value, (int)slider_rectangles.Value, input_sequence.Text);

            currentStateLeft = State.UNDEFINED;
            currentStateRight = State.UNDEFINED;

            //sequenceManager.PrintSequence();
            sequenceManager.SequenceCounter = 0;
            sequenceManager.ErrorCounter = 0;
        }

        #region INPUT SEQUENCE
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void input_sequence_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Match(e.Text))
                e.Handled = true;

            if ((int)slider_rectangles.Value == 0)
                sequenceManager.GenerateSequence(0, 1, "");
            else
                sequenceManager.GenerateSequence((int)slider_rectangles.Value, (int)slider_rectangles.Value,input_sequence.Text + e.Text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void input_sequence_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool Match(String s)
        {
            if (slider_rectangles.Value > 0)
            {
                char majMin = 'A';
                char majMax = (char)(((int)'A') + (int)slider_rectangles.Value - 1);
                char minMin = 'a';
                char minMax = (char)(((int)'a') + (int)slider_rectangles.Value - 1);

                Regex regex = new Regex($"^[{minMin}-{minMax}{majMin}-{majMax}]+");
                return regex.IsMatch(s);
            }
            return false;
        }
        #endregion INPUT SEQUENCE

        #endregion CONTROL PANEL

        #region MENU
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Replay_Click(object sender, RoutedEventArgs e)
        {
             OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = @"RSSDK clip|*.rssdk|All files|*.*",
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openFileDialog.ShowDialog() == true)
                filename = openFileDialog.FileName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void JSON_Click(object sender, RoutedEventArgs e)
        {
            //https://www.newtonsoft.com/json/help/html/Introduction.htm
            //open file dialog
            SaveFileDialog dialog = new SaveFileDialog
            {
                //setting filter
                DefaultExt = ".json",
                Filter = "JSON data format (.json)|*.json" // Filter files by extension
            };

            //show dialog call
            Nullable<bool> result = dialog.ShowDialog();
            //get selected file name
            if (result == true)
            {
                string filename = dialog.FileName;
                JSONHandler.Save(filename, rectangleManager, history);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppExit_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Are you sure?", "Quitting the application", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            StartOrStop(false);
            Application.Current.Shutdown();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, "" +
                "Developped by Julia Németh, 3dlma\n" +
                "HE-ARC, Fall semester, January 2018",
                "About Learning Robots", MessageBoxButton.OK, MessageBoxImage.Information
                );
        }
        #endregion MENU

        #endregion CONTROLS

        #region RECOGNITION EVENTS MANAGEMENT

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void Recognition_NewRGBImageEvent(Bitmap obj)
        {
            try
            {
                this.Dispatcher.Invoke(() => DrawRGBImage(obj));
            }
            catch(OperationCanceledException) { }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="frameNumber"></param>
        /// <param name="handMetadata"></param>
        private void Recognition_NewDataEvent(PXCMHandData data, int frameNumber, HandMetadata handMetadata)
        {
            try
            {
                this.Dispatcher.Invoke(() => SaveData(data, frameNumber, handMetadata));
            }
            catch (OperationCanceledException) { }
            

            #region SEQUENCE MANAGEMENT
            directedRectangleLeft = 0;
            directedRectangleRight = 0;
            //check if direction vector is contained in one of the rectangles
            

            List<double> percentageList = Utils.GetPercentage(new PointF((float)lastHandMetadata.LeftHandPosition[0], (float)lastHandMetadata.LeftHandPosition[1]),
                                                    new PointF((float)lastHandMetadata.RightHandPosition[0], (float)lastHandMetadata.RightHandPosition[1]),
                                                    rectangleManager.GetAll());

            for(int i = 0; i < percentageList.Count - 2; i++)
            {

                if (percentageList[i] < percentageList[i + 2])
                {

                    if (i % 2 == 0)
                        directedRectangleLeft = i - (int)Math.Round(i / 2d, MidpointRounding.AwayFromZero)+1;
                    else
                        directedRectangleRight = i - (int)Math.Round(i / 2d, MidpointRounding.AwayFromZero)+1;
                }
                         
            }
           
               
            leftRectIndex = -1;
            rightRectIndex = -1;
            //check if hand is contained in one of the rectangles
            if (lastHandMetadata.LeftHandPosition[0] + lastHandMetadata.LeftHandPosition[1] > 0)
                leftRectIndex = rectangleManager.GetPointInRectangle(lastHandMetadata.LeftHandPosition[0], lastHandMetadata.LeftHandPosition[1]);
            if (lastHandMetadata.RightHandPosition[0] + lastHandMetadata.RightHandPosition[1] > 0)
                rightRectIndex = rectangleManager.GetPointInRectangle(lastHandMetadata.RightHandPosition[0], lastHandMetadata.RightHandPosition[1]);

            //test if sequense is ok
            currentStateLeft = sequenceManager.GetState(leftRectIndex, directedRectangleLeft, true);
            currentStateRight = sequenceManager.GetState(rightRectIndex, directedRectangleRight, false);
            #endregion SEQUENCE MANAGEMENT
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="frameNumber"></param>
        /// <param name="handMetadata"></param>
        private void SaveData(PXCMHandData data, int frameNumber, HandMetadata handMetadata)
        {
            lastData = data;
            lastFrameNumber = frameNumber;
            lastHandMetadata = handMetadata;

            history.Save(handMetadata);

            label_completed.Content = "" + sequenceManager.SequenceCounter;
            label_error.Content = "" + sequenceManager.ErrorCounter;
        }

        //
        private void DrawRGBImage(Bitmap bitmap)
        {
            //miror
            image.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            ScaleTransform scaleTransform = new ScaleTransform
            {
                ScaleX = -1,
                ScaleY = 1
            };
            image.RenderTransform = scaleTransform;
            
            //draw stuff
            if (check_hands.IsChecked == true)
                drawingManager.DrawHands(bitmap, recognition, lastData, lastFrameNumber);
            //drawingManager.DrawHistory(bitmap, history);
            if (check_direction_vectors.IsChecked == true)
                drawingManager.DrawHandDirectionVector(bitmap, lastHandMetadata, history);
            if (check_distance_vectors.IsChecked == true)
                drawingManager.DrawDistanceVector(bitmap, lastHandMetadata, rectangleManager.GetAll());
            drawingManager?.Rectangles_DrawAll(bitmap, lastHandMetadata, rectangleManager, sequenceManager, currentStateLeft, currentStateRight, leftRectIndex, rightRectIndex, directedRectangleRight);
            
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
        #endregion RECOGNITION EVENTS MANAGEMENT
    }
}
