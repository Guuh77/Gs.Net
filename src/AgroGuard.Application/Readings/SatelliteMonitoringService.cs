using AgroGuard.Application.Abstractions;
using AgroGuard.Application.Alerts;
using AgroGuard.Application.Common.Exceptions;
using AgroGuard.Application.Common.Security;
using AgroGuard.Domain.Entities;
using AgroGuard.Domain.Services;

namespace AgroGuard.Application.Readings;

public sealed class SatelliteMonitoringService : ISatelliteMonitoringService
{
    private readonly IFieldRepository _fields;
    private readonly ISatelliteReadingRepository _readings;
    private readonly IAlertRepository _alerts;
    private readonly ICurrentUser _currentUser;
    private readonly RiskAssessmentService _riskAssessmentService;
    private readonly IUnitOfWork _unitOfWork;

    public SatelliteMonitoringService(
        IFieldRepository fields,
        ISatelliteReadingRepository readings,
        IAlertRepository alerts,
        ICurrentUser currentUser,
        RiskAssessmentService riskAssessmentService,
        IUnitOfWork unitOfWork)
    {
        _fields = fields;
        _readings = readings;
        _alerts = alerts;
        _currentUser = currentUser;
        _riskAssessmentService = riskAssessmentService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReadingAnalysisResponse> AddReadingAsync(
        Guid fieldId,
        CreateSatelliteReadingRequest request,
        CancellationToken cancellationToken)
    {
        var field = await _fields.GetOwnedByIdAsync(fieldId, RequireUserId(), cancellationToken);
        if (field is null)
        {
            throw new NotFoundException("Field", fieldId);
        }

        var previousReading = await _readings.GetLatestByFieldAsync(fieldId, cancellationToken);
        var reading = new SatelliteReading(
            fieldId,
            request.CapturedAt ?? DateTime.UtcNow,
            request.Source,
            request.Ndvi,
            request.SoilMoisturePercent,
            request.SurfaceTemperatureCelsius,
            request.RainfallMillimeters,
            request.CloudCoveragePercent);

        var generatedAlerts = _riskAssessmentService
            .Assess(field, reading, previousReading)
            .Select(assessment => assessment.ToAlert(field.Id, reading.Id))
            .ToArray();

        await _readings.AddAsync(reading, cancellationToken);
        if (generatedAlerts.Length > 0)
        {
            await _alerts.AddRangeAsync(generatedAlerts, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReadingAnalysisResponse(
            ToResponse(reading),
            generatedAlerts.Select(alert => AlertService.ToResponse(alert, field.Name)).ToArray());
    }

    private Guid RequireUserId()
    {
        return _currentUser.UserId ?? throw new ForbiddenException("Authenticated user was not found.");
    }

    private static SatelliteReadingResponse ToResponse(SatelliteReading reading)
    {
        return new SatelliteReadingResponse(
            reading.Id,
            reading.FieldId,
            reading.CapturedAt,
            reading.Source,
            reading.Ndvi,
            reading.SoilMoisturePercent,
            reading.SurfaceTemperatureCelsius,
            reading.RainfallMillimeters,
            reading.CloudCoveragePercent);
    }
}
