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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
