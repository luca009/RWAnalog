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
    /// Interaction logic for AddAxis.xaml
    /// </summary>
    public partial class AddAxis : Window
    {
        TrainControl? internalControl = null;
        public TrainControl TrainControl { get; private set; }

        public AddAxis()
        {
            InitializeComponent();
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            if (internalControl == null)
            {
                MessageBox.Show("Complete the dialog first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TrainControl = new TrainControl(tboxName.Text, internalControl.Value.ControllerId);
            DialogResult = true;
        }

        private async void bDetectAxis_Click(object sender, RoutedEventArgs e)
        {
            bDetectAxis.Content = "...";
            await Task.Run(() => { internalControl = TrainSimulatorManager.GetMovingControl(0.2f); });
            tboxAxisName.Text = internalControl.Value.Name;
            bDetectAxis.Content = "Detect";
        }
    }
}
