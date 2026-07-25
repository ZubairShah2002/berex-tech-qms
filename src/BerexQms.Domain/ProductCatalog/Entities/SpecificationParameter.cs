using BerexQms.Domain.ProductCatalog.Enums;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.ProductCatalog.Entities;

public sealed class SpecificationParameter : Entity<Guid>
{
    public Guid PartRevisionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ParameterType Type { get; private set; }
    public string? Unit { get; private set; }
    public decimal? NominalValue { get; private set; }
    public decimal? UpperTolerance { get; private set; }
    public decimal? LowerTolerance { get; private set; }
    public string? TextValue { get; private set; }
    public bool IsCritical { get; private set; }
    public int SortOrder { get; private set; }

    private SpecificationParameter() { }

    internal static SpecificationParameter Create(
        Guid id,
        TenantId tenantId,
        Guid partRevisionId,
        string name,
        ParameterType type,
        string? unit,
        decimal? nominalValue,
        decimal? upperTolerance,
        decimal? lowerTolerance,
        string? textValue,
        bool isCritical,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Specification parameter name is required.");

        if (type != ParameterType.Visual && type != ParameterType.Other)
        {
            if (nominalValue.HasValue && upperTolerance.HasValue && lowerTolerance.HasValue)
            {
                if (lowerTolerance > upperTolerance)
                    throw new DomainException("Lower tolerance cannot exceed upper tolerance.");
            }
        }

        return new SpecificationParameter
        {
            Id = id,
            TenantId = tenantId,
            PartRevisionId = partRevisionId,
            Name = name.Trim(),
            Type = type,
            Unit = unit?.Trim(),
            NominalValue = nominalValue,
            UpperTolerance = upperTolerance,
            LowerTolerance = lowerTolerance,
            TextValue = textValue?.Trim(),
            IsCritical = isCritical,
            SortOrder = sortOrder
        };
    }

    internal void Update(
        string name,
        ParameterType type,
        string? unit,
        decimal? nominalValue,
        decimal? upperTolerance,
        decimal? lowerTolerance,
        string? textValue,
        bool isCritical,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Specification parameter name is required.");

        if (type != ParameterType.Visual && type != ParameterType.Other)
        {
            if (nominalValue.HasValue && upperTolerance.HasValue && lowerTolerance.HasValue)
            {
                if (lowerTolerance > upperTolerance)
                    throw new DomainException("Lower tolerance cannot exceed upper tolerance.");
            }
        }

        Name = name.Trim();
        Type = type;
        Unit = unit?.Trim();
        NominalValue = nominalValue;
        UpperTolerance = upperTolerance;
        LowerTolerance = lowerTolerance;
        TextValue = textValue?.Trim();
        IsCritical = isCritical;
        SortOrder = sortOrder;
    }
}
