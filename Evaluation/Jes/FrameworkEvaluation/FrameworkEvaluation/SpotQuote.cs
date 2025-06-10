using EventSourcingFramework.Core.Models.Entity;
namespace FrameworkEvaluation;

public class SpotQuote:Entity
{
    public Guid CustomerId { get; set; }
    public double Price { get; set; }

    public SpotQuote(Guid customerId, double price)
    {
        CustomerId = customerId;
        Price = price;
    }
}