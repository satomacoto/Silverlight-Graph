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
using System.Windows.Data;
using System.Xml.Linq;

namespace Graph
{

    public partial class Page : UserControl
    {

        public Page(IDictionary<string,string> initParams)
        {
            InitializeComponent();

            this.LayoutRoot.Children.Add(new Graph(initParams));
        }

    }
}
