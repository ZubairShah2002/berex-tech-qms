using BerexQms.SharedKernel.Exceptions;

namespace BerexQms.Domain.AiEngine.ValueObjects;

/// <summary>
/// References the source record in another bounded context that grounded an AI output,
/// supporting explainability and auditability of AI suggestions.
/// </summary>
public sealed record SourceReference
{
    public string ModuleName { get; }

    public Guid RecordId { get; }

    public string RecordType { get; }

    public SourceReference(string moduleName, Guid recordId, string recordType)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
            throw new DomainException("Module name is required for a source reference.");
        if (recordId == Guid.Empty)
            throw new DomainException("Record id is required for a source reference.");
        if (string.IsNullOrWhiteSpace(recordType))
            throw new DomainException("Record type is required for a source reference.");

        ModuleName = moduleName.Trim();
        RecordId = recordId;
        RecordType = recordType.Trim();
    }
}
