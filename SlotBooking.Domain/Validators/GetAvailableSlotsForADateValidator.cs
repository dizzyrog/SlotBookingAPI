using FluentValidation;

namespace SlotBooking.Domain.Validators;

public class GetAvailableSlotsForADateValidator : AbstractValidator<DateTimeOffset>
{
    public GetAvailableSlotsForADateValidator()
    {
        RuleFor(date => date)
            .Must(x => x.DayOfWeek == DayOfWeek.Monday)
            .WithMessage("The date should be a Monday.");
    }
}