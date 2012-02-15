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
using Physics;
using System.Xml.Linq;
using System.Windows.Data;

namespace Graph
{
    /// <summary>
    /// Quick hack to remove event handling that mark events as handled
    /// </summary>
    public class Button : System.Windows.Controls.Button
    {
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            e.Handled = false;
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            e.Handled = false;
        }
    }

    public partial class Graph : UserControl
    {
        public Graph()
        {
            InitializeComponent();
        }

        public Graph(IDictionary<string, string> initParams)
        {
            InitializeComponent();

            sim = new Simulation();
            particles = new Dictionary<string, Particle>();
            Loaded += new RoutedEventHandler(Graph_Loaded);

            string xml;
            if (initParams.TryGetValue("xml", out xml))
                LoadXML(initParams["xml"]);
        }

        public Simulation sim;
        public Dictionary<string, Particle> particles;

        /// <summary>
        /// 境界を設定する
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetBounds(double width, double height)
        {
            sim.bounds = new Rect(0, 0, width, height);
        }

        void Graph_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetBounds(500, 500);

            // add nodes and edges
            int col = 5, row = 5;

            var root = new XElement("Root");

            for (int i = 0; i < col * row; i++)
            {
                root.Add(new XElement("Node", 
                    new XAttribute("Name", i.ToString())));
            }

            for (int i = 0; i < col; i++)
            {
                for (int j = 0; j < row; j++)
                {
                    if (i % col != col - 1)
                        root.Add(new XElement("Link",
                            new XAttribute("Source", (i + col * j).ToString()),
                            new XAttribute("Target", (i + col * j + 1).ToString())));
                    if (j % row != row - 1)
                        root.Add(new XElement("Link",
                            new XAttribute("Source", (i + col * j).ToString()),
                            new XAttribute("Target", (i + col * (j + 1)).ToString())));
                }
            }

            LoadXML(root.ToString());

            sim.Particles.ForEach(p => p.IsActive = true);
        }

        /// <summary>
        /// ボタンのノードを追加する
        /// </summary>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        Particle AddButtonNode(string text, double x, double y, string style)
        {
            // creates a new Particle instance
            var p = sim.AddParticle(10, x, y);

            // creates a new TextBlock instance
            Button button = new Button()
            {
                Content = text,
                Style = Resources[style] as Style,
            };

            // sets binding
            button.DataContext = p;
            button.SetBinding(Canvas.LeftProperty, new Binding("x") { Mode = BindingMode.TwoWay });
            button.SetBinding(Canvas.TopProperty, new Binding("y") { Mode = BindingMode.TwoWay });

            // sets mouse event
            button.MouseLeftButtonDown += (s, e) =>
            {
                p.IsActive = false;
            };
            button.MouseLeftButtonUp += (s, e) =>
            {
                p.IsActive = true;
            };
            SetHandle(button);

            // adds
            LayoutRoot.Children.Add(button);
            particles[text] = p;

            p.IsActive = true;
            button.SizeChanged += (s, e) =>
            {
                var size = button.RenderSize;
                button.RenderTransform =
                    new TranslateTransform() { X = -size.Width / 2, Y = -size.Height / 2 };
            };

            // returns added the Particle instance
            return p;
        }


        /// <summary>
        /// テキストのノードを追加する
        /// </summary>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        Particle AddTextNode(string text, double x, double y)
        {
            // creates a new Particle instance
            var p = sim.AddParticle(10, x, y);

            // creates a new TextBlock instance
            TextBlock textBlock = new TextBlock()
            {
                Text = text,
            };

            // sets binding
            textBlock.DataContext = p;
            textBlock.SetBinding(Canvas.LeftProperty, new Binding("x") { Mode = BindingMode.TwoWay });
            textBlock.SetBinding(Canvas.TopProperty, new Binding("y") { Mode = BindingMode.TwoWay });

            // sets mouse event
            textBlock.MouseLeftButtonDown += (s, e) =>
            {
                p.IsActive = false;
            };
            textBlock.MouseLeftButtonUp += (s, e) =>
            {
                p.IsActive = true;
            };
            SetHandle(textBlock);

            // adds
            LayoutRoot.Children.Add(textBlock);
            particles[text] = p;

            p.IsActive = true;

            // returns added the Particle instance
            return p;
        }

        /// <summary>
        /// 線のエッジを追加する
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        Spring AddLineEdge(Particle p1, Particle p2)
        {
            var s = sim.AddSpring(p1, p2, 10, 0.1, 0.1);

            Line line = new Line()
            {
                Fill = new SolidColorBrush(Colors.Brown),
                Stroke = new SolidColorBrush(Colors.Brown),
            };
            line.SetBinding(Line.X1Property, new Binding("x") { Source = p1, Mode = BindingMode.TwoWay });
            line.SetBinding(Line.Y1Property, new Binding("y") { Source = p1, Mode = BindingMode.TwoWay });
            line.SetBinding(Line.X2Property, new Binding("x") { Source = p2, Mode = BindingMode.TwoWay });
            line.SetBinding(Line.Y2Property, new Binding("y") { Source = p2, Mode = BindingMode.TwoWay });

            LayoutRoot.Children.Insert(0, line);
            return s;
        }

        /// <summary>
        /// 多角形のエッジを追加する
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        Spring AddPolygonEdge(Particle p1, Particle p2, string style)
        {
            var spring = sim.AddSpring(p1, p2, 100, 0.1, 0.1);

            var group = new TransformGroup();
            var scale = new ScaleTransform();
            var rotate = new RotateTransform();
            var translate = new TranslateTransform();
            group.Children.Add(scale);
            group.Children.Add(rotate);
            group.Children.Add(translate);

            p1.PropertyChanged += (s, e) =>
            {
                var dx = p2.x - p1.x;
                var dy = p2.y - p1.y;
                var d = Math.Sqrt(dx * dx + dy * dy);
                var angle = Math.Atan2(dy, dx) / Math.PI * 180;

                scale.ScaleX = d / 100.0;
                rotate.Angle = angle;
                translate.X = p1.x;
                translate.Y = p1.y;
            };
            p2.PropertyChanged += (s, e) =>
            {
                var dx = p2.x - p1.x;
                var dy = p2.y - p1.y;
                var d = Math.Sqrt(dx * dx + dy * dy);
                var angle = Math.Atan2(dy, dx) / Math.PI * 180;

                scale.ScaleX = d / 100;
                rotate.Angle = angle;
                translate.X = p1.x;
                translate.Y = p1.y;
            };

            Polygon poly = new Polygon()
            {
                Points = new PointCollection(){
                    new Point(0,1),
                    new Point(100,0),
                    new Point(0,-1)
                },
                RenderTransform = group,
                Style = Resources[style] as Style
            };

            LayoutRoot.Children.Insert(0, poly);

            return spring;
        }

        /// <summary>
        /// Load from XML
        /// </summary>
        /// <example>
        /// &lt;Nodes&gt;
        ///   &lt;Node Name="foo" /&gt;
        ///   &lt;Node Name="bar" Style="Max" /&gt;
        /// &lt;/Nodes&gt;
        /// &lt;Links&gt;
        ///   &lt;Link Source="foo" Target="bar" /&gt;
        /// &lt;/Links&gt;
        /// </example>
        /// <param name="text"></param>
        public void LoadXML(string text)
        {
            var xdoc = XDocument.Parse(text);

            var a = 1;
            foreach (var node in xdoc.Descendants("Node"))
            {
                var name = node.Attribute("Name").Value;
                var style = node.Attribute("Style") == null ? "More" : node.Element("Style").Value;
                var x = 100 + Math.Cos(a);
                var y = 100 + Math.Sin(a);
                AddTextNode(name, x, y);
                //AddButtonNode(name, x, y, style);
                a++;
            }

            foreach (var link in xdoc.Descendants("Link"))
            {
                var source = link.Attribute("Source").Value;
                var target = link.Attribute("Target").Value;
                Particle s, t;
                if (particles.TryGetValue(source, out s) && particles.TryGetValue(target, out t))
                    AddLineEdge(s, t);
                    //AddPolygonEdge(particles[source], particles[target], "Edge");
            }
        }

        #region ハンドルのイベント

        void SetHandle(FrameworkElement element)
        {
            element.MouseLeftButtonDown += new MouseButtonEventHandler(element_MouseDown);
            element.MouseMove += new MouseEventHandler(element_MouseMove);
            element.MouseLeftButtonUp += new MouseButtonEventHandler(element_MouseUp);
            element.MouseEnter += new MouseEventHandler(element_MouseEnter);
            element.MouseLeave += new MouseEventHandler(element_MouseLeave);
        }


        void element_MouseLeave(object sender, MouseEventArgs e)
        {
            var item = sender as FrameworkElement;
            item.Cursor = null;
        }

        void element_MouseEnter(object sender, MouseEventArgs e)
        {
            var item = sender as FrameworkElement;
            item.Cursor = Cursors.Hand;
        }


        bool isMouseCaptured;
        Point mousePosition;

        void element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement item = sender as FrameworkElement;
            mousePosition = e.GetPosition(null);
            isMouseCaptured = true;
            item.CaptureMouse();
            item.Cursor = Cursors.Hand;

            e.Handled = true;
        }
        void element_MouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement item = sender as FrameworkElement;
            if (isMouseCaptured)
            {
                // Calculate the current position of the object.
                double deltaV = e.GetPosition(null).Y - mousePosition.Y;
                double deltaH = e.GetPosition(null).X - mousePosition.X;
                double newTop = deltaV + (double)item.GetValue(Canvas.TopProperty);
                double newLeft = deltaH + (double)item.GetValue(Canvas.LeftProperty);

                // Set new position of object.
                item.SetValue(Canvas.TopProperty, newTop);
                item.SetValue(Canvas.LeftProperty, newLeft);

                // Update position global variables.
                mousePosition = e.GetPosition(null);
            }

        }
        void element_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement item = sender as FrameworkElement;
            isMouseCaptured = false;
            item.ReleaseMouseCapture();
            mousePosition.X = mousePosition.Y = 0;
            item.Cursor = null;

            e.Handled = true;
        }


        #endregion

        #region スライダーのイベント処理
        private void drag_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sim != null)
                sim.dragForce.drag = e.NewValue;
        }
        private void nbody_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sim != null)
                sim.nbodyForce.gravitation = e.NewValue;
        }
        private void gx_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sim != null)
                sim.gravityForce.gx = e.NewValue;
        }
        private void gy_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sim != null)
                sim.gravityForce.gy = e.NewValue;
        }
        private void start_Click(object sender, RoutedEventArgs e)
        {
            sim.Start();
        }
        private void stop_Click(object sender, RoutedEventArgs e)
        {
            sim.Stop();
        }
        #endregion
    }
}
