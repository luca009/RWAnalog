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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
