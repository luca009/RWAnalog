using System.Windows.Controls;

namespace RWAnalog.Controls
{
    /// <summary>
    /// Interaction logic for AxisItem.xaml
    /// </summary>
    public partial class AxisItem : UserControl
    {
        public string Label { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double Value { get { return pbarValue.Value; } set { pbarValue.Value = value; } }
        public int AxisSequence { get; set; }
        public string AxisName { get; set; }

        public AxisItem()
        {
            InitializeComponent();
        }
    }
}
