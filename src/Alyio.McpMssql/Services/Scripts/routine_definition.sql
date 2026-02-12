-- routine_definition: T-SQL body from sys.sql_modules for one routine.
SELECT m.definition
FROM sys.objects o
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
INNER JOIN sys.sql_modules m ON m.object_id = o.object_id
WHERE o.type IN ('P', 'FN', 'TF', 'IF')
    AND (@schema IS NULL OR s.name = @schema)
    AND o.name = @name
