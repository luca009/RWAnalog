using RWAnalog.Classes;
using RWAnalog.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RWAnalog
{
    /// <summary>
    /// Interaction logic for TrainConfiguration.xaml
    /// </summary>
    public partial class TrainConfiguration : Window
    {
        public TrainConfiguration()
        {
            InitializeComponent();
            cboxTrains.ItemsSource = ConfigurationManager.GetSavedTrains();

            Train currentTrain = ConfigurationManager.GetCurrentTrain();
            for (int i = 0; i < cboxTrains.Items.Count; i++)
            {
                if (!cboxTrains.Items[i].Equals(currentTrain))
                    continue;

                cboxTrains.SelectedIndex = i;
                break;
            }
        }

        private void cboxTrains_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboxTrains.SelectedItem.GetType() != typeof(Train))
                return;

            TrainControl[] controls = ((Train)cboxTrains.SelectedItem).Controls;
            foreach (TrainControl control in controls)
            {
                if (control.AssociatedAxis == null)
                    continue;

                ControlItem controlItem = new ControlItem(control);
                listboxOptions.Items.Add(controlItem);
            }
        }

        private void bAddAxis_Click(object sender, RoutedEventArgs e)
        {
            AddAxis addAxis = new AddAxis();
            addAxis.ShowDialog();

            List<Train> cboxList = (List<Train>)cboxTrains.ItemsSource;
            Train train = cboxTrains.SelectedItem as Train;
            List<Train> savedTrains = ConfigurationManager.GetSavedTrains();
            foreach (Train savedTrain in savedTrains)
            {
                if (savedTrain.ToSingleString().Equals(cboxTrains.SelectedItem))
                {
                    train = savedTrain;
                }
            }

            train.Controls[addAxis.TrainControl.ControllerId] = addAxis.TrainControl;
            //ConfigurationManager.SaveTrain(train);

            cboxList[cboxTrains.SelectedIndex] = train;
            cboxTrains.ItemsSource = cboxList;

            train.UnsavedChanges = true;

            ControlItem controlItem = new ControlItem(addAxis.TrainControl);
            listboxOptions.Items.Add(controlItem);
        }

        private void bRemoveAxis_Click(object sender, RoutedEventArgs e)
        {
            if (listboxOptions.SelectedItem == null || listboxOptions.SelectedItem.GetType() != typeof(ControlItem))
                return;

            Train train = cboxTrains.SelectedItem as Train;
            List<Train> savedTrains = ConfigurationManager.GetSavedTrains();
            foreach (Train savedTrain in savedTrains)
            {
                if (savedTrain.ToSingleString().Equals(cboxTrains.SelectedItem))
                {
                    train = savedTrain;
                }
            }

            ControlItem controlItem = (ControlItem)listboxOptions.SelectedItem;

            train.Controls[controlItem.Control.ControllerId] = new TrainControl("", controlItem.Control.ControllerId);
            train.UnsavedChanges = true;

            listboxOptions.Items.Remove(controlItem);
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            if (cboxTrains.SelectedItem.GetType() != typeof(Train))
                return;

            Train currentTrain = ((MainWindow)App.Current.MainWindow).GetInputManagerTrain();
            List<Train> temp = cboxTrains.ItemsSource as List<Train>;
            for (int i = 0; i < temp.Count; i++)
            {
                Train train = temp[i];
                if (!train.UnsavedChanges)
                    continue;

                ConfigurationManager.SaveTrain(train);

                if (train.ToSingleString() == TrainSimulatorManager.QuickGetCurrentTrain())
                    ((MainWindow)App.Current.MainWindow).ChangeInputManagerTrain(train);
            }

            DialogResult = true;
        }
    }
}
