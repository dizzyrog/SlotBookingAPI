using FluentValidation;
using SlotBooking.Domain.DTOs;

namespace SlotBooking.Domain.Validators;

public class BookAvailableSlotDtoValidator : AbstractValidator<BookAvailableSlotDto>
{
    public BookAvailableSlotDtoValidator()
    {
        RuleFor(slot => slot.FacilityId)
            .NotEmpty()
            .WithMessage("FacilityId is required.");
        
        RuleFor(slot => slot.Comments)
            .NotEmpty()
            .WithMessage("Comments is required.");
        
        RuleFor(slot => slot.Start)
            .NotEmpty()
            .WithMessage("Start is required.");

        RuleFor(slot => slot.End)
            .NotEmpty()
            .WithMessage("End is required.");
        
        RuleFor(slot => slot.Patient)
            .NotEmpty()
            .WithMessage("Patient is required.");
    }
}