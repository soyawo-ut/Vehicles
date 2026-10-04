namespace Vehicles.Core
{
    public class Car : Vehicle, IDriveable
    {
        public Car(string make, string model)
            : base(make, model)
        {
        }

        public string Drive(double km)
        {
            AddKm(km);
            return $"{Make} {Model} drove {km:0.##} km.";
        }

        public override string Move(double km)
        {
            return Drive(km);
        }
    }
}