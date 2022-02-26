using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RWAnalog.Classes
{
    public enum ConnectionStatus
    {
        NotConnected, Connecting, Connected, Reconnecting, Failed, NewTrain
    }

    public enum ValueType
    {
        Current, Minimum, Maximum
    }

    public static class TrainSimulatorManager
    {
#if WIN64
        [DllImport(@"RailDriver64.dll")]
        static extern void SetRailDriverConnected(bool isConnected);

        [DllImport(@"RailDriver64.dll")]
        static extern bool GetRailSimConnected();

        [DllImport(@"RailDriver64.dll")]
        static extern IntPtr GetLocoName();

        [DllImport(@"RailDriver64.dll")]
        static extern IntPtr GetControllerList();

        [DllImport(@"RailDriver64.dll")]
        static extern float GetControllerValue(int controlID, int type);

        [DllImport(@"RailDriver64.dll")]
        static extern void SetControllerValue(int controlID, float value);
#else
        [DllImport(@"RailDriver.dll")]
        static extern void SetRailDriverConnected(bool isConnected);

        [DllImport(@"RailDriver.dll")]
        static extern bool GetRailSimConnected();

        [DllImport(@"RailDriver.dll")]
        static extern IntPtr GetLocoName();

        [DllImport(@"RailDriver.dll")]
        static extern IntPtr GetControllerList();

        [DllImport(@"RailDriver.dll")]
        static extern float GetControllerValue(int controlID, int type);

        [DllImport(@"RailDriver.dll")]
        static extern void SetControllerValue(int controlID, float value);
#endif

        public static bool ConnectToTrainSimulator(int secondsWait = 5)
        {
            //retry a few times in quick succession
            //íf this fails, wait a bit longer and try again
            for (int longWait = 0; longWait < secondsWait * 2; longWait++)
            {
                for (int smallWait = 0; smallWait < 5; smallWait++)
                {
                    if (GetRailSimConnected() == true)
                        return true;

                    SetRailDriverConnected(true);

                    System.Threading.Thread.Sleep(10);
                }

                System.Threading.Thread.Sleep(500);
            }

            return GetRailSimConnected();
        }

        public static bool ConnectedToTrainSimulator()
        {
            return GetRailSimConnected();
        }

        public static void DisconnectFromTrainSimulator()
        {
            SetRailDriverConnected(false);
        }

        public static Train GetCurrentTrain()
        {
            string locoName = Marshal.PtrToStringAnsi(GetLocoName());
            string controllers = Marshal.PtrToStringAnsi(GetControllerList());

            return new Train(locoName, controllers);
        }

        public static string QuickGetCurrentTrain()
        {
            return Marshal.PtrToStringAnsi(GetLocoName());
        }

        public static void SetControlValue(TrainControl control, float value)
        {
            SetControllerValue(control.ControllerId, value);
        }

        public static float GetControlValue(TrainControl control, ValueType type = ValueType.Current)
        {
            switch (type)
            {
                default:
                case ValueType.Current:
                    return GetControllerValue(control.ControllerId, 0);

                case ValueType.Minimum:
                    return GetControllerValue(control.ControllerId, 1);

                case ValueType.Maximum:
                    return GetControllerValue(control.ControllerId, 2);
            }
        }

        public static string[] GetActiveControllers()
        {
            string controllers = Marshal.PtrToStringAnsi(GetControllerList());
            return controllers.Split(new string[] { "::" }, StringSplitOptions.None);
        }

        public static TrainControl GetMovingControl(float tolerance)
        {
            //save all current values as reference, to detect which ones have changed
            string[] controllers = GetActiveControllers();
            float[,] referenceValues = new float[controllers.Length, 3];

            for (int i = 0; i < controllers.Length; i++)
            {
                referenceValues[i, 0] = GetControllerValue(i, 0);
                referenceValues[i, 1] = GetControllerValue(i, 1);
                referenceValues[i, 2] = GetControllerValue(i, 2);
            }

            Dictionary<int, float> changedControllersWithDifference = new Dictionary<int, float>();
            bool changesFound = false;
            while (!changesFound)
            {
                //detect changes within the tolerance, save items with a high enough one
                for (int i = 0; i < controllers.Length; i++)
                {
                    if (EvaluateControl(referenceValues, i, GetControllerValue(i, 0)) < tolerance)
                        continue;

                    changesFound = true;
                    changedControllersWithDifference.Add(i, EvaluateControl(referenceValues, i, GetControllerValue(i, 0)));
                }

                System.Threading.Thread.Sleep(100);
            }

            //return the item with the largest difference
            var sortedDict = from entry in changedControllersWithDifference orderby entry.Value descending select entry;
            int index = sortedDict.First().Key;
            TrainControl control = new TrainControl(controllers[index], index);
            return control;
        }

        private static float EvaluateControl(float[,] referenceValues, int index, float currentValue)
        {
            float oldPercent = LinearRemap(referenceValues[index, 0], referenceValues[index, 1], referenceValues[index, 2], 0f, 1f);
            float newPercent = LinearRemap(currentValue, referenceValues[index, 1], referenceValues[index, 2], 0f, 1f);
            return Math.Abs(oldPercent - newPercent);
        }

        private static float LinearRemap(float value, float originMin, float originMax, float resultMin, float resultMax)
        {
            return (value - originMin) * (resultMax - resultMin) / (originMax - originMin) + resultMin;
        }
    }
}
