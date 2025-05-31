using EventSourcingFramework.Core.Interfaces;

namespace Example;

public class Test
{
    private readonly IOrderRepository orderRepository;
    private readonly IRepository<Order> genericOrderRepository;

    public Test(IOrderRepository orderRepository, IRepository<Order> genericOrderRepository)
    {
        this.orderRepository = orderRepository;
        this.genericOrderRepository = genericOrderRepository;
    }

    [Fact]
    public async Task Test_Create()
    {
        var customerId = Guid.NewGuid();
        await orderRepository.CreateOrderAsync(customerId);

        var orderIds = await genericOrderRepository.ReadAllAsync();
        Assert.Contains(orderIds, o => o.CustomerId == customerId);
    }
    
    [Fact]
    public async Task Test_Update()
    {
        var customerId1 = Guid.NewGuid();
        var customerId2 = Guid.NewGuid();
        var orderId = await orderRepository.CreateOrderAsync(customerId1);
        
        var order = await genericOrderRepository.ReadByIdAsync(orderId);
        Assert.Equal(customerId1, order!.CustomerId);
        
        await orderRepository.UpdateOrderCustomerId(orderId, customerId2);
        
        var updatedOrder = await genericOrderRepository.ReadByIdAsync(orderId);
        Assert.Equal(customerId2, updatedOrder!.CustomerId);
    }
    
    [Fact]
    public async Task Test_Delete()
    {
        var customerId = Guid.NewGuid();
        var orderId = await orderRepository.CreateOrderAsync(customerId);
        
        var order = await genericOrderRepository.ReadByIdAsync(orderId);
        Assert.NotNull(order);
        
        await orderRepository.DeleteOrder(orderId);
        
        var deletedOrder = await genericOrderRepository.ReadByIdAsync(orderId);
        Assert.Null(deletedOrder);
    }
    
    [Fact]
    public async Task Test_GetAllOrderIdsForCustomerId()
    {
        var customerId = Guid.NewGuid();
        await orderRepository.CreateOrderAsync(customerId);
        await orderRepository.CreateOrderAsync(customerId);
        
        var orderIds = await orderRepository.GetAllOrderIdsForCustomerIdAsync(customerId);
        Assert.Equal(2, orderIds.Count);
    }
}