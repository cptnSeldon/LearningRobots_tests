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
        #region ATTRIBUTES

        #endregion ATTRIBUTES

        public DrawingManager()
        {

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
