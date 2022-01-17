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
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Setup : Window
    {
        public int SelectedIndex = 0;
        DirectInput directInput;
        List<DeviceInstance> controllers = new List<DeviceInstance>();

        public Setup()
        {
            InitializeComponent();

            directInput = new DirectInput();

            ScanDevices();
        }

        private void bChangeThrottleAxis_Click(object sender, RoutedEventArgs e)
        {
            ChooseAxis chooseAxis = new ChooseAxis(controllers[SelectedIndex]);
            chooseAxis.ShowDialog();

            chooseAxis.SelectedIndex;
        }

        private void bChangeBrakeAxis_Click(object sender, RoutedEventArgs e)
        {

        }

        private void listboxDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedIndex == -1)
                return;
            SelectedIndex = listboxDevices.SelectedIndex;
        }

        private void cboxShowAllDevices_Click(object sender, RoutedEventArgs e)
        {
            ScanDevices(cboxShowAllDevices.IsChecked == true);
        }

        private void ScanDevices(bool scanAllDevices = false)
        {
            listboxDevices.Items.Clear();
            controllers.Clear();

            IList<DeviceInstance> devices;
            if (scanAllDevices)
                devices = directInput.GetDevices();
            else
                devices = directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices);

            foreach (var deviceInstance in devices)
            {
                // Add each found Device to setups ListBox and to Controllers
                listboxDevices.Items.Add(deviceInstance.ProductName.ToString());
                controllers.Add(deviceInstance);
            }
        }
    }
}
