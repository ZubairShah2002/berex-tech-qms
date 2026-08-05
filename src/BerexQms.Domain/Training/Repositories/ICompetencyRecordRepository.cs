using BerexQms.Domain.Training.Entities;

namespace BerexQms.Domain.Training.Repositories;

public interface ICompetencyRecordRepository
{
    Task<CompetencyRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CompetencyRecord?> GetByEmployeeAndQualificationAsync(Guid employeeId, Guid qualificationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetencyRecord>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetencyRecord>> GetExpiringAsync(int withinDays, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetencyRecord>> GetAllForSkillMatrixAsync(string? department, string? productFamily, CancellationToken cancellationToken = default);
    Task AddAsync(CompetencyRecord record, CancellationToken cancellationToken = default);
    void Update(CompetencyRecord record);
}
