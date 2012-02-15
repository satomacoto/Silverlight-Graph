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
using System.Windows.Data;

namespace Vis
{
    public partial class Block : UserControl
    {
        public Block()
        {
            InitializeComponent();

            textBlock.SetBinding(TextBlock.TextProperty, new Binding("Text") { Source = this });
            rectangle.SetBinding(Rectangle.FillProperty, new Binding("Fill") { Source = this });
            rectangle.SetBinding(Rectangle.StrokeProperty, new Binding("Stroke") { Source = this });
            rectangle.SetBinding(Rectangle.StrokeThicknessProperty, new Binding("StrokeThickness") { Source = this });
        }

        #region Fill
        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }
        public static DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(Block),
            new PropertyMetadata(new SolidColorBrush(Colors.White)));
        #endregion Fill

        #region Stroke
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }
        public static DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(Brush), typeof(Block),
            new PropertyMetadata(new SolidColorBrush(Colors.Black)));
        #endregion Stroke

        #region StrokeThickness
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(Block),
            new PropertyMetadata(1.0));
        #endregion StorokeThickness



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Block), 
            new PropertyMetadata(default(string), new PropertyChangedCallback(OnPropertyChanged)));

        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

    }
}
