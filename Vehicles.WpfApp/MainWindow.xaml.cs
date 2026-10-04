using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Vehicles.Core;

namespace Vehicles.WpfApp
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<Vehicle> _vehicles = new();

        public MainWindow()
        {
            InitializeComponent();
            VehiclesListBox.ItemsSource = _vehicles;
        }

        private void AddVehicle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string make = MakeTextBox.Text;
                string model = ModelTextBox.Text;

                if (VehicleTypeComboBox.SelectedItem is not ComboBoxItem selectedItem)
                {
                    Log("Select vehicle type.");
                    return;
                }

                string type = selectedItem.Content?.ToString() ?? "";

                Vehicle vehicle;

                switch (type)
                {
                    case "Car":
                        vehicle = new Car(make, model);
                        break;

                    case "Boat":
                        vehicle = new Boat(make, model);
                        break;

                    case "AmphibiousCar":
                        vehicle = new AmphibiousCar(make, model);
                        break;

                    default:
                        Log("Unknown vehicle type.");
                        return;
                }

                _vehicles.Add(vehicle);

                Log($"Added: {vehicle.Make} {vehicle.Model}");

                MakeTextBox.Clear();
                ModelTextBox.Clear();
            }
            catch (ArgumentException ex)
            {
                Log(ex.Message);
            }
        }

        private void MoveVehicle_Click(object sender, RoutedEventArgs e)
        {
            if (VehiclesListBox.SelectedItem is not Vehicle vehicle)
            {
                Log("Select a vehicle first.");
                return;
            }

            if (!double.TryParse(DistanceTextBox.Text, out double km))
            {
                Log("Distance must be a number.");
                return;
            }

            if (km <= 0)
            {
                Log("Distance must be greater than zero.");
                return;
            }

            try
            {
                string message = vehicle.Move(km);

                Log(message);
                Log($"Odometer: {vehicle.Odometer:0.##} km");

                VehiclesListBox.Items.Refresh();
            }
            catch (ArgumentException ex)
            {
                Log(ex.Message);
            }
        }

        private void RemoveVehicle_Click(object sender, RoutedEventArgs e)
        {
            if (VehiclesListBox.SelectedItem is not Vehicle vehicle)
            {
                Log("Select a vehicle first.");
                return;
            }

            _vehicles.Remove(vehicle);

            Log($"Removed: {vehicle.Make} {vehicle.Model}");
        }

        private void Log(string message)
        {
            LogTextBox.AppendText(message + Environment.NewLine);
            LogTextBox.ScrollToEnd();
        }
    }
}