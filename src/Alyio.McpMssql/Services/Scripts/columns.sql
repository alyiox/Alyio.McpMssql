SELECT
    c.name AS [name],
    t.name AS [type],
    c.is_nullable,
    c.column_id
FROM sys.columns c
JOIN sys.types t
    ON t.user_type_id = c.user_type_id
JOIN sys.objects o
    ON o.object_id = c.object_id
JOIN sys.schemas s
    ON s.schema_id = o.schema_id
WHERE
    o.type IN ('U','V')
    AND (@is_ms_shipped IS NULL OR o.is_ms_shipped = @is_ms_shipped)
    AND o.name = @name
    AND (@schema IS NULL OR s.name = @schema)
ORDER BY
    c.column_id;
