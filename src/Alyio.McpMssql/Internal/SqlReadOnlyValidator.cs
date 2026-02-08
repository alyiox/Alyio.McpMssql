// MIT License

using System.Text;

namespace Alyio.McpMssql.Internal;

/// <summary>
/// Validates that a SQL statement is read-only and safe to execute
/// in the MCP query engine.
/// </summary>
/// <remarks>
/// This validator is intentionally conservative. It is not a full
/// SQL parser, but it reliably blocks all common write paths while
/// avoiding false positives caused by string literals or identifiers.
/// </remarks>
internal static class SqlReadOnlyValidator
{
    /// <summary>
    /// Validates that the provided SQL text represents a single,
    /// read-only SELECT query.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when the SQL is null or empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the SQL is not a read-only SELECT query.
    /// </exception>
    public static void Validate(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentException("SQL query cannot be empty.", nameof(sql));
        }

        var normalized = StripComments(sql).Trim();

        // Allow a single trailing semicolon
        if (normalized.EndsWith(';'))
        {
            normalized = normalized[..^1].TrimEnd();
        }

        // Disallow multiple statements
        if (normalized.Contains(';'))
        {
            throw new InvalidOperationException(
                "Multiple SQL statements are not allowed.");
        }

        var executable = StripNonExecutableContent(normalized);

        if (!StartsWithSelectOrCte(executable))
        {
            throw new InvalidOperationException(
                "Only read-only SELECT queries are allowed.");
        }

        if (ContainsForbiddenKeywords(executable))
        {
            throw new InvalidOperationException(
                "The query contains forbidden SQL operations.");
        }
    }

    private static bool StartsWithSelectOrCte(string sql)
    {
        var trimmed = sql.TrimStart();

        if (trimmed.StartsWith("select", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // CTEs are allowed only if they ultimately execute a SELECT
        if (trimmed.StartsWith("with", StringComparison.OrdinalIgnoreCase)
            && trimmed.Contains("select", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static bool ContainsForbiddenKeywords(string sql)
    {
        var text = sql.ToLowerInvariant();

        var forbidden = new[]
        {
            " insert ",
            " update ",
            " delete ",
            " merge ",
            " exec ",
            " execute ",
            " create ",
            " alter ",
            " drop ",
            " truncate ",
            " grant ",
            " revoke ",
            " into "
        };

        return forbidden.Any(text.Contains);
    }

    /// <summary>
    /// Removes string literals and delimited identifiers so that
    /// keyword detection only applies to executable SQL tokens.
    /// </summary>
    private static string StripNonExecutableContent(string sql)
    {
        var sb = new StringBuilder(sql.Length);

        bool inString = false;
        bool inBracket = false;
        bool inQuotedIdentifier = false;

        for (int i = 0; i < sql.Length; i++)
        {
            char c = sql[i];

            if (!inBracket && !inQuotedIdentifier && c == '\'')
            {
                inString = !inString;
                continue;
            }

            if (!inString && !inQuotedIdentifier)
            {
                if (c == '[')
                {
                    inBracket = true;
                    continue;
                }

                if (c == ']')
                {
                    inBracket = false;
                    continue;
                }
            }

            if (!inString && !inBracket && c == '"')
            {
                inQuotedIdentifier = !inQuotedIdentifier;
                continue;
            }

            if (inString || inBracket || inQuotedIdentifier)
            {
                continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Removes SQL line and block comments.
    /// </summary>
    private static string StripComments(string sql)
    {
        var sb = new StringBuilder(sql.Length);

        for (int i = 0; i < sql.Length; i++)
        {
            if (i + 1 < sql.Length && sql[i] == '-' && sql[i + 1] == '-')
            {
                while (i < sql.Length && sql[i] != '\n')
                {
                    i++;
                }
                continue;
            }

            if (i + 1 < sql.Length && sql[i] == '/' && sql[i + 1] == '*')
            {
                i += 2;
                while (i + 1 < sql.Length &&
                       !(sql[i] == '*' && sql[i + 1] == '/'))
                {
                    i++;
                }
                i++;
                continue;
            }

            sb.Append(sql[i]);
        }

        return sb.ToString();
    }
}
