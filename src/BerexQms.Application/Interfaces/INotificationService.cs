namespace BerexQms.Application.Interfaces;

/// <summary>
/// Abstraction for sending notifications across multiple channels
/// (email, in-app, SMS). Implemented in the Infrastructure layer.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification to the specified recipients.
    /// </summary>
    /// <param name="notification">The notification payload containing recipients, subject, body, and channel.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendAsync(Notification notification, CancellationToken ct);
}

/// <summary>
/// Represents a notification to be sent through one or more channels.
/// </summary>
public sealed record Notification(
    IReadOnlyList<string> Recipients,
    string Subject,
    string Body,
    NotificationChannel Channel);

/// <summary>
/// The delivery channel for a notification.
/// </summary>
public enum NotificationChannel
{
    Email,
    InApp,
    Sms
}
