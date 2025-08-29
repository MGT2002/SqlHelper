using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Smo;
using SqlHelper.App;
using SqlHelper.App.Scripters;
using System.Collections.Specialized;
using static SqlHelper.Helpers.StaticHelper;

class Program
{
    static void Main(string[] args)
    {
        RunScripter<InsertRandomDataInTableScripter>(out var outputFilePath3);
        Console.WriteLine($"Script written to: {outputFilePath3}");

        //RunScripter<CreateColumnScripter>(out var outputFilePath);
        //Console.WriteLine($"Script written to: {outputFilePath}");

        //RunScripter<CreateColumnScripter.DropColumnScripter>(out var outputFilePath2);
        //Console.WriteLine($"Script written to: {outputFilePath2}");
    }

    private static void RunScripter<T>(out string outputFilePath) where T : IScripter
    {
        ConnectToDb(out var sqlConn, out var server, out var db, out var config);

        IScripter scripter = CreateScripter<T>(server, db, config);

        StringCollection lines = scripter.Run();

        outputFilePath = GetOutputPath(scripter);

        Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath)!);
        File.WriteAllLines(outputFilePath, ToStringArray(lines).Select(l => $"{l}\n"));
    }

    private static IScripter CreateScripter<T>(Server server, Database db, IConfigurationRoot config) where T : IScripter
    {
        return T.Create(server, db, config);
    }
}
