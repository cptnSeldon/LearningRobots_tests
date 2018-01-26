using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace P3_LearningRobots
{
    /// <summary>
    /// Logique d'interaction pour UserControl1.xaml
    /// </summary>
    public partial class ResizableRectangle : UserControl
    {
        public ResizableRectangle()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Path path = new Path { Fill = Brushes.Blue, Stroke = Brushes.Black, Data = new RectangleGeometry(new Rect(0, 0, 150, 100)) };
            theGrid.Children.Add(path);
            StrokeThickness = 3;
        }

        #region Variables
        MatrixTransform transform = new MatrixTransform();
        Point StartPoint;
        Point CurrentPoint;
        double[] Dimensions = new double[2];
        Rect rect = new Rect();
        bool IsResizeMode = false;
        bool IsDragAndDropMode = true;
        public Brush FillBrush { get; set; }
        public Brush StrokeBrush { get; set; }
        public double StrokeThickness { get; set; }

        #endregion
        private Path getPath() {
            if (theGrid.Children[0] is Path path)
                return path;

            return null;
        }
        private void ResizeMode(object sender, RoutedEventArgs args)
        {
            IsResizeMode = true;
            IsDragAndDropMode = false;
        }
        private void DragAndDropMode(object sender, RoutedEventArgs args)
        {
            IsDragAndDropMode = true;
            IsResizeMode = false;
        }

        public Rect GetRect() {
            return new Rect(CurrentPoint.X,CurrentPoint.Y,ActualHeight, ActualWidth);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            //The first block 
            HitTestResult result = VisualTreeHelper.HitTest(this, Mouse.GetPosition(this));
            if (result.VisualHit is Path path)
            {

                //The second block
                if (path.Data.GetType() == typeof(EllipseGeometry))
                {
                    Dimensions[0] = (path.Data as EllipseGeometry).RadiusX;
                    Dimensions[1] = (path.Data as EllipseGeometry).RadiusY;
                    path.Tag = Dimensions;
                }

                if (path.Data.GetType() == typeof(RectangleGeometry))
                {
                    rect = (path.Data as RectangleGeometry).Rect;
                    path.Tag = rect;
                }

                //The third block
                path.Fill = Brushes.Violet;

                //The fourth block
                if (StartPoint == null)
                {
                    StartPoint = e.GetPosition(this);
                }
                StartPoint = CurrentPoint;
            }
        }

        

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            //First Block
            HitTestResult result = VisualTreeHelper.HitTest(this, Mouse.GetPosition(this));
            Path path = result.VisualHit as Path;

            //Second block
            if (IsDragAndDropMode == true && IsResizeMode == false)
            {
                if (path.Data.GetType() == typeof(EllipseGeometry))
                {
                    EllipseGeometry geometry = new EllipseGeometry(new Point(50, 50), 50, 50);
                    if ((path.Tag as double[]).Length > 0)
                    {

                        geometry.RadiusX = (path.Tag as double[])[0]; ;
                        geometry.RadiusY = (path.Tag as double[])[1];
                    }
                    geometry.Transform = new TranslateTransform { X = CurrentPoint.X - geometry.RadiusX, Y = CurrentPoint.Y - geometry.RadiusY };
                    Path FinalPath = new Path { Fill = FillBrush, Stroke = StrokeBrush, Data = geometry, StrokeThickness = StrokeThickness};
                    this.theGrid.Children.Add(FinalPath);
                    this.theGrid.Children.Remove(path);
                }
                if (path.Data.GetType() == typeof(RectangleGeometry))
                {
                    RectangleGeometry geometry = new RectangleGeometry(new Rect(0, 0, 150, 100));
                    if ((Rect)path.Tag != null)
                    {
                        geometry.Rect = (Rect)path.Tag;
                    }
                    geometry.Transform = new TranslateTransform { X = CurrentPoint.X - geometry.Rect.Width / 2, Y = CurrentPoint.Y - geometry.Rect.Height / 2 };
                    Path FinalPath = new Path { Fill = FillBrush, Stroke = StrokeBrush, Data = geometry, StrokeThickness = StrokeThickness};
                    this.theGrid.Children.Add(FinalPath);
                    this.theGrid.Children.Remove(path);
                }
            }

            //Third block
            if (IsDragAndDropMode == false && IsResizeMode == true)
            {
                Geometry geometry = path.Data;
                if (path.Data.GetType() == typeof(EllipseGeometry))
                {
                    path.Fill = Brushes.Red;
                    (geometry as EllipseGeometry).RadiusX = (path.Tag as double[])[0];
                    (geometry as EllipseGeometry).RadiusY = (path.Tag as double[])[1];
                }
                if (path.Data.GetType() == typeof(RectangleGeometry))
                {
                    path.Fill = Brushes.Blue;
                    (geometry as RectangleGeometry).Rect = (Rect)path.Tag;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs args)
        {
            //First block
            CurrentPoint = args.GetPosition(this);

            //Second block
            HitTestResult result = VisualTreeHelper.HitTest(this, Mouse.GetPosition(this));
            if (result.VisualHit is Path path)
            {

                //Third block
                if (IsDragAndDropMode == true && IsResizeMode == false)
                {
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        transform.Matrix = new Matrix(1, 0, 0, 1, CurrentPoint.X - StartPoint.X, CurrentPoint.Y - StartPoint.Y);
                        path.RenderTransform = transform;
                    }
                }

                //Fourth block
                if (IsDragAndDropMode == false && IsResizeMode == true)
                {
                    Geometry geomerty = path?.Data;
                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        if (geomerty.GetType() == typeof(EllipseGeometry))
                        {
                            EllipseGeometry currentShape = geomerty as EllipseGeometry;
                            currentShape.RadiusX = CurrentPoint.X - currentShape.Center.X;
                            currentShape.RadiusY = CurrentPoint.Y - currentShape.Center.Y;
                            double[] Dimensions = new double[2] { currentShape.RadiusX, currentShape.RadiusY };
                            path.Tag = Dimensions;
                        }
                        if (geomerty.GetType() == typeof(RectangleGeometry))
                        {
                            RectangleGeometry currentShape = geomerty as RectangleGeometry;
                            Vector vector = CurrentPoint - new Point(currentShape.Bounds.X - (currentShape.Bounds.Width/2), currentShape.Bounds.Y- (currentShape.Bounds.Height/2));
                            Rect rect = new Rect(currentShape.Rect.Location, vector);
                            currentShape.Rect = rect;
                            path.Tag = rect;
                        }

                    }
                }
            }

        }
    }
}
