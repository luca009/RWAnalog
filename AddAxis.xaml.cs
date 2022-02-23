using RWAnalog.Classes;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public InputGraph InputGraph { get; private set; }

        public AddAxis()
        {
            InitializeComponent();
        }

        public bool? ShowEditDialog(TrainControl control)
        {
            internalControl = control;
            axisOffset = control.AssociatedAxis.AxisOffset;
            tboxAxisName.Text = "same as before";
            tboxControlName.Text = "same as before";
            tboxName.Text = control.Name;
            return this.ShowDialog();
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            if (internalControl == null)
            {
                MessageBox.Show("Complete the dialog first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TrainControl = new TrainControl(tboxName.Text, internalControl.Value.ControllerId) { AssociatedAxis = new Axis(axisOffset), OverrideInputGraph = InputGraph };
            DialogResult = true;
        }

        private async void bDetectControl_Click(object sender, RoutedEventArgs e)
        {
            bDetectControl.Content = "...";
            await Task.Run(() => { internalControl = TrainSimulatorManager.GetMovingControl(0.2f); });
            tboxControlName.Text = internalControl.Value.Name;
            bDetectControl.Content = "Detect";
        }

        private void bPickControl_Click(object sender, RoutedEventArgs e)
        {
            ChooseControl chooseControl = new ChooseControl();
            chooseControl.ShowDialog();
            internalControl = new TrainControl(chooseControl.SelectedName, chooseControl.SelectedIndex);
            tboxControlName.Text = chooseControl.SelectedName;
        }

        private void bPickAxis_Click(object sender, RoutedEventArgs e)
        {
            ChooseAxis chooseAxis = new ChooseAxis(App.Current.Properties["CurrentDevice"] as DeviceInstance);
            chooseAxis.ShowDialog();

            tboxAxisName.Text = chooseAxis.SelectedAxisName;
            axisOffset = chooseAxis.SelectedIndex;
        }

        private void bEditGraph_Click(object sender, RoutedEventArgs e)
        {
            if (internalControl == null)
            {
                MessageBox.Show("Select train control first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            TrainControl control = new TrainControl(tboxName.Text, internalControl.Value.ControllerId) { AssociatedAxis = new Axis(axisOffset) };

            try
            {
                control.OverrideInputGraph = internalControl.Value.OverrideInputGraph;
            }
            catch (Exception)
            {
                control.OverrideInputGraph = new InputGraph(false);
                control.OverrideInputGraph.Points.Add(new GraphPoint(0, control.MinimumValue));
                control.OverrideInputGraph.Points.Add(new GraphPoint(65535, control.MaximumValue));
            }

            GraphDialog graphDialog = new GraphDialog(control.OverrideInputGraph);

            bool threadRunning = false;
            GeneralConfiguration configuration = (GeneralConfiguration)App.Current.Properties["Configuration"];
            Guid deviceGuid = configuration.SelectedDevice;
            Thread inputThread = new Thread(() =>
            {
                DirectInput directInput = new DirectInput();
                Joystick joystick = new Joystick(directInput, deviceGuid);

                joystick.Properties.BufferSize = 128;
                joystick.Acquire();

                while (threadRunning)
                {
                    joystick.Poll();
                    var data = joystick.GetBufferedData();

                    for (int i = 0; i < data.Length; i++)
                    {
                        if (data[i].RawOffset != axisOffset)
                            continue;

                        Dispatcher.Invoke(() => { graphDialog.SetControllerValue(data[i].Value); });
                        break;
                    }

                    Thread.Sleep(100);
                }

                joystick.Unacquire();
                joystick.Dispose();
                directInput.Dispose();
            });

            threadRunning = true;
            inputThread.Start();

            graphDialog.Minimum = control.MinimumValue;
            graphDialog.Maximum = control.MaximumValue;
            graphDialog.SetPoints(control.OverrideInputGraph);
            graphDialog.ShowDialog();
            threadRunning = false;

            InputGraph = new InputGraph(false);
            InputGraph.Points.AddRange(graphDialog.Points);
        }
    }
}
