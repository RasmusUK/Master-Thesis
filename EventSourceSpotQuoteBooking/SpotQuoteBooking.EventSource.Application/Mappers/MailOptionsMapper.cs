using SpotQuoteBooking.EventSource.Application.DTOs;
using SpotQuoteBooking.EventSource.Core.ValueObjects;

namespace SpotQuoteBooking.EventSource.Application.Mappers;

public static class MailOptionsMapper
{
    public static MailOptionsDto ToDto(
        this MailOptions mailOptions,
        ICollection<UserDto> userRecipients
    )
    {
        return new MailOptionsDto
        {
            ShowCostSpec = mailOptions.ShowCostSpec,
            Comments = mailOptions.Comments,
            SendCopyToMe = mailOptions.SendCopyToMe,
            UserRecipients = userRecipients,
        };
    }

    public static MailOptions ToDomain(this MailOptionsDto mailOptionsDto)
    {
        return new MailOptions(
            mailOptionsDto.SendCopyToMe,
            mailOptionsDto.ShowCostSpec,
            mailOptionsDto.Comments,
            mailOptionsDto.UserRecipients.Select(x => x.Id).ToList()
        );
    }
}
