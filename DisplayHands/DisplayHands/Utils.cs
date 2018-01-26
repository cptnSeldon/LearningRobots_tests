using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DisplayHands
{
    class Utils
    {
        public Utils() { }

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

        public double[][] CalculateVector(double[][] rectangleCenter, History history)
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
                vector[1] = new double[] { rightPoints[0][0], rectangleCenter[0][0]};

                return vector;
            }
            return null;
        }

    }
}
