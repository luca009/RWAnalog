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

            Train currentTrain = TrainSimulatorManager.GetCurrentTrain();
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

            ControlItem controlItem = new ControlItem(addAxis.TrainControl);
            listboxOptions.Items.Add(controlItem);
        }
    }
}
