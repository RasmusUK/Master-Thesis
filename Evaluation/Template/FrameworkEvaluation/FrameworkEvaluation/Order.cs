using EventSourcingFramework.Core.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkEvaluation
{
    public class Order : Entity
    {
        public Guid CarId { get; set; }

        public Order(Guid carId)
        {
            CarId = carId;
        }
    }
}
