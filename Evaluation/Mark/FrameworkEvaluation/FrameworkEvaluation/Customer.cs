using EventSourcingFramework.Core.Models.Entity;

namespace FrameworkEvaluation;

public class Customer: Entity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
