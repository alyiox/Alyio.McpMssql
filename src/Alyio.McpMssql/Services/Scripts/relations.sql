SELECT
    o.name AS [name],
    CASE o.type
        WHEN 'U' THEN 'TABLE'
        WHEN 'V' THEN 'VIEW'
    END AS [type]
FROM sys.objects o
JOIN sys.schemas s
    ON s.schema_id = o.schema_id
WHERE
    o.type IN ('U', 'V')
    AND (@is_ms_shipped IS NULL OR o.is_ms_shipped = @is_ms_shipped)
    AND (@schema IS NULL OR s.name = @schema)
ORDER BY
    o.type,
    o.name;
