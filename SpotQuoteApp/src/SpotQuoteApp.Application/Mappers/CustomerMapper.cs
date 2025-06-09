using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Core.DomainObjects;

namespace SpotQuoteApp.Application.Mappers;

public static class CustomerMapper
{
    public static CustomerDto ToDto(this Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Users = customer.Users.Select(UserMapper.ToDto).ToList(),
            ConcurrencyVersion = customer.ConcurrencyVersion,
        };
    }

    public static Customer ToDomain(this CustomerDto customerDto)
    {
        return new Customer(customerDto.Name)
        {
            Id = customerDto.Id,
            Users = customerDto.Users.Select(UserMapper.ToDomain).ToList(),
            ConcurrencyVersion = customerDto.ConcurrencyVersion,
        };
    }
}
