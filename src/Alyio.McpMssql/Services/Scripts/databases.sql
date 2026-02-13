SELECT
    [name],
    state_desc,
    is_read_only,
    CAST(CASE
        WHEN [name] IN ('master', 'tempdb', 'model', 'msdb') OR is_distributor = 1 THEN 1
        ELSE 0
    END AS BIT) AS is_system_db
FROM sys.databases
ORDER BY database_id
