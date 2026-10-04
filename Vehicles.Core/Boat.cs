namespace Vehicles.Core
{
    public class Boat : Vehicle, ISwimmable
    {
        public Boat(string make, string model)
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
            return Swim(km);
        }
    }
}