using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Collections.Specialized;

namespace SqlHelper.Helpers;

public static class StaticHelper
{
    public static void ConnectToDb(out SqlConnection sqlConn, out Server server, out Database db, out IConfigurationRoot config)
    {
        config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // read values directly without a class
        string serverName = config[Constants.Settings.ServerName]!;
        string databaseName = config[Constants.Settings.DatabaseName]!;

        Console.WriteLine($"Server:   {serverName}");
        Console.WriteLine($"Database: {databaseName}");


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
        server = new Server(svrConn);
        db = server.Databases[databaseName] ?? throw new Exception($"DB '{databaseName}' not found.");
    }

    public static string GetOutputPath(string tableName)
    {
        string exeDir = AppDomain.CurrentDomain.BaseDirectory;
        string projectDir = Path.GetFullPath(Path.Combine(exeDir, @"..\..\.."));
        var outputFile = Path.Combine(projectDir, "Generated", $"{tableName}_with_deps.sql");

        return outputFile;
    }

    public static string[] ToStringArray(StringCollection sc)
    {
        var arr = new string[sc.Count];
        sc.CopyTo(arr, 0);
        return arr;
    }
}
