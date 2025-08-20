// Ignore Spelling: Scripter

using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Smo;
using System.Collections.Specialized;

namespace SqlHelper.App.Scripters;

public partial class CreateColumnScripter
{
    public partial class DropColumnScripter : IScripter
    {
        public string OutputFileName { get; }

        private CreateColumnScripter columnScripter;

        public DropColumnScripter(CreateColumnScripter columnScripter)
        {
            this.columnScripter = columnScripter;
            OutputFileName = columnScripter.OutputFileName.Replace(nameof(CreateColumnScripter), nameof(DropColumnScripter));

            columnScripter.scripter.Options.ScriptDrops = true;
        }


        public static IScripter Create(Server server, Database db, IConfigurationRoot config)
        {
            return new DropColumnScripter((CreateColumnScripter)CreateColumnScripter.Create(server, db, config));
        }

        public StringCollection Run() => columnScripter.Run();
    }
}
