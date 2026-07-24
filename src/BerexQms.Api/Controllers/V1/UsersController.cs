using Asp.Versioning;
using BerexQms.Application.Identity.Commands.ActivateUser;
using BerexQms.Application.Identity.Commands.AssignRole;
using BerexQms.Application.Identity.Commands.ChangePassword;
using BerexQms.Application.Identity.Commands.DeactivateUser;
using BerexQms.Application.Identity.Commands.RemoveRole;
using BerexQms.Application.Identity.Commands.UpdateUser;
using BerexQms.Application.Identity.Queries.GetUserById;
using BerexQms.Application.Identity.Queries.ListUsers;
using BerexQms.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BerexQms.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListUsersQuery(search, status, page, pageSize),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserByIdQuery(id), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateUserCommand(id, request.FirstName, request.LastName, request.PhoneNumber, request.Department, request.JobTitle),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateUserCommand(id), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ActivateUserCommand(id), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> AssignRole(Guid id, Guid roleId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AssignRoleCommand(id, roleId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpDelete("{id:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> RemoveRole(Guid id, Guid roleId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RemoveRoleCommand(id, roleId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/change-password")]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ChangePasswordCommand(id, request.CurrentPassword, request.NewPassword),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }
}

public sealed record UpdateUserRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? Department,
    string? JobTitle);

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);
