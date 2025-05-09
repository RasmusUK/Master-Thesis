using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Core.ValueObjects;

namespace SpotQuoteApp.Application.Mappers;

public static class UserMapper
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Office = user.Office,
        };
    }

    public static User ToDomain(this UserDto userDto)
    {
        return new User(userDto.Id, userDto.Name, userDto.Email, userDto.Phone, userDto.Office);
    }
}
