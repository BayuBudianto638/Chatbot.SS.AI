using Chatbot.SS.AI.EncryptionLib.Models;
using FluentValidation;

public class EncryptionValidator : AbstractValidator<EncryptionRequest>
{
    public EncryptionValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password cannot be empty.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
    }
}


