using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Data;

namespace Vis
{
    public partial class Triangle : UserControl
    {
        public Triangle()
        {
            InitializeComponent();

            l1 = new LineSegment();
            l2 = new LineSegment();

            f = new PathFigure();
            f.Segments.Add(l1);
            f.Segments.Add(l2);
            f.IsClosed = true;

            PathGeometry g = new PathGeometry();
            g.Figures.Add(f);

            Path p = new Path();
            this.SetBinding(FillProperty, new Binding("Fill") { Source = p, Mode = BindingMode.TwoWay });
            this.SetBinding(StrokeProperty, new Binding("Stroke") { Source = p, Mode = BindingMode.TwoWay });
            this.SetBinding(StrokeThicknessProperty, new Binding("StrokeThickness") { Source = p, Mode = BindingMode.TwoWay });
            p.Data = g;

            this.Fill = new SolidColorBrush(Colors.White);
            this.Stroke = new SolidColorBrush(Colors.Black);

            this.LayoutRoot.Children.Add(p);
        }

        PathFigure f;
        LineSegment l1, l2;

        #region Property

        #region Fill
        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }
        public static DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(Triangle),
            new PropertyMetadata(new SolidColorBrush(Colors.White)));
        #endregion Fill

        #region Stroke
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }
        public static DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(Brush), typeof(Triangle),
            new PropertyMetadata(new SolidColorBrush(Colors.White)));
        #endregion Stroke

        #region StrokeThickness
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(Triangle),
            new PropertyMetadata(1.0));
        #endregion StorokeThickness


        public double X1
        {
            get { return (double)GetValue(X1Property); }
            set { SetValue(X1Property, value); }
        }

        // Using a DependencyProperty as the backing store for X1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty X1Property =
            DependencyProperty.Register("X1", typeof(double), typeof(Triangle), new PropertyMetadata(OnPropertyChanged));

        public double Y1
        {
            get { return (double)GetValue(Y1Property); }
            set { SetValue(Y1Property, value); }
        }

        // Using a DependencyProperty as the backing store for Y1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Y1Property =
            DependencyProperty.Register("Y1", typeof(double), typeof(Triangle), new PropertyMetadata(OnPropertyChanged));

        public double X2
        {
            get { return (double)GetValue(X2Property); }
            set { SetValue(X2Property, value); }
        }

        // Using a DependencyProperty as the backing store for X2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty X2Property =
            DependencyProperty.Register("X2", typeof(double), typeof(Triangle), new PropertyMetadata(OnPropertyChanged));

        public double Y2
        {
            get { return (double)GetValue(Y2Property); }
            set { SetValue(Y2Property, value); }
        }

        // Using a DependencyProperty as the backing store for Y2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Y2Property =
            DependencyProperty.Register("Y2", typeof(double), typeof(Triangle), new PropertyMetadata(OnPropertyChanged));




        public double BaseWidth
        {
            get { return (double)GetValue(BaseWidthProperty); }
            set { SetValue(BaseWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Base.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BaseWidthProperty =
            DependencyProperty.Register("BaseWidth", typeof(double), typeof(Triangle), new PropertyMetadata(2.0));


        public static void OnPropertyChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var tri = sender as Triangle;
            var dx = tri.X2 - tri.X1;
            var dy = tri.Y2 - tri.Y1;
            var l = Math.Sqrt(dx * dx + dy * dy);
            var scale = tri.BaseWidth / l;
            dx *= scale;
            dy *= scale;

            tri.f.StartPoint = new Point(tri.X1 - dy, tri.Y1 + dx);
            tri.l1.Point = new Point(tri.X2, tri.Y2);
            tri.l2.Point = new Point(tri.X1 + dy, tri.Y1 - dx);
        }


        #endregion


    }
}
