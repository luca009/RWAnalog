using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RWAnalog.Classes
{
    public class InputManager
    {
        Train train;
        Thread thread;
        bool threadRunning = false;
        DirectInput directInput;
        Guid deviceGuid;
        Joystick joystick;

        public InputManager(int pollWait, Guid deviceGuid)
        {
            this.deviceGuid = deviceGuid;

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

        public void ChangeTrain(Train train)
        {
            this.train = train;
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
