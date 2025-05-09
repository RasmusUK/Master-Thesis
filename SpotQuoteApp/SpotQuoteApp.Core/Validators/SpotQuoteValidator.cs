using EventSourcingFramework.Core.Interfaces;
using FluentValidation;
using SpotQuoteApp.Core.AggregateRoots;
using SpotQuoteApp.Core.ValueObjects.Enums;

namespace SpotQuoteApp.Core.Validators;

public class SpotQuoteValidator : AbstractValidator<SpotQuote>
{
    public SpotQuoteValidator(
        IRepository<Customer> customerRepository,
        IRepository<Address> addressRepository
    )
    {
        RuleFor(spotQuote => spotQuote.CustomerId)
            .NotEmpty()
            .WithMessage("Customer id cannot be empty.")
            .MustAsync(
                async (customerId, _) =>
                {
                    var customer = await customerRepository.ReadByIdAsync(customerId);
                    return customer is not null;
                }
            )
            .WithMessage("Customer does not exist.");

        RuleFor(spotQuote => spotQuote.AddressFromId)
            .NotEmpty()
            .WithMessage("Address from id cannot be empty.")
            .MustAsync(
                async (addressFromId, _) =>
                {
                    var address = await addressRepository.ReadByIdAsync(addressFromId);
                    return address is not null;
                }
            )
            .WithMessage("Address from does not exist.");

        RuleFor(spotQuote => spotQuote.AddressToId)
            .NotEmpty()
            .WithMessage("Address to id cannot be empty.")
            .MustAsync(
                async (addressToId, _) =>
                {
                    var address = await addressRepository.ReadByIdAsync(addressToId);
                    return address is not null;
                }
            )
            .WithMessage("Address to does not exist.");

        RuleFor(spotQuote => spotQuote.ValidUntil)
            .NotEmpty()
            .WithMessage("Valid until date cannot be empty.")
            .Must(validUntil => validUntil > DateTime.UtcNow)
            .WithMessage("Valid until date must be in the future.");

        RuleFor(spotQuote => spotQuote.Quotes)
            .NotEmpty()
            .WithMessage("Quotes cannot be empty.")
            .Must(quotes => quotes.Count > 0)
            .WithMessage("At least one quote is required.")
            .Must(quotes => quotes.All(q => q.TotalPrice > 0))
            .WithMessage("All quotes must have a price greater than zero.");

        RuleFor(spotQuote => spotQuote.ShippingDetails)
            .NotEmpty()
            .WithMessage("Shipping details cannot be empty.")
            .Must(shippingDetails => shippingDetails.Collis.Count > 0)
            .WithMessage("At least one colli is required.")
            .Must(shippingDetails => shippingDetails.Collis.All(c => c.Weight > 0))
            .WithMessage("All collis must have a weight greater than zero.")
            .Must(shippingDetails => shippingDetails.Collis.All(c => c.Cbm > 0))
            .WithMessage("All collis must have a cbm greater than zero.");

        RuleFor(spotQuote => spotQuote.Direction)
            .NotEmpty()
            .WithMessage("Direction cannot be empty.")
            .Must(d => d == Direction.Import || d == Direction.Export)
            .WithMessage("Invalid direction value.");

        RuleFor(spotQuote => spotQuote.Incoterm)
            .NotEmpty()
            .WithMessage("Incoterm cannot be empty.")
            .Must(i => Incoterm.GetAll().Contains(i))
            .WithMessage("Invalid incoterm value.");

        RuleFor(spotQuote => spotQuote.TransportMode)
            .NotEmpty()
            .WithMessage("Transport mode cannot be empty.")
            .Must(tm => TransportMode.GetAll().Contains(tm))
            .WithMessage("Invalid transport mode value.");

        RuleFor(spotQuote => spotQuote.Status)
            .NotEmpty()
            .WithMessage("Status cannot be empty.")
            .Must(s => BookingStatus.GetAll().Contains(s))
            .WithMessage("Invalid status value.");
    }
}
