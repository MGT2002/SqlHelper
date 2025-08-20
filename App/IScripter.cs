// Ignore Spelling: Scripter

using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Smo;
using System.Collections.Specialized;

namespace SqlHelper.App;

public interface IScripter
{
    string OutputFileName { get; }

    static abstract IScripter Create(Server server, Database db, IConfigurationRoot config);
    StringCollection Run();
}
