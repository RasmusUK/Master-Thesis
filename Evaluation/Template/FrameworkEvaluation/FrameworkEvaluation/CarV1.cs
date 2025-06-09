using EventSourcingFramework.Core.Models.Entity;

namespace FrameworkEvaluation
{
    public class CarV1 : Entity
    {
        public string Model { get; set; }
        public int Year { get; set; }
        public int SchemaVersion { get; set; } = 1;

        public CarV1(string model, int year)
        {
            Model = model;
            Year = year;
        }
    }
}
