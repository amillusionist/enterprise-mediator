using Amazon.S3;
using Amazon.S3.Model;
using EnterpriseMediator.ProjectManagement.Application.Configuration;
using EnterpriseMediator.ProjectManagement.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnterpriseMediator.ProjectManagement.Infrastructure.Gateways;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly AwsOptions _options;
    private readonly ILogger<S3FileStorageService> _logger;

    public S3FileStorageService(IAmazonS3 s3Client, IOptions<AwsOptions> options, ILogger<S3FileStorageService> logger)
    {
        _s3Client = s3Client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        var storageKey = $"sow/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}/{fileName}";
        _logger.LogInformation("Uploading file {FileName} to S3 key {StorageKey}", fileName, storageKey);

        var request = new PutObjectRequest
        {
            BucketName = _options.SowBucketName,
            Key = storageKey,
            InputStream = fileStream,
            ContentType = contentType,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
        };

        await _s3Client.PutObjectAsync(request, ct);
        return storageKey;
    }

    public async Task<Stream> DownloadAsync(string storageKey, CancellationToken ct = default)
    {
        var response = await _s3Client.GetObjectAsync(_options.SowBucketName, storageKey, ct);
        return response.ResponseStream;
    }

    public async Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        await _s3Client.DeleteObjectAsync(_options.SowBucketName, storageKey, ct);
        _logger.LogInformation("File deleted from S3: {StorageKey}", storageKey);
    }
}
