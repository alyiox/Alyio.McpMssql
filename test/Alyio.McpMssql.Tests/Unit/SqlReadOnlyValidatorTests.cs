// MIT License

using Alyio.McpMssql.Internal;

namespace Alyio.McpMssql.Tests.Unit;

public class SqlReadOnlyValidatorTests
{
    [Theory]
    [InlineData("select * from Users")]
    [InlineData("SELECT Id, Name FROM dbo.Users")]
    [InlineData(" select * from Users ; ")]
    [InlineData("with cte as (select * from Users) select * from cte")]
    [InlineData("-- comment\nselect * from Users")]
    [InlineData("select 'insert into table' as Value")]
    [InlineData("select [insert] from [table]")]
    [InlineData("select \"delete\" from \"table\"")]
    public void Validate_Allows_ReadOnly_Select_Queries(string sql)
    {
        // Act / Assert
        SqlReadOnlyValidator.Validate(sql);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Throws_For_Null_Or_Empty(string? sql)
    {
        Assert.Throws<ArgumentException>(() =>
            SqlReadOnlyValidator.Validate(sql!));
    }

    [Theory]
    [InlineData("insert into Users values (1)")]
    [InlineData("update Users set Name = 'x'")]
    [InlineData("delete from Users")]
    [InlineData("merge into Users as t using Users as s on 1=0")]
    [InlineData("create table Test(Id int)")]
    [InlineData("drop table Users")]
    [InlineData("truncate table Users")]
    [InlineData("exec SomeProc")]
    [InlineData("execute SomeProc")]
    [InlineData("select * into TempTable from Users")]
    public void Validate_Throws_For_Forbidden_Keywords(string sql)
    {
        Assert.Throws<InvalidOperationException>(() =>
            SqlReadOnlyValidator.Validate(sql));
    }

    [Fact]
    public void Validate_Throws_For_Multiple_Statements()
    {
        var sql = "select * from Users; select * from Orders";

        Assert.Throws<InvalidOperationException>(() =>
            SqlReadOnlyValidator.Validate(sql));
    }


    [Theory]
    [InlineData("select 'delete from Users' as SqlText")]
    [InlineData("select '-- drop table Users' as Comment")]
    [InlineData("select 'insert into x values (1)'")]
    [InlineData("select * from Users /* update Users */")]
    public void Validate_Ignores_Keywords_In_Strings_And_Comments(string sql)
    {
        SqlReadOnlyValidator.Validate(sql);
    }

    [Theory]
    [InlineData("with cte as (select * from Users) update Users set Name = 'x'")]
    [InlineData("with cte as (select * from Users) delete from Users")]
    public void Validate_Throws_For_Cte_That_Executes_Write(string sql)
    {
        Assert.Throws<InvalidOperationException>(() =>
            SqlReadOnlyValidator.Validate(sql));
    }

    [Theory]
    [InlineData("update Users set Name = 'x' -- select")]
    [InlineData("delete from Users /* select */")]
    public void Validate_Throws_When_Not_Starting_With_Select_Or_Cte(string sql)
    {
        Assert.Throws<InvalidOperationException>(() =>
            SqlReadOnlyValidator.Validate(sql));
    }
}

