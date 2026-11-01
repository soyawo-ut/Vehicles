using System.Diagnostics;

namespace Vehicles.Core
{
    public static class TripCalculator
    {
        private const int Steps = 40_000_000;

        // Heavy CPU calculation for one vehicle.
        // The token is checked on every loop step.
        public static double Calculate(string name, double odometer, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (odometer <= 0)
            {
                throw new InvalidOperationException($"{name} has no trips yet (odometer is 0 km).");
            }

            double sum = 0;
            for (int i = 1; i <= Steps; i++)
            {
                token.ThrowIfCancellationRequested();
                sum += Math.Sqrt(i + odometer) * Math.Sin(i);
            }

            return sum;
        }

        // Runs the calculation for one vehicle and catches errors,
        // so one bad vehicle does not stop the others.
        public static WorkResult Run(Vehicle vehicle, CancellationToken token)
        {
            WorkResult result = new WorkResult();
            result.Name = $"{vehicle.Make} {vehicle.Model}";
            double odometer = vehicle.Odometer;

            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                double value = Calculate(result.Name, odometer, token);
                result.State = "Done";
                result.Result = value.ToString("F4");
            }
            catch (OperationCanceledException)
            {
                result.State = "Cancelled";
                result.Result = "-";
            }
            catch (Exception ex)
            {
                result.State = "Error";
                result.Result = ex.Message;
            }
            stopwatch.Stop();
            result.DurationMs = stopwatch.ElapsedMilliseconds;

            return result;
        }
    }
}
