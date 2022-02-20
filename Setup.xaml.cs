using Microsoft.Win32;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.IO;
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
using Ookii.Dialogs.Wpf;

namespace RWAnalog
{
    /// <summary>
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Setup : Window
    {
        public int SelectedIndex = 0;
        public DeviceInstance SelectedDevice;
        DirectInput directInput;
        List<DeviceInstance> controllers = new List<DeviceInstance>();
        int throttleAxisOffset = -1;
        int brakeAxisOffset = -1;

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

            throttleAxisOffset = chooseAxis.SelectedIndex;
        }

        private void bChangeBrakeAxis_Click(object sender, RoutedEventArgs e)
        {
            ChooseAxis chooseAxis = new ChooseAxis(controllers[SelectedIndex]);
            chooseAxis.ShowDialog();

            brakeAxisOffset = chooseAxis.SelectedIndex;
        }

        private void listboxDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            SelectedIndex = -1;

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

        private void bRaildriverPathBrowse_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();

            if (folderDialog.ShowDialog() == true)
            {
                tboxRaildriverPath.Text = folderDialog.SelectedPath;

                Directory.SetCurrentDirectory(folderDialog.SelectedPath);
            }
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            SelectedDevice = controllers[SelectedIndex];
            this.DialogResult = true;
        }
    }
}
