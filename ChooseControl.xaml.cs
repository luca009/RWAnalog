using RWAnalog.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    /// Interaction logic for ChooseControl.xaml
    /// </summary>
    public partial class ChooseControl : Window
    {
        public string SelectedName { get; set; }
        public int SelectedIndex { get; set; }
        string[] controllerValues;

        public ChooseControl()
        {
            controllerValues = TrainSimulatorManager.GetActiveControllers();
            InitializeComponent();
            lboxList.ItemsSource = controllerValues;
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            if (lboxList.SelectedItem == null) return;

            SelectedIndex = lboxList.SelectedIndex;
            SelectedName = lboxList.SelectedItem.ToString();
            this.Close();
        }
    }
}
