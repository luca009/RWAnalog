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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using RWAnalog.Controls;

namespace RWAnalog
{
    /// <summary>
    /// Interaction logic for ChooseAxis.xaml
    /// </summary>
    public partial class ChooseAxis : Window
    {
        public int SelectedIndex = -1;
        public string SelectedAxisName = null;
        DirectInput directInput;
        //ListBoxItem axisListBoxItem = new ListBoxItem() { Content = new Grid(), HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top, Height = 22, Margin = new Thickness(1) };

        public ChooseAxis(DeviceInstance selectedDevice)
        {
            AsyncSetup(selectedDevice);
        }

        private async void AsyncSetup(DeviceInstance selectedDevice)
        {
            directInput = new DirectInput();

            Joystick joystick = new Joystick(directInput, selectedDevice.ProductGuid);
            joystick.Properties.BufferSize = 128;
            joystick.Acquire();

            await Task.Run(() => { Dispatcher.Invoke(() => { InitializeComponent(); }); });

            foreach (var axis in joystick.GetObjects())
            {
                AxisItem axisItem = new AxisItem();
                axisItem.Label = axis.Name;
                axisItem.Minimum = 0;
                axisItem.Maximum = 65535;
                axisItem.AxisSequence = axis.Offset;

                listboxAxes.Items.Add(axisItem);
            }

            await Task.Run(() => { UpdateProgressBarLoop(joystick); });
        }

        private void UpdateProgressBarLoop(Joystick joystick)
        {
            while (true)
            {
                joystick.Poll();
                var data = joystick.GetBufferedData();
                foreach (var state in data)
                {
                    try
                    {
                        int offset = state.RawOffset;

                        Dispatcher.Invoke(() => {
                            foreach (AxisItem item in listboxAxes.Items)
                            {
                                if (item.AxisSequence == offset)
                                {
                                    item.AxisName = state.Offset.ToString();
                                    item.Value = state.Value;
                                }
                            }
                        });
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                System.Threading.Thread.Sleep(50);
            }
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            SelectedIndex = ((AxisItem)listboxAxes.SelectedItem).AxisSequence;
            SelectedAxisName = ((AxisItem)listboxAxes.SelectedItem).AxisName;
            if (SelectedIndex == -1)
                return;
            if (SelectedAxisName == null)
            {
                MessageBox.Show("Due to technical limitations, you have to move the selected Axis before clicking OK.", "Sorry!",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            this.DialogResult = true;
        }
    }
}
