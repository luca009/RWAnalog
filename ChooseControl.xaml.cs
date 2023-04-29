using RWAnalog.Classes;
using System.Windows;

namespace RWAnalog
{
    /// <summary>
    /// Interaction logic for ChooseControl.xaml
    /// </summary>
    public partial class ChooseControl : Window
    {
        public string SelectedName { get; set; }
        public int SelectedIndex { get; set; }

        public ChooseControl()
        {
            var controllerValues = TrainSimulatorManager.GetActiveControllers();
            InitializeComponent();
            lboxList.ItemsSource = controllerValues;
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            if (lboxList.SelectedItem == null) return;

            SelectedIndex = lboxList.SelectedIndex;
            SelectedName = lboxList.SelectedItem.ToString();
            this.Close();
        }
    }
}
