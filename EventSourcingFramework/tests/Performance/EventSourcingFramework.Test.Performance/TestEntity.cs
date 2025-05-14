using EventSourcingFramework.Core.Attributes;
using EventSourcingFramework.Core.Models.Entity;

namespace EventSourcingFramework.Test.Performance;

public class TestEntity : Entity
{
    public string Name { get; set; }

    [PersonalData] public string Name1 { get; set; }

    [PersonalData] public string Name2 { get; set; }

    [PersonalData] public string Name3 { get; set; }

    [PersonalData] public string Name4 { get; set; }

    [PersonalData] public string Name5 { get; set; }

    public int Nr1 { get; set; }
    public int Nr2 { get; set; }
    public int Nr3 { get; set; }
    public int Nr4 { get; set; }
    public int Nr5 { get; set; }
    public double Nr6 { get; set; }
    public double Nr7 { get; set; }
    public double Nr8 { get; set; }
    public double Nr9 { get; set; }
    public double Nr10 { get; set; }

    public List<TestEntity> Children { get; set; } = new();
}