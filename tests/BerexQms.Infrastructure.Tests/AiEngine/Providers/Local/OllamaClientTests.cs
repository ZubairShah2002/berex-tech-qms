using BerexQms.Infrastructure.AiEngine.Configuration;
using BerexQms.Infrastructure.AiEngine.Providers.Local;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BerexQms.Infrastructure.Tests.AiEngine.Providers.Local;

public class OllamaClientTests
{
    private readonly IOptions<AiProviderOptions> _options;
    private readonly ILogger<OllamaClient> _logger;

    public OllamaClientTests()
    {
        _options = Options.Create(new AiProviderOptions
        {
            Local = new LocalProviderOptions
            {
                Enabled = true,
                BaseUrl = "http://localhost:11434",
                DefaultModel = "qwen3",
                TimeoutSeconds = 30,
                HealthCheckTimeoutSeconds = 5,
            },
        });
        _logger = new LoggerFactory().CreateLogger<OllamaClient>();
    }

    [Fact]
    public void ResolveModel_NoTaskOverride_ReturnsDefault()
    {
        using var httpClient = new HttpClient();
        var client = new OllamaClient(httpClient, _options, _logger);

        var model = client.ResolveModel("DocumentAnalysis");

        Assert.Equal("qwen3", model);
    }

    [Fact]
    public void ResolveModel_WithTaskOverride_ReturnsOverride()
    {
        var opts = Options.Create(new AiProviderOptions
        {
            Local = new LocalProviderOptions
            {
                DefaultModel = "qwen3",
                TaskModels = new Dictionary<string, string>
                {
                    ["Summarization"] = "llama3.1:8b",
                },
            },
        });
        using var httpClient = new HttpClient();
        var client = new OllamaClient(httpClient, opts, _logger);

        var model = client.ResolveModel("Summarization");

        Assert.Equal("llama3.1:8b", model);
    }

    [Fact]
    public void ResolveModel_NullTaskType_ReturnsDefault()
    {
        using var httpClient = new HttpClient();
        var client = new OllamaClient(httpClient, _options, _logger);

        var model = client.ResolveModel(null);

        Assert.Equal("qwen3", model);
    }

    [Fact]
    public void ResolveModel_UnknownTaskType_ReturnsDefault()
    {
        using var httpClient = new HttpClient();
        var client = new OllamaClient(httpClient, _options, _logger);

        var model = client.ResolveModel("UnknownTask");

        Assert.Equal("qwen3", model);
    }

    [Fact]
    public async Task IsServerReachableAsync_UnreachableServer_ReturnsFalse()
    {
        var opts = Options.Create(new AiProviderOptions
        {
            Local = new LocalProviderOptions
            {
                BaseUrl = "http://127.0.0.1:19999", // Nothing listening here
                HealthCheckTimeoutSeconds = 1,
            },
        });
        using var httpClient = new HttpClient();
        var client = new OllamaClient(httpClient, opts, _logger);

        var reachable = await client.IsServerReachableAsync(CancellationToken.None);

        Assert.False(reachable);
    }

    [Fact]
    public async Task ListModelsAsync_UnreachableServer_ReturnsNull()
    {
        var opts = Options.Create(new AiProviderOptions
        {
            Local = new LocalProviderOptions
            {
                BaseUrl = "http://127.0.0.1:19999",
                HealthCheckTimeoutSeconds = 1,
            },
        });
        using var httpClient = new HttpClient();
        var client = new OllamaClient(httpClient, opts, _logger);

        var models = await client.ListModelsAsync(CancellationToken.None);

        Assert.Null(models);
    }

    [Fact]
    public async Task IsModelAvailableAsync_ServerUnavailable_ReturnsFalse()
    {
        var opts = Options.Create(new AiProviderOptions
        {
            Local = new LocalProviderOptions
            {
                BaseUrl = "http://127.0.0.1:19999",
                HealthCheckTimeoutSeconds = 1,
            },
        });
        using var httpClient = new HttpClient();
        var client = new OllamaClient(httpClient, opts, _logger);

        var available = await client.IsModelAvailableAsync("qwen3", CancellationToken.None);

        Assert.False(available);
    }
}
