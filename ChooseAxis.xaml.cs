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

namespace RWAnalog
{
    /// <summary>
    /// Interaction logic for ChooseAxis.xaml
    /// </summary>
    public partial class ChooseAxis : Window
    {
        public int SelectedIndex = -1;
        DirectInput directInput;
        //ListBoxItem axisListBoxItem = new ListBoxItem() { Content = new Grid(), HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top, Height = 22, Margin = new Thickness(1) };

        public ChooseAxis(DeviceInstance selectedDevice)
        {
            AsyncSetup(selectedDevice);
        }

        private async void AsyncSetup(DeviceInstance selectedDevice)
        {
            directInput = new DirectInput();

            //TextBlock axisText = new TextBlock() { Margin = new Thickness(1), HorizontalAlignment = HorizontalAlignment.Left };
            //ProgressBar axisValue = new ProgressBar() { Margin = new Thickness(70, 1, 0, 1), HorizontalAlignment = HorizontalAlignment.Right, Width = 180, Maximum = 65535 };
            //((Grid)axisListBoxItem.Content).Children.Add(axisText);
            //((Grid)axisListBoxItem.Content).Children.Add(axisValue);

            Joystick joystick = new Joystick(directInput, selectedDevice.ProductGuid);
            joystick.Properties.BufferSize = 128;
            joystick.Acquire();


            await Task.Run(() => { Dispatcher.Invoke(() => { InitializeComponent(); }); });

            foreach (var axis in joystick.GetObjects())
            {
                //string axisListBoxItemXaml = XamlWriter.Save(axisListBoxItem);
                //StringReader stringReader = new StringReader(axisListBoxItemXaml);
                //XmlReader xmlReader = XmlReader.Create(stringReader);
                //ListBoxItem axisListBoxItemClone = (ListBoxItem)XamlReader.Load(xmlReader);
                //((TextBlock)((Grid)axisListBoxItemClone.Content).Children[0]).Text = axis.Name;
                //listboxAxes.Items.Add(axisListBoxItemClone);

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
                                        item.Value = state.Value;
                            }
                        });
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                System.Threading.Thread.Sleep(10);
            }
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            SelectedIndex = listboxAxes.SelectedIndex;
            if (SelectedIndex == -1)
                return;
            this.DialogResult = true;
        }
    }
}
