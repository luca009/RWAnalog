using RWAnalog.Classes;
using SharpDX.DirectInput;
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
        int axisOffset;
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

            TrainControl = new TrainControl(tboxName.Text, internalControl.Value.ControllerId) { AssociatedAxis = new Axis(axisOffset) };
            DialogResult = true;
        }

        private async void bDetectControl_Click(object sender, RoutedEventArgs e)
        {
            bDetectControl.Content = "...";
            await Task.Run(() => { internalControl = TrainSimulatorManager.GetMovingControl(0.2f); });
            tboxControlName.Text = internalControl.Value.Name;
            bDetectControl.Content = "Detect";
        }

        private void bPickAxis_Click(object sender, RoutedEventArgs e)
        {
            ChooseAxis chooseAxis = new ChooseAxis(App.Current.Properties["CurrentDevice"] as DeviceInstance);
            chooseAxis.ShowDialog();

            tboxAxisName.Text = chooseAxis.SelectedAxisName;
            axisOffset = chooseAxis.SelectedIndex;
        }
    }
}
