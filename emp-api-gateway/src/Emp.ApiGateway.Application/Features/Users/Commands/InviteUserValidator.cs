using FluentValidation;

namespace Emp.ApiGateway.Application.Features.Users.Commands;

public class InviteUserValidator : AbstractValidator<InviteUserCommand>
{
    private static readonly string[] ValidRoles = { "SystemAdministrator", "VendorContact", "ClientContact" };

    public InviteUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => ValidRoles.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", ValidRoles)}.");

        RuleFor(x => x.InvitedBy)
            .NotEmpty().WithMessage("InvitedBy user ID is required.");
    }
}
