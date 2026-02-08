// MIT License

namespace Alyio.McpMssql.Tests.Infrastructure.Fixtures;

/// <summary>
/// xUnit collection marker that serializes all tests using <see cref="SqlServerFixture"/>.
/// </summary>
/// <remarks>
/// Parallel execution is disabled because the fixture manages a shared SQL Server
/// test database that is created, seeded, and mutated during test execution.
/// </remarks>
[CollectionDefinition("SqlServer", DisableParallelization = true)]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "<Pending>")]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture> { }
