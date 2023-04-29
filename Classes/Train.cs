using System;

namespace RWAnalog.Classes
{
    public class Train : IEquatable<Train>
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

        public override bool Equals(object other)
        {
            return Equals(other as Train);
        }
        
        public bool Equals(Train other)
        {
            return other != null &&
                   other.EngineName.Equals(EngineName) &&
                   other.Provider.Equals(Provider) &&
                   other.Product.Equals(Product);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Provider != null ? Provider.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Product != null ? Product.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (EngineName != null ? EngineName.GetHashCode() : 0);
                return hashCode;
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
