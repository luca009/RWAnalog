using RWAnalog.Classes;
using System.Collections.Generic;
using System.Windows;

namespace RWAnalog
{
    /// <summary>
    /// Interaction logic for GraphDialog.xaml
    /// </summary>
    public partial class GraphDialog : Window
    {
        public List<GraphPoint> Points { get; set; }
        public double Minimum { get { return graphEditor.Minimum;  } set { graphEditor.Minimum = value;} }
        public double Maximum { get { return graphEditor.Maximum;  } set { graphEditor.Maximum = value;} }

        public GraphDialog()
        {
            Points = new List<GraphPoint>();
            InitializeComponent();
        }
        public GraphDialog(InputGraph graph)
        {
            InitializeComponent();

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

        public void SetControllerValue(float controllerValue, float trainControlValue = -1)
        {
            graphEditor.SetControllerValue(controllerValue, trainControlValue);
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            Points = graphEditor.GetPoints();
            this.DialogResult = true;
        }
    }
}
