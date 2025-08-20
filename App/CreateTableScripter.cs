// Ignore Spelling: Scripter

using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Smo;

namespace SqlHelper.App;

internal class CreateTableScripter : IScripter
{
    private readonly Scripter scripter;
    private readonly IConfigurationRoot config;
    private readonly Table table;

    private CreateTableScripter(Server server, IConfigurationRoot config, Table table)
    {
        scripter = new Scripter(server) { Options = OptionBuilder.CreateDefaultDriAllOptions() };
        this.config = config;
        this.table = table;
    }

    static IScripter IScripter.Create(Server server, Database db, IConfigurationRoot config)
    {
        var schemaName = config["ScripterData:SchemaName"];
        var tableName = config["ScripterData:BillItemType"];
        var table = db.Tables[tableName, schemaName] ?? throw new Exception($"Table {schemaName}.{tableName} not found. Please update appsettings.json.");

        return new CreateTableScripter(server, config, table);
    }

    public void Run()
    {
        throw new NotImplementedException();
    }
}
