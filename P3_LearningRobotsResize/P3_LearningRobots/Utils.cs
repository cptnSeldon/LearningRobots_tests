using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace P3_LearningRobots
{
    class Utils
    {
        //get a rectangle's center
        public static PointF GetCenter(Rect r)
        {
            return new PointF((float)(r.X + (r.Width / 2)), (float)(r.Y + (r.Height / 2)));
        }

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
        public static List<double> GetTotalDistances(PointF left, PointF right, List<Rect> rectangles)
        {
            List<double> totalDistances = new List<double>
            {
                [0] = 0,  //left
                [1] = 0  //right
            };

            if (rectangles != null)
            {
                foreach (Rect r in rectangles)
                {
                    if (left.X + left.Y + right.X + right.Y == 0)
                    {
                        totalDistances[0] += CalculateDistance(left, right, r)[0];
                        totalDistances[1] += CalculateDistance(left, right, r)[1];
                    }
                }
            }

            return totalDistances;
        }

        //calculate the distance from both hands to a rectangle
        public static List<double> CalculateDistance(PointF left, PointF right, Rect r)
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

        public static List<Tuple<int, int>> GetPercentage(PointF left, PointF right, List<Rect> rectangles)
        {
            List<Tuple<int, int>> percentages = new List<Tuple<int, int>>();
            List<double> total = GetTotalDistances(left, right, rectangles);

            double totalDistanceLeft = total[0];
            double totalDistanceRight = total[1];

            foreach (Rect r in rectangles)
            {
                percentages.Add( new Tuple<int, int>((int)(CalculateDistance(left, right, r)[0] / totalDistanceLeft), (int)(CalculateDistance(left, right, r)[1] / totalDistanceRight)));
            }

            return percentages;
        }

    }
}
