using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayHands
{
    class ProbabilityManager
    {
        #region ATTRIBUTES

        #endregion ATTRIBUTES

        public ProbabilityManager()
        {

        }

        //using distance from box center
        public List<Tuple<double, double>> UseDistance(HandMetadata handMetadata, RectangleManager rectangleManager)
        {
            List<Tuple<double, double>> probabilities = new List<Tuple<double, double>>();  //left, right
            List<Rectangle> rectangles = rectangleManager.GetAll();
            List<double> totalDistances = rectangleManager.GetTotalDistances(handMetadata);

            for (int i = 0; i < rectangles.Count; i++)
            {
                //get distances for rectangle r (left, right hand)
                List<double> distances = rectangleManager.CalculateDistance(handMetadata, rectangles[i]);
                //calculate probability (percentage)         left                                      right
                probabilities.Add(new Tuple<double, double>((distances[0] / totalDistances[i]) * 100, (distances[1] / totalDistances[i]) * 100));
            }

            return probabilities;
        }

        //using vectors

    }
}
