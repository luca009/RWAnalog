using SharpDX.DirectInput;
using System;
using System.Threading;

namespace RWAnalog.Classes
{
    public class InputManager
    {
        Train train;
        Thread thread;
        int pollWait;
        bool threadRunning;
        DirectInput directInput;
        Guid deviceGuid;
        Joystick joystick;

        public InputManager(int pollWait, Guid deviceGuid)
        {
            this.deviceGuid = deviceGuid;
            this.pollWait = pollWait;

            thread = new Thread(() =>
            {
                directInput = new DirectInput();
                joystick = new Joystick(directInput, deviceGuid);

                joystick.Properties.BufferSize = 128;
                joystick.Acquire();

                while (threadRunning)
                {
                    Update();
                    Thread.Sleep(pollWait);
                }

                joystick.Unacquire();
                joystick.Dispose();
                directInput.Dispose();
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
                    directInput = new DirectInput();
                    joystick = new Joystick(directInput, deviceGuid);

                    joystick.Properties.BufferSize = 128;
                    joystick.Acquire();

                    while (threadRunning)
                    {
                        Update();
                        Thread.Sleep(pollWait);
                    }
                });
                thread.Start();
            }
        }

        public void StopThread()
        {
            if (joystick != null)
                joystick.Unacquire();
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

        public void ChangeTrain(Train train)
        {
            this.train = train;
        }

        public Train GetTrain()
        {
            return this.train;
        }

        public void Update()
        {
            if (train == null) return;

            joystick.Poll();
            var data = joystick.GetBufferedData();

            for (int i = 0; i < data.Length; i++)
            {
                for (int control = 0; control < train.Controls.Length; control++)
                {
                    if (train.Controls[control].AssociatedAxis == null)
                        continue;

                    int offset = train.Controls[control].AssociatedAxis.AxisOffset;

                    if (data[i].RawOffset != offset)
                        continue;

                    float y = train.Controls[control].OverrideInputGraph.Evaluate(data[i].Value);
                    TrainSimulatorManager.SetControlValue(train.Controls[control], y);

                    break;
                }
            }
        }
    }
}
