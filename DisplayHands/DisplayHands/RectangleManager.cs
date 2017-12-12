using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DisplayHands
{
    class RectangleManager
    {
        #region ATTRIBUTES
        private List<Rectangle> rectangles = new List<Rectangle>();
        private List<double> totalDistances = new List<double>();
        ProbabilityManager probabilityManager = new ProbabilityManager();
        #endregion ATTRIBUTES

        public RectangleManager(int rectanglesToCreate)
        {
            int x = 50;
            int y = 75;
            int width = 100;
            int height = 100;

            for (int i = 0; i < rectanglesToCreate; i++)
            {
                rectangles.Add(new Rectangle(x, y, width, height));
                x += width + 10;
            }
        }

        public void DestroyAll()
        {
            rectangles.Clear();
        }

        public PointF GetCenter(Rectangle r)
        {
            return new PointF(r.X + (r.Width / 2), r.Y + (r.Height / 2));
        }

        public Rectangle GetRectangleAt(int index)
        {
            return rectangles.ElementAt(index);
        }

        public List<double> GetTotalDistances(HandMetadata handMetadata)
        {
            double leftX = handMetadata.LeftHandPosition[0];
            double leftY = handMetadata.LeftHandPosition[1];
            double rightX = handMetadata.RightHandPosition[0];
            double rightY = handMetadata.RightHandPosition[1];

            totalDistances[0] = 0;  //left
            totalDistances[1] = 0;  //right

            if (rectangles != null)
            {
                foreach (Rectangle r in rectangles)
                {
                    if(leftX + leftY + rightX + rightY == 0)
                    {
                        totalDistances[0] += CalculateDistance(handMetadata, r)[0];
                        totalDistances[1] += CalculateDistance(handMetadata, r)[1];
                    }
                }
            }

            return totalDistances;
        }

        public List<Rectangle> GetAll()
        {
            return rectangles;
        }

        public List<double> CalculateDistance(HandMetadata handMetadata, Rectangle r)
        {
            //hand center
            double leftX = handMetadata.LeftHandPosition[0];
            double leftY = handMetadata.LeftHandPosition[1];
            double rightX = handMetadata.RightHandPosition[0];
            double rightY = handMetadata.RightHandPosition[1];

            //box center
            float centerX = GetCenter(r).X;
            float centerY = GetCenter(r).Y;

            //left
            double left_distanceSquared = ((leftX - centerX) * (leftX - centerX)) + ((leftY - centerY) * (leftY - centerY));
            double left_distance = Math.Sqrt(left_distanceSquared);
            //right
            double right_distanceSquared = ((rightX - centerX) * (rightX - centerX)) + ((rightY - centerY) * (rightY - centerY));
            double right_distance = Math.Sqrt(right_distanceSquared);

            List<double> distances = new List<double>();
            distances.Add(left_distance);
            distances.Add(right_distance);

            return distances;
        }
    }
}
