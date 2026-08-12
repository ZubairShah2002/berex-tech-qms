using Npgsql;

namespace BerexQms.Api;

/// <summary>
/// Runs the init-db.sql script on application startup if the database
/// has not been initialized yet (checks for the shared.audit_log table).
/// This enables self-contained deployment on PaaS platforms like Render
/// where you can't mount SQL init scripts into PostgreSQL's init directory.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        var config = app.Services.GetRequiredService<IConfiguration>();

        var connectionString = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogWarning("No DefaultConnection configured — skipping database initialization");
            return;
        }

        try
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            // Check if the database has already been initialized
            await using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = @"
                SELECT EXISTS (
                    SELECT FROM information_schema.tables
                    WHERE table_schema = 'shared' AND table_name = 'audit_log'
                );";
            var exists = (bool)(await checkCmd.ExecuteScalarAsync() ?? false);

            if (exists)
            {
                logger.LogInformation("Database already initialized — skipping init-db.sql");
                return;
            }

            logger.LogInformation("Database not initialized — running init-db.sql...");

            // Look for init-db.sql in several locations
            var scriptPaths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "init-db.sql"),
                Path.Combine(AppContext.BaseDirectory, "..", "docker", "init-db.sql"),
                "/app/init-db.sql",
            };

            var scriptPath = scriptPaths.FirstOrDefault(File.Exists);
            if (scriptPath is null)
            {
                logger.LogWarning("init-db.sql not found in any expected location — skipping. Searched: {Paths}",
                    string.Join(", ", scriptPaths));
                return;
            }

            var sql = await File.ReadAllTextAsync(scriptPath);

            // Some statements reference the docker-compose user that may not exist on PaaS;
            // wrap the REVOKE in a try-catch so it doesn't block initialization.
            sql = sql.Replace(
                "REVOKE UPDATE, DELETE ON shared.audit_log FROM berexqms_app;",
                @"DO $$ BEGIN
                    EXECUTE 'REVOKE UPDATE, DELETE ON shared.audit_log FROM ' || current_user;
                EXCEPTION WHEN OTHERS THEN NULL;
                END; $$;"
            );

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandTimeout = 120;
            await cmd.ExecuteNonQueryAsync();

            logger.LogInformation("Database initialized successfully from {ScriptPath}", scriptPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization failed — the application may not work correctly");
            // Don't throw — let the app start so health checks can report the issue
        }
    }
}
