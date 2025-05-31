namespace Example;

public interface IOrderRepository
{
    Task<Guid> CreateOrderAsync(Guid customerId);
    Task UpdateOrderCustomerId(Guid orderId, Guid customerId);
    Task DeleteOrder(Guid orderId);
    Task<IReadOnlyCollection<Guid>> GetAllOrderIdsForCustomerIdAsync(Guid customerId);  
}