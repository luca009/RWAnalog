using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWAnalog.Classes
{
    public static class ConfigurationManager
    {
        public static List<Train> GetSavedTrains()
        {
            return new List<Train>() { TrainSimulatorManager.GetCurrentTrain() };
        }
    }
}
