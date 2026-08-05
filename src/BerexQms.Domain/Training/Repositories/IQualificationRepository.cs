using BerexQms.Domain.Training.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Training.Repositories;

public interface IQualificationRepository : IRepository<Qualification>
{
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
}
