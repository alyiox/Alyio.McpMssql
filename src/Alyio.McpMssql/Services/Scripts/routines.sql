-- list_routines: procedures and functions (P, FN, TF, IF) from sys.objects.
SELECT
    o.name AS [name],
    o.type AS [type]
FROM sys.objects o
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
WHERE o.type IN ('P', 'FN', 'TF', 'IF')
    AND (@schema IS NULL OR s.name = @schema)
    AND (@is_ms_shipped IS NULL OR o.is_ms_shipped = @is_ms_shipped)
ORDER BY o.name;
