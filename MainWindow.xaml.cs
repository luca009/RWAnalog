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

        //Start changing here
        [DllImport(@"E:\SteamLibrary\steamapps\common\RailWorks\plugins\RailDriver64.dll")]
        static extern void SetRailDriverConnected(bool isConnected);

        [DllImport(@"E:\SteamLibrary\steamapps\common\RailWorks\plugins\RailDriver64.dll")]
        static extern bool GetRailSimConnected();

        [DllImport(@"E:\SteamLibrary\steamapps\common\RailWorks\plugins\RailDriver64.dll")]
        static extern void SetRailSimValue(int controlID, float value);

        [DllImport(@"E:\SteamLibrary\steamapps\common\RailWorks\plugins\RailDriver64.dll")]
        static extern float GetRailSimValue(int controlID, int type);

        [DllImport(@"E:\SteamLibrary\steamapps\common\RailWorks\plugins\RailDriver64.dll")]
        static extern Boolean GetRailSimLocoChanged();

        [DllImport(@"E:\SteamLibrary\steamapps\common\RailWorks\plugins\RailDriver64.dll")]
        static extern Boolean GetRailSimCombinedThrottleBrake();

        [DllImport(@"E:\SteamLibrary\steamapps\common\RailWorks\plugins\RailDriver64.dll")]
        static extern string GetLocoName();

        [DllImport(@"E:\SteamLibrary\steamapps\common\RailWorks\plugins\RailDriver64.dll")]
        static extern IntPtr GetControllerList();

        [DllImport(@"E:\SteamLibrary\steamapps\common\RailWorks\plugins\RailDriver64.dll")]
        static extern float GetControllerValue(int controlID, int type);

        [DllImport(@"E:\SteamLibrary\steamapps\common\RailWorks\plugins\RailDriver64.dll")]
        static extern void SetControllerValue(int controlID, float value);
        //Stop changing

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
