using System.Text;

namespace Alyio.McpMssql;

/// <summary>
/// Read-only SQL validation: only a single SELECT statement is allowed.
/// Blocks DML/DDL and other dangerous keywords.
/// </summary>
internal static class SqlValidation
{
    /// <summary>
    /// Keywords that are not allowed anywhere in the statement (case-insensitive).
    /// Covers DML, DDL, DCL, TCL, and execution.
    /// </summary>
    private static readonly HashSet<string> BlockedKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "INSERT", "UPDATE", "DELETE", "MERGE",
        "DROP", "CREATE", "ALTER", "TRUNCATE", "RENAME",
        "EXEC", "EXECUTE", "EXECUTEQUERY",
        "GRANT", "REVOKE", "DENY",
        "BEGIN", "COMMIT", "ROLLBACK", "SAVE", "TRANSACTION",
        "KILL", "SHUTDOWN", "RECONFIGURE",
        "BACKUP", "RESTORE", "BULK",
        "OPENROWSET", "OPENDATASOURCE", "OPENQUERY", "OPENXML",
        "ADMIN",
    };

    /// <summary>
    /// Must start with SELECT (after trimming and optional leading comments).
    /// </summary>
    private static readonly string[] AllowedFirstKeyword = ["SELECT"];

    /// <summary>
    /// Validates that the SQL is a single read-only SELECT statement.
    /// </summary>
    /// <exception cref="ArgumentException">When validation fails.</exception>
    public static void ValidateReadOnly(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentException("SQL must be provided.", nameof(sql));
        }

        // Normalize: remove single-line and multi-line comments for keyword scanning,
        // but keep a copy for "first token" check so we don't allow hidden statements.
        var trimmed = sql.Trim();
        var withoutComments = RemoveComments(trimmed);

        // 1) First significant token must be SELECT
        var firstToken = GetFirstSignificantToken(withoutComments);
        if (string.IsNullOrEmpty(firstToken) || !AllowedFirstKeyword.Contains(firstToken, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Only SELECT statements are allowed. The statement must begin with SELECT.", nameof(sql));
        }

        // 2) No multiple statements (allow trailing semicolon only)
        var semicolonIndex = trimmed.IndexOf(';');
        if (semicolonIndex >= 0 && semicolonIndex < trimmed.Length - 1)
        {
            var afterSemicolon = trimmed[(semicolonIndex + 1)..].Trim();
            if (!string.IsNullOrEmpty(RemoveComments(afterSemicolon)))
            {
                throw new ArgumentException("Multiple statements are not allowed.", nameof(sql));
            }
        }

        // 3) Block dangerous keywords anywhere in the normalized text
        var tokens = TokenizeSql(withoutComments);
        foreach (var token in tokens)
        {
            if (BlockedKeywords.Contains(token))
            {
                throw new ArgumentException($"Statement must be read-only. Keyword not allowed: '{token}'.", nameof(sql));
            }

            // Block stored procedure / extended procedure style prefixes
            if (token.StartsWith("SP_", StringComparison.OrdinalIgnoreCase) ||
                token.StartsWith("XP_", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Statement must be read-only. Call not allowed: '{token}'.", nameof(sql));
            }
        }
    }

    private static string RemoveComments(string sql)
    {
        var result = new StringBuilder(sql.Length);
        var i = 0;
        while (i < sql.Length)
        {
            if (i + 1 < sql.Length && sql[i] == '-' && sql[i + 1] == '-')
            {
                i += 2;
                while (i < sql.Length && sql[i] != '\n')
                {
                    result.Append(' ');
                    i++;
                }
                continue;
            }

            if (i + 1 < sql.Length && sql[i] == '/' && sql[i + 1] == '*')
            {
                i += 2;
                while (i + 1 < sql.Length && (sql[i] != '*' || sql[i + 1] != '/'))
                {
                    result.Append(' ');
                    i++;
                }
                if (i + 1 < sql.Length)
                {
                    i += 2;
                }
                result.Append(' ');
                continue;
            }

            result.Append(sql[i]);
            i++;
        }

        return result.ToString();
    }

    private static string GetFirstSignificantToken(string normalizedSql)
    {
        var tokens = TokenizeSql(normalizedSql);
        return tokens.Count > 0 ? tokens[0] : string.Empty;
    }

    private static List<string> TokenizeSql(string sql)
    {
        var tokens = new List<string>();
        var i = 0;
        while (i < sql.Length)
        {
            if (char.IsWhiteSpace(sql[i]))
            {
                i++;
                continue;
            }

            if (IsIdentifierChar(sql[i]))
            {
                var start = i;
                while (i < sql.Length && (IsIdentifierChar(sql[i]) || sql[i] == '.'))
                {
                    i++;
                }
                var word = sql[start..i];
                // Split on dot for "dbo.table" -> "dbo", "table"
                foreach (var part in word.Split('.'))
                {
                    if (!string.IsNullOrEmpty(part))
                    {
                        tokens.Add(part);
                    }
                }
                continue;
            }

            // Skip string literals so we don't flag keywords inside strings
            if (sql[i] == '\'')
            {
                i++;
                while (i < sql.Length && sql[i] != '\'')
                {
                    if (sql[i] == '\\' && i + 1 < sql.Length)
                    {
                        i += 2;
                        continue;
                    }
                    i++;
                }
                if (i < sql.Length)
                {
                    i++;
                }
                continue;
            }

            if (sql[i] == '[')
            {
                i++;
                while (i < sql.Length && sql[i] != ']')
                {
                    i++;
                }
                if (i < sql.Length)
                {
                    i++;
                }
                continue;
            }

            i++;
        }

        return tokens;
    }

    private static bool IsIdentifierChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_' || c == '#';
    }
}
