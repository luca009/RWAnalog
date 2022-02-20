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

        public static void SaveTrain(Train train)
        {
            if (!App.Current.Properties.Contains("SavedTrains"))
                App.Current.Properties.Add("SavedTrains", new List<Train>());

            List<Train> trains = (List<Train>)App.Current.Properties["SavedTrains"];

            if (trains.Contains(train))
            {
                int index = trains.IndexOf(train);
                trains[index] = train;
            }
            else
            {
                trains.Add(train);
            }

            App.Current.Properties["SavedTrains"] = trains;
        }

        public static Train GetCurrentTrainWithConfiguration()
        {
            Train train = TrainSimulatorManager.GetCurrentTrain();
            if (train == null)
                return null;
            if (((List<Train>)App.Current.Properties["SavedTrains"]) == null)
                return train;
            
            Train newTrain = ((List<Train>)App.Current.Properties["SavedTrains"]).First((x) => { return x.ToString() == train.ToString(); });
            return newTrain;
        }
    }
}
