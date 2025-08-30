// Ignore Spelling: Scripter

using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using SqlHelper.Helpers;
using System.Collections.Specialized;

namespace SqlHelper.App.Scripters;

internal class CreateTableScripter : IScripter
{
    public string OutputFileName { get; }

    private readonly Scripter scripter;
    private readonly AppSettings appSettings;
    private readonly Table table;

    private CreateTableScripter(Server server, AppSettings appSettings, Table table)
    {
        scripter = new Scripter(server) { Options = OptionBuilder.CreateDefaultDriAllOptions() };
        this.appSettings = appSettings;
        this.table = table;
        OutputFileName = $"{nameof(CreateTableScripter)}_{table.Name}";
    }

    static IScripter IScripter.Create(Server server, Database db, AppSettings appSettings)
    {
        var schemaName = appSettings.ScripterData.SchemaName;
        var tableName = appSettings.ScripterData.TableName;
        var table = db.Tables[tableName, schemaName] ?? throw new Exception($"Table {schemaName}.{tableName} not found. Please update appsettings.json.");

        return new CreateTableScripter(server, appSettings, table);
    }

    public StringCollection Run()
    {
        // Build a URN list rooted at the table, then let SMO discover dependencies
        var roots = new List<Urn> { table.Urn };

        //// Parents + Children ensures keys/refs and dependent bits are ordered correctly
        //var tree = scripter.DiscoverDependencies(roots.ToArray(), parents: false);

        //// Walk dependency tree to get URNs in executable order
        //var ordered = scripter.WalkDependencies(tree);

        //var result = ordered;

        //// Script all objects in order
        //var lines = scripter.ScriptWithList(ordered);

        var lines = scripter.Script(roots.ToArray());

        return lines;
    }
}
