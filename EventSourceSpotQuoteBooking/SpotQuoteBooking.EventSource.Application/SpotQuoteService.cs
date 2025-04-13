using EventSource.Core.Exceptions;
using EventSource.Core.Interfaces;
using FluentValidation;
using SpotQuoteBooking.EventSource.Application.DTOs;
using SpotQuoteBooking.EventSource.Application.Interfaces;
using SpotQuoteBooking.EventSource.Application.Mappers;
using SpotQuoteBooking.EventSource.Core.AggregateRoots;
using SpotQuoteBooking.EventSource.Core.Validators;

namespace SpotQuoteBooking.EventSource.Application;

public class SpotQuoteService : ISpotQuoteService
{
    private readonly IRepository<SpotQuote> spotQuoteRepository;
    private readonly IAddressService addressService;
    private readonly ICustomerService customerService;
    private readonly AbstractValidator<SpotQuote> spotQuoteValidator;

    public SpotQuoteService(
        IRepository<SpotQuote> spotQuoteRepository,
        IAddressService addressService,
        ICustomerService customerService,
        AbstractValidator<SpotQuote> spotQuoteValidator
    )
    {
        this.spotQuoteRepository = spotQuoteRepository;
        this.addressService = addressService;
        this.customerService = customerService;
        this.spotQuoteValidator = spotQuoteValidator;
    }

    public async Task CreateSpotQuoteAsync(SpotQuoteDto spotQuote)
    {
        await addressService.UpsertAddressAsync(spotQuote.AddressFrom);
        await addressService.UpsertAddressAsync(spotQuote.AddressTo);

        var spotQuoteDomain = spotQuote.ToDomain();

        var validationResult = await spotQuoteValidator.ValidateAsync(spotQuoteDomain);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        await spotQuoteRepository.CreateAsync(spotQuoteDomain);
    }

    public async Task UpdateSpotQuoteAsync(SpotQuoteDto spotQuote)
    {
        await addressService.UpsertAddressAsync(spotQuote.AddressFrom);
        await addressService.UpsertAddressAsync(spotQuote.AddressTo);

        var spotQuoteDomain = spotQuote.ToDomain();

        var validationResult = await spotQuoteValidator.ValidateAsync(spotQuoteDomain);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        await spotQuoteRepository.UpdateAsync(spotQuoteDomain);
    }

    public async Task<SpotQuoteDto?> GetSpotQuoteByIdAsync(Guid id)
    {
        var spotQuote = await spotQuoteRepository.ReadByIdAsync(id);
        if (spotQuote is null)
            return null;

        return await FillSpotQuoteDtoAsync(spotQuote);
    }

    public async Task<IReadOnlyCollection<SpotQuoteDto>> GetAllSpotQuotesAsync()
    {
        var spotQuotes = await spotQuoteRepository.ReadAllAsync();
        var dtos = await Task.WhenAll(spotQuotes.Select(FillSpotQuoteDtoAsync));
        return dtos;
    }

    private async Task<SpotQuoteDto> FillSpotQuoteDtoAsync(SpotQuote spotQuote)
    {
        var customer = await customerService.GetCustomerByIdAsync(spotQuote.CustomerId);
        if (customer is null)
            throw new NotFoundException(
                $"Customer with id '{spotQuote.CustomerId}' not found for SpotQuote with id '{spotQuote.Id}'."
            );

        var users = customer
            .Users.Where(u => spotQuote.MailOptions.RecipientUserIds.Contains(u.Id))
            .ToList();

        var addressFrom = await addressService.GetAddressByIdAsync(spotQuote.AddressFromId);
        if (addressFrom is null)
            throw new NotFoundException(
                $"Address with id '{spotQuote.AddressFromId}' not found for SpotQuote with id '{spotQuote.Id}'."
            );
        var addressTo = await addressService.GetAddressByIdAsync(spotQuote.AddressToId);
        if (addressTo is null)
            throw new NotFoundException(
                $"Address with id '{spotQuote.AddressToId}' not found for SpotQuote with id '{spotQuote.Id}'."
            );

        return spotQuote.ToDto(customer, addressFrom, addressTo, users);
    }
}
