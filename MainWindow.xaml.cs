using RWAnalog.Classes;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RWAnalog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        InputManager inputManager;
        ConnectionManager connectionManager;
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
        static extern IntPtr GetLocoName();

        [DllImport(@"RailDriver64.dll")]
        static extern IntPtr GetControllerList();

        [DllImport(@"RailDriver64.dll")]
        static extern float GetControllerValue(int controlID, int type);

        [DllImport(@"RailDriver64.dll")]
        static extern void SetControllerValue(int controlID, float value);

        public MainWindow()
        {
            App.Current.Properties.Add("ConfigPath", AppDomain.CurrentDomain.BaseDirectory + @"\config.xml");

            directInput = new DirectInput();

            Setup setup = new Setup();
            if (!setup.Configured)
            {
                setup.ShowDialog();

                App.Current.Properties.Add("CurrentDevice", setup.SelectedDevice);
            }

            Guid controllerGuid = ((DeviceInstance)App.Current.Properties["CurrentDevice"]).ProductGuid;
            ConfigurationManager.GetSavedTrains();

            InitializeComponent();

            inputManager = new InputManager(50, controllerGuid);
            connectionManager = new ConnectionManager(50);

            connectionManager.ConnectionStatusChanged += connectionManager_ConnectionStatusChanged;
            connectionManager.TrainChanged += connectionManager_TrainChanged;
        }

        public void ChangeInputManagerTrain(Train train)
        {
            inputManager.ChangeTrain(train);
        }

        public Train GetInputManagerTrain()
        {
            return inputManager.GetTrain();
        }

        private async void connectionManager_ConnectionStatusChanged(ConnectionManager sender, bool connected)
        {
            if (connected)
            {
                inputManager.ChangeTrain(ConfigurationManager.GetCurrentTrainWithConfiguration());
                SetConnectionStatus(ConnectionStatus.Connected);
            }
            else
            {
                SetConnectionStatus(ConnectionStatus.Connecting);
                bool connectedTemp = false;
                await Task.Run(() => { connectedTemp = TrainSimulatorManager.ConnectToTrainSimulator(300); });

                if (connectedTemp)
                {
                    inputManager.ChangeTrain(ConfigurationManager.GetCurrentTrainWithConfiguration());
                    SetConnectionStatus(ConnectionStatus.Connected);
                }
                else
                    SetConnectionStatus(ConnectionStatus.Failed);
            }
        }

        private void connectionManager_TrainChanged(ConnectionManager sender, Train train)
        {
            textCurrentTrain.Text = train.ToString();
            inputManager.ChangeTrain(train);
        }

        private async void bConnect_Click(object sender, RoutedEventArgs e)
        {
            bool connected = false;

            SetConnectionStatus(ConnectionStatus.Connecting);
            await Task.Run(() => { connected = TrainSimulatorManager.ConnectToTrainSimulator(); });

            if (connected)
            {
                inputManager.ChangeTrain(ConfigurationManager.GetCurrentTrainWithConfiguration());
                SetConnectionStatus(ConnectionStatus.Connected);

                inputManager.StartThread();
                //inputManager.ChangeTrain(ConfigurationManager.GetCurrentTrainWithConfiguration());
                connectionManager.StartThread();
            }
            else
                SetConnectionStatus(ConnectionStatus.Failed);

            //connectionThread.Start();
            //connectionThread.Join();
            //MessageBox.Show(TrainSimulatorManager.ConnectToTrainSimulator().ToString());
        }

        private void SetConnectionStatus(ConnectionStatus connectionStatus)
        {
            switch (connectionStatus)
            {
                case ConnectionStatus.Failed:
                    gridConnectionStatus.Background = new SolidColorBrush(Color.FromRgb(180, 20, 20));
                    textConnectionStatus.Text = "Could not connect";
                    textCurrentTrain.Text = "no train";
                    bConnect.Visibility = Visibility.Visible;
                    bConnect.Content = "Try again";
                    break;
                case ConnectionStatus.NotConnected:
                    gridConnectionStatus.Background = new SolidColorBrush(Color.FromRgb(240, 115, 100));
                    textConnectionStatus.Text = "Disconnected from Train Simulator";
                    textCurrentTrain.Text = "no train";
                    bConnect.Visibility = Visibility.Visible;
                    bConnect.Content = "Connect";
                    break;
                case ConnectionStatus.Connecting:
                    gridConnectionStatus.Background = new SolidColorBrush(Color.FromRgb(240, 190, 50));
                    bConnect.Visibility = Visibility.Collapsed;
                    textConnectionStatus.Text = "Connecting...";
                    break;
                case ConnectionStatus.Connected:
                    gridConnectionStatus.Background = new SolidColorBrush(Color.FromRgb(70, 190, 65));
                    textConnectionStatus.Text = "Connected to Train Simulator";
                    bConnect.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string path = App.Current.Properties["ConfigPath"].ToString();
            List<Train> savedTrains = (List<Train>)App.Current.Properties["SavedTrains"];
            GeneralConfiguration configuration = (GeneralConfiguration)App.Current.Properties["Configuration"];

            SaveLoadManager.Save(path, savedTrains, configuration);

            inputManager.StopThread();
            connectionManager.StopThread();

            Thread.Sleep(1000); //wait a second for threads to close down gracefully, then force quit

            inputManager.ForceStopThread();
            connectionManager.ForceStopThread();
            Application.Current.Shutdown();
        }

        private void bConfigure_Click(object sender, RoutedEventArgs e)
        {
            TrainConfiguration trainConfiguration = new TrainConfiguration();
            trainConfiguration.ShowDialog();
        }
    }
}
