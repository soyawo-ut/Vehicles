using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Vehicles.Core;

namespace Vehicles.WpfApp
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<Vehicle> _carGarage = new();
        private readonly ObservableCollection<Vehicle> _boatGarage = new();

        public MainWindow()
        {
            InitializeComponent();

            CarGarageListBox.ItemsSource = _carGarage;
            BoatGarageListBox.ItemsSource = _boatGarage;
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

                        SignalGarage(_carGarage);

                        _carGarage.Add(vehicle);
                        Log($"Added to car garage: {vehicle.Make} {vehicle.Model}");
                        break;

                    case "Boat":
                        vehicle = new Boat(make, model);

                        SignalGarage(_boatGarage);

                        _boatGarage.Add(vehicle);
                        Log($"Added to boat garage: {vehicle.Make} {vehicle.Model}");
                        break;

                    case "AmphibiousCar":
                        vehicle = new AmphibiousCar(make, model);

                        SignalGarage(_carGarage);

                        _carGarage.Add(vehicle);
                        Log($"Added to car garage: {vehicle.Make} {vehicle.Model}");
                        break;

                    default:
                        Log("Unknown vehicle type.");
                        return;
                }

                MakeTextBox.Clear();
                ModelTextBox.Clear();
            }
            catch (ArgumentException ex)
            {
                Log(ex.Message);
            }
        }

        private void MoveCar_Click(object sender, RoutedEventArgs e)
        {
            MoveSelectedVehicle(CarGarageListBox);
        }

        private void MoveBoat_Click(object sender, RoutedEventArgs e)
        {
            MoveSelectedVehicle(BoatGarageListBox);
        }

        private void MoveSelectedVehicle(ListBox listBox)
        {
            if (listBox.SelectedItem is not Vehicle vehicle)
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

                listBox.Items.Refresh();
            }
            catch (ArgumentException ex)
            {
                Log(ex.Message);
            }
        }

        private void RemoveCar_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedVehicle(CarGarageListBox, _carGarage);
        }

        private void RemoveBoat_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedVehicle(BoatGarageListBox, _boatGarage);
        }

        private void RemoveSelectedVehicle(
            ListBox listBox,
            ObservableCollection<Vehicle> garage)
        {
            if (listBox.SelectedItem is not Vehicle vehicle)
            {
                Log("Select a vehicle first.");
                return;
            }

            garage.Remove(vehicle);

            Log($"Removed: {vehicle.Make} {vehicle.Model}");

            BlinkGarage(garage);
        }

        private void SignalGarage(ObservableCollection<Vehicle> garage)
        {
            foreach (Vehicle vehicle in garage)
            {
                Log(vehicle.Signal());
            }
        }

        private void BlinkGarage(ObservableCollection<Vehicle> garage)
        {
            foreach (Vehicle vehicle in garage)
            {
                Log(vehicle.BlinkLights());
            }
        }

        private void Log(string message)
        {
            LogTextBox.AppendText(message + Environment.NewLine);
            LogTextBox.ScrollToEnd();
        }
    }
}