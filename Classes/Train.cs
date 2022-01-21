using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RWAnalog.Classes
{
    public struct Train
    {

        public string Provider { get; }
        public string Product { get; }
        public string EngineName { get; }
        public TrainControl[] Controls { get; set; }

        public Train(string locoNameString, string controlsString)
        {
            string[] properties = locoNameString.Split(new string[] { ".:." }, StringSplitOptions.None);

            Provider = properties[0];
            Product = properties[1];
            EngineName = properties[2];

            string[] controls = controlsString.Split(new string[] { "::" }, StringSplitOptions.None);
            Controls = new TrainControl[controls.Length];

            for (int i = 0; i < controls.Length; i++)
            {
                Controls[i] = new TrainControl(controls[i], i);
            }
        }
    }

    public struct TrainControl
    {
        [DllImport(@"RailDriver64.dll")]
        static extern float GetControllerValue(int controlID, int type);

        public string Name { get; }
        public int ControllerId { get; }
        public Axis AssociatedAxis { get; set; }
        public float MinimumValue { get { return GetControllerValue(ControllerId, 1); } }
        public float MaximumValue { get { return GetControllerValue(ControllerId, 2); } }

        private InputGraph overrideInputGraph;
        public InputGraph OverrideInputGraph {
            get
            {
                if (overrideInputGraph == null)
                    return AssociatedAxis.DefaultAxisGraph;
                return overrideInputGraph;
            }
            set
            {
                overrideInputGraph = value;
            }
        }

        public TrainControl(string name, int controllerId)
        {
            Name = name;
            ControllerId = controllerId;
            AssociatedAxis = null;
            overrideInputGraph = null;
        }
    }
}
