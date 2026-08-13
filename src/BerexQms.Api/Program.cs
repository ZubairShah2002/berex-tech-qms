using Asp.Versioning;
using BerexQms.Api.Middleware;
using BerexQms.Api.Services;
using BerexQms.Application;
using BerexQms.Application.Interfaces;
using BerexQms.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // PaaS platforms set PORT env var — configure Kestrel to listen on it
    var port = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrWhiteSpace(port))
    {
        builder.WebHost.UseUrls($"http://+:{port}");
    }

    // PaaS platforms (Render, Railway) provide DATABASE_URL — convert to Npgsql format
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2); // Split on first ':' only — password may contain ':'
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var database = uri.AbsolutePath.TrimStart('/');
        var npgsqlConn = $"Host={uri.Host};Port={uri.Port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        builder.Configuration["ConnectionStrings:DefaultConnection"] = npgsqlConn;
    }

    // PaaS platforms provide REDIS_URL as redis://host:port or rediss://user:pass@host:port
    // StackExchange.Redis expects host:port,password=xxx format
    var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL");
    if (!string.IsNullOrWhiteSpace(redisUrl))
    {
        try
        {
            var redisUri = new Uri(redisUrl);
            var redisHost = redisUri.Host;
            var redisPort = redisUri.Port > 0 ? redisUri.Port : 6379;
            var redisConn = $"{redisHost}:{redisPort}";

            // Extract password if present (redis://default:PASSWORD@host:port)
            if (!string.IsNullOrWhiteSpace(redisUri.UserInfo))
            {
                var redisParts = redisUri.UserInfo.Split(':', 2);
                if (redisParts.Length > 1 && !string.IsNullOrWhiteSpace(redisParts[1]))
                    redisConn += $",password={Uri.UnescapeDataString(redisParts[1])}";
            }

            // Render's free Redis uses TLS (rediss://)
            if (redisUri.Scheme.Equals("rediss", StringComparison.OrdinalIgnoreCase))
                redisConn += ",ssl=true,sslProtocols=tls12|tls13,abortConnect=false";
            else
                redisConn += ",abortConnect=false";

            builder.Configuration["ConnectionStrings:Redis"] = redisConn;
        }
        catch (UriFormatException)
        {
            // If it's already in host:port format, use as-is
            builder.Configuration["ConnectionStrings:Redis"] = redisUrl;
        }
    }

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("Application", "BerexQms"));

    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var jwtSection = builder.Configuration.GetSection("Jwt");
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidAudience = jwtSection["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddControllers();

    builder.Services
        .AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"));
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Berex Tech QMS API",
            Version = "v1",
            Description = "Quality Management System for Discrete Manufacturing"
        });

        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer {token}'",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Default", policy =>
        {
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                          ?? new[] { "http://localhost:5173" };
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(
            context =>
            {
                var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                    remoteIp,
                    _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 100,
                        QueueLimit = 10,
                        QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst
                    });
            });
    });

    builder.Services.AddHealthChecks();

    var app = builder.Build();

    // Self-initialize database on PaaS platforms (Render, Railway, etc.)
    await BerexQms.Api.DatabaseInitializer.InitializeAsync(app);

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        };
    });

    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Berex Tech QMS API v1");
            options.RoutePrefix = "swagger";
        });
    }

    app.UseCors("Default");

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<TenantMiddleware>();

    app.UseRateLimiter();

    // Serve React SPA from wwwroot (for combined PaaS deployments)
    if (Directory.Exists(Path.Combine(app.Environment.ContentRootPath, "wwwroot")))
    {
        app.UseDefaultFiles();
        app.UseStaticFiles();
    }

    app.MapControllers();

    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });

    // SPA fallback — serve index.html for client-side routes
    if (Directory.Exists(Path.Combine(app.Environment.ContentRootPath, "wwwroot")))
    {
        app.MapFallbackToFile("index.html");
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
