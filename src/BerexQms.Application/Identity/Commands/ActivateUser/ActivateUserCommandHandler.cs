using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Identity.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Identity.Commands.ActivateUser;

public sealed class ActivateUserCommandHandler : ICommandHandler<ActivateUserCommand>
{
    private readonly IUserRepository _userRepository;

    public ActivateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.UserNotFound(request.UserId));

        user.Activate();
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
