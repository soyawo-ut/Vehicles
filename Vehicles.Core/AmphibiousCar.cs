namespace Vehicles.Core
{
    public class AmphibiousCar : Car, ISwimmable
    {
        public AmphibiousCar(string make, string model)
            : base(make, model)
        {
        }

        public string Swim(double km)
        {
            AddKm(km);
            return $"{Make} {Model} swam {km:0.##} km.";
        }

        public override string Move(double km)
        {
            return Drive(km);
        }
    }
}