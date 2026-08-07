namespace BerexQms.Domain.AiEngine.Enums;

public enum ModelStatus
{
    Training = 0,
    Validating = 1,
    Shadow = 2,
    Active = 3,
    Deprecated = 4,
    Retired = 5
}
