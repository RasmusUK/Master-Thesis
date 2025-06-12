using EventSourcingFramework.Application.Abstractions.ApiGateway;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Repositories.Interfaces;

namespace Example;

public class OrderRepository : IOrderRepository
{
    private readonly IRepository<Order> orderRepository;
    private readonly ITransactionManager transactionManager;

    public OrderRepository(IRepository<Order> orderRepository, ITransactionManager transactionManager)
    {
        this.orderRepository = orderRepository;
        this.transactionManager = transactionManager;
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
    
    public async Task DeleteAllOrdersForCustomerIdAsync(Guid customerId)
    {
        var orders = await orderRepository.ReadAllByFilterAsync(o => o.CustomerId == customerId);
        
        transactionManager.Begin();

        try
        {
            foreach (var order in orders)
            {
                await orderRepository.DeleteAsync(order);
            }
            await transactionManager.CommitAsync();
        }
        catch
        {
            await transactionManager.RollbackAsync();
            throw;
        }
    }
}
