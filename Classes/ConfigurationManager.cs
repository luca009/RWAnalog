using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWAnalog.Classes
{
    public static class ConfigurationManager
    {
        public static Train GetCurrentTrain()
        {
            string comparison = TrainSimulatorManager.QuickGetCurrentTrain();
            foreach (Train train in GetSavedTrains())
            {
                if (train.ToSingleString() != comparison)
                    continue;

                return train;
            }

            return TrainSimulatorManager.GetCurrentTrain();
        }

        public static List<Train> GetSavedTrains()
        {
            List<Train> trains = new List<Train>();

            if (!App.Current.Properties.Contains("SavedTrains"))
            {
                trains = SaveLoadManager.LoadTrains(App.Current.Properties["ConfigPath"].ToString());
                App.Current.Properties.Add("SavedTrains", trains);
            }

            if (!TrainSimulatorManager.ConnectedToTrainSimulator())
                return trains;

            Train currentTrain = GetCurrentTrainWithConfiguration(); //see if this works
            if (!trains.Contains(currentTrain))
                trains.Add(currentTrain);

            return trains;
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

            Train newTrain = train;
            try
            {
                newTrain = ((List<Train>)App.Current.Properties["SavedTrains"]).First((x) => { return x.ToString() == train.ToString(); });
            }
            catch (Exception) { } //ignore if it can't find the train saved
            return newTrain;
        }
    }
}
