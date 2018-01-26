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

        //constructor
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

        //destroy all the rectangles contained in the list
        public void DestroyAll()
        {
            rectangles.Clear();
        }

        //get a rectangle's center
        public PointF GetCenter(Rectangle r)
        {
            return new PointF(r.X + (r.Width / 2), r.Y + (r.Height / 2));
        }

        //get rectangle at a certain index
        public Rectangle GetRectangleAt(int index)
        {
            return rectangles.ElementAt(index);
        }

        //get total distances from each hand to each boxes
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

        //get all rectangles from the list
        public List<Rectangle> GetAll()
        {
            return rectangles;
        }

        //calculate the distance from both hands to a rectangle
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

        //current -> Sequence Manager: x,y -> point
        public int GetPointInRectangle(double x, double y)
        {
            //for each rectangle check if point contained in X, return X's index, -1 otherwise
            for (int i = 0; i < rectangles.Count; i++)
            {
                if (isPointInRectangle(x, y, new double[4, 2] {
                    //00 - 01
                    { (double) rectangles[i].X, (double) rectangles[i].Y }
                    //10 - 11
                   ,{ (double) (rectangles[i].X + rectangles[i].Width), (double)rectangles[i].Y}
                    //20 - 21
                   ,{ (double) (rectangles[i].X + rectangles[i].Width), (double) (rectangles[i].Y + rectangles[i].Height)}
                    //30 - 31
                    ,{ (double) rectangles[i].X, (double) (rectangles[i].Y + rectangles[i].Height) } }))
                    return i;
            }

            return -1;
        }

        //directed -> Sequence Manager
        public int GetPointInRectangle(double[][] vector)
        {
            //TODO: for each rectangle if vector goes towards X, return X's index, -1 otherwise
            Utils utils = new Utils();



            return -1;
        }

        //checks if point is contained in rectangle 
        private bool isPointInRectangle(double x, double y, double[,] rectangle)
        {
            return x > rectangle[0, 0] && x < rectangle[1, 0] && y > rectangle[0 , 1] && y< rectangle[2,1];
        }
    }
}
