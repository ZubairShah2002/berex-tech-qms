using BerexQms.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace BerexQms.Infrastructure.FileStorage;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";
    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool UseSSL { get; set; }
}

public sealed class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _client;
    private readonly ILogger<MinioFileStorageService> _logger;

    public MinioFileStorageService(IMinioClient client, ILogger<MinioFileStorageService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<string> UploadAsync(
        string bucket,
        string fileName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(bucket, cancellationToken);

        var args = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName)
            .WithStreamData(content)
            .WithObjectSize(content.Length)
            .WithContentType(contentType);

        await _client.PutObjectAsync(args, cancellationToken);
        _logger.LogInformation("Uploaded {FileName} to bucket {Bucket}", fileName, bucket);

        return $"{bucket}/{fileName}";
    }

    public async Task<Stream> DownloadAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();

        var args = new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream));

        await _client.GetObjectAsync(args, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task DeleteAsync(
        string bucket,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName);

        await _client.RemoveObjectAsync(args, cancellationToken);
        _logger.LogInformation("Deleted {FileName} from bucket {Bucket}", fileName, bucket);
    }

    public async Task<string> GetPresignedUrlAsync(
        string bucket,
        string fileName,
        int expiryMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(bucket)
            .WithObject(fileName)
            .WithExpiry(expiryMinutes * 60);

        return await _client.PresignedGetObjectAsync(args);
    }

    private async Task EnsureBucketExistsAsync(string bucket, CancellationToken cancellationToken)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(bucket);
        var exists = await _client.BucketExistsAsync(existsArgs, cancellationToken);

        if (!exists)
        {
            var makeArgs = new MakeBucketArgs().WithBucket(bucket);
            await _client.MakeBucketAsync(makeArgs, cancellationToken);
            _logger.LogInformation("Created bucket {Bucket}", bucket);
        }
    }
}
