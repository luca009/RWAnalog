using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RWAnalog.Classes
{
    public class ConnectionManager
    {
        public delegate void ConnectionStatusChangedDelegate(ConnectionManager sender, bool connected);
        public event ConnectionStatusChangedDelegate ConnectionStatusChanged;
        public delegate void TrainChangedDelegate(ConnectionManager sender, Train train);
        public event TrainChangedDelegate TrainChanged;

        string previousTrain;
        bool previousConnected;
        bool threadRunning = false;
        Thread thread;
        

        public ConnectionManager(int pollWait)
        {
            thread = new Thread(() =>
            {
                while (threadRunning)
                {
                    Thread.Sleep(pollWait);

                    bool sameConnected = previousConnected == TrainSimulatorManager.ConnectedToTrainSimulator();
                    bool sameTrain = previousTrain == TrainSimulatorManager.QuickGetCurrentTrain();

                    if (sameConnected && sameTrain)
                        continue;

                    if (!sameConnected)
                    {
                        if (ConnectionStatusChanged != null)
                            App.Current.Dispatcher.Invoke(() => { ConnectionStatusChanged(this, TrainSimulatorManager.ConnectedToTrainSimulator()); });
                        previousConnected = TrainSimulatorManager.ConnectedToTrainSimulator();
                        continue;
                    }

                    if (!sameTrain)
                    {
                        if (TrainChanged != null)
                            App.Current.Dispatcher.Invoke(() => { TrainChanged(this, ConfigurationManager.GetCurrentTrainWithConfiguration()); });
                        previousTrain = TrainSimulatorManager.QuickGetCurrentTrain();
                    }
                }
            });
        }

        public void StartThread()
        {
            if (threadRunning)
                return;

            threadRunning = true;
            thread.Start();
        }

        public void StopThread()
        {
            threadRunning = false;
        }
    }
}
