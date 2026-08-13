using BerexQms.Application.Identity.Interfaces;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Identity.Repositories;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.Domain.AuditManagement.Repositories;
using BerexQms.Domain.Capa.Repositories;
using BerexQms.Domain.DocumentControl.Repositories;
using BerexQms.Domain.NonConformance.Repositories;
using BerexQms.Domain.ProductCatalog.Repositories;
using BerexQms.Infrastructure.Caching;
using BerexQms.Infrastructure.FileStorage;
using BerexQms.Infrastructure.Identity.Repositories;
using BerexQms.Infrastructure.Identity.Services;
using BerexQms.Infrastructure.Inspection.Repositories;
using BerexQms.Infrastructure.AuditManagement.Repositories;
using BerexQms.Infrastructure.Capa.Repositories;
using BerexQms.Infrastructure.DocumentControl.Repositories;
using BerexQms.Infrastructure.NonConformance.Repositories;
using BerexQms.Infrastructure.Persistence;
using BerexQms.Infrastructure.Persistence.Interceptors;
using BerexQms.Infrastructure.ProductCatalog.Repositories;
using BerexQms.Domain.SupplierQuality.Repositories;
using BerexQms.Infrastructure.SupplierQuality.Repositories;
using BerexQms.Domain.Calibration.Repositories;
using BerexQms.Infrastructure.Calibration.Repositories;
using BerexQms.Domain.Training.Repositories;
using BerexQms.Infrastructure.Training.Repositories;
using BerexQms.Domain.Spc.Repositories;
using BerexQms.Infrastructure.Spc.Repositories;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.AiEngine.Configuration;
using BerexQms.Infrastructure.AiEngine.Providers;
using BerexQms.Infrastructure.AiEngine.Providers.Local;
using BerexQms.Infrastructure.AiEngine.Repositories;
using BerexQms.Infrastructure.AiEngine.Services;
using BerexQms.Infrastructure.Services;
using BerexQms.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using StackExchange.Redis;

namespace BerexQms.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddRedisCache(configuration);
        services.AddMinioStorage(configuration);
        services.AddAiProviders(configuration);
        services.AddInfrastructureServices();

        return services;
    }

    private static void AddAiProviders(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AiProviderOptions>(configuration.GetSection(AiProviderOptions.SectionName));

        // Register Claude provider with its own HttpClient
        services.AddHttpClient<ClaudeAiProvider>();
        services.AddScoped<IAiProvider, ClaudeAiProvider>(sp =>
        {
            var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
            var client = httpFactory.CreateClient(nameof(ClaudeAiProvider));
            var options = sp.GetRequiredService<IOptions<AiProviderOptions>>();
            var logger = sp.GetRequiredService<ILogger<ClaudeAiProvider>>();
            return new ClaudeAiProvider(client, options, logger);
        });

        // Register OpenAI provider with its own HttpClient
        services.AddHttpClient<OpenAiProvider>();
        services.AddScoped<IAiProvider, OpenAiProvider>(sp =>
        {
            var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
            var client = httpFactory.CreateClient(nameof(OpenAiProvider));
            var options = sp.GetRequiredService<IOptions<AiProviderOptions>>();
            var logger = sp.GetRequiredService<ILogger<OpenAiProvider>>();
            return new OpenAiProvider(client, options, logger);
        });

        // Register Local AI (Ollama) provider with its own HttpClient
        services.AddHttpClient<OllamaClient>();
        services.AddScoped<OllamaClient>(sp =>
        {
            var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
            var client = httpFactory.CreateClient(nameof(OllamaClient));
            var options = sp.GetRequiredService<IOptions<AiProviderOptions>>();
            var logger = sp.GetRequiredService<ILogger<OllamaClient>>();
            return new OllamaClient(client, options, logger);
        });
        services.AddScoped<IAiProvider, LocalAiProvider>();

        // Register orchestrator
        services.AddScoped<IAiOrchestrator, AiOrchestratorService>();
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditTrailInterceptor>();
        services.AddScoped<TenantConnectionInterceptor>();

        services.AddDbContext<QmsDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditTrailInterceptor>();
            var tenantInterceptor = sp.GetRequiredService<TenantConnectionInterceptor>();

            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(QmsDbContext).Assembly.FullName);
                    npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "shared");
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                });

            options.AddInterceptors(tenantInterceptor, auditInterceptor);
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<QmsDbContext>());
        services.AddScoped<IExecutionStrategyFactory, EfExecutionStrategyFactory>();
    }

    private static void AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");

        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var options = ConfigurationOptions.Parse(redisConnection);
                options.AbortOnConnectFail = false; // Don't crash if Redis is temporarily unavailable
                return ConnectionMultiplexer.Connect(options);
            });

            services.AddScoped<ICacheService, RedisCacheService>();
        }

        var healthChecks = services.AddHealthChecks()
            .AddNpgSql(
                configuration.GetConnectionString("DefaultConnection")!,
                name: "postgresql",
                tags: new[] { "db", "ready" });

        // Only add Redis health check if Redis is actually configured
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            healthChecks.AddRedis(
                redisConnection,
                name: "redis",
                tags: new[] { "cache", "ready" });
        }
    }

    private static void AddMinioStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));

        services.AddSingleton<IMinioClient>(sp =>
        {
            var options = configuration.GetSection(FileStorageOptions.SectionName).Get<FileStorageOptions>()
                          ?? new FileStorageOptions();

            var client = new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey);

            if (options.UseSSL)
                client = client.WithSSL();

            return client.Build();
        });

        services.AddScoped<IFileStorageService, MinioFileStorageService>();
    }

    private static void AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IClockService, ClockService>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();

        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddScoped<IPartRepository, PartRepository>();

        services.AddScoped<IInspectionRepository, InspectionRepository>();
        services.AddScoped<ISamplingPlanRepository, SamplingPlanRepository>();

        services.AddScoped<INonConformanceRepository, NonConformanceRepository>();

        services.AddScoped<ICAPARepository, CAPARepository>();

        services.AddScoped<IDocumentRepository, DocumentRepository>();

        services.AddScoped<IAuditRepository, AuditRepository>();

        services.AddScoped<ISupplierRepository, SupplierRepository>();

        services.AddScoped<IEquipmentRepository, EquipmentRepository>();

        services.AddScoped<IQualificationRepository, QualificationRepository>();
        services.AddScoped<ITrainingCourseRepository, TrainingCourseRepository>();
        services.AddScoped<ITrainingAssignmentRepository, TrainingAssignmentRepository>();
        services.AddScoped<ICompetencyRecordRepository, CompetencyRecordRepository>();

        services.AddScoped<IControlChartRepository, ControlChartRepository>();

        services.AddScoped<IAiInteractionRepository, AiInteractionRepository>();
        services.AddScoped<IAiModelRepository, AiModelRepository>();
        services.AddScoped<IAiCapabilityConfigRepository, AiCapabilityConfigRepository>();

        services.AddScoped<IAiActionLogRepository, AiActionLogRepository>();
        services.AddScoped<IAiPermissionPolicyRepository, AiPermissionPolicyRepository>();
        services.AddScoped<IAiWorkflowDefinitionRepository, AiWorkflowDefinitionRepository>();
        services.AddScoped<IAiWorkflowExecutionRepository, AiWorkflowExecutionRepository>();
        services.AddScoped<IAiPermissionService, AiPermissionService>();

        services.AddScoped<IAiContextDocumentRepository, AiContextDocumentRepository>();
        services.AddScoped<IAiKnowledgeSourceRepository, AiKnowledgeSourceRepository>();
        services.AddScoped<IAiContextService, AiContextService>();
        services.AddScoped<IEmbeddingService, EmbeddingService>();

        services.AddScoped<IAiRecommendationRepository, AiRecommendationRepository>();
        services.AddScoped<IAiRecommendationService, AiRecommendationService>();

        // Sprint 16: AI Provider Integration
        services.AddScoped<IAiUsageRecordRepository, AiUsageRecordRepository>();
        services.AddScoped<IAiPromptTemplateRepository, AiPromptTemplateRepository>();
        services.AddScoped<IAiUsageService, AiUsageTrackingService>();
        services.AddScoped<AiPromptTemplateManager>();

        // Sprint 18: AI Governance & User Preferences
        services.AddScoped<IAiUserPreferenceRepository, AiUserPreferenceRepository>();
        services.AddScoped<IAiGovernancePolicyRepository, AiGovernancePolicyRepository>();
        services.AddScoped<IAiGovernanceService, AiGovernanceService>();
    }
}
