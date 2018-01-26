using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace P3_LearningRobots
{
    public class Utils
    {
        //get a rectangle's center
        public static PointF GetCenter(Rectangle r)
        {
            return new PointF(r.X + (r.Width / 2), r.Y + (r.Height / 2));
        }

        //direction vector
        public static double[][] CalculateNaiveVector(int scale, History history)
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

        //distance vector
        public static double[][] CalculateVector(double[][] rectangleCenter, History history)
        {

            IList<HandMetadata> historyList = history.GetList();

            if (historyList.Count > 0)
            {

                double[][] leftPoints = new double[1][];
                double[][] rightPoints = new double[1][];

                leftPoints[0] = new double[] { historyList[0].LeftHandPosition[0], historyList[0].LeftHandPosition[1] };
                rightPoints[0] = new double[] { historyList[0].RightHandPosition[0], historyList[0].RightHandPosition[1] };

                double[][] vector = new double[2][];

                vector[0] = new double[] { leftPoints[0][0], rectangleCenter[0][0] };
                vector[1] = new double[] { rightPoints[0][0], rectangleCenter[0][0] };

                return vector;
            }
            return null;
        }
        
        //get total distances from each hand to each boxes
        public static List<double> GetTotalDistances(PointF left, PointF right, List<Rectangle> rectangles)
        {
            List<double> totalDistances = new List<double>
            {
                0,  //left
                0  //right
            };

            if (rectangles != null)
            {
                foreach (Rectangle r in rectangles)
                {
                    if (left.X + left.Y + right.X + right.Y != 0)
                    {
                        totalDistances[0] += CalculateDistance(left, right, r)[0];
                        totalDistances[1] += CalculateDistance(left, right, r)[1];
                    }
                }
            }

            return totalDistances;
        }

        //calculate the distance from both hands to a rectangle
        public static List<double> CalculateDistance(PointF left, PointF right, Rectangle r)
        {
            //box center
            float centerX = GetCenter(r).X;
            float centerY = GetCenter(r).Y;

            //left
            double left_distanceSquared = ((left.X - centerX) * (left.X - centerX)) + ((left.Y - centerY) * (left.Y - centerY));
            double left_distance = Math.Sqrt(left_distanceSquared);
            //right
            double right_distanceSquared = ((right.X - centerX) * (right.X - centerX)) + ((right.Y - centerY) * (right.Y - centerY));
            double right_distance = Math.Sqrt(right_distanceSquared);

            List<double> distances = new List<double>
            {
                left_distance,
                right_distance
            };

            return distances;
        }

        public static List<double> GetPercentage(PointF left, PointF right, List<Rectangle> rectangles)
        {
            List<double> percentages = new List<double>();
            List<double> total = GetTotalDistances(left, right, rectangles);

            double leftTotalPercentage = 0;
            double rightTotalPercentage = 0;

            double totalDistanceLeft = total.First();
            double totalDistanceRight = total.Last();

            foreach (Rectangle r in rectangles)
            {
                double p = 1 - ((CalculateDistance(left, right, r).First() / totalDistanceLeft));
                percentages.Add(p);
                leftTotalPercentage += p;
                p = 1 - ((CalculateDistance(left, right, r).Last() / totalDistanceRight));
                percentages.Add(p);
                rightTotalPercentage += p;
            }

            if (leftTotalPercentage == 0)
                leftTotalPercentage = 1;

            if (rightTotalPercentage == 0)
                rightTotalPercentage = 1;

            //special case if there's only one zone
            if (percentages.Count == 2)
            {
                percentages[0] = 1;
                percentages[1] = 1;
            }
            else
            {
                for (int i = 0; i < rectangles.Count; i++)
                {
                    percentages[2 * i] /= leftTotalPercentage;
                    percentages[2 * i + 1] /= rightTotalPercentage;
                }
            }
            
            
            

            return percentages;
        }
    }
}
