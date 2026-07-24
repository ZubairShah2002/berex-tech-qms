namespace BerexQms.Application.Interfaces;

/// <summary>
/// Abstraction for file storage operations. Supports upload, download, deletion,
/// and pre-signed URL generation for bucket-based object storage (e.g., S3, Azure Blob, MinIO).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to the specified bucket.
    /// </summary>
    /// <param name="bucket">The storage bucket name.</param>
    /// <param name="fileName">The name of the file to store.</param>
    /// <param name="content">The file content stream.</param>
    /// <param name="contentType">The MIME content type of the file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The storage key or path of the uploaded file.</returns>
    Task<string> UploadAsync(string bucket, string fileName, Stream content, string contentType, CancellationToken ct);

    /// <summary>
    /// Downloads a file from the specified bucket.
    /// </summary>
    /// <param name="bucket">The storage bucket name.</param>
    /// <param name="fileName">The name of the file to download.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A stream containing the file content.</returns>
    Task<Stream> DownloadAsync(string bucket, string fileName, CancellationToken ct);

    /// <summary>
    /// Deletes a file from the specified bucket.
    /// </summary>
    /// <param name="bucket">The storage bucket name.</param>
    /// <param name="fileName">The name of the file to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteAsync(string bucket, string fileName, CancellationToken ct);

    /// <summary>
    /// Generates a pre-signed URL for time-limited access to a file.
    /// </summary>
    /// <param name="bucket">The storage bucket name.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="expiryMinutes">The number of minutes until the URL expires.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A pre-signed URL string.</returns>
    Task<string> GetPresignedUrlAsync(string bucket, string fileName, int expiryMinutes, CancellationToken ct);
}
