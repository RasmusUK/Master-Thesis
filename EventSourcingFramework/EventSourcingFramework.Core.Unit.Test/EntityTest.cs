namespace EventSourcingFramework.Core.Unit.Test;

public class EntityTest
{
    [Fact]
    public void Create_Test_Entity_Succeeds()
    {
        var entity = Entity.Create<TestEntity>(Guid.NewGuid());
        Assert.NotNull(entity);
    }

    [Fact]
    public void Create_Test_Entity_With_Id_Returns_Entity_With_Id()
    {
        var id = Guid.NewGuid();
        var entity = Entity.Create<TestEntity>(id);
        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void Create_Test_Entity_With_Empty_Id_Fails()
    {
        Assert.Throws<ArgumentException>(() => Entity.Create<TestEntity>(Guid.Empty));
    }
}
