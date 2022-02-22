using RWAnalog.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RWAnalog
{
    /// <summary>
    /// Interaction logic for GraphDialog.xaml
    /// </summary>
    public partial class GraphDialog : Window
    {
        public List<GraphPoint> Points = new List<GraphPoint>();
        public double Minimum { get { return graphEditor.Minimum;  } set { graphEditor.Minimum = value;} }
        public double Maximum { get { return graphEditor.Maximum;  } set { graphEditor.Maximum = value;} }

        public GraphDialog()
        {
            InitializeComponent();
        }
        public GraphDialog(InputGraph graph)
        {
            InitializeComponent();

            double a = graphEditor.Width;
            graphEditor.SetPoints(graph.Points, graphEditor.Width - 50, graphEditor.Height - 50);
        }

        public void SetPoints(InputGraph graph)
        {
            graphEditor.SetPoints(graph.Points, graphEditor.Width - 50, graphEditor.Height - 50);
        }

        public void UpdateCanvas()
        {
            graphEditor.UpdateCanvas();
        }

        public void SetControllerValue(float value)
        {
            graphEditor.SetControllerValue(value);
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            Points = graphEditor.GetPoints();
            this.DialogResult = true;
        }
    }
}
