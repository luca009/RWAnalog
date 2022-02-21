using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RWAnalog.Classes
{
    public class Train
    {
        public string Provider { get; }
        public string Product { get; }
        public string EngineName { get; }
        public TrainControl[] Controls { get; set; }
        public bool UnsavedChanges { get; set; }

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

        public string ToSingleString()
        {
            return $"{Provider}.:.{Product}.:.{EngineName}";
        }

        public string ToSingleControlsString()
        {
            string returnString = "";
            foreach (TrainControl control in Controls)
            {
                returnString += control.ControllerId;
                returnString += "::";
            }
            return returnString;
        }

        public override string ToString()
        {
            return $"{Product} by {Provider}";
        }

        public override bool Equals(object obj)
        {
            try
            {
                Train temp = obj as Train;

                return temp.EngineName.Equals(EngineName) &&
                    temp.Provider.Equals(Provider) &&
                    temp.Product.Equals(Product);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public struct TrainControl
    {
        public string Name { get; }
        public int ControllerId { get; }
        public Axis AssociatedAxis { get; set; }
        public float Value { get { return TrainSimulatorManager.GetControlValue(this); } }
        public float MinimumValue { get { return TrainSimulatorManager.GetControlValue(this, ValueType.Minimum); } }
        public float MaximumValue { get { return TrainSimulatorManager.GetControlValue(this, ValueType.Maximum); } }

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
