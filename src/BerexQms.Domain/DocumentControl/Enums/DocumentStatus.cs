namespace BerexQms.Domain.DocumentControl.Enums;

public enum DocumentStatus
{
    Draft = 0,
    UnderReview = 1,
    PendingApproval = 2,
    Released = 3,
    Superseded = 4,
    Obsolete = 5,
}
