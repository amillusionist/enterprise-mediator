using Emp.ApiGateway.Application.Behaviors;
using Emp.ApiGateway.Application.Features.Projects.Commands;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;

namespace Emp.ApiGateway.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        var behavior = new ValidationBehavior<CreateProjectCommand, Guid>(
            Enumerable.Empty<IValidator<CreateProjectCommand>>());

        var next = Substitute.For<RequestHandlerDelegate<Guid>>();
        var expected = Guid.NewGuid();
        next().Returns(expected);

        var result = await behavior.Handle(
            new CreateProjectCommand { Name = "Test", ClientId = Guid.NewGuid() },
            next,
            CancellationToken.None);

        result.Should().Be(expected);
        await next.Received(1)();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallNext()
    {
        var validator = Substitute.For<IValidator<CreateProjectCommand>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<CreateProjectCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var behavior = new ValidationBehavior<CreateProjectCommand, Guid>(new[] { validator });

        var next = Substitute.For<RequestHandlerDelegate<Guid>>();
        var expected = Guid.NewGuid();
        next().Returns(expected);

        var result = await behavior.Handle(
            new CreateProjectCommand { Name = "Test", ClientId = Guid.NewGuid() },
            next,
            CancellationToken.None);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ShouldThrowValidationException()
    {
        var validator = Substitute.For<IValidator<CreateProjectCommand>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<CreateProjectCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Name", "Name is required")
            }));

        var behavior = new ValidationBehavior<CreateProjectCommand, Guid>(new[] { validator });
        var next = Substitute.For<RequestHandlerDelegate<Guid>>();

        var act = () => behavior.Handle(
            new CreateProjectCommand(),
            next,
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Any(e => e.PropertyName == "Name"));

        await next.DidNotReceive()();
    }
}
