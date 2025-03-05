namespace EventSource.Core.Unit.Test;

public class EventTest
{
    [Fact]
    public void Create_Test_Event_Succeeds()
    {
        var e = new TestEvent(Guid.NewGuid());
        Assert.NotNull(e);
    }

    [Fact]
    public void Create_Test_Event_With_Id_Returns_Event_With_Id()
    {
        var id = Guid.NewGuid();
        var e = new TestEvent(id);
        Assert.Equal(id, e.EntityId);
    }

    [Fact]
    public void Create_Test_Event_With_Empty_Id_Fails()
    {
        Assert.Throws<ArgumentException>(() => new TestEvent(Guid.Empty));
    }
}
