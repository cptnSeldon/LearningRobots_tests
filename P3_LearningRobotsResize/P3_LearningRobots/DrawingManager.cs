using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace P3_LearningRobots
{
    class DrawingManager
    {
        #region ATTRIBUTES
        public int scale = 5;
        State lastStateRight = State.UNDEFINED;
        State lastStateLeft = State.UNDEFINED;
        #endregion ATTRIBUTES

        //constructor
        public DrawingManager() { }

        //HANDS
        public void DrawHands(Bitmap bitmap, HandsRecognition recognition, PXCMHandData lastData, int lastFrameNumber)
        {
            if (lastData == null)
                return;
            //Bitmap bitmap = new Bitmap((int)image.Width, (int)image.Height);

            recognition.DisplayJoints(lastData, bitmap, lastFrameNumber);
        }
        
        public void DrawJoints(PXCMHandData.JointData[][] nodes, Bitmap bitmap, int numOfHands, PXCMHandData.BodySideType[] bodySideType)
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

                        g.TranslateTransform(wristX, wristY + 5, MatrixOrder.Append);
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

                            g.DrawLine(boneColor, new System.Drawing.Point(baseX, baseY), new System.Drawing.Point(x, y));
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

        //HISTORY
        public void DrawHistory(Bitmap bitmap, History history)
        {
            Graphics g = Graphics.FromImage(bitmap);
            Pen penLeft = new Pen(Color.Red, 3.0f);
            Pen penRight = new Pen(Color.Blue, 3.0f);

            IList<HandMetadata> historyList = history.GetList();

            {
                System.Drawing.Point[] leftPoints = new System.Drawing.Point[historyList.Count];

                System.Drawing.Point[] rightPoints = new System.Drawing.Point[historyList.Count];

                for (int i = 0; i < historyList.Count; i++)
                {
                    leftPoints[i] = new System.Drawing.Point((int)historyList[i].LeftHandPosition[0], (int)historyList[i].LeftHandPosition[1]);
                    rightPoints[i] = new System.Drawing.Point((int)historyList[i].RightHandPosition[0], (int)historyList[i].RightHandPosition[1]);
                }

                if (leftPoints.Last().X + leftPoints.Last().Y > 0)
                    g.DrawLines(penLeft, leftPoints);
                if (rightPoints.Last().X + rightPoints.Last().Y > 0)
                    g.DrawLines(penRight, rightPoints);
            }
        }

        //VECTORS
        public void DrawHandDirectionVector(Bitmap bitmap, HandMetadata handMetadata, History history)
        {
            //vector = 0B - 0A
            double[][] v = Utils.CalculateNaiveVector(scale, history);

            if (v != null)
            {
                Graphics g = Graphics.FromImage(bitmap);
                Pen penLeft = new Pen(Color.Red, 3.0f);
                Pen penRight = new Pen(Color.Blue, 3.0f);

                double leftX = handMetadata.LeftHandPosition[0];
                double leftY = handMetadata.LeftHandPosition[1];
                double rightX = handMetadata.RightHandPosition[0];
                double rightY = handMetadata.RightHandPosition[1];

                //v[left or right][point1 or point2][x or y]
                if (leftX + leftY > 0)
                {
                    g.DrawLine(penLeft, (int)leftX, (int)leftY, (int)leftX + (int)v[0][0], (int)leftY + (int)v[0][1]);
                    g.DrawEllipse(penLeft, new RectangleF((float)leftX + (float)v[0][0], (float)leftY + (float)v[0][1], 5, 5));
                }

                if (rightX + rightY > 0)
                {
                    g.DrawLine(penRight, (int)rightX, (int)rightY, (int)rightX + (int)v[1][0], (int)rightY + (int)v[1][1]);
                    g.DrawEllipse(penRight, new RectangleF((float)rightX + (float)v[1][0], (float)rightY + (float)v[1][1], 5, 5));
                }
            }
        }

        public void DrawDistanceVector(Bitmap bitmap, HandMetadata handMetadata, List<Rect> rectangleList)
        {
            Utils utils = new Utils();

            if (rectangleList != null)
            {
                List<PointF> centers = new List<PointF>();

                foreach (Rect r in rectangleList)
                {
                    centers.Add(Utils.GetCenter(r));
                }

                Graphics g = Graphics.FromImage(bitmap);
                Pen penLeft = new Pen(Color.Red, 3.0f);
                Pen penRight = new Pen(Color.Blue, 3.0f);

                double leftX = handMetadata.LeftHandPosition[0];
                double leftY = handMetadata.LeftHandPosition[1];
                double rightX = handMetadata.RightHandPosition[0];
                double rightY = handMetadata.RightHandPosition[1];

                foreach (PointF coordinate in centers)
                {
                    if (leftX + leftY > 0)
                    {
                        g.DrawLine(penLeft, (int)leftX, (int)leftY, (int)coordinate.X, (int)coordinate.Y);
                    }

                    if (rightX + rightY > 0)
                    {
                        g.DrawLine(penRight, (int)rightX, (int)rightY , (int)coordinate.X, (int)coordinate.Y);
                    }
                }
            }
        }

        //RECTANGLE MANAGER
        public void Rectangles_DrawAll(Bitmap bitmap, 
            HandMetadata handMetadata, RectangleManager rectangleManager, SequenceManager sequenceManager,
            State currentStateLeft, State currentStateRight,
            int leftRectangle, int rightRectangle, int directedRectangle)
        {
            List<Rect> rectangles = rectangleManager.GetAllRect();
            if (rectangles != null)
            {

                if (rectangles != null)
                {
                    Graphics g = Graphics.FromImage(bitmap);

                    char sequence = 'A';

                    float leftX = (float)handMetadata.LeftHandPosition[0];
                    float leftY = (float)handMetadata.LeftHandPosition[1];
                    float rightX = (float)handMetadata.RightHandPosition[0];
                    float rightY = (float)handMetadata.RightHandPosition[1];

                    for (int i = 0; i < rectangles.Count; i++)
                    {
                        List<double> distances = Utils.CalculateDistance(new PointF(leftX, leftY), new PointF(rightX, rightY), rectangles[i]);
                        #region TEXT
                        Font drawFont = new Font("Arial", 10);
                        SolidBrush drawBrush = new SolidBrush(Color.White);

                        Font drawFontD = new Font("Arial", 10);
                        SolidBrush drawBrushD = new SolidBrush(Color.OrangeRed);

                        GraphicsState state = g.Save();

                        g.ResetTransform();
                        g.ScaleTransform(-1, 1);

                        //sequence
                        g.TranslateTransform((float)(rectangles[i].X + (rectangles[i].Width / 2) + (drawFont.Size / 2)), (float)(rectangles[i].Y - 20), MatrixOrder.Append);
                        //g.DrawString(sequence.ToString(), drawFont, drawBrush, 0, 0);
                        sequence += (char)1;

                        //left
                        if (leftX + leftY > 0)
                            g.DrawString(("L " + (int)distances.First()).ToString(), drawFontD, drawBrushD, 0, (float)(rectangles[i].Height / 2) - drawFontD.Size);
                        /*
                        //right
                        if (rightX + rightY > 0)
                            g.DrawString(("R " + (int)distances.Last()).ToString(), drawFontD, drawBrushD, 0, (float)(rectangles[i].Height / 2) + drawFontD.Size);
                            */
                        //probability: distance


                        g.Restore(state);
                        #endregion TEXT

                        System.Windows.Media.Brush fillBrush = System.Windows.Media.Brushes.Black; 
                        System.Windows.Media.Brush strokeBrush = System.Windows.Media.Brushes.Black;
                        double strokeThickness = 3;

                        /**
                            next    -> blue contour
                            ok      -> blue fill
                            warning -> orange fill  + sound
                            success -> green fill
                            error   -> red fill     + sound

                            normal  -> black countour
                         
                         */

                        #region TEST

                        switch (currentStateLeft)
                        {
                            case State.SUCCESS:
                                if (leftRectangle == i)
                                    fillBrush = System.Windows.Media.Brushes.Green;
                                break;
                            case State.OK:
                                if (directedRectangle == i)
                                {
                                    strokeBrush = System.Windows.Media.Brushes.Blue;
                                    strokeThickness = 5;
                                }
                                break;
                            case State.WARNING:
                                if (directedRectangle == i)
                                {
                                    strokeBrush = System.Windows.Media.Brushes.Orange;
                                    strokeThickness = 5;
                                }
                                break;
                            case State.ERROR:
                                if (leftRectangle == i)
                                    fillBrush = System.Windows.Media.Brushes.Red;
                                break;
                        }

                        switch (currentStateRight)
                        {
                            case State.SUCCESS:
                                if (rightRectangle == i)
                                    fillBrush = System.Windows.Media.Brushes.Green;
                                break;
                            case State.OK:
                                if (directedRectangle == i)
                                {
                                    strokeBrush = System.Windows.Media.Brushes.Blue; 
                                    strokeThickness = 5;
                                }
                                    break;
                            case State.WARNING:
                                if (directedRectangle == i)
                                {
                                    strokeBrush = System.Windows.Media.Brushes.Orange;
                                    strokeThickness = 5;
                                }
                                    break;
                            case State.ERROR:
                                if (rightRectangle == i)
                                    fillBrush = System.Windows.Media.Brushes.Red;
                                break;
                        }

                        #endregion TEST

                        if ((lastStateLeft == State.ERROR && currentStateLeft != State.ERROR) ||(lastStateRight == State.ERROR && currentStateRight != State.ERROR))
                        {
                            System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"wrongAnswer.wav");
                            player.Play();

                            sequenceManager.ErrorCounter++;
                        }
                            

                        lastStateRight = currentStateRight;
                        lastStateLeft = currentStateLeft;

                        if (i == sequenceManager.GetCurrent())
                            rectangleManager.SetStrokeBrush(i, System.Windows.Media.Brushes.Blue);

                        if (strokeBrush != System.Windows.Media.Brushes.Black)
                            rectangleManager.SetFillBrush(i, fillBrush);

                        rectangleManager.SetStrokeThickness(i,strokeThickness);

                    }
                }
            }
        }

    }
}
