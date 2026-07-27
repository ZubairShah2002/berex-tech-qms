using BerexQms.Application.Identity.Interfaces;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Identity.Repositories;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.Domain.NonConformance.Repositories;
using BerexQms.Domain.ProductCatalog.Repositories;
using BerexQms.Infrastructure.Caching;
using BerexQms.Infrastructure.FileStorage;
using BerexQms.Infrastructure.Identity.Repositories;
using BerexQms.Infrastructure.Identity.Services;
using BerexQms.Infrastructure.Inspection.Repositories;
using BerexQms.Infrastructure.NonConformance.Repositories;
using BerexQms.Infrastructure.Persistence;
using BerexQms.Infrastructure.Persistence.Interceptors;
using BerexQms.Infrastructure.ProductCatalog.Repositories;
using BerexQms.Infrastructure.Services;
using BerexQms.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddInfrastructureServices();

        return services;
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditTrailInterceptor>();

        services.AddDbContext<QmsDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<AuditTrailInterceptor>();

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

            options.AddInterceptors(interceptor);
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<QmsDbContext>());
    }

    private static void AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");

        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConnection));

            services.AddScoped<ICacheService, RedisCacheService>();
        }

        services.AddHealthChecks()
            .AddNpgSql(
                configuration.GetConnectionString("DefaultConnection")!,
                name: "postgresql",
                tags: new[] { "db", "ready" })
            .AddRedis(
                redisConnection ?? "localhost:6379",
                name: "redis",
                tags: new[] { "cache", "ready" });
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
    }
}
