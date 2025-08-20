using Microsoft.Data.SqlClient;
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

        var outputFile = Path.GetFullPath($"./{tableName}_with_deps.sql");
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
        File.WriteAllLines(outputFile, ToStringArray(lines));

        Console.WriteLine($"✔ Script written to: {outputFile}");
    }

    private static StringCollection RunScripter(Scripter scripter, Table table)
    {
        // Build a URN list rooted at the table, then let SMO discover dependencies
        var roots = new List<Urn> { table.Urn };

        // Parents + Children ensures keys/refs and dependent bits are ordered correctly
        var tree = scripter.DiscoverDependencies(roots.ToArray(), DependencyType.Parents | DependencyType.Children);

        // Walk dependency tree to get URNs in executable order
        var ordered = scripter.WalkDependencies(tree);

        // Script all objects in order
        var lines = scripter.ScriptWithList(ordered);
        return lines;
    }

    private static Scripter CreateScripter(out string tableName, out SqlConnection sqlConn, out Table table)
    {
        // === CONFIG ===
        var serverName = @"DESKTOP-EB20ETC\SQL2022";
        var databaseName = "Quinn";
        var schemaName = "phx";
        tableName = "Bill";


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
            WithDependencies = true,          // include dependent/related objects
            Indexes = true,                   // script indexes
            DriAll = true,                    // PK, FK, CK, UQ
            Triggers = true,                  // table triggers
            ExtendedProperties = true,        // EPs
            IncludeIfNotExists = true,
            ScriptBatchTerminator = true,     // adds "GO"
            SchemaQualify = true,
            NoFileGroup = false               // include filegroups if present
            // You can also set: ScriptDataCompression = true, ScriptStatistics = true, NoCollation = false, IncludeHeaders = true, etc.
        };

        return new Scripter(server) { Options = opt };
    }

    static string[] ToStringArray(StringCollection sc)
    {
        var arr = new string[sc.Count];
        sc.CopyTo(arr, 0);
        return arr;
    }
}
