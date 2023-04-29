using System;
using System.Threading;
using System.Windows;

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
        bool threadRunning;
        int pollWait;
        Thread thread;


        public ConnectionManager(int pollWait)
        {
            this.pollWait = pollWait;

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
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ConnectionStatusChanged(this, TrainSimulatorManager.ConnectedToTrainSimulator());
                            });
                        previousConnected = TrainSimulatorManager.ConnectedToTrainSimulator();
                        continue;
                    }

                    if (!sameTrain)
                    {
                        if (TrainChanged != null)
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                TrainChanged(this, ConfigurationManager.GetCurrentTrainWithConfiguration());
                            });
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

            try
            {
                thread.Start();
            }
            catch (Exception)
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
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    ConnectionStatusChanged(this,
                                        TrainSimulatorManager.ConnectedToTrainSimulator());
                                });
                            previousConnected = TrainSimulatorManager.ConnectedToTrainSimulator();
                            continue;
                        }

                        if (TrainChanged != null)
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                TrainChanged(this, ConfigurationManager.GetCurrentTrainWithConfiguration());
                            });
                        previousTrain = TrainSimulatorManager.QuickGetCurrentTrain();
                    }
                });
                thread.Start();
            }
        }

        public void StopThread()
        {
            threadRunning = false;
        }

        public void ForceStopThread()
        {
            threadRunning = false;
            thread.Abort();
        }

        public bool ThreadRunning()
        {
            return threadRunning;
        }
    }
}