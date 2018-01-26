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
    class RectangleManager
    {
        #region ATTRIBUTES
        private List<ResizableRectangle> rectangles = new List<ResizableRectangle>();

        #endregion ATTRIBUTES

        //constructor
        public RectangleManager(int rectanglesToCreate)
        {
            for (int i = 0; i < rectanglesToCreate; i++)
            {
                rectangles.Add(new ResizableRectangle());
            }
        }

        public void SetInitialPosition()
        {
            double x = 50;
            double y = 75;
            foreach (ResizableRectangle rr in rectangles)
            {
                Canvas.SetLeft(rr, x);
                Canvas.SetTop(rr, y);
                x += rr.Width + 10;

            }
        }

        //destroy all the rectangles contained in the list
        public void DestroyAll()
        {
            rectangles.Clear();
        }

        //get rectangle at a certain index
        public ResizableRectangle GetRectangleAt(int index)
        {
            return rectangles.ElementAt(index);
        }

        //get all rectangles from the list
        public List<ResizableRectangle> GetAll()
        {
            return rectangles;
        }

        //get all rectangles from the list
        public List<Rect> GetAllRect()
        {
            List<Rect> rects = new List<Rect>();
            foreach (ResizableRectangle rr in rectangles)
                rects.Add(rr.GetRect());

            return rects;
                
        }

        //current -> Sequence Manager: x,y -> point
        public int GetPointInRectangle(double x, double y)
        {
            List<Rect> rects = GetAllRect();
            //for each rectangle check if point contained in X, return X's index, -1 otherwise
            for (int i = 0; i < rects.Count; i++)
            {
                if (IsPointInRectangle(x, y, new double[4, 2] {
                     
                    //00 - 01
                    { rects[i].X,  rects[i].Y }
                    //10 - 11
                   ,{ rects[i].X  + rects[i].Width, rects[i].Y }
                    //20 - 21
                   ,{ rects[i].X  + rects[i].Width, rects[i].Y  + rects[i].Height}
                    //30 - 31
                    ,{ rects[i].X , (rects[i]).Y  + rects[i].Height } }))
                    return i;
            }

            return -1;
        }
        public void SetFillBrush(int index, System.Windows.Media.Brush brush)
        {
            rectangles[index].FillBrush = brush;
        }
        public void SetStrokeThickness(int index, double thickness)
        {
            rectangles[index].StrokeThickness = thickness;
        }
        public void SetStrokeBrush(int index, System.Windows.Media.Brush brush)
        {
            rectangles[index].StrokeBrush = brush;
        }
        //checks if point is contained in rectangle 
        private bool IsPointInRectangle(double x, double y, double[,] rectangle)
        {
            return x > rectangle[0, 0] && x < rectangle[1, 0] && y > rectangle[0, 1] && y < rectangle[2, 1];
        }
    }
}
