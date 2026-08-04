using Asp.Versioning;
using BerexQms.Application.Calibration.Commands.AttachCertificate;
using BerexQms.Application.Calibration.Commands.RecordCalibration;
using BerexQms.Application.Calibration.Commands.RecordGaugeStudy;
using BerexQms.Application.Calibration.Commands.RegisterEquipment;
using BerexQms.Application.Calibration.Commands.ReviewImpactAssessment;
using BerexQms.Application.Calibration.Commands.SetSchedule;
using BerexQms.Application.Calibration.Commands.UpdateEquipment;
using BerexQms.Application.Calibration.Queries.GetCalibrationSchedule;
using BerexQms.Application.Calibration.Queries.GetEquipmentById;
using BerexQms.Application.Calibration.Queries.GetImpactAssessment;
using BerexQms.Application.Calibration.Queries.GetOverdueEquipment;
using BerexQms.Application.Calibration.Queries.ListEquipment;
using BerexQms.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BerexQms.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/equipment")]
[Authorize]
public sealed class EquipmentController : ControllerBase
{
    private readonly ISender _sender;

    public EquipmentController(ISender sender)
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
            new ListEquipmentQuery(search, status, page, pageSize), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetEquipmentByIdQuery(id), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error.Message });
    }

    [HttpPost]
    public async Task<IActionResult> Register(
        [FromBody] RegisterEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RegisterEquipmentCommand(
            request.Code,
            request.Name,
            request.Type,
            request.Manufacturer,
            request.Model,
            request.SerialNumber,
            request.Location,
            request.Department,
            request.Area,
            request.CustodianId), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateEquipmentCommand(
            id,
            request.Name,
            request.Type,
            request.Manufacturer,
            request.Model,
            request.SerialNumber,
            request.Location,
            request.Department,
            request.Area,
            request.CustodianId), cancellationToken);

        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/calibrations")]
    public async Task<IActionResult> RecordCalibration(
        Guid id,
        [FromBody] RecordCalibrationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RecordCalibrationCommand(
            id,
            request.CalibrationDate,
            request.Result,
            request.TechnicianId,
            request.ProcedureRef,
            request.Notes,
            request.EnvironmentalConditions), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/calibrations/{calId:guid}/certificate")]
    public async Task<IActionResult> AttachCertificate(
        Guid id,
        Guid calId,
        [FromBody] AttachCertificateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AttachCertificateCommand(
            id,
            calId,
            request.IssuingLab,
            request.AccreditationRef,
            request.FileRef,
            request.ValidFrom,
            request.ValidUntil), cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/gauge-rr")]
    public async Task<IActionResult> RecordGaugeStudy(
        Guid id,
        [FromBody] RecordGaugeStudyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RecordGaugeStudyCommand(
            id,
            request.CharacteristicId,
            request.StudyDate,
            request.TotalGRRPct,
            request.RepeatabilityPct,
            request.ReproducibilityPct,
            request.PartVariationPct,
            request.Ndc), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}/schedule")]
    public async Task<IActionResult> SetSchedule(
        Guid id,
        [FromBody] SetScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SetScheduleCommand(
            id,
            request.IntervalDays,
            request.LeadTimeDays,
            request.LabType,
            request.ProcedureRef,
            request.NextDueDate), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetEquipmentByIdQuery(id), cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error.Message });

        return Ok(new
        {
            result.Value.Id,
            result.Value.Status,
            result.Value.Schedule?.NextDueDate,
        });
    }
}

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/calibration")]
[Authorize]
public sealed class CalibrationController : ControllerBase
{
    private readonly ISender _sender;

    public CalibrationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("schedule")]
    public async Task<IActionResult> GetScheduleDashboard(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCalibrationScheduleQuery(), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdue(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetOverdueEquipmentQuery(), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("impact-assessment/{id:guid}")]
    public async Task<IActionResult> GetImpactAssessment(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetImpactAssessmentQuery(id), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error.Message });
    }

    [HttpPut("impact-assessment/{id:guid}")]
    public async Task<IActionResult> ReviewImpactAssessment(
        Guid id,
        [FromBody] ReviewImpactAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReviewImpactAssessmentCommand(
            id, request.Action, request.Notes), cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error.Message });
    }
}

public sealed record RegisterEquipmentRequest(
    string Code, string Name, string? Type, string? Manufacturer,
    string? Model, string? SerialNumber, string? Location,
    string? Department, string? Area, Guid? CustodianId);

public sealed record UpdateEquipmentRequest(
    string Name, string? Type, string? Manufacturer,
    string? Model, string? SerialNumber, string? Location,
    string? Department, string? Area, Guid? CustodianId);

public sealed record RecordCalibrationRequest(
    DateTime CalibrationDate, string Result, Guid? TechnicianId,
    string? ProcedureRef, string? Notes, string? EnvironmentalConditions);

public sealed record AttachCertificateRequest(
    string IssuingLab, string? AccreditationRef, string? FileRef,
    DateTime ValidFrom, DateTime ValidUntil);

public sealed record RecordGaugeStudyRequest(
    Guid? CharacteristicId, DateTime StudyDate, decimal TotalGRRPct,
    decimal RepeatabilityPct, decimal ReproducibilityPct,
    decimal? PartVariationPct, int? Ndc);

public sealed record SetScheduleRequest(
    int IntervalDays, int LeadTimeDays, string LabType,
    string? ProcedureRef, DateTime NextDueDate);

public sealed record ReviewImpactAssessmentRequest(string Action, string? Notes);
