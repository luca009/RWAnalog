using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
namespace RWAnalog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DirectInput directInput;

        [DllImport(@"RailDriver64.dll")]
        static extern void SetRailDriverConnected(bool isConnected);

        [DllImport(@"RailDriver64.dll")]
        static extern bool GetRailSimConnected();

        [DllImport(@"RailDriver64.dll")]
        static extern void SetRailSimValue(int controlID, float value);

        [DllImport(@"RailDriver64.dll")]
        static extern float GetRailSimValue(int controlID, int type);

        [DllImport(@"RailDriver64.dll")]
        static extern Boolean GetRailSimLocoChanged();

        [DllImport(@"RailDriver64.dll")]
        static extern Boolean GetRailSimCombinedThrottleBrake();

        [DllImport(@"RailDriver64.dll")]
        static extern string GetLocoName();

        [DllImport(@"RailDriver64.dll")]
        static extern IntPtr GetControllerList();

        [DllImport(@"RailDriver64.dll")]
        static extern float GetControllerValue(int controlID, int type);

        [DllImport(@"RailDriver64.dll")]
        static extern void SetControllerValue(int controlID, float value);

        public MainWindow()
        {
            directInput = new DirectInput();
            List<DeviceInstance> controllers = new List<DeviceInstance>();
            Setup setup = new Setup();
            DeviceInstance selectedController = new DeviceInstance();
            Joystick powerLever;
            bool trainHasNegativeThrottle = true; 
            bool trainHasCenterBrake = false;
            float throttleMax = 1f;
            float brakeMax = 1f;
            string throttleName = "ThrottleAndBrake";
            string brakeName = "VirtualBrake";
            bool debugMode = false;
            /*foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
            {
                // Add each found Device to setups ListBox and to Controllers
                setup.listboxDevices.Items.Add(deviceInstance.ProductName.ToString());
                setup.listboxDevices.SelectedItem = setup.listboxDevices.Items[0];
                controllers.Add(deviceInstance);
            }*/
            setup.ShowDialog();
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ChooseAxis chooseAxis = new ChooseAxis();
            //chooseAxis.Show();
        }
    }
}
