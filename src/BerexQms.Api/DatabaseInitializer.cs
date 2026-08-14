using System.Text;
using Npgsql;

namespace BerexQms.Api;

/// <summary>
/// Runs the init-db.sql script on application startup if the database
/// has not been initialized yet (checks for the shared.audit_log table).
/// This enables self-contained deployment on PaaS platforms like Render
/// where you can't mount SQL init scripts into PostgreSQL's init directory.
///
/// Statements are executed individually so that a failure in one (e.g.
/// CREATE EXTENSION on a managed host) does not abort the rest.
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

            // Split into individual statements and execute each separately,
            // so a single failure (e.g. CREATE EXTENSION on managed PaaS)
            // doesn't prevent the rest from running.
            var statements = SplitSqlStatements(sql);
            var succeeded = 0;
            var failed = 0;

            foreach (var statement in statements)
            {
                try
                {
                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText = statement;
                    cmd.CommandTimeout = 60;
                    await cmd.ExecuteNonQueryAsync();
                    succeeded++;
                }
                catch (PostgresException ex)
                {
                    failed++;
                    // Log the first 200 chars of the statement for diagnostics
                    var preview = statement.Length > 200
                        ? statement[..200] + "..."
                        : statement;
                    logger.LogWarning("SQL statement failed ({SqlState}): {Message} — {Preview}",
                        ex.SqlState, ex.MessageText, preview);
                }
            }

            logger.LogInformation(
                "Database initialization complete from {ScriptPath}: {Succeeded} succeeded, {Failed} failed",
                scriptPath, succeeded, failed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization failed — the application may not work correctly");
            // Don't throw — let the app start so health checks can report the issue
        }
    }

    /// <summary>
    /// Splits a SQL script into individual statements on semicolons,
    /// respecting dollar-quoted string blocks ($$...$$, $tag$...$tag$).
    /// </summary>
    private static List<string> SplitSqlStatements(string sql)
    {
        var statements = new List<string>();
        var current = new StringBuilder();
        var lines = sql.Split('\n');
        string? activeDollarTag = null;

        foreach (var line in lines)
        {
            var trimmed = line.TrimStart();

            // Skip blank lines and pure comment lines when building a new statement
            if (current.Length == 0 && (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("--")))
                continue;

            current.AppendLine(line);

            // Track dollar-quoting ($$ or $tag$)
            if (activeDollarTag is null)
            {
                // Look for opening dollar-quote
                var tagStart = FindDollarTag(trimmed);
                if (tagStart is not null)
                {
                    activeDollarTag = tagStart;
                    // Check if closing tag is also on this line (after the opener)
                    var afterOpen = trimmed[(trimmed.IndexOf(tagStart, StringComparison.Ordinal) + tagStart.Length)..];
                    if (afterOpen.Contains(activeDollarTag, StringComparison.Ordinal))
                    {
                        // Both open and close on same line — still outside dollar-quote
                        activeDollarTag = null;
                    }
                }
            }
            else
            {
                // Inside dollar-quoted block — look for closing tag
                if (trimmed.Contains(activeDollarTag, StringComparison.Ordinal))
                    activeDollarTag = null;
                continue; // Don't check for semicolons inside dollar-quoted blocks
            }

            // Outside dollar-quoted blocks: check if line ends with semicolon
            if (activeDollarTag is null && trimmed.TrimEnd().EndsWith(';'))
            {
                var stmt = current.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(stmt))
                    statements.Add(stmt);
                current.Clear();
            }
        }

        // Flush any remaining content
        var remaining = current.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(remaining))
            statements.Add(remaining);

        return statements;
    }

    /// <summary>
    /// Finds a dollar-quote tag ($$ or $identifier$) in the given text.
    /// Returns the full tag (e.g. "$$" or "$outer$") or null.
    /// Only matches tags at SQL statement level — the '$' must be preceded
    /// by whitespace, '(', '=', or be at the start of the line. This avoids
    /// false positives on values like BCrypt hashes ($2b$12$...) that appear
    /// inside single-quoted string literals.
    /// </summary>
    private static string? FindDollarTag(string text)
    {
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != '$') continue;

            // Dollar tag must be at a word boundary: start of line,
            // after whitespace, or after '(' / '='
            if (i > 0)
            {
                var prev = text[i - 1];
                if (prev != ' ' && prev != '\t' && prev != '(' && prev != '=')
                    continue;
            }

            // Find the matching closing $
            var end = text.IndexOf('$', i + 1);
            if (end < 0) continue;

            // The content between the two $ must be empty or a valid identifier
            var inner = text[(i + 1)..end];
            if (inner.Length == 0 || inner.All(c => char.IsLetterOrDigit(c) || c == '_'))
                return text[i..(end + 1)]; // e.g. "$$" or "$outer$"
        }

        return null;
    }
}
