using RWAnalog.Classes;
using SharpDX.DirectInput;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

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

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        
        public AddAxis()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            RegisterHotKey(new WindowInteropHelper(this).Handle, 2, 6,
                122); // Register hotkey Ctl + Shift + F11 to detect axis (2 = Hotkey ID, 6 = Ctl + Shift, 122 = F11) 
            (PresentationSource.FromVisual(this) as HwndSource)?.AddHook(WndProc); // Register WndProc, to catch hotkey
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0312 && wParam.ToInt32() == 2) // If msg = Hotkey and Hotkey ID = 2 (The one we registered)
            {
                DetectAxis(); // Detect axis
            }

            return IntPtr.Zero;
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

        private async void DetectAxis()
        {
            bDetectControl.Content = "Detecting...";
            await Task.Run(() => { internalControl = TrainSimulatorManager.GetMovingControl(0.25f); });
            tboxControlName.Text = internalControl?.Name ?? string.Empty;
            bDetectControl.Content = "Detect (Ctl+Shift+F11)";
        }

        private void bDetectControl_Click(object sender, RoutedEventArgs e)
        {
            DetectAxis();
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

            if (InputGraph == null)
            {
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
            }
            else
            {
                control.OverrideInputGraph = InputGraph;
            }

            GraphDialog graphDialog = new GraphDialog(control.OverrideInputGraph);

            bool threadRunning = false;
            bool connectedToTS = TrainSimulatorManager.ConnectedToTrainSimulator();
            int currentControlId = -1;
            if (connectedToTS)
                currentControlId = internalControl.Value.ControllerId;

            GeneralConfiguration configuration = (GeneralConfiguration)App.Current.Properties["Configuration"];
            Guid deviceGuid = configuration.SelectedDevice;
            Thread inputThread = new Thread(() =>
            {
                DirectInput directInput = new DirectInput();
                Joystick joystick = new Joystick(directInput, deviceGuid);

                joystick.Properties.BufferSize = 128;
                joystick.Acquire();

                int controllerValue = 0;
                while (threadRunning)
                {
                    joystick.Poll();
                    var data = joystick.GetBufferedData();

                    for (int i = 0; i < data.Length; i++)
                    {
                        if (data[i].RawOffset != axisOffset)
                            continue;

                        controllerValue = data[i].Value;
                        break;
                    }

                    if (connectedToTS)
                        Dispatcher.Invoke(() => {
                            graphDialog.SetControllerValue(controllerValue, TrainSimulatorManager.GetControlValue(currentControlId));
                        });
                    else
                        Dispatcher.Invoke(() => { graphDialog.SetControllerValue(controllerValue); });

                    Thread.Sleep(50);
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
