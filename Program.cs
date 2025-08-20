using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using SqlHelper.App;
using SqlHelper.Helpers;
using System.Collections.Specialized;
using static SqlHelper.Helpers.StaticHelper;

class Program
{
    static void Main(string[] args)
    {
        ConnectToDb(out var sqlConn, out var server, out var db, out var config);

        IScripter scripter = CreateScripter<CreateTableScripter>(server, db, config);

        StringCollection lines = scripter.Run();

        string outputFile = GetOutputPath(config[Constants.Settings.TableName]!);

        Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
        File.WriteAllLines(outputFile, ToStringArray(lines).Select(l => $"{l}\n"));

        Console.WriteLine($"Script written to: {outputFile}");
    }

    private static IScripter CreateScripter<T>(Server server, Database db, IConfigurationRoot config) where T : IScripter
    {
        return T.Create(server, db, config);
    }

    /*
     using System;
using System.IO;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
 
class Program
{
    static void Main(string[] args)
    {
        // Define connection and object details
        string serverName = "localhost"; // Replace with your server name
        string databaseName = "AdventureWorks"; // Replace with your database name
        string tableName = "Person"; // Replace with your table name
        string columnName = "FirstName"; // Replace with your column name
        string outputFile = $@"C:\Scripts\{tableName}_{columnName}_related.sql"; // Output file path
 
        try
        {
            // Create a server connection
            ServerConnection serverConnection = new ServerConnection(serverName);
            Server server = new Server(serverConnection);
 
            // Reference the database
            Database database = server.Databases[databaseName];
            if (database == null)
            {
                Console.WriteLine($"Database {databaseName} not found.");
                return;
            }
 
            // Reference the specific table
            Table table = database.Tables[tableName];
            if (table == null)
            {
                Console.WriteLine($"Table {tableName} not found in database {databaseName}.");
                return;
            }
 
            // Verify the column exists
            Column column = table.Columns[columnName];
            if (column == null)
            {
                Console.WriteLine($"Column {columnName} not found in table {tableName}.");
                return;
            }
 
            // Initialize the Scripter object
            Scripter scripter = new Scripter(server);
 
            // Configure scripting options
            scripter.Options.ScriptDrops = false; // Generate CREATE statements
            scripter.Options.WithDependencies = false; // Exclude dependent objects
            scripter.Options.Indexes = true; // Include indexes
            scripter.Options.DriAllConstraints = true; // Include all constraints
            scripter.Options.Triggers = true; // Include triggers
            scripter.Options.IncludeIfNotExists = true; // Add IF NOT EXISTS clause
            scripter.Options.ToFileOnly = true; // Write to file
            scripter.Options.FileName = outputFile; // Set output file
            scripter.Options.AppendToFile = false; // Overwrite file
 
            // Collect objects related to the column
            UrnCollection urnsToScript = new UrnCollection();
 
            // 1. Script the table (only the column definition)
            Console.WriteLine($"Scripting table definition for column: {columnName}");
            urnsToScript.Add(table.Urn);
 
            // 2. Script constraints (Primary Key, Foreign Key, Check, Default)
            foreach (Check check in table.Checks)
            {
                if (check.Text.Contains($"[{columnName}]")) // Check if column is referenced
                {
                    Console.WriteLine($"Found check constraint: {check.Name}");
                    urnsToScript.Add(check.Urn);
                }
            }
 
            foreach (ForeignKey fk in table.ForeignKeys)
            {
                foreach (ForeignKeyColumn fkCol in fk.Columns)
                {
                    if (fkCol.Name == columnName)
                    {
                        Console.WriteLine($"Found foreign key: {fk.Name}");
                        urnsToScript.Add(fk.Urn);
                        break;
                    }
                }
            }
 
            foreach (Column col in table.Columns)
            {
                if (col.Name == columnName && col.DefaultConstraint != null)
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
                        if (idxCol.Name == columnName)
                        {
                            Console.WriteLine($"Found primary key: {index.Name}");
                            urnsToScript.Add(index.Urn);
                            break;
                        }
                    }
                }
            }
 
            // 3. Script indexes
            foreach (Index index in table.Indexes)
            {
                foreach (IndexedColumn idxCol in index.IndexedColumns)
                {
                    if (idxCol.Name == columnName)
                    {
                        Console.WriteLine($"Found index: {index.Name}");
                        urnsToScript.Add(index.Urn);
                        break;
                    }
                }
            }
 
            // 4. Script triggers (filter for column references)
            foreach (Trigger trigger in table.Triggers)
            {
                string triggerText = trigger.TextBody;
                if (Regex.IsMatch(triggerText, $@"\b{Regex.Escape(columnName)}\b", RegexOptions.IgnoreCase))
                {
                    Console.WriteLine($"Found trigger referencing column: {trigger.Name}");
                    urnsToScript.Add(trigger.Urn);
                }
            }
 
            // 5. Script the collected objects
            if (urnsToScript.Count > 0)
            {
                Console.WriteLine($"Generating scripts for {columnName} related objects...");
                StringCollection scripts = scripter.Script(urnsToScript.ToArray());
 
                // Write scripts to file with batch terminators
                using (StreamWriter writer = new StreamWriter(outputFile))
                {
                    foreach (string script in scripts)
                    {
                        writer.WriteLine(script);
                        writer.WriteLine("GO"); // Add batch terminator
                    }
                }
                Console.WriteLine($"Scripts generated successfully at {outputFile}");
            }
            else
            {
                Console.WriteLine($"No objects found related to column {columnName}.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
 
     */
}
