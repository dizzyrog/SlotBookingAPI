using FluentValidation;
using SlotBooking.Domain.DTOs;

namespace SlotBooking.Domain.Validators;

public class BookAvailableSlotDtoValidator : AbstractValidator<BookAvailableSlotDto>
{
    public BookAvailableSlotDtoValidator()
    {
        RuleFor(slot => slot.FacilityId)
            .NotEmpty()
            .WithMessage("FacilityId is required. from Validator");

        RuleFor(slot => slot.Start)
            .NotEmpty()
            .WithMessage("Slot start is required.")
            .Must((slot, start) => start > DateTimeOffset.Now)
            .WithMessage("Slot start should be in the future.")
            .Must((slot, start) => start < slot.End)
            .WithMessage("Slot start should be before end.");

        RuleFor(slot => slot.End)
            .NotEmpty()
            .WithMessage("Slot end is required.")
            .Must((slot, end) => end > DateTimeOffset.Now)
            .WithMessage("Slot end should be in the future.")
            .Must((slot, end) => end > slot.Start)
            .WithMessage("Slot end should be after end.");
        
        RuleFor(slot => slot.Patient)
            .NotEmpty()
            .WithMessage("Patient information is required.");
    }
}