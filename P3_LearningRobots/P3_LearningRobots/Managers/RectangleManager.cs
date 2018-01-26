using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace P3_LearningRobots
{
    public class RectangleManager
    {
        #region ATTRIBUTES
        private List<Rectangle> rectangles = new List<Rectangle>();

        #endregion ATTRIBUTES

        //constructor
        public RectangleManager(int rectanglesToCreate)
        {
            int x = 75;
            int y = 250;
            int width = 150;
            int height = 150;

            for (int i = 0; i < rectanglesToCreate; i++)
            {
                rectangles.Add(new Rectangle(x, y, width, height));
                x += width + 20;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public List<float> getPosition(Rectangle r)
        {
            List<float> coordinates = new List<float>()
            {
                (float)r.X,
                (float)r.Y,
                (float)r.Width,
                (float)r.Height
            };

            return coordinates;
        }

        //destroy all the rectangles contained in the list
        public void DestroyAll()
        {
            rectangles.Clear();
        }

        //get rectangle at a certain index
        public Rectangle GetRectangleAt(int index)
        {
            return rectangles.ElementAt(index);
        }

        //get all rectangles from the list
        public List<Rectangle> GetAll()
        {
            return rectangles;
        }

        //current -> Sequence Manager: x,y -> point
        public int GetPointInRectangle(double x, double y)
        {
            //for each rectangle check if point contained in X, return X's index, -1 otherwise
            for (int i = 0; i < rectangles.Count; i++)
            {
                if (IsPointInRectangle(x, y, new double[4, 2] {
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

        //checks if point is contained in rectangle 
        private bool IsPointInRectangle(double x, double y, double[,] rectangle)
        {
            return x > rectangle[0, 0] && x < rectangle[1, 0] && y > rectangle[0, 1] && y < rectangle[2, 1];
        }
    }
}
