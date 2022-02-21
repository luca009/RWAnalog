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
using RWAnalog.Classes;
using Path = System.IO.Path;

namespace RWAnalog
{
    /// <summary>
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Setup : Window
    {
        public bool Configured = false;
        public int SelectedIndex = 0;
        public DeviceInstance SelectedDevice;
        DirectInput directInput;
        List<DeviceInstance> controllers = new List<DeviceInstance>();
        int throttleAxisOffset = -1;
        int brakeAxisOffset = -1;

        public Setup()
        {
            directInput = new DirectInput();

            GeneralConfiguration? configuration = SaveLoadManager.LoadConfiguration(App.Current.Properties["ConfigPath"].ToString());
            if (configuration != null && File.Exists(Path.Combine(configuration.Value.PluginsPath, "RailDriver64.dll")))
            {
                Directory.SetCurrentDirectory(configuration.Value.PluginsPath);

                IList<DeviceInstance> devices = directInput.GetDevices();

                foreach (var deviceInstance in devices)
                {
                    if (deviceInstance.ProductGuid == configuration.Value.SelectedDevice)
                    {
                        App.Current.Properties.Add("CurrentDevice", deviceInstance);
                        App.Current.Properties.Add("Configuration", configuration);
                        Configured = true;
                        return;
                    }
                }
            }

            InitializeComponent();
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
                if (!File.Exists(Path.Combine(folderDialog.SelectedPath, "RailDriver64.dll")))
                {
                    MessageBox.Show("This path does not contain the required files.\nTry again, and if this persists, validate Train Simulator's files.");
                    return;
                }

                tboxRaildriverPath.Text = folderDialog.SelectedPath;

                Directory.SetCurrentDirectory(folderDialog.SelectedPath);
            }
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(Path.Combine(tboxRaildriverPath.Text, "RailDriver64.dll")))
            {
                MessageBox.Show("The selected path does not contain the required files.\nTry again, and if this persists, validate Train Simulator's files.");
                return;
            }

            SelectedDevice = controllers[SelectedIndex];

            GeneralConfiguration configuration = new GeneralConfiguration();
            configuration.PluginsPath = tboxRaildriverPath.Text;
            configuration.SelectedDevice = SelectedDevice.ProductGuid;
            App.Current.Properties.Add("Configuration", configuration);

            this.DialogResult = true;
        }
    }
}
