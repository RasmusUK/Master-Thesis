using EventSourcingFramework.Core.Models.Entity;

namespace FrameworkEvaluation;

public class SpotQuote : Entity
{
    public Guid CustomerId { get; set; }
    public decimal Price { get; set; }
}