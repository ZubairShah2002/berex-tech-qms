namespace BerexQms.Domain.SupplierQuality.Enums;

public enum ScarStatus
{
    Issued = 0,
    AwaitingResponse = 1,
    Overdue = 2,
    UnderReview = 3,
    Accepted = 4,
    Rejected = 5,
    FollowUp = 6,
    Closed = 7,
}
