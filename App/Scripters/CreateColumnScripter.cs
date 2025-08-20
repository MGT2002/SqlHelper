// Ignore Spelling: Scripter

using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using System.Collections.Specialized;
using static SqlHelper.Helpers.Constants.Settings;

namespace SqlHelper.App.Scripters;

internal class CreateColumnScripter : IScripter
{
    public string OutputFileName { get; }

    private readonly Scripter scripter;
    private readonly IConfigurationRoot config;
    private readonly Table table;
    private readonly Column column;

    private CreateColumnScripter(Server server, IConfigurationRoot config, Table table, Column column)
    {
        scripter = new Scripter(server) { Options = OptionBuilder.CreateDefaultDriAllOptions() };
        this.config = config;
        this.table = table;
        this.column = column;
        OutputFileName = $"{nameof(CreateColumnScripter)}_{table.Name}_{column.Name}";
    }


    static IScripter IScripter.Create(Server server, Database db, IConfigurationRoot config)
    {
        var schemaName = config[TableSchemaName];
        var tableName = config[TableName];
        var columnName = config[ColumnName];
        var table = db.Tables[tableName, schemaName] ?? throw new InvalidOperationException($"Table {schemaName}.{tableName} not found. Please update appsettings.json.");
        var column = table.Columns[columnName] ?? throw new InvalidOperationException($"Column {columnName} not found in table {tableName}.");

        return new CreateColumnScripter(server, config, table, column);
    }

    public StringCollection Run()
    {
        var roots = new List<Urn> { table.Urn };

        var lines = scripter.Script(roots.ToArray());

        return lines;
    }
}
