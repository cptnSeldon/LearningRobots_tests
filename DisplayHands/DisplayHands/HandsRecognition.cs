using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

/// <summary>
/// WPF version of IntelRealSense's SDK samples (converted from WindowsForm): HandViewer C#
/// </summary>
namespace DisplayHands
{
    class HandsRecognition
    {
        #region ATTRIBUTES
        public event Action<PXCMHandData, int, HandMetadata> NewDataEvent; //PXCMHandData : manages handtracking module output data
        public event Action<Bitmap> NewRGBImageEvent; //PXCMHandData : manages handtracking module output data

        public PXCMSession g_session;   //maintains the SDK context
        public Dictionary<string, PXCMCapture.DeviceInfo> Devices { get; set; } //devices' information
        private readonly Queue<PXCMImage> _mImages; //contains depth image - synchronization purpose
        private PXCMCapture _capture;   //provides functions to query video capture devices + their instance creation

        private PXCMHandData.BodySideType[] bodySideType;

        private const int NumberOfFramesToDelay = 3;

        private float _maxRange;    //maximum range

        //device's state
        private bool _disconnected = false;
        private bool isStopping = false;
        #endregion ATTRIBUTES

        /// <summary>
        /// Constructor: initialize current session
        /// </summary>
        public HandsRecognition() {

            _mImages = new Queue<PXCMImage>();  //PXCMImage: image buffer access, here put into a queue
            g_session = PXCMSession.CreateInstance();   //session instance creation
            bodySideType = new PXCMHandData.BodySideType[2];    //sides: left, right, unknown
        }


        /* Displaying current frames hand joints */
        /// <summary>
        /// DisplayJoints: 
        /// </summary>
        /// <param name="handOutput"></param>
        /// <param name="bitmap"></param>
        /// <param name="timeStamp"></param>
        public void DisplayJoints(PXCMHandData handOutput, Bitmap bitmap, long timeStamp = 0)
        {
            
            //Iterate hands
            PXCMHandData.JointData[][] nodes = new PXCMHandData.JointData[][] { new PXCMHandData.JointData[PXCMHandData.NUMBER_OF_JOINTS], new PXCMHandData.JointData[PXCMHandData.NUMBER_OF_JOINTS] };
            PXCMHandData.ExtremityData[][] extremityNodes = new PXCMHandData.ExtremityData[][] { new PXCMHandData.ExtremityData[PXCMHandData.NUMBER_OF_EXTREMITIES], new PXCMHandData.ExtremityData[PXCMHandData.NUMBER_OF_EXTREMITIES] };

            int numOfHands = handOutput.QueryNumberOfHands();

            for (int i = 0; i < numOfHands; i++)
            {
                
                #region GET HAND BY TIME OF APPEARANCE
                if (handOutput.QueryHandData(PXCMHandData.AccessOrderType.ACCESS_ORDER_BY_TIME, i, out PXCMHandData.IHand handData) == pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    if (handData != null)
                    {
                       //retrieve body side per hand
                       bodySideType[i] = handData.QueryBodySide();

                        #region ITERATE OVER JOINTS
                        for (int j = 0; j < PXCMHandData.NUMBER_OF_JOINTS; j++)
                        {
                            handData.QueryTrackedJoint((PXCMHandData.JointType)j, out PXCMHandData.JointData jointData);
                            nodes[i][j] = jointData;

                        }
                        #endregion ITERATE OVER JOINTS

                        #region ITERATE OVER EXTREMITY POINTS 
                        for (int j = 0; j < PXCMHandData.NUMBER_OF_EXTREMITIES; j++)
                        {
                            handData.QueryExtremityPoint((PXCMHandData.ExtremityType)j, out extremityNodes[i][j]);
                        }
                        #endregion ITERATE OVER EXTREMITY POINTS
                    }
                }
                #endregion GET HAND BY TIME OF APPEARANCE
            }

            //draw joints on screen
            DrawJoints(nodes, bitmap, numOfHands, bodySideType);

           /* if (numOfHands > 0)
            {
            
                    DisplayExtremities(numOfHands, extremityNodes);
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="bitmap"></param>
        /// <param name="numOfHands"></param>
        private void DrawJoints(PXCMHandData.JointData[][] nodes, Bitmap bitmap, int numOfHands, PXCMHandData.BodySideType[] bodySideType)
        {
            //nothing is drawn if no data is received
            if (bitmap == null) return;
            if (nodes == null) return;

            //critical section lock
            lock (this)
            {
                int scaleFactor = 1;

                Graphics g = Graphics.FromImage(bitmap);

                using (Pen boneColor = new Pen(Color.DodgerBlue, 3.0f))
                {
                    for (int i = 0; i < numOfHands; i++)
                    {
                        //TODO: correct SCALE FACTOR
                        if (nodes[i][0] == null) continue;
                        int baseX = (int)nodes[i][0].positionImage.x / scaleFactor;
                        int baseY = (int)nodes[i][0].positionImage.y / scaleFactor;

                        int wristX = (int)nodes[i][0].positionImage.x / scaleFactor;
                        int wristY = (int)nodes[i][0].positionImage.y / scaleFactor;

                        #region HAND ID
                        Font drawFont = new Font("Arial", 10);
                        SolidBrush drawBrush = new SolidBrush(Color.White);

                        GraphicsState state = g.Save();
                        g.ResetTransform();
                        g.ScaleTransform(-1, 1);

                        g.TranslateTransform(wristX, wristY+5, MatrixOrder.Append);
                        g.DrawString(bodySideType[i].ToString(), drawFont, drawBrush, 0, 0);

                        g.Restore(state);

                        #endregion HAND ID

                        for (int j = 1; j < 22; j++)
                        {
                            if (nodes[i][j] == null) continue;
                            int x = (int)nodes[i][j].positionImage.x / scaleFactor;
                            int y = (int)nodes[i][j].positionImage.y / scaleFactor;

                            if (nodes[i][j].confidence <= 0) continue;

                            if (j == 2 || j == 6 || j == 10 || j == 14 || j == 18)
                            {

                                baseX = wristX;
                                baseY = wristY;
                            }

                            g.DrawLine(boneColor, new Point(baseX, baseY), new Point(x, y));
                            baseX = x;
                            baseY = y;

                        }



                        using (
                            Pen red = new Pen(Color.Red, 3.0f),
                                black = new Pen(Color.Black, 3.0f),
                                green = new Pen(Color.Green, 3.0f),
                                blue = new Pen(Color.Blue, 3.0f),
                                cyan = new Pen(Color.Cyan, 3.0f),
                                yellow = new Pen(Color.Yellow, 3.0f),
                                orange = new Pen(Color.Orange, 3.0f))
                        {
                            Pen currentPen = black;

                            for (int j = 0; j < PXCMHandData.NUMBER_OF_JOINTS; j++)
                            {
                                float sz = 4;

                                int x = (int)nodes[i][j].positionImage.x / scaleFactor;
                                int y = (int)nodes[i][j].positionImage.y / scaleFactor;

                                if (nodes[i][j].confidence <= 0) continue;

                                //Wrist
                                if (j == 0)
                                {
                                    currentPen = black;
                                }

                                //Center
                                if (j == 1)
                                {
                                    currentPen = red;
                                    sz += 4;

                                    Console.WriteLine("x: " + x + ", y: " + y);

                                }

                                //Thumb
                                if (j == 2 || j == 3 || j == 4 || j == 5)
                                {
                                    currentPen = green;
                                }
                                //Index Finger
                                if (j == 6 || j == 7 || j == 8 || j == 9)
                                {
                                    currentPen = blue;
                                }
                                //Finger
                                if (j == 10 || j == 11 || j == 12 || j == 13)
                                {
                                    currentPen = yellow;
                                }
                                //Ring Finger
                                if (j == 14 || j == 15 || j == 16 || j == 17)
                                {
                                    currentPen = cyan;
                                }
                                //Pinkey
                                if (j == 18 || j == 19 || j == 20 || j == 21)
                                {
                                    currentPen = orange;
                                }


                                if (j == 5 || j == 9 || j == 13 || j == 17 || j == 21)
                                {
                                    sz += 4;
                                }

                                g.DrawEllipse(currentPen, x - sz / 2, y - sz / 2, sz, sz);
                            }
                        }

                    }
                }
                g.Dispose();
            }

        }

        public void SimplePipeline(string filename, bool isRecord)
        {
            
            Console.WriteLine("Started pipeline");
            bool liveCamera = false;

            bool flag = true;
            PXCMSenseManager instance = null;
            _disconnected = false;
            instance = g_session.CreateSenseManager();
            if (instance == null)
            {
                System.Console.WriteLine("Failed to initialise");
                return;
            }

            //check if recording or not
            if (!string.IsNullOrEmpty(filename))
                instance.captureManager.SetFileName(filename, isRecord);
            
            PXCMCaptureManager captureManager = instance.captureManager;
            PXCMCapture.DeviceInfo info = null;
            if (captureManager != null)
            {

                Devices = new Dictionary<string, PXCMCapture.DeviceInfo>();

                PXCMSession.ImplDesc desc = new PXCMSession.ImplDesc
                {
                    @group = PXCMSession.ImplGroup.IMPL_GROUP_SENSOR,
                    subgroup = PXCMSession.ImplSubgroup.IMPL_SUBGROUP_VIDEO_CAPTURE
                };
                for (int i = 0; ; i++)
                {
                    if (g_session.QueryImpl(desc, i, out PXCMSession.ImplDesc desc1) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;

                    if (g_session.CreateImpl<PXCMCapture>(desc1, out _capture) < pxcmStatus.PXCM_STATUS_NO_ERROR) continue;
                    
                    for (int j = 0; ; j++)
                    {
                        if (_capture.QueryDeviceInfo(j, out PXCMCapture.DeviceInfo dinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                        string name = dinfo.name;
                        if (Devices.ContainsKey(dinfo.name))
                        {
                            name += j;
                        }
                        Devices.Add(name, dinfo);
                    }
                }

                //Change this if you want an other camera sensor
                info = Devices.Values.First<PXCMCapture.DeviceInfo>();
                
                captureManager.FilterByDeviceInfo(info);
                liveCamera = true;
                
                if (info == null)
                {
                    Console.WriteLine("Device Failure");
                    return;
                }

            }

            /* Set Module */

            PXCMHandModule handAnalysis;

            PXCMSenseManager.Handler handler = new PXCMSenseManager.Handler
            {
                onModuleProcessedFrame = new PXCMSenseManager.Handler.OnModuleProcessedFrameDelegate(OnNewFrame)
            };


            PXCMHandConfiguration handConfiguration = null;
            PXCMHandData handData = null;

            instance.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 640, 480, 30);

            pxcmStatus status = instance.EnableHand();
            handAnalysis = instance.QueryHand();

            if (status != pxcmStatus.PXCM_STATUS_NO_ERROR || handAnalysis == null)
            {
                Console.WriteLine("Failed Loading Module");

                return;
            }

            handConfiguration = handAnalysis.CreateActiveConfiguration();
            if (handConfiguration == null)
            {
                Console.WriteLine("Failed Create Configuration");
                instance.Close();
                instance.Dispose();
                return;
            }
            handData = handAnalysis.CreateOutput();
            if (handData == null)
            {
                Console.WriteLine("Failed Create Output");
                handConfiguration.Dispose();
                instance.Close();
                instance.Dispose();
                return;
            }

            
            FPSTimer timer = new FPSTimer();
            Console.WriteLine("Init Started");
            if (instance.Init(handler) == pxcmStatus.PXCM_STATUS_NO_ERROR)
            {

                PXCMCapture.DeviceModel dModel = PXCMCapture.DeviceModel.DEVICE_MODEL_F200;
                PXCMCapture.Device device = instance.captureManager.device;
                if (device != null)
                {
                    device.QueryDeviceInfo(out PXCMCapture.DeviceInfo dinfo);
                    dModel = dinfo.model;
                    _maxRange = device.QueryDepthSensorRange().max;

                }

                
                if (handConfiguration != null)
                {
                    PXCMHandData.TrackingModeType trackingMode = PXCMHandData.TrackingModeType.TRACKING_MODE_FULL_HAND;
                    
                    trackingMode = PXCMHandData.TrackingModeType.TRACKING_MODE_FULL_HAND;

                    handConfiguration.SetTrackingMode(trackingMode);

                    handConfiguration.EnableAllAlerts();
                    handConfiguration.EnableSegmentationImage(true);
                    bool isEnabled = handConfiguration.IsSegmentationImageEnabled();

                    handConfiguration.ApplyChanges();
                    
                }

                Console.WriteLine("Streaming");
                int frameCounter = 0;
                int frameNumber = 0;

                while (!isStopping)
                {
                    
                    if (instance.AcquireFrame(true) < pxcmStatus.PXCM_STATUS_NO_ERROR)
                    {
                        break;
                    }

                    frameCounter++;

                    if (!DisplayDeviceConnection(!instance.IsConnected()))
                    {
                        PXCMCapture.Sample sample = instance.QueryHandSample();
                        
                        if (sample != null && sample.depth != null)
                        {
                            frameNumber = liveCamera ? frameCounter : instance.captureManager.QueryFrameIndex();
                            
                            if (handData != null)
                            {
                                handData.Update();
                                NewDataEvent?.Invoke(handData, frameNumber, getMetaData(handData));
                                //DisplayPicture(sample.depth, handData);
                                //DisplayGesture(handData, frameNumber);
                                //DisplayJoints(handData,bitmap);
                                //DisplayAlerts(handData, frameNumber);
                            }
                        }
                        OnNewSample(instance.QuerySample());
                        timer.Tick();
                    }
                    instance.ReleaseFrame();
                }
            }
            else
            {
                Console.WriteLine("Init Failed");
                flag = false;
            }
            foreach (PXCMImage pxcmImage in _mImages)
            {
                pxcmImage.Dispose();
            }

            // Clean Up
            if (handData != null) handData.Dispose();
            if (handConfiguration != null) handConfiguration.Dispose();
            
          
            instance.Close();
            instance.Dispose();

            if (flag)
            {
                Console.WriteLine("Stopped");
            }
        }

        public void OnNewSample(PXCMCapture.Sample sample) {
            if (sample == null || sample.color ==  null)
                return;

            sample.color.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_RGB32, out PXCMImage.ImageData data);
            Bitmap bitmap = data.ToBitmap(0, sample.color.info.width, sample.color.info.height);
            if (bitmap != null)
            {
                NewRGBImageEvent?.Invoke(bitmap);
            }
        }
        public void SignalStop() { isStopping = true; }

        public static pxcmStatus OnNewFrame(Int32 mid, PXCMBase module, PXCMCapture.Sample sample)
        {
            return pxcmStatus.PXCM_STATUS_NO_ERROR;
        }

        /// <summary>
        /// Device connection display
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private bool DisplayDeviceConnection(bool state)
        {
            if (state)
            {
                if (!_disconnected) Console.WriteLine("Device Disconnected");
                _disconnected = true;
            }
            else
            {
                if (_disconnected) Console.WriteLine("Device Reconnected");
                _disconnected = false;
            }
            return _disconnected;
        }


        private void DisplayGesture(PXCMHandData handAnalysis, int frameNumber)
        {

            int firedGesturesNumber = handAnalysis.QueryFiredGesturesNumber();
            string gestureStatusLeft = string.Empty;
            string gestureStatusRight = string.Empty;

            if (firedGesturesNumber == 0)
            {
                return;
            }

            for (int i = 0; i < firedGesturesNumber; i++)
            {
                PXCMHandData.GestureData gestureData;
                if (handAnalysis.QueryFiredGestureData(i, out gestureData) == pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    PXCMHandData.IHand handData;
                    if (handAnalysis.QueryHandDataById(gestureData.handId, out handData) != pxcmStatus.PXCM_STATUS_NO_ERROR)
                        return;

                    PXCMHandData.BodySideType bodySideType = handData.QueryBodySide();
                    if (bodySideType == PXCMHandData.BodySideType.BODY_SIDE_LEFT)
                    {
                        gestureStatusLeft += "Left Hand Gesture: " + gestureData.name;

                        Console.WriteLine("Gesture: " + gestureStatusLeft);

                    }
                    else if (bodySideType == PXCMHandData.BodySideType.BODY_SIDE_RIGHT)
                    {
                        gestureStatusRight += "Right Hand Gesture: " + gestureData.name;

                        Console.WriteLine("Gesture: " + gestureStatusRight);

                    }
                }
            }
        }

        private HandMetadata getMetaData(PXCMHandData data)
        {
            data.QueryHandData(PXCMHandData.AccessOrderType.ACCESS_ORDER_LEFT_HANDS, 0, out PXCMHandData.IHand leftHand);

            PXCMPointF32 leftPosition = leftHand?.QueryMassCenterImage() ?? new PXCMPointF32();

            data.QueryHandData(PXCMHandData.AccessOrderType.ACCESS_ORDER_RIGHT_HANDS, 0, out PXCMHandData.IHand rightHand);

            PXCMPointF32 rightPosition = rightHand?.QueryMassCenterImage() ?? new PXCMPointF32();

            return new HandMetadata(new double[] { leftPosition.x, leftPosition.y}, new double[] { rightPosition.x, rightPosition.y });
        }
    }
}
