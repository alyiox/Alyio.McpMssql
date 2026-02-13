SELECT DISTINCT s.name
FROM sys.schemas s
JOIN sys.objects o
    ON o.schema_id = s.schema_id
WHERE
    @is_ms_shipped IS NULL OR o.is_ms_shipped = @is_ms_shipped
ORDER BY s.name;
