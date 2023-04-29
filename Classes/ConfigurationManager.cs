using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RWAnalog.Classes
{
    public static class ConfigurationManager
    {
        public static Train GetCurrentTrain()
        {
            string currentTrainName = TrainSimulatorManager.QuickGetCurrentTrain();
            return GetSavedTrains().Single(t => t.ToSingleString() == currentTrainName);
        }

        public static List<Train> GetSavedTrains()
        {
            List<Train> trains = new List<Train>();

            if (!Application.Current.Properties.Contains("SavedTrains"))
            {
                trains = SaveLoadManager.LoadTrains(Application.Current.Properties["ConfigPath"].ToString());
                Application.Current.Properties.Add("SavedTrains", trains);
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
            List<Train> trains = (List<Train>)Application.Current.Properties["SavedTrains"];

            if (!trains.Contains(train))
            {
                trains.Add(train);
            }

            Application.Current.Properties["SavedTrains"] = trains;
        }

        public static Train GetCurrentTrainWithConfiguration()
        {
            Train train = TrainSimulatorManager.GetCurrentTrain();
            if (train == null)
                return null;
            if (((List<Train>)Application.Current.Properties["SavedTrains"]) == null)
                return train;

            Train newTrain = train;
            try
            {
                newTrain = ((List<Train>)Application.Current.Properties["SavedTrains"]).First((x) =>
                {
                    return x.ToString() == train.ToString();
                });
            }
            catch (Exception)
            {
                //ignore if it can't find the train saved
            }

            return newTrain;
        }

        public static bool IsCurrentTrainNew()
        {
            Train train = TrainSimulatorManager.GetCurrentTrain();
            if (train == null || ((List<Train>)Application.Current.Properties["SavedTrains"]) == null)
                return true;

            return !((List<Train>)Application.Current.Properties["SavedTrains"]).Contains(train);
        }
    }
}