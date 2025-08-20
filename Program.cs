using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;

class Program
{
    static void Main(string[] args)
    {
        Scripter scripter = CreateScripter(out var tableName, out var sqlConn, out var table);

        StringCollection lines = RunScripter(scripter, table);

        string outputFile = GetOutputPath(tableName);

        Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
        File.WriteAllLines(outputFile, ToStringArray(lines));

        Console.WriteLine($"✔ Script written to: {outputFile}");
    }

    private static StringCollection RunScripter(Scripter scripter, Table table)
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

    private static Scripter CreateScripter(out string tableName, out SqlConnection sqlConn, out Table table)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // read values directly without a class
        string serverName = config["Connection:ServerName"]!;
        string databaseName = config["Connection:DatabaseName"]!;

        Console.WriteLine($"Server:   {serverName}");
        Console.WriteLine($"Database: {databaseName}");

        var schemaName = "phx";
        tableName = "BillItemType";


        // Use SQL Auth:
        var csb = new SqlConnectionStringBuilder
        {
            DataSource = serverName,
            InitialCatalog = databaseName,

            //UserID = "sa",            // or your SQL login
            //Password = "YourPassword",
            //TrustServerCertificate = true

            IntegratedSecurity = true,   // <-- Windows Auth
            Encrypt = true,              // keep if your server enforces TLS
            TrustServerCertificate = true // optional for dev; prefer real certs in prod
        };

        sqlConn = new SqlConnection(csb.ConnectionString);
        var svrConn = new ServerConnection(sqlConn);
        var server = new Server(svrConn);
        var db = server.Databases[databaseName] ?? throw new Exception($"DB '{databaseName}' not found.");

        table = db.Tables[tableName, schemaName] ?? throw new Exception($"Table {schemaName}.{tableName} not found.");

        // --- Scripting options (similar to SSMS “Generate Scripts…/Advanced”) ---
        var opt = new ScriptingOptions
        {
            ScriptDrops = false,
            WithDependencies = false,          // include dependent/related objects
            Indexes = true,                   // script indexes
            DriAll = true,                    // PK, FK, CK, UQ
            FullTextIndexes = true,
            IncludeHeaders = true,
            Triggers = true,                  // table triggers
            ExtendedProperties = true,        // EPs
            
            IncludeIfNotExists = false,
            ScriptBatchTerminator = true,     // adds "GO"
            SchemaQualify = true,
            NoFileGroup = false,             // include filegroups if present
            ScriptDataCompression = true,           
            Statistics = true,
            NoCollation = false,

            TargetServerVersion = SqlServerVersion.VersionLatest,
        };

        return new Scripter(server) { Options = opt };
    }

    private static string GetOutputPath(string tableName)
    {
        string exeDir = AppDomain.CurrentDomain.BaseDirectory;
        string projectDir = Path.GetFullPath(Path.Combine(exeDir, @"..\..\.."));
        var outputFile = Path.Combine(projectDir, "Generated", $"{tableName}_with_deps.sql");

        return outputFile;
    }

    static string[] ToStringArray(StringCollection sc)
    {
        var arr = new string[sc.Count];
        sc.CopyTo(arr, 0);
        return arr;
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
