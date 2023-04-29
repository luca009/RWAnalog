using RWAnalog.Classes;
using System.Windows.Controls;

namespace RWAnalog.Controls
{
    /// <summary>
    /// Interaction logic for ControlItem.xaml
    /// </summary>
    public partial class ControlItem : UserControl
    {
        public string Label { get { return Control.Name; } }
        public TrainControl Control { get; set; }

        public ControlItem(TrainControl control)
        {
            Control = control;
            InitializeComponent();
        }
        public ControlItem()
        {
            InitializeComponent();
        }
    }
}
