using EventSourcingFramework.Core.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkEvaluation
{
    public class Car : Entity
    {
        public Model Model { get; set; }
        public int Year { get; set; }
        public new int SchemaVersion { get; set; } = 2;

        public Car(Model model, int year)
        {
            Model = model;
            Year = year;
        }
    }
}
