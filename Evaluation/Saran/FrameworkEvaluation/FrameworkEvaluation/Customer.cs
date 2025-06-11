using EventSourcingFramework.Core.Attributes;
using EventSourcingFramework.Core.Models.Entity;

namespace FrameworkEvaluation;

public class Customer : Entity
{
    [PersonalData]
    public string Name { get; set; }
    public int SchmeaVersion { get; set; } = 2;

}
