using Asp.Versioning;
using BerexQms.Application.Training.Commands.CompleteAssignment;
using BerexQms.Application.Training.Commands.CreateAssignment;
using BerexQms.Application.Training.Commands.CreateCourse;
using BerexQms.Application.Training.Commands.CreateQualification;
using BerexQms.Application.Training.Commands.ManageCompetency;
using BerexQms.Application.Training.Commands.UpdateCourse;
using BerexQms.Application.Training.Commands.UpdateQualification;
using BerexQms.Application.Training.Queries.GetAssignment;
using BerexQms.Application.Training.Queries.GetEmployeeCompetencies;
using BerexQms.Application.Training.Queries.GetExpiringQualifications;
using BerexQms.Application.Training.Queries.GetQualification;
using BerexQms.Application.Training.Queries.GetSkillMatrix;
using BerexQms.Application.Training.Queries.ListAssignments;
using BerexQms.Application.Training.Queries.ListCourses;
using BerexQms.Application.Training.Queries.ListQualifications;
using BerexQms.Application.Training.Queries.ValidateQualification;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BerexQms.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/qualifications")]
[Authorize]
public sealed class QualificationsController : ControllerBase
{
    private readonly ISender _sender;

    public QualificationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListQualificationsQuery(search, isActive, page, pageSize), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetQualificationQuery(id), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error.Message });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateQualificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateQualificationCommand(
            request.Code,
            request.Name,
            request.Description,
            request.ScopeProductFamily,
            request.ScopeInspectionType,
            request.ScopeProcessArea,
            request.ValidityMonths,
            request.RenewalWindowDays), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateQualificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateQualificationCommand(
            id,
            request.Name,
            request.Description,
            request.ScopeProductFamily,
            request.ScopeInspectionType,
            request.ScopeProcessArea,
            request.ValidityMonths,
            request.RenewalWindowDays), cancellationToken);

        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error.Message });
    }
}

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/training")]
[Authorize]
public sealed class TrainingController : ControllerBase
{
    private readonly ISender _sender;

    public TrainingController(ISender sender)
    {
        _sender = sender;
    }

    // ── Courses ──────────────────────────────────────────────────────

    [HttpGet("courses")]
    public async Task<IActionResult> ListCourses(
        [FromQuery] string? search,
        [FromQuery] Guid? qualificationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListCoursesQuery(search, qualificationId, page, pageSize), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("courses")]
    public async Task<IActionResult> CreateCourse(
        [FromBody] CreateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateCourseCommand(
            request.Code,
            request.Name,
            request.Description,
            request.DurationHours,
            request.AssessmentType,
            request.PassCriteria,
            request.QualificationId), cancellationToken);

        return result.IsSuccess
            ? Created($"/api/v1/training/courses/{result.Value}", new { id = result.Value })
            : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("courses/{id:guid}")]
    public async Task<IActionResult> UpdateCourse(
        Guid id,
        [FromBody] UpdateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateCourseCommand(
            id,
            request.Name,
            request.Description,
            request.DurationHours,
            request.AssessmentType,
            request.PassCriteria,
            request.QualificationId), cancellationToken);

        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error.Message });
    }

    // ── Assignments ──────────────────────────────────────────────────

    [HttpGet("assignments")]
    public async Task<IActionResult> ListAssignments(
        [FromQuery] Guid? employeeId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListAssignmentsQuery(employeeId, status, page, pageSize), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("assignments/{id:guid}")]
    public async Task<IActionResult> GetAssignment(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAssignmentQuery(id), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error.Message });
    }

    [HttpPost("assignments")]
    public async Task<IActionResult> CreateAssignment(
        [FromBody] CreateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateAssignmentCommand(
            request.EmployeeId,
            request.CourseId,
            request.DueDate), cancellationToken);

        return result.IsSuccess
            ? Created($"/api/v1/training/assignments/{result.Value}", new { id = result.Value })
            : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("assignments/{id:guid}/complete")]
    public async Task<IActionResult> CompleteAssignment(
        Guid id,
        [FromBody] CompleteAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CompleteAssignmentCommand(
            id,
            request.CompletionDate,
            request.Score,
            request.Result,
            request.AssessorId,
            request.EvidenceRef), cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error.Message });
    }

    // ── Skill Matrix & Expiring ──────────────────────────────────────

    [HttpGet("skill-matrix")]
    public async Task<IActionResult> GetSkillMatrix(
        [FromQuery] string? department,
        [FromQuery] string? productFamily,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetSkillMatrixQuery(department, productFamily), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiringQualifications(
        [FromQuery] int withinDays = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetExpiringQualificationsQuery(withinDays), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }
}

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/employees")]
[Authorize]
public sealed class EmployeeCompetenciesController : ControllerBase
{
    private readonly ISender _sender;

    public EmployeeCompetenciesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{employeeId:guid}/competencies")]
    public async Task<IActionResult> GetCompetencies(Guid employeeId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetEmployeeCompetenciesQuery(employeeId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{employeeId:guid}/competencies/validate")]
    public async Task<IActionResult> ValidateQualification(
        Guid employeeId,
        [FromQuery] Guid qualificationId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ValidateQualificationQuery(employeeId, qualificationId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error.Message });
    }

    [HttpPost("{employeeId:guid}/competencies")]
    public async Task<IActionResult> ManageCompetency(
        Guid employeeId,
        [FromBody] ManageCompetencyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ManageCompetencyCommand(
            employeeId,
            request.QualificationId,
            request.Action,
            request.QualifiedDate,
            request.AssessorId,
            request.EvidenceRef), cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error.Message });
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────

public sealed record CreateQualificationRequest(
    string Code, string Name, string? Description,
    string? ScopeProductFamily, string? ScopeInspectionType,
    string? ScopeProcessArea, int ValidityMonths, int RenewalWindowDays);

public sealed record UpdateQualificationRequest(
    string Name, string? Description,
    string? ScopeProductFamily, string? ScopeInspectionType,
    string? ScopeProcessArea, int ValidityMonths, int RenewalWindowDays);

public sealed record CreateCourseRequest(
    string Code, string Name, string? Description,
    decimal DurationHours, string? AssessmentType,
    string? PassCriteria, Guid? QualificationId);

public sealed record UpdateCourseRequest(
    string Name, string? Description,
    decimal DurationHours, string? AssessmentType,
    string? PassCriteria, Guid? QualificationId);

public sealed record CreateAssignmentRequest(
    Guid EmployeeId, Guid CourseId, DateTime DueDate);

public sealed record CompleteAssignmentRequest(
    DateTime CompletionDate, decimal? Score, string Result,
    Guid? AssessorId, string? EvidenceRef);

public sealed record ManageCompetencyRequest(
    Guid QualificationId, string Action,
    DateTime? QualifiedDate, Guid? AssessorId, string? EvidenceRef);
