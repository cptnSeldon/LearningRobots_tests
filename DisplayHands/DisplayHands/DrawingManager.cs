using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHands
{
    class DrawingManager
    {
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
            System.Drawing.Pen penLeft = new System.Drawing.Pen(System.Drawing.Color.Red, 3.0f);
            System.Drawing.Pen penRight = new System.Drawing.Pen(System.Drawing.Color.Blue, 3.0f);

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

        public double[][] CalculateNaiveVector(int scale, History history)
        {

            IList<HandMetadata> historyList = history.GetList();

            if (historyList.Count > history.MaxHistory - 1)
            {
                double[][] leftPoints = new double[2][];
                double[][] rightPoints = new double[2][];

                leftPoints[1] = new double[] { historyList[historyList.Count - 1].LeftHandPosition[0], historyList[historyList.Count - 1].LeftHandPosition[1] };
                rightPoints[1] = new double[] { historyList[historyList.Count - 1].RightHandPosition[0], historyList[historyList.Count - 1].RightHandPosition[1] };
                leftPoints[0] = new double[] { historyList[0].LeftHandPosition[0], historyList[0].LeftHandPosition[1] };
                rightPoints[0] = new double[] { historyList[0].RightHandPosition[0], historyList[0].RightHandPosition[1] };

                double[][] vector = new double[2][];

                vector[0] = new double[] { (leftPoints[1][0] - leftPoints[0][0]) * scale, (leftPoints[1][1] - leftPoints[0][1]) * scale };
                vector[1] = new double[] { (rightPoints[1][0] - rightPoints[0][0]) * scale, (rightPoints[1][1] - rightPoints[0][1]) * scale };

                return vector;
            }
                return null;
        }

        //VECTOR
        public void DrawVector(Bitmap bitmap, HandMetadata handMetadata, History history)
        {
            //vector = 0B - 0A
            double[][] v = CalculateNaiveVector(2, history);

            if (v != null)
            {
                Graphics g = Graphics.FromImage(bitmap);
                System.Drawing.Pen penLeft = new System.Drawing.Pen(System.Drawing.Color.Red, 3.0f);
                System.Drawing.Pen penRight = new System.Drawing.Pen(System.Drawing.Color.Blue, 3.0f);

                double leftX = handMetadata.LeftHandPosition[0];
                double leftY = handMetadata.LeftHandPosition[1];
                double rightX = handMetadata.RightHandPosition[0];
                double rightY = handMetadata.RightHandPosition[1];

                //v[left or right][point1 or point2][x or y]
                if (leftX + leftY > 0)
                {
                    g.DrawLine(penLeft, new System.Drawing.Point((int)leftX, (int)leftY), new System.Drawing.Point((int)leftX + (int)v[0][0], (int)leftY + (int)v[0][1]));
                    g.DrawEllipse(penLeft, new RectangleF((float)leftX + (float)v[0][0], (float)leftY + (float)v[0][1], 5, 5));
                }

                if (rightX + rightY > 0)
                {
                    g.DrawLine(penRight, new System.Drawing.Point((int)rightX, (int)rightY), new System.Drawing.Point((int)rightX + (int)v[1][0], (int)rightY + (int)v[1][1]));
                    g.DrawEllipse(penRight, new RectangleF((float)rightX + (float)v[1][0], (float)rightY + (float)v[1][1], 5, 5));
                }
            }
        }

        //RECTANGLE MANAGER
        public void Rectangles_DrawAll(Bitmap bitmap, HandMetadata handMetadata, RectangleManager rectangleManager)
        {
            if (rectangleManager != null)
            {
                List<Rectangle> rectangles = rectangleManager.GetAll();

                if (rectangles != null)
                {
                    Graphics g = Graphics.FromImage(bitmap);
                    Pen p = new Pen(Color.Black);
                    char sequence = 'A';

                    double leftX = handMetadata.LeftHandPosition[0];
                    double leftY = handMetadata.LeftHandPosition[1];
                    double rightX = handMetadata.RightHandPosition[0];
                    double rightY = handMetadata.RightHandPosition[1];

                    foreach (Rectangle r in rectangles)
                    {
                        List<double> distances = rectangleManager.CalculateDistance(handMetadata, r);
                        #region TEXT
                        Font drawFont = new Font("Arial", 10);
                        SolidBrush drawBrush = new SolidBrush(Color.White);

                        Font drawFontD = new Font("Arial", 10);
                        SolidBrush drawBrushD = new SolidBrush(Color.OrangeRed);

                        GraphicsState state = g.Save();

                        g.ResetTransform();
                        g.ScaleTransform(-1, 1);

                        //sequence
                        g.TranslateTransform(r.X + (r.Width / 2) + (drawFont.Size / 2), r.Y - 20, MatrixOrder.Append);
                        g.DrawString(sequence.ToString(), drawFont, drawBrush, 0, 0);
                        sequence += (char)1;

                        //left
                        if (leftX + leftY > 0)
                            g.DrawString(("L " + (int)distances.First()).ToString(), drawFontD, drawBrushD, 0, (r.Height / 2) - drawFontD.Size);

                        //right
                        if (rightX + rightY > 0)
                            g.DrawString(("R " + (int)distances.Last()).ToString(), drawFontD, drawBrushD, 0, (r.Height / 2) + drawFontD.Size);

                        //probability: distance


                        g.Restore(state);
                        #endregion TEXT
                        g.DrawRectangle(p, r);
                    }
                }
            }
            
        }
    }
}
