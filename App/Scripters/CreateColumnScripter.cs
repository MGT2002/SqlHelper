// Ignore Spelling: Scripter

using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using SqlHelper.Helpers;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Index = Microsoft.SqlServer.Management.Smo.Index;

namespace SqlHelper.App.Scripters;

public partial class CreateColumnScripter : IScripter
{
    public string OutputFileName { get; }

    private readonly Scripter scripter;
    private readonly AppSettings appSettings;
    private readonly Table table;
    private readonly Column column;

    private CreateColumnScripter(Server server, AppSettings appSettings, Table table, Column column)
    {
        scripter = new Scripter(server) { Options = OptionBuilder.CreateDefaultDriAllOptions() };
        this.appSettings = appSettings;
        this.table = table;
        this.column = column;
        OutputFileName = $"{nameof(CreateColumnScripter)}_{table.Name}_{column.Name}";
    }


    public static IScripter Create(Server server, Database db, AppSettings appSettings)
    {
        var schemaName = appSettings.ScripterData.SchemaName;
        var tableName = appSettings.ScripterData.TableName;
        var columnName = appSettings.ScripterData.ColumnName;
        var table = db.Tables[tableName, schemaName] ?? throw new InvalidOperationException($"Table {schemaName}.{tableName} not found. Please update appsettings.json.");
        var column = table.Columns[columnName] ?? throw new InvalidOperationException($"Column {columnName} not found in table {tableName}.");

        return new CreateColumnScripter(server, appSettings, table, column);
    }

    #region Run
    public StringCollection Run()
    {
        UrnCollection urnsToScript = new UrnCollection();
        StringCollection scripts = [];

        CreateAlterScript(scripts);

        AddRelatedConstraints(urnsToScript);

        AddRelatedIndexes(urnsToScript);

        AddRelatedTriggers(urnsToScript);

        AddRelatedExtendedProperties(scripts);

        ScriptFullTextIfColumnParticipates(urnsToScript);

        // Script the collected objects
        scripts.AddRange([.. scripter.Script(urnsToScript.ToArray())!]);
        if (urnsToScript.Count > 0)
        {
            Console.WriteLine($"Generating scripts for {column.Name} related objects...\n");
        }
        else
        {
            Console.WriteLine($"No objects found related to column {column.Name}.\n");
        }

        return scripts;
    }

    private void AddRelatedExtendedProperties(StringCollection scripts)
    {
        foreach (ExtendedProperty ep in column.ExtendedProperties)
        {
            var epName = ep.Name.Replace("'", "''");
            var epValue = (ep.Value?.ToString() ?? "").Replace("'", "''");

            Console.WriteLine($"Found extended property: {ep.Name}");

            scripts.Add(
                $@"EXEC sys.sp_addextendedproperty
@name = N'{epName}', @value = N'{epValue}', 
@level0type = N'SCHEMA', @level0name = N'{table.Schema}', 
@level1type = N'TABLE',  @level1name = N'{table.Name}', 
@level2type = N'COLUMN', @level2name = N'{column.Name}'");
        }
    }

    /// <summary>Script triggers (filter for column references)</summary>
    private void AddRelatedTriggers(UrnCollection urnsToScript)
    {
        foreach (Trigger trigger in table.Triggers)
        {
            string triggerText = trigger.TextBody;
            if (Regex.IsMatch(triggerText, $@"\b{Regex.Escape(column.Name)}\b", RegexOptions.IgnoreCase))
            {
                Console.WriteLine($"Found trigger referencing column: {trigger.Name}");
                urnsToScript.Add(trigger.Urn);
            }
        }
    }

    /// <summary>Script indexes</summary>
    private void AddRelatedIndexes(UrnCollection urnsToScript)
    {
        foreach (Index index in table.Indexes)
        {
            foreach (IndexedColumn idxCol in index.IndexedColumns)
            {
                if (idxCol.Name == column.Name)
                {
                    Console.WriteLine($"Found index: {index.Name}");
                    urnsToScript.Add(index.Urn);
                    break;
                }
            }
        }
    }

    /// <summary>Script the table (only the column definition)</summary>
    private void CreateAlterScript(StringCollection scripts)
    {
        Console.WriteLine($"Scripting Alter for column: {column.Name}");

        var dt = column.DataType;

        string typeDecl = dt.SqlDataType.ToString();
        // handle lengths/precision/scale
        if (dt.SqlDataType == SqlDataType.VarChar)
            typeDecl += $"({(dt.MaximumLength == -1 ? "MAX" : dt.MaximumLength)})";
        if (dt.SqlDataType == SqlDataType.Decimal)
            typeDecl += $"({dt.NumericPrecision},{dt.NumericScale})";

        string alter = $@"
ALTER TABLE [{table.Schema}].[{table.Name}]
ADD [{column.Name}] {typeDecl} {(column.Nullable ? "NULL" : "NOT NULL")};
            ";

        scripts.Add(alter);
    }

    /// <summary>Script constraints (Primary Key, Foreign Key, Check, Default)</summary>
    private void AddRelatedConstraints(UrnCollection urnsToScript)
    {
        foreach (Check check in table.Checks)
        {
            if (check.Text.Contains($"[{column.Name}]")) // Check if column is referenced
            {
                Console.WriteLine($"Found check constraint: {check.Name}");
                urnsToScript.Add(check.Urn);
            }
        }

        foreach (ForeignKey fk in table.ForeignKeys)
        {
            foreach (ForeignKeyColumn fkCol in fk.Columns)
            {
                if (fkCol.Name == column.Name)
                {
                    Console.WriteLine($"Found foreign key: {fk.Name}");
                    urnsToScript.Add(fk.Urn);
                    break;
                }
            }
        }

        foreach (Column col in table.Columns)
        {
            if (col.Name == column.Name && col.DefaultConstraint != null)
            {
                Console.WriteLine($"Found default constraint: {col.DefaultConstraint.Name}");
                urnsToScript.Add(col.DefaultConstraint.Urn);
            }
        }

        // Check for primary key
        foreach (Index index in table.Indexes)
        {
            if (index.IndexKeyType == IndexKeyType.DriPrimaryKey)
            {
                foreach (IndexedColumn idxCol in index.IndexedColumns)
                {
                    if (idxCol.Name == column.Name)
                    {
                        Console.WriteLine($"Found primary key: {index.Name}");
                        urnsToScript.Add(index.Urn);
                        break;
                    }
                }
            }
        }
    }

    // Convenience: one call that checks and returns the script (or empty string)
    void ScriptFullTextIfColumnParticipates(UrnCollection urns)
    {
        if (IsColumnInFullText())
        {
            Console.WriteLine($"Found FullTextIndex: {table.FullTextIndex.CatalogName}_{table.FullTextIndex.UniqueIndexName}");

            ScriptFullTextObjects();
        }

        bool IsColumnInFullText()
        {
            return table.FullTextIndex != null &&
                   table.FullTextIndex.IndexedColumns.Contains(column.Name);
        }

        void ScriptFullTextObjects()
        {
            var fti = table.FullTextIndex;
            if (fti == null) return;

            // If the FT index targets a catalog, try to include the catalog object too
            // (so the script is self-contained).
            if (!string.IsNullOrEmpty(fti.CatalogName))
            {
                var db = table.Parent; // Database
                var catalog = db.FullTextCatalogs[fti.CatalogName];
                if (catalog != null)
                    urns.Add(catalog.Urn);
            }

            urns.Add(fti.Urn);
        }
    }
    #endregion

    public partial class DropColumnScripter { }
}
