using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace DisplayHands
{
    class DrawingManager
    {
        #region ATTRIBUTES
        public int scale = 2;
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

        //HISTORY
        public void DrawHistory(Bitmap bitmap, History history)
        {
            Graphics g = Graphics.FromImage(bitmap);
            Pen penLeft = new Pen(Color.Red, 3.0f);
            Pen penRight = new Pen(Color.Blue, 3.0f);

            IList<HandMetadata> historyList = history.GetList();

            {
                Point[] leftPoints = new Point[historyList.Count];

                Point[] rightPoints = new Point[historyList.Count];

                for (int i = 0; i < historyList.Count; i++)
                {
                    leftPoints[i] = new Point((int)historyList[i].LeftHandPosition[0], (int)historyList[i].LeftHandPosition[1]);
                    rightPoints[i] = new Point((int)historyList[i].RightHandPosition[0], (int)historyList[i].RightHandPosition[1]);
                }

                if (leftPoints.Last().X + leftPoints.Last().Y > 0)
                    g.DrawLines(penLeft, leftPoints);
                if (rightPoints.Last().X + rightPoints.Last().Y > 0)
                    g.DrawLines(penRight, rightPoints);
            }
        }

        //VECTOR
        public void DrawVector(Bitmap bitmap, HandMetadata handMetadata, History history)
        {
            Utils utils = new Utils();
            //vector = 0B - 0A
            double[][] v = utils.CalculateNaiveVector(scale, history);

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
                    g.DrawLine(penLeft, new Point((int)leftX, (int)leftY), new Point((int)leftX + (int)v[0][0], (int)leftY + (int)v[0][1]));
                    g.DrawEllipse(penLeft, new RectangleF((float)leftX + (float)v[0][0], (float)leftY + (float)v[0][1], 5, 5));
                }

                if (rightX + rightY > 0)
                {
                    g.DrawLine(penRight, new Point((int)rightX, (int)rightY), new Point((int)rightX + (int)v[1][0], (int)rightY + (int)v[1][1]));
                    g.DrawEllipse(penRight, new RectangleF((float)rightX + (float)v[1][0], (float)rightY + (float)v[1][1], 5, 5));
                }
            }
        }

        //RECTANGLE MANAGER
        public void Rectangles_DrawAll(Bitmap bitmap, HandMetadata handMetadata, RectangleManager rectangleManager, 
            State currentStateLeft, State currentStateRight ,
            int leftRectangle, int rightRectangle, int directedRectangle)
        {
            if (rectangleManager != null)
            {
                List<Rectangle> rectangles = rectangleManager.GetAll();

                if (rectangles != null)
                {
                    Graphics g = Graphics.FromImage(bitmap);

                    char sequence = 'A';

                    double leftX = handMetadata.LeftHandPosition[0];
                    double leftY = handMetadata.LeftHandPosition[1];
                    double rightX = handMetadata.RightHandPosition[0];
                    double rightY = handMetadata.RightHandPosition[1];

                    for (int i = 0; i < rectangles.Count; i++)
                    {
                        List<double> distances = rectangleManager.CalculateDistance(handMetadata, rectangles[i]);
                        #region TEXT
                        Font drawFont = new Font("Arial", 10);
                        SolidBrush drawBrush = new SolidBrush(Color.White);

                        Font drawFontD = new Font("Arial", 10);
                        SolidBrush drawBrushD = new SolidBrush(Color.OrangeRed);

                        GraphicsState state = g.Save();

                        g.ResetTransform();
                        g.ScaleTransform(-1, 1);

                        //sequence
                        g.TranslateTransform(rectangles[i].X + (rectangles[i].Width / 2) + (drawFont.Size / 2), rectangles[i].Y - 20, MatrixOrder.Append);
                        g.DrawString(sequence.ToString(), drawFont, drawBrush, 0, 0);
                        sequence += (char)1;

                        //left
                        if (leftX + leftY > 0)
                            g.DrawString(("L " + (int)distances.First()).ToString(), drawFontD, drawBrushD, 0, (rectangles[i].Height / 2) - drawFontD.Size);

                        //right
                        if (rightX + rightY > 0)
                            g.DrawString(("R " + (int)distances.Last()).ToString(), drawFontD, drawBrushD, 0, (rectangles[i].Height / 2) + drawFontD.Size);

                        //probability: distance


                        g.Restore(state);
                        #endregion TEXT

                        Pen pen = new Pen(Color.Black, 5);    //undefined

                        switch (currentStateLeft)
                        {
                            case State.SUCCESS:
                                if(leftRectangle == i)
                                    pen = new Pen(Color.Green, 5);
                                break;
                            case State.OK:
                                if (directedRectangle == i)
                                    pen = new Pen(Color.Blue, 5);
                                break;
                            case State.WARNING:
                                if (directedRectangle == i)
                                    pen = new Pen(Color.Orange, 5);
                                break;
                            case State.ERROR:
                                if (leftRectangle == i)
                                    pen = new Pen(Color.Red, 5);
                                break;
                        }

                        switch (currentStateRight)
                        {
                            case State.SUCCESS:
                                if (rightRectangle == i)
                                    pen = new Pen(Color.Green, 5);
                                break;
                            case State.OK:
                                if (directedRectangle == i)
                                    pen = new Pen(Color.Blue, 5);
                                break;
                            case State.WARNING:
                                if (directedRectangle == i)
                                    pen = new Pen(Color.Orange, 5);
                                break;
                            case State.ERROR:
                                if (rightRectangle == i)
                                    pen = new Pen(Color.Red, 5);
                                break;
                        }

                        g.DrawRectangle(pen, rectangles[i]);
                    }
                }
            }
        }
        
    }
}
