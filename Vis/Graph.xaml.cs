using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Physics;
using System.Windows.Browser;

namespace Vis
{
    public partial class Graph : UserControl
    {
		#region Fields (5) 

        bool isMouseCaptured;
        Point mousePosition;
        public Dictionary<string, Particle> particles;
        Simulation sim;
        public List<Spring> springs;

		#endregion Fields 

		#region Constructors (2) 

        public Graph(IDictionary<string, string> initParams)
        {
            InitializeComponent();

            Initialize();

            Loaded += new RoutedEventHandler(Graph_Loaded);

            // Get initial values
            string json;
            if (initParams.TryGetValue("json", out json))
                // json should be urlencoded.
                LoadJson(HttpUtility.UrlDecode(json));

            //string xml;
            //if (initParams.TryGetValue("xml", out xml))
            //    LoadXml(initParams["xml"]);
        }

        public Graph()
        {
            InitializeComponent();

            Initialize();

            Loaded += new RoutedEventHandler(Graph_Loaded);
        }

		#endregion Constructors 

		#region Properties (4) 

        /// <summary>
        /// The drag co-efficient.
        /// </summary>
        public double drag
        {
            get { return sim.dragForce.drag; }
            set { sim.dragForce.drag = value; }
        }

        /// <summary>
        /// The gravitational constant to use. Negative value produce a repulsive force.
        /// </summary>
        public double gravitation
        {
            get { return sim.nbodyForce.gravitation; }
            set { sim.nbodyForce.gravitation = value; }
        }

        /// <summary>
        /// The gravitational accelaration in the horizontal dimension.
        /// </summary>
        public double gx
        {
            get { return sim.gravityForce.gx; }
            set { sim.gravityForce.gx = value; }
        }

        /// <summary>
        /// The gravitational accelaration in the vertical dimension.
        /// </summary>
        public double gy
        {
            get { return sim.gravityForce.gy; }
            set { sim.gravityForce.gy = value; }
        }

		#endregion Properties 

		#region Methods (19) 

		// Public Methods (12) 

        /// <summary>
        /// Adds FrameworkElement edge.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public Spring AddEdgeFrameworkElement(FrameworkElement element, double length, Particle p1, Particle p2)
        {
            var spring = sim.AddSpring(p1, p2, length, 0.1, 0.1);
            
            springs.Add(spring);
            LayoutRoot.Children.Insert(0, element);

            var group = new TransformGroup();
            var scale = new ScaleTransform();
            var rotate = new RotateTransform();
            var translate = new TranslateTransform();
            group.Children.Add(scale);
            group.Children.Add(rotate);
            group.Children.Add(translate);

            p1.PositionChanged += (s, e) =>
            {
                var dx = p2.x - p1.x;
                var dy = p2.y - p1.y;
                var d = Math.Sqrt(dx * dx + dy * dy);
                var angle = Math.Atan2(dy, dx) / Math.PI * 180.0;

                scale.ScaleX = d / length;
                rotate.Angle = angle;
                translate.X = p1.x;
                translate.Y = p1.y;
            };
            p2.PositionChanged += (s, e) =>
            {
                var dx = p2.x - p1.x;
                var dy = p2.y - p1.y;
                var d = Math.Sqrt(dx * dx + dy * dy);
                var angle = Math.Atan2(dy, dx) / Math.PI * 180.0;

                scale.ScaleX = d / length;
                rotate.Angle = angle;
                translate.X = p1.x;
                translate.Y = p1.y;
            };

            element.RenderTransform = group;
 
            return spring;
        }

        /// <summary>
        /// Adds a line edge.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public Spring AddEdgeLine(Particle p1, Particle p2, double restLength, double tension, double damping)
        {
            var s = sim.AddSpring(p1, p2, restLength, tension, damping);

            Line line = new Line()
            {
                Fill = new SolidColorBrush(Colors.Brown),
                Stroke = new SolidColorBrush(Colors.Brown),
                StrokeThickness = 1,
                StrokeEndLineCap = PenLineCap.Triangle
            };
            line.SetBinding(Line.X1Property, new Binding("x") { Source = p1, Mode = BindingMode.TwoWay });
            line.SetBinding(Line.Y1Property, new Binding("y") { Source = p1, Mode = BindingMode.TwoWay });
            line.SetBinding(Line.X2Property, new Binding("x") { Source = p2, Mode = BindingMode.TwoWay });
            line.SetBinding(Line.Y2Property, new Binding("y") { Source = p2, Mode = BindingMode.TwoWay });

            // adds
            LayoutRoot.Children.Insert(0, line);
            springs.Add(s);

            return s;
        }

        /// <summary>
        /// Adds a polygon edge.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public Spring AddEdgePolygon(Particle p1, Particle p2)
        {
            Polygon poly = new Polygon()
            {
                Points = new PointCollection(){
                    new Point(0.0,1.0),
                    new Point(50.0,0.0),
                    new Point(0.0,-1.0)
                },
                Stroke = new SolidColorBrush(Colors.Cyan),
            };

            return AddEdgeFrameworkElement(poly, 50.0, p1, p2);
        }

        public Spring AddEdgeTriangle(Particle p1, Particle p2,
            double restLength, double tension, double damping,
            double baseWidth,
            string fill, string stroke, double strokeThickness)
        {
            var s = sim.AddSpring(p1, p2, restLength, tension, damping);

            Triangle tri = new Triangle()
            {
                Fill = new SolidColorBrush(Str2Color(fill)),
                Stroke = new SolidColorBrush(Str2Color(stroke)),
                StrokeThickness = strokeThickness,
                BaseWidth = baseWidth
            };
            tri.SetBinding(Triangle.X1Property, new Binding("x") { Source = p1, Mode = BindingMode.TwoWay });
            tri.SetBinding(Triangle.Y1Property, new Binding("y") { Source = p1, Mode = BindingMode.TwoWay });
            tri.SetBinding(Triangle.X2Property, new Binding("x") { Source = p2, Mode = BindingMode.TwoWay });
            tri.SetBinding(Triangle.Y2Property, new Binding("y") { Source = p2, Mode = BindingMode.TwoWay });

            // adds
            LayoutRoot.Children.Insert(0, tri);
            //LayoutRoot.Children.Add(tri);
            springs.Add(s);

            return s;
        }

        /// <summary>
        /// Adds FrameworkElement node.
        /// </summary>
        /// <param name="element">FrameworkElement</param>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Particle AddNodeFrameworkElement(FrameworkElement element, string name, double mass, double x, double y)
        {
            // creates a new Particle instance
            var p = sim.AddParticle(mass, x, y);

            // sets binding
            element.DataContext = p;
            element.SetBinding(Canvas.LeftProperty, new Binding("x") { Mode = BindingMode.TwoWay });
            element.SetBinding(Canvas.TopProperty, new Binding("y") { Mode = BindingMode.TwoWay });

            // sets mouse event
            element.MouseLeftButtonDown += (s, e) =>
            {
                p.IsActive = false;
            };
            element.MouseLeftButtonUp += (s, e) =>
            {
                p.IsActive = true;
            };
            SetHandle(element);

            // adds
            LayoutRoot.Children.Add(element);
            particles[name] = p;

            // set center
            element.SizeChanged += (s, e) =>
            {
                var size = element.RenderSize;
                element.RenderTransform =
                    new TranslateTransform() { X = -size.Width / 2, Y = -size.Height / 2 };
            };

            p.IsActive = true;

            // returns added the Particle instance
            return p;
        }

        public void Initialize()
        {
            sim = new Simulation();
            particles = new Dictionary<string, Particle>();
            springs = new List<Spring>();
            LayoutRoot.Children.Clear();
        }

        /// <summary>
        /// Adds a Grid + TextBlock node.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Particle AddNodeGridTextBlock(string text, double x, double y)
        {
            // creates a new TextBlock instance
            var textBlock = new TextBlock()
            {
                Text = text,
                FontSize = 14.0,                
                //Foreground = new SolidColorBrush(Colors.White)
            };
            var grid = new Grid()
            {
                //Background = new SolidColorBrush(Colors.DarkGray),
            };
            grid.Children.Add(textBlock);

            return AddNodeFrameworkElement(grid, text, 10, x, y);
        }


        public Particle AddNodeBlock(string text, double mass, double x, double y, 
            double fontSize, string fontColor, 
            string fill, string stroke, double strokeThickness)
        {
            // creates a new TextBlock instance
            var block = new Block()
            {
                Text = text,
                Fill = new SolidColorBrush(Str2Color(fill)),
                Stroke = new SolidColorBrush(Str2Color(stroke)),
                StrokeThickness = strokeThickness,
                FontSize = fontSize,
                Foreground = new SolidColorBrush(Str2Color(fontColor)),
                //FontWeight = FontWeights.Bold
            };

            return AddNodeFrameworkElement(block, text, mass, x, y);
        }

        /// <summary>
        /// Add a Xaml node.
        /// </summary>
        /// <param name="xaml"></param>
        /// <param name="name"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Particle AddNodeXaml(string xaml, string name, double x, double y)
        {
            var element = XamlReader.Load(xaml) as FrameworkElement;
            return AddNodeFrameworkElement(element, name, 10, x, y);
        }

        /// <summary>
        /// Deserializes JSON.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string jsonString)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(graph));
                return (T)ser.ReadObject(ms);
            }
        }

        /// <summary>
        /// Load from JSON.
        /// </summary>
        /// <param name="jsonString"></param>
        public void LoadJson(string jsonString)
        {
            var g = Deserialize<graph>(jsonString);
            foreach (var n in g.Nodes)
            {
                var name = n.Name;
                var x = n.X;
                var y = n.Y;
                AddNodeBlock(name, 10, x, y, 12, "#FFFFFF", "#FF0000", "#FF0000", 2);
            }
            foreach (var e in g.Edges)
            {
                Particle s, t;
                var source = e.Source;
                var target = e.Target;
                var Length = e.Length > 0 ? e.Length : 50.0;
                if (particles.TryGetValue(source, out s) && particles.TryGetValue(target, out t))
                    AddEdgeTriangle(s, t, Length, 0.1, 0.1, 2.0, "#777777", "#555555", 0.5);
            }
        }

        /// <summary>
        /// Load from XML
        /// </summary>
        /// <example>
        /// var text = @'
        /// &lt;Nodes&gt;
        ///   &lt;Node Name="foo" /&gt;
        ///   &lt;Node Name="bar" Style="Max" /&gt;
        /// &lt;/Nodes&gt;
        /// &lt;Links&gt;
        ///   &lt;Link Source="foo" Target="bar" /&gt;
        /// &lt;/Links&gt;';
        /// LoadXml(text);
        /// </example>
        /// <param name="text"></param>
        public void LoadXml(string text)
        {
            var xdoc = XDocument.Parse(text);

            var a = 1;
            foreach (var node in xdoc.Descendants("Node"))
            {
                var name = node.Attribute("Name").Value;
                var style = node.Attribute("Style") == null ? "More" : node.Element("Style").Value;
                var x = 100 + Math.Cos(a);
                var y = 100 + Math.Sin(a);
                AddNodeGridTextBlock(name, x, y);
                a++;
            }

            foreach (var link in xdoc.Descendants("Link"))
            {
                var source = link.Attribute("Source").Value;
                var target = link.Attribute("Target").Value;
                Particle s, t;
                if (particles.TryGetValue(source, out s) && particles.TryGetValue(target, out t))
                    AddEdgeLine(s, t, 100, 0.1, 0.1);
            }
        }

        /// <summary>
        /// Sets a bounding box for particles in this simulation.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetBounds(double width, double height)
        {
            sim.bounds = new Rect(0, 0, width, height);
        }

        public void Start()
        {
            sim.Start();
        }

        public void Stop()
        {
            sim.Stop();
        }
		// Private Methods (7) 

        void element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement item = sender as FrameworkElement;
            mousePosition = e.GetPosition(null);
            isMouseCaptured = true;
            item.CaptureMouse();
            item.Cursor = Cursors.Hand;

            e.Handled = true;
        }

        void element_MouseEnter(object sender, MouseEventArgs e)
        {
            var item = sender as FrameworkElement;
            item.Cursor = Cursors.Hand;
        }

        void element_MouseLeave(object sender, MouseEventArgs e)
        {
            var item = sender as FrameworkElement;
            item.Cursor = null;
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

        void Graph_Loaded(object sender, RoutedEventArgs e)
        {
            sim.Particles.ForEach(p => p.IsActive = true);
        }

        void SetHandle(FrameworkElement element)
        {
            element.MouseLeftButtonDown += new MouseButtonEventHandler(element_MouseDown);
            element.MouseMove += new MouseEventHandler(element_MouseMove);
            element.MouseLeftButtonUp += new MouseButtonEventHandler(element_MouseUp);
            element.MouseEnter += new MouseEventHandler(element_MouseEnter);
            element.MouseLeave += new MouseEventHandler(element_MouseLeave);
        }

		#endregion Methods 

		#region Nested Classes (3) 


        public class edge
        {
		#region Properties (5) 

            public double Damping { get; set; }

            public double Length { get; set; }

            public string Source { get; set; }

            public string Target { get; set; }

            public double Tenstion { get; set; }

		#endregion Properties 
        }
        public class graph
        {
		#region Fields (2) 

            public List<edge> Edges = new List<edge>();
            public List<node> Nodes = new List<node>();

		#endregion Fields 
        }
        public class node
        {
		#region Properties (3) 

            public string Name { get; set; }

            public double X { get; set; }

            public double Y { get; set; }

		#endregion Properties 
        }
		#endregion Nested Classes 

        /// <summary>
        /// string str = "#ffffff"
        /// </summary>
        /// <param name="hexaColor"></param>
        /// <returns></returns>
        public static Color Str2Color(string str)
        {
            return Color.FromArgb(255,
                Convert.ToByte(str.Substring(1, 2), 16),
                Convert.ToByte(str.Substring(3, 2), 16),
                Convert.ToByte(str.Substring(5, 2), 16));
        }
    }
}
