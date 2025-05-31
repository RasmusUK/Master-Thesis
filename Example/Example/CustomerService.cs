using EventSourcingFramework.Application.Abstractions.ApiGateway;
using EventSourcingFramework.Core.Interfaces;

namespace Example;

public class CustomerService
{
    private readonly IRepository<Customer> repository;
    private readonly IApiGateway apiGateway;

    public CustomerService(IRepository<Customer> repository, IApiGateway apiGateway)
    {
        this.repository = repository;
        this.apiGateway = apiGateway;
    }

    public Task CreateCustomerAsync(string name)
    {
        var customer = new Customer { Name = name };
        return repository.CreateAsync(customer); 
    }
    
    public Task<IReadOnlyCollection<Customer>> FetchCustomersExternallyAsync()
    {
        return apiGateway.GetAsync<IReadOnlyCollection<Customer>>("/customers");
    }

    public Task SendCustomerAsync(Customer customer)
    {
        return apiGateway.SendAsync<Customer>(new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("/customers")
        });
    }
    
    public Task<CustomerResponse> PostCustomerAsync(CustomerRequest request)
    {
        return apiGateway.PostAsync<CustomerRequest, CustomerResponse>("/customers", request);
    }
}

public class CustomerRequest{}
public class CustomerResponse{}