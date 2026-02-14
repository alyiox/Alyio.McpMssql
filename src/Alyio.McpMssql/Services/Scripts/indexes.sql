SELECT
    i.name                    AS index_name,
    i.type_desc               AS index_type,        -- CLUSTERED / NONCLUSTERED
    i.is_unique               AS is_unique,
    i.is_disabled             AS is_disabled,
    i.has_filter              AS has_filter,
    i.filter_definition,
    ic.key_ordinal            AS key_ordinal,       -- 0 = included column, 1+ = key
    ic.is_descending_key      AS is_descending,
    c.name                    AS column_name,
    CASE WHEN ic.key_ordinal = 0 THEN 1 ELSE 0 END AS is_included_column
FROM sys.indexes i
INNER JOIN sys.index_columns ic
    ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c
    ON ic.object_id = c.object_id AND ic.column_id = c.column_id
INNER JOIN sys.objects o
    ON i.object_id = o.object_id AND o.type IN ('U', 'V')
INNER JOIN sys.schemas s
    ON o.schema_id = s.schema_id
WHERE o.name = @table
  AND (@schema IS NULL OR s.name = @schema)
  AND i.type > 0                              -- exclude heap (index_id = 0)
ORDER BY
    i.name,
    CASE WHEN ic.key_ordinal = 0 THEN 1 ELSE 0 END,  -- key columns (1..n) first, then included (0)
    ic.key_ordinal,
    ic.index_column_id;
