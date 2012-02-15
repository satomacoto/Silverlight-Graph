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
using System.Windows.Browser;
using Physics;

namespace Graph
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();

            HtmlPage.RegisterScriptableObject("MainPage", this);

            graph = new Vis.Graph();
            this.LayoutRoot.Children.Add(graph);
            this.Dispatcher.BeginInvoke(
                () =>
                {
                    graph.SetBounds(this.ActualWidth, this.ActualHeight);
                }
            );
        }

        public MainPage(IDictionary<string, string> initParams)
        {
            InitializeComponent();

            HtmlPage.RegisterScriptableObject("MainPage", this);

            graph = new Vis.Graph();
            this.LayoutRoot.Children.Add(graph);
            this.Dispatcher.BeginInvoke(
                () =>
                {
                    graph.SetBounds(this.ActualWidth, this.ActualHeight);
                }
            );

            string json;
            if (initParams.TryGetValue("json", out json))
            {
                LoadJson(json);
            }
        }


        Vis.Graph graph;

        [ScriptableMember]
        public void Start()
        {
            graph.Start();
        }

        [ScriptableMember]
        public void Stop()
        {
            graph.Stop();
        }

        [ScriptableMember]
        public void Clear()
        {
            graph.Initialize();
        }

        [ScriptableMember]
        public void SetBounds(double width, double height)
        {
            graph.Width = width;
            graph.Height = height;
            graph.SetBounds(width, height);
        }


        Dictionary<string, Particle> particles = new Dictionary<string, Particle>();
        [ScriptableMember]
        public void AddNode(string text)
        {
            AddNode(text, 12, "#777777");
        }
        [ScriptableMember]
        public void AddNode(string text, double size, string color)
        {
            particles[text] = graph.AddNodeBlock(text, size, ActualWidth / 2, ActualHeight / 2, size, "#FFFFFFF", color, "#FFFFFF", 1);
        }


        [ScriptableMember]
        public void AddEdge(string source, string target)
        {
            AddEdge(source, target, 100, "#cccccc");
        }
        [ScriptableMember]
        public void AddEdge(string source, string target, double length)
        {
            AddEdge(source, target, length, "#cccccc");
        }
        [ScriptableMember]
        public void AddEdge(string source, string target, string color)
        {
            AddEdge(source, target, 100, color);
        }
        [ScriptableMember]
        public void AddEdge(string source, string target, double length, string color)
        {
            Particle s, t;
            if (particles.TryGetValue(source, out s) && particles.TryGetValue(target, out t))
                graph.AddEdgeTriangle(s, t, length, 0.1, 0.1, 3.0, color, color, 0.5);
        }
        [ScriptableMember]
        public void AddEdge(string source, string target, double length, double basewidth, double thickness, string color)
        {
            Particle s, t;
            if (particles.TryGetValue(source, out s) && particles.TryGetValue(target, out t))
                graph.AddEdgeTriangle(s, t, length, 0.1, 0.1, basewidth, color, color, thickness);
        }

        [ScriptableMember]
        public void LoadJson(string json)
        {
            graph.LoadJson(HttpUtility.UrlDecode(json));
        }

        [ScriptableMember]
        public double GetWidth()
        {
            return ActualWidth;
        }
        [ScriptableMember]
        public double GetHeight()
        {
            return ActualHeight;
        }
    }
}
