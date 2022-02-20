using RWAnalog.Classes;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
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
            directInput = new DirectInput();

            Setup setup = new Setup();
            setup.ShowDialog();

            App.Current.Properties.Add("CurrentDevice", setup.SelectedDevice);
            Guid controllerGuid = setup.SelectedDevice.ProductGuid;

            InitializeComponent();

            inputManager = new InputManager(50, controllerGuid);
            connectionManager = new ConnectionManager(50);

            connectionManager.ConnectionStatusChanged += connectionManager_ConnectionStatusChanged;
            connectionManager.TrainChanged += connectionManager_TrainChanged;
        }

        private void connectionManager_ConnectionStatusChanged(ConnectionManager sender, bool connected)
        {
            if (connected)
                SetConnectionStatus(ConnectionStatus.Connected);
            else
            {
                Thread connectionThread = new Thread(() =>
                {
                    Dispatcher.Invoke(() => { SetConnectionStatus(ConnectionStatus.Connecting); });
                    bool connectedTemp = TrainSimulatorManager.ConnectToTrainSimulator(120);

                    if (connectedTemp)
                        Dispatcher.Invoke(() => { SetConnectionStatus(ConnectionStatus.Connected); });
                    else
                        Dispatcher.Invoke(() => { SetConnectionStatus(ConnectionStatus.Failed); });
                });
                    
                connectionThread.Start();
            }
        }

        private void connectionManager_TrainChanged(ConnectionManager sender, Train train)
        {
            textCurrentTrain.Text = train.ToString();
            inputManager.ChangeTrain(train);
        }

        private void bConnect_Click(object sender, RoutedEventArgs e)
        {
            Thread connectionThread = new Thread(() =>
            {
                Dispatcher.Invoke(() => { SetConnectionStatus(ConnectionStatus.Connecting); });
                bool connected = TrainSimulatorManager.ConnectToTrainSimulator();

                if (connected)
                    Dispatcher.Invoke(() => { SetConnectionStatus(ConnectionStatus.Connected); });
                else
                    Dispatcher.Invoke(() => { SetConnectionStatus(ConnectionStatus.Failed); });
            });

            connectionThread.Start();

            inputManager.StartThread();
            connectionManager.StartThread();

            //MessageBox.Show(TrainSimulatorManager.ConnectToTrainSimulator().ToString());
        }

        private void SetConnectionStatus(ConnectionStatus connectionStatus)
        {
            switch (connectionStatus)
            {
                case ConnectionStatus.Failed:
                    gridConnectionStatus.Background = new SolidColorBrush(Color.FromRgb(180, 20, 20));
                    bConnect.Visibility = Visibility.Visible;
                    textConnectionStatus.Text = "Could not connect";
                    bConnect.Content = "Try again";
                    break;
                case ConnectionStatus.NotConnected:
                    gridConnectionStatus.Background = new SolidColorBrush(Color.FromRgb(240, 115, 100));
                    bConnect.Visibility = Visibility.Visible;
                    textConnectionStatus.Text = "Disconnected from Train Simulator";
                    bConnect.Content = "Connect";
                    break;
                case ConnectionStatus.Connecting:
                    gridConnectionStatus.Background = new SolidColorBrush(Color.FromRgb(240, 190, 50));
                    bConnect.Visibility = Visibility.Collapsed;
                    textConnectionStatus.Text = "Connecting...";
                    break;
                case ConnectionStatus.Connected:
                    gridConnectionStatus.Background = new SolidColorBrush(Color.FromRgb(70, 190, 65));
                    bConnect.Visibility = Visibility.Collapsed;
                    textConnectionStatus.Text = "Connected to Train Simulator";
                    break;
                default:
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            inputManager.StopThread();
            connectionManager.StopThread();
        }

        private void bConfigure_Click(object sender, RoutedEventArgs e)
        {
            TrainConfiguration trainConfiguration = new TrainConfiguration();
            trainConfiguration.ShowDialog();
        }
    }
}
