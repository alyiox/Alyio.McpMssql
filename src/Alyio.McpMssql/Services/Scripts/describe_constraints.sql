/*
    describe_constraints
    --------------------
    Returns logical constraints grouped by type:

    Result set 1: PRIMARY KEYS
    Result set 2: UNIQUE CONSTRAINTS
    Result set 3: FOREIGN KEYS
    Result set 4: CHECK CONSTRAINTS
    Result set 5: DEFAULT CONSTRAINTS

    Notes:
    - Physical index metadata intentionally excluded.
    - Column order preserved for multi-column constraints.
    - @schema is optional.
*/

------------------------------------------------------------
-- 1 PRIMARY KEYS
------------------------------------------------------------
SELECT
    kc.name                AS constraint_name,
    c.name                 AS column_name,
    ic.key_ordinal         AS column_ordinal   -- 1..n in key order
FROM sys.key_constraints kc
INNER JOIN sys.tables t
    ON kc.parent_object_id = t.object_id
INNER JOIN sys.schemas s
    ON t.schema_id = s.schema_id
INNER JOIN sys.index_columns ic
    ON kc.parent_object_id = ic.object_id
   AND kc.unique_index_id = ic.index_id
INNER JOIN sys.columns c
    ON ic.object_id = c.object_id
   AND ic.column_id = c.column_id
WHERE
    kc.type = 'PK'
    AND t.name = @table
    AND (@schema IS NULL OR s.name = @schema)
ORDER BY
    kc.name,
    ic.key_ordinal;


------------------------------------------------------------
-- 2 UNIQUE CONSTRAINTS
------------------------------------------------------------
SELECT
    kc.name                AS constraint_name,
    c.name                 AS column_name,
    ic.key_ordinal         AS column_ordinal
FROM sys.key_constraints kc
INNER JOIN sys.tables t
    ON kc.parent_object_id = t.object_id
INNER JOIN sys.schemas s
    ON t.schema_id = s.schema_id
INNER JOIN sys.index_columns ic
    ON kc.parent_object_id = ic.object_id
   AND kc.unique_index_id = ic.index_id
INNER JOIN sys.columns c
    ON ic.object_id = c.object_id
   AND ic.column_id = c.column_id
WHERE
    kc.type = 'UQ'
    AND t.name = @table
    AND (@schema IS NULL OR s.name = @schema)
ORDER BY
    kc.name,
    ic.key_ordinal;


------------------------------------------------------------
-- 3 FOREIGN KEYS
------------------------------------------------------------
SELECT
    fk.name                        AS constraint_name,
    pc.name                        AS column_name,
    fkc.constraint_column_id       AS column_ordinal,      -- 1..n in FK order
    rs.name                        AS referenced_schema,
    rt.name                        AS referenced_table,
    rc.name                        AS referenced_column,
    fk.delete_referential_action_desc AS on_delete,
    fk.update_referential_action_desc AS on_update,
    fk.is_disabled,
    fk.is_not_trusted
FROM sys.foreign_keys fk
INNER JOIN sys.tables t
    ON fk.parent_object_id = t.object_id
INNER JOIN sys.schemas s
    ON t.schema_id = s.schema_id
INNER JOIN sys.foreign_key_columns fkc
    ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns pc
    ON fkc.parent_object_id = pc.object_id
   AND fkc.parent_column_id = pc.column_id
INNER JOIN sys.tables rt
    ON fkc.referenced_object_id = rt.object_id
INNER JOIN sys.schemas rs
    ON rt.schema_id = rs.schema_id
INNER JOIN sys.columns rc
    ON fkc.referenced_object_id = rc.object_id
   AND fkc.referenced_column_id = rc.column_id
WHERE
    t.name = @table
    AND (@schema IS NULL OR s.name = @schema)
ORDER BY
    fk.name,
    fkc.constraint_column_id;


------------------------------------------------------------
-- 4 CHECK CONSTRAINTS
------------------------------------------------------------
SELECT
    cc.name                AS constraint_name,
    cc.definition,
    cc.is_disabled,
    cc.is_not_trusted
FROM sys.check_constraints cc
INNER JOIN sys.tables t
    ON cc.parent_object_id = t.object_id
INNER JOIN sys.schemas s
    ON t.schema_id = s.schema_id
WHERE
    t.name = @table
    AND (@schema IS NULL OR s.name = @schema)
ORDER BY
    cc.name;


------------------------------------------------------------
-- 5 DEFAULT CONSTRAINTS
------------------------------------------------------------
SELECT
    dc.name                AS constraint_name,
    c.name                 AS column_name,
    dc.definition
FROM sys.default_constraints dc
INNER JOIN sys.tables t
    ON dc.parent_object_id = t.object_id
INNER JOIN sys.schemas s
    ON t.schema_id = s.schema_id
INNER JOIN sys.columns c
    ON dc.parent_object_id = c.object_id
   AND dc.parent_column_id = c.column_id
WHERE
    t.name = @table
    AND (@schema IS NULL OR s.name = @schema)
ORDER BY
    dc.name;
