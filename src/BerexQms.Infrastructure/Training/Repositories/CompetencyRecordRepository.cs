using BerexQms.Domain.Training.Entities;
using BerexQms.Domain.Training.Enums;
using BerexQms.Domain.Training.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.Training.Repositories;

public sealed class CompetencyRecordRepository : ICompetencyRecordRepository
{
    private readonly QmsDbContext _context;

    public CompetencyRecordRepository(QmsDbContext context)
    {
        _context = context;
    }

    public async Task<CompetencyRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<CompetencyRecord>()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<CompetencyRecord?> GetByEmployeeAndQualificationAsync(
        Guid employeeId, Guid qualificationId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<CompetencyRecord>()
            .FirstOrDefaultAsync(r => r.EmployeeId == employeeId && r.QualificationId == qualificationId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<CompetencyRecord>> GetByEmployeeAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<CompetencyRecord>()
            .Where(r => r.EmployeeId == employeeId)
            .OrderBy(r => r.QualificationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CompetencyRecord>> GetExpiringAsync(
        int withinDays, CancellationToken cancellationToken = default)
    {
        var qualifiedStatus = QualificationStatus.Qualified.ToString();
        var cutoff = DateTime.UtcNow.AddDays(withinDays);

        return await _context.Set<CompetencyRecord>()
            .Where(r => r.Status == qualifiedStatus &&
                         r.ExpiryDate != null &&
                         r.ExpiryDate <= cutoff)
            .OrderBy(r => r.ExpiryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CompetencyRecord>> GetAllForSkillMatrixAsync(
        string? department, string? productFamily, CancellationToken cancellationToken = default)
    {
        // Department/productFamily filtering would require joining with employee/qualification data.
        // For now, return all competency records; frontend applies additional filtering.
        return await _context.Set<CompetencyRecord>()
            .OrderBy(r => r.EmployeeId)
            .ThenBy(r => r.QualificationId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CompetencyRecord record, CancellationToken cancellationToken = default)
    {
        await _context.Set<CompetencyRecord>().AddAsync(record, cancellationToken);
    }

    public void Update(CompetencyRecord record)
    {
        _context.Set<CompetencyRecord>().Update(record);
    }
}
