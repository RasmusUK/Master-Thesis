using EventSourcingFramework.Application.Abstractions.ApiGateway;
using EventSourcingFramework.Core.Interfaces;

namespace Example;

public class OrderRepository : IOrderRepository
{
    private readonly IRepository<Order> orderRepository;

    public OrderRepository(IRepository<Order> orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    public async Task<Guid> CreateOrderAsync(Guid customerId)
    {
        var order = new Order
        {
            CustomerId = customerId
        };
        await orderRepository.CreateAsync(order);
        return order.Id;
    }

    public async Task UpdateOrderCustomerId(Guid orderId, Guid customerId)
    {
        var order = await orderRepository.ReadByIdAsync(orderId);
        order!.CustomerId = customerId;
        await orderRepository.UpdateAsync(order);
    }

    public async Task DeleteOrder(Guid orderId)
    {
        var order = await orderRepository.ReadByIdAsync(orderId);
        await orderRepository.DeleteAsync(order!);
    }

    public Task<IReadOnlyCollection<Guid>> GetAllOrderIdsForCustomerIdAsync(Guid customerId)
    {
        return orderRepository.ReadAllProjectionsByFilterAsync(o => o.Id, o => o.CustomerId == customerId);
    }
}
