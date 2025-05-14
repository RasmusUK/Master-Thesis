namespace SpotQuoteApp.Core.ValueObjects;

public record MailOptions(
    bool SendCopyToMe,
    bool ShowCostSpec,
    string Comments,
    ICollection<Guid> RecipientUserIds
);
