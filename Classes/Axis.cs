using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWAnalog.Classes
{
    public class Axis
    {
        public int AxisOffset { get; }
        //public string AxisName { get; }
        public InputGraph DefaultAxisGraph { get; set; }

        public Axis(int offset)
        {
            AxisOffset = offset;
            //AxisName = name;
            DefaultAxisGraph = new InputGraph();
        }
    }
}
