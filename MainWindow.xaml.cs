using RWAnalog.Classes;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Resources;
using System.Windows.Interop;

namespace RWAnalog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ConnectionStatus ConnectionStatus // ConnectionStatus Property so you know what the connection status is whenever you need it.
        {
            get => _connectionStatus;
            private set
            {
                StreamResourceInfo stream = null;
                switch (value)
                {
                    case ConnectionStatus.Failed:
                        gridConnectionStatus.Background = new SolidColorBrush(Color.FromRgb(180, 20, 20));
                        textConnectionStatus.Text = "Could not connect";
                        textCurrentTrain.Text = "no train";
                        bConnect.Visibility = Visibility.Visible;
                        bConnect.Content = "Try again (Ctl+Shift+F12)";
                        bConfigure.Visibility = Visibility.Collapsed;
                        stream = Application.GetResourceStream(
                            new Uri("pack://application:,,,/RWAnalog;component/Resources/connection_lost.wav"));
                        break;
                    case ConnectionStatus.NotConnected:
                        gridConnectionStatus.Background = new SolidColorBrush(Color.FromRgb(240, 115, 100));
                        textConnectionStatus.Text = "Disconnected from Train Simulator";
                        textCurrentTrain.Text = "no train";
                        bConnect.Visibility = Visibility.Visible;
                        bConnect.Content = "Connect (Ctl+Shift+F12)";
                        bConfigure.Visibility = Visibility.Collapsed;
                        break;
                    case ConnectionStatus.Connecting:
                        gridConnectionStatus.Background = new SolidColorBrush(Color.FromRgb(240, 190, 50));
                        bConnect.Visibility = Visibility.Collapsed;
                        textConnectionStatus.Text = "Connecting...";
                        bConfigure.Visibility = Visibility.Collapsed;
                        break;
                    case ConnectionStatus.Reconnecting:
                        gridConnectionStatus.Background = new SolidColorBrush(Color.FromRgb(240, 190, 50));
                        bConnect.Visibility = Visibility.Collapsed;
                        textConnectionStatus.Text = "Reconnecting...";
                        bConfigure.Visibility = Visibility.Collapsed;
                        stream = Application.GetResourceStream(
                            new Uri("pack://application:,,,/RWAnalog;component/Resources/reconnecting.wav"));
                        break;
                    case ConnectionStatus.NewTrain:
                        gridConnectionStatus.Background = new SolidColorBrush(Color.FromRgb(240, 190, 50));
                        bConnect.Visibility = Visibility.Collapsed;
                        textConnectionStatus.Text = "New vehicle! Configure now.";
                        bConfigure.Visibility = Visibility.Visible;
                        stream = Application.GetResourceStream(
                            new Uri("pack://application:,,,/RWAnalog;component/Resources/new_train.wav"));
                        break;
                    case ConnectionStatus.Connected:
                        gridConnectionStatus.Background = new SolidColorBrush(Color.FromRgb(70, 190, 65));
                        textConnectionStatus.Text = "Connected to Train Simulator";
                        bConnect.Visibility = Visibility.Collapsed;
                        bConfigure.Visibility = Visibility.Visible;
                        stream = Application.GetResourceStream(
                            new Uri("pack://application:,,,/RWAnalog;component/Resources/connected.wav"));
                        break;
                }

                if (stream == null) return;

                SoundPlayer player = new SoundPlayer(stream.Stream);
                player.Load();
                player.Play();
                player.Dispose();
                
                _connectionStatus = value;
            }
        }

        private ConnectionStatus _connectionStatus;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        InputManager inputManager;
        ConnectionManager connectionManager;
        DirectInput directInput;

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

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            RegisterHotKey(new WindowInteropHelper(this).Handle, 1, 6,
                123); // Register hotkey Ctl + Shift + F12 to connect to Train Sim (1 = Hotkey ID, 6 = Ctl + Shift, 123 = F12) 
            (PresentationSource.FromVisual(this) as HwndSource)?.AddHook(WndProc); // Register WndProc, to catch hotkey
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0312 && wParam.ToInt32() == 1) // If msg = Hotkey and Hotkey ID = 1 (The one we registered)
            {
                if (ConnectionStatus == ConnectionStatus.NotConnected || ConnectionStatus == ConnectionStatus.Failed) // If not connected or failed
                {
                    Connect(); // Connect to Train Sim
                }
            }

            return IntPtr.Zero;
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
                if (ConfigurationManager.IsCurrentTrainNew())
                {
                    ConnectionStatus = ConnectionStatus.NewTrain;
                    return;
                }

                inputManager.ChangeTrain(ConfigurationManager.GetCurrentTrainWithConfiguration());
                ConnectionStatus = ConnectionStatus.Connected;
            }
            else
            {
                ConnectionStatus = ConnectionStatus.Reconnecting;
                bool connectedTemp = false;
                await Task.Run(() => { connectedTemp = TrainSimulatorManager.ConnectToTrainSimulator(300); });

                if (connectedTemp)
                {
                    inputManager.ChangeTrain(ConfigurationManager.GetCurrentTrainWithConfiguration());
                    ConnectionStatus = ConnectionStatus.Connected;
                }
                else
                    ConnectionStatus = ConnectionStatus.Failed;
            }
        }

        private void connectionManager_TrainChanged(ConnectionManager sender, Train train)
        {
            textCurrentTrain.Text = train.ToString();
            inputManager.ChangeTrain(train);
        }

        private async void Connect()
        {
            bool connected = false;

            ConnectionStatus = ConnectionStatus.Connecting;
            await Task.Run(() => { connected = TrainSimulatorManager.ConnectToTrainSimulator(); });

            if (connected)
            {
                inputManager.ChangeTrain(ConfigurationManager.GetCurrentTrainWithConfiguration());

                inputManager.StartThread();
                connectionManager.StartThread();
            }
            else
                ConnectionStatus = ConnectionStatus.Failed;
        }

        private void bConnect_Click(object sender, RoutedEventArgs e)
        {
            Connect();
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
            inputManager.StopThread();
            TrainConfiguration trainConfiguration = new TrainConfiguration();
            trainConfiguration.ShowDialog();
            inputManager.StartThread();
            TrainSimulatorManager.ConnectToTrainSimulator();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            InputGraph graph = new InputGraph();
            graph.Points.Add(new GraphPoint(5000, -1f));
            GraphDialog graphDialog = new GraphDialog(graph);
            graphDialog.Show();
            graphDialog.Hide();
            graphDialog.UpdateCanvas();
            graphDialog.ShowDialog();
        }
    }
}
