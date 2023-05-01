using SharpDX.DirectInput;
using System;
using System.Threading.Tasks;
using System.Windows;
using RWAnalog.Controls;

namespace RWAnalog
{
    /// <summary>
    /// Interaction logic for ChooseAxis.xaml
    /// </summary>
    public partial class ChooseAxis : Window
    {
        public int SelectedIndex { get; private set; }
        public string SelectedAxisName { get; private set; }
        DirectInput directInput;
        //ListBoxItem axisListBoxItem = new ListBoxItem() { Content = new Grid(), HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top, Height = 22, Margin = new Thickness(1) };

        public ChooseAxis(DeviceInstance selectedDevice)
        {
            SelectedIndex = -1;
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
                        // ignored
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
