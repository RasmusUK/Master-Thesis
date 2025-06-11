using EventSourcingFramework.Core.Models.Entity;

namespace FrameworkEvaluation;

public class CustomerV1 : Entity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int SchmeaVersion { get; set; } = 1;

}
