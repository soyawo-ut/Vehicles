using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Vehicles.Core;

namespace Vehicles.WpfApp
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<Vehicle> _carGarage = new();
        private readonly ObservableCollection<Vehicle> _boatGarage = new();

        // Vol.3: one shared token for all calculations
        private CancellationTokenSource? _cts;
        private WorkResult[]? _lastSequential;
        private WorkResult[]? _lastParallel;
        private long _sequentialMs;
        private long _parallelMs;

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

        // ---------- Vol.3: sequential and parallel work ----------

        // Both modes use the same input: all vehicles from both garages
        private List<Vehicle> GetAllVehicles()
        {
            List<Vehicle> list = new List<Vehicle>();
            list.AddRange(_carGarage);
            list.AddRange(_boatGarage);
            return list;
        }

        private async void Sequential_Click(object sender, RoutedEventArgs e)
        {
            List<Vehicle> input = GetAllVehicles();
            if (input.Count == 0)
            {
                WorkStatusText.Text = "Add vehicles first.";
                return;
            }

            StartWork("Sequential");
            CancellationToken token = _cts!.Token;
            List<WorkResult> results = new List<WorkResult>();

            Stopwatch total = Stopwatch.StartNew();
            try
            {
                foreach (Vehicle vehicle in input)
                {
                    WorkStatusText.Text = $"Sequential: working on {vehicle.Make} {vehicle.Model}...";

                    // wait until this vehicle is finished, then start the next one
                    WorkResult result = await Task.Run(() => TripCalculator.Run(vehicle, token));
                    results.Add(result);
                    WriteReportLine(result);
                }
            }
            finally
            {
                total.Stop();
                _lastSequential = results.ToArray();
                _sequentialMs = total.ElapsedMilliseconds;
                FinishWork(total.ElapsedMilliseconds);
            }
        }

        private async void Parallel_Click(object sender, RoutedEventArgs e)
        {
            List<Vehicle> input = GetAllVehicles();
            if (input.Count == 0)
            {
                WorkStatusText.Text = "Add vehicles first.";
                return;
            }

            StartWork("Parallel");
            CancellationToken token = _cts!.Token;
            WorkStatusText.Text = "Parallel: all vehicles are working at the same time...";

            WorkResult[] results = new WorkResult[0];
            Stopwatch total = Stopwatch.StartNew();
            try
            {
                List<Task<WorkResult>> tasks = new List<Task<WorkResult>>();
                foreach (Vehicle vehicle in input)
                {
                    // TripCalculator.Run checks the token itself and returns "Cancelled",
                    // so Task.WhenAll does not throw
                    tasks.Add(Task.Run(() => TripCalculator.Run(vehicle, token)));
                }

                results = await Task.WhenAll(tasks);

                foreach (WorkResult result in results)
                {
                    WriteReportLine(result);
                }
            }
            finally
            {
                total.Stop();
                _lastParallel = results;
                _parallelMs = total.ElapsedMilliseconds;
                FinishWork(total.ElapsedMilliseconds);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (_cts != null)
            {
                _cts.Cancel();
                CancelButton.IsEnabled = false;
                WorkStatusText.Text = "Cancelling...";
            }
        }

        private void ClearReport_Click(object sender, RoutedEventArgs e)
        {
            ReportTextBox.Clear();
            _lastSequential = null;
            _lastParallel = null;
        }

        private void StartWork(string mode)
        {
            _cts = new CancellationTokenSource();

            SequentialButton.IsEnabled = false;
            ParallelButton.IsEnabled = false;
            ClearReportButton.IsEnabled = false;
            CancelButton.IsEnabled = true;
            WorkProgressBar.IsIndeterminate = true; // it moves, so the UI is not frozen

            ReportTextBox.AppendText($"===== {mode} mode ({DateTime.Now:HH:mm:ss}) ====={Environment.NewLine}");
            ReportTextBox.AppendText($"{"Vehicle",-24}{"State",-11}{"Duration",11}   Result / error{Environment.NewLine}");
        }

        private void FinishWork(long totalMs)
        {
            ReportTextBox.AppendText($"Total time: {totalMs} ms{Environment.NewLine}");
            CompareResults();
            ReportTextBox.AppendText(Environment.NewLine);
            ReportTextBox.ScrollToEnd();

            WorkProgressBar.IsIndeterminate = false;
            SequentialButton.IsEnabled = true;
            ParallelButton.IsEnabled = true;
            ClearReportButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
            WorkStatusText.Text = $"Finished. Total time {totalMs} ms.";

            _cts?.Dispose();
            _cts = null;
        }

        private void WriteReportLine(WorkResult result)
        {
            ReportTextBox.AppendText($"{result.Name,-24}{result.State,-11}{result.DurationMs,8} ms   {result.Result}{Environment.NewLine}");
            ReportTextBox.ScrollToEnd();
        }

        // When both modes have been run, compare the results
        private void CompareResults()
        {
            if (_lastSequential == null || _lastParallel == null)
            {
                return;
            }

            bool same = _lastSequential.Length == _lastParallel.Length;
            if (same)
            {
                for (int i = 0; i < _lastSequential.Length; i++)
                {
                    if (_lastSequential[i].Name != _lastParallel[i].Name ||
                        _lastSequential[i].State != _lastParallel[i].State ||
                        _lastSequential[i].Result != _lastParallel[i].Result)
                    {
                        same = false;
                    }
                }
            }

            ReportTextBox.AppendText($"--- Comparison ---{Environment.NewLine}");
            ReportTextBox.AppendText($"Results are the same: {(same ? "yes" : "no")}{Environment.NewLine}");
            ReportTextBox.AppendText($"Sequential: {_sequentialMs} ms, parallel: {_parallelMs} ms{Environment.NewLine}");
            if (_parallelMs > 0)
            {
                double faster = (double)_sequentialMs / _parallelMs;
                ReportTextBox.AppendText($"Parallel was {faster:F1}x faster{Environment.NewLine}");
            }
        }

        private void Log(string message)
        {
            LogTextBox.AppendText(message + Environment.NewLine);
            LogTextBox.ScrollToEnd();
        }
    }
}