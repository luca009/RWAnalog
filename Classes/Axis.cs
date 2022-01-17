using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWAnalog.Classes
{
    public class Axis
    {
        public string AxisName { get; }
        public InputGraph DefaultAxisGraph { get; set; }

        public Axis(string name)
        {
            AxisName = name;
            DefaultAxisGraph = new InputGraph();
        }
    }
}
