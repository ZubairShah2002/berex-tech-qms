using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Identity.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Identity.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler : ICommandHandler<DeactivateUserCommand>
{
    private readonly IUserRepository _userRepository;

    public DeactivateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.UserNotFound(request.UserId));

        user.Deactivate();
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
