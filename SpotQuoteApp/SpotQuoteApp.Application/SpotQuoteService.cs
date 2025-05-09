using EventSourcingFramework.Core.Exceptions;
using EventSourcingFramework.Core.Interfaces;
using EventSourcingFramework.Infrastructure.Repositories.Interfaces;
using FluentValidation;
using SpotQuoteApp.Application.DTOs;
using SpotQuoteApp.Application.Interfaces;
using SpotQuoteApp.Application.Mappers;
using SpotQuoteApp.Core.AggregateRoots;
using SpotQuoteApp.Core.Validators;
using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Application;

public class SpotQuoteService : ISpotQuoteService
{
    private readonly IRepository<SpotQuote> spotQuoteRepository;
    private readonly IAddressService addressService;
    private readonly ICustomerService customerService;
    private readonly SpotQuoteValidator spotQuoteValidator;
    private readonly IBuyingRateService buyingRateService;
    private readonly ITransactionManager transactionManager;

    public SpotQuoteService(
        IRepository<SpotQuote> spotQuoteRepository,
        IAddressService addressService,
        ICustomerService customerService,
        SpotQuoteValidator spotQuoteValidator,
        IBuyingRateService buyingRateService,
        ITransactionManager transactionManager
    )
    {
        this.spotQuoteRepository = spotQuoteRepository;
        this.addressService = addressService;
        this.customerService = customerService;
        this.spotQuoteValidator = spotQuoteValidator;
        this.buyingRateService = buyingRateService;
        this.transactionManager = transactionManager;
    }

    public Task CreateSpotQuoteAsync(SpotQuoteDto spotQuote) =>
        HandleSpotQuoteUpsertAsync(spotQuote, isUpdate: false);

    public Task UpdateSpotQuoteAsync(SpotQuoteDto spotQuote) =>
        HandleSpotQuoteUpsertAsync(spotQuote, isUpdate: true);

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

    private async Task HandleSpotQuoteUpsertAsync(SpotQuoteDto spotQuote, bool isUpdate)
    {
        transactionManager.Begin();

        try
        {
            var addressFromId = await addressService.CreateIfNotExistsAsync(spotQuote.AddressFrom);
            var addressToId = await addressService.CreateIfNotExistsAsync(spotQuote.AddressTo);
            spotQuote.AddressFrom.Id = addressFromId;
            spotQuote.AddressTo.Id = addressToId;

            await buyingRateService.UpsertBuyingRatesAsync(spotQuote);

            var spotQuoteDomain = spotQuote.ToDomain();

            if (spotQuote.Status != BookingStatus.Draft)
            {
                var validationResult = await spotQuoteValidator.ValidateAsync(spotQuoteDomain);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);
            }

            if (isUpdate)
                await spotQuoteRepository.UpdateAsync(spotQuoteDomain);
            else
                await spotQuoteRepository.CreateAsync(spotQuoteDomain);

            await transactionManager.CommitAsync();
        }
        catch
        {
            await transactionManager.RollbackAsync();
            throw;
        }
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
